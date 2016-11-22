using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using BenchmarkDotNet.Running;

using JetBrains.Annotations;

using static System.Linq.Expressions.Expression;
using HideFromIntelliSense = System.ComponentModel.EditorBrowsableAttribute; // we don't want people to use it

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	/// <summary>
	/// Helper class to create <see cref="IRunnableBenchmark"/> from <see cref="Benchmark"/> instance.
	/// </summary>
	public static class RunnableBenchmarkFactory
	{
		/// <summary>Creates the runnable benchmark.</summary>
		/// <param name="benchmark">Benchmark metadata.</param>
		/// <returns>An instance of runnable benchmark.</returns>
		public static IRunnableBenchmark Create(Benchmark benchmark)
		{
			if (benchmark == null)
				throw new ArgumentNullException(nameof(benchmark));

			var target = benchmark.Target;
			if (target.Type.IsAbstract || target.Type.IsValueType)
				throw new ArgumentException(
					$"The benchmark target type {target.Type.FullName} should be instantiable (not abstract and not static) class.",
					nameof(benchmark));

			var resultType = GetResultType(target.Method);
			if (resultType.IsVoid())
			{
				var programType = typeof(RunnableBenchmark<>).MakeGenericType(
					target.Type);
				var program = Activator.CreateInstance(programType);
				return (IRunnableBenchmark)program;
			}
			else
			{
				var programType = typeof(RunnableBenchmark<,,>).MakeGenericType(
					target.Type,
					GetRunType(resultType, target.Method),
					GetIdleType(resultType, target.Method));
				var program = Activator.CreateInstance(programType);
				return (IRunnableBenchmark)program;
			}
		}

		#region RunnableBenchmark helpers

		[HideFromIntelliSense(System.ComponentModel.EditorBrowsableState.Never)]
		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		private class RunnableWrapper
		{
			public const string IdlePrefix = "Idle";
			public const string RunPrefix = "ExecuteBlocking";
			public const string StaticSuffix = "Static";
			public const string InstanceSuffix = null;

			// can't use Task.CompletedTask here because it's new in .NET 4.6 (we target 4.5)
			private static readonly Task _completedTask = Task.FromResult((object)null);
			public static readonly RunnableWrapper Instance = new RunnableWrapper();

			public static void IdleVoidStatic() { }
			public void IdleVoid() { }

			public static void IdleTaskStatic() => ExecuteBlockingTask(() => _completedTask);
			public void IdleTask() => ExecuteBlockingTask(() => _completedTask);

			// we use GetAwaiter().GetResult() because it's fastest way to obtain the result in blocking way, 
			// and will eventually throw actual exception, not aggregated one
			public static void ExecuteBlockingTask(Func<Task> future) => future.Invoke().GetAwaiter().GetResult();
		}

		[HideFromIntelliSense(System.ComponentModel.EditorBrowsableState.Never)]
		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		private class RunnableWrapper<T>
		{
			private static readonly Task<T> _completedTask = Task.FromResult(default(T));
			public static readonly RunnableWrapper<T> Instance = new RunnableWrapper<T>();

			public static T IdleValueTypeStatic() => default(T);
			public T IdleValueType() => default(T);
			public static T IdleRefTypeStatic() => default(T);
			public T IdleRefType() => default(T);

			public static T IdleTaskOfTStatic() => ExecuteBlockingTaskOfT(() => _completedTask);
			public T IdleTaskOfT() => ExecuteBlockingTaskOfT(() => _completedTask);

			public static T IdleValueTaskOfTStatic() => ExecuteBlockingValueTaskOfT(() => new ValueTask<T>(default(T)));
			public T IdleValueTaskOfT() => ExecuteBlockingValueTaskOfT(() => new ValueTask<T>(default(T)));

			// we use GetAwaiter().GetResult() because it's fastest way to obtain the result in blocking way, 
			// and will eventually throw actual exception, not aggregated one
			public static T ExecuteBlockingTaskOfT(Func<Task<T>> future) => future.Invoke().GetAwaiter().GetResult();
			public static T ExecuteBlockingValueTaskOfT(Func<ValueTask<T>> future) => future.Invoke().Result;
		}

		private static Delegate CreateIdle(ResultType resultType, Type returnType, bool isStatic, int unrollFactor)
		{
			var isVoid = resultType.IsVoid();
			var delegateType = isVoid ?
				typeof(Action) :
				typeof(Func<>).MakeGenericType(returnType);
			var runnableWrapperType = isVoid
				? typeof(RunnableWrapper)
				: typeof(RunnableWrapper<>).MakeGenericType(returnType);
			var wrapperInstance = isStatic
				? null
				: runnableWrapperType.GetField(nameof(RunnableWrapper.Instance)).GetValue(null);

			var methodName = RunnableWrapper.IdlePrefix +
				resultType +
				(isStatic ? RunnableWrapper.StaticSuffix : RunnableWrapper.InstanceSuffix);

			var resultMethod = runnableWrapperType.GetMethod(methodName);
			if (resultMethod == null)
				throw new InvalidOperationException($"Method {runnableWrapperType}.{methodName} not found.");

			if (unrollFactor <= 0)
				return Delegate.CreateDelegate(delegateType, wrapperInstance, resultMethod);

			// TODO: combine delegates if .net native
			var target = isStatic ? null : Constant(wrapperInstance);
			return Lambda(
				delegateType,
				Block(
					Enumerable.Repeat(resultMethod, unrollFactor)
						.Select(m => Call(target, m))
					)).Compile();
		}

		private static Delegate CreateRun(ResultType resultType, Type returnType, object instance, MethodInfo method, int unrollFactor)
		{
			bool notTask = !resultType.IsTaskLike();
			var isVoid = resultType.IsVoid();
			var delegateType = isVoid ?
				typeof(Action) :
				typeof(Func<>).MakeGenericType(returnType);
			instance = method.IsStatic ? null : instance;

			if (unrollFactor <= 0 && notTask)
				return Delegate.CreateDelegate(delegateType, instance, method);

			var target = method.IsStatic ? null : Constant(instance);

			if (notTask)
				return Lambda(
					delegateType,
						Block(
							Enumerable.Repeat(method, unrollFactor)
								.Select(m => Call(target, m))
							)).Compile();

			var runnableWrapperType = isVoid
				? typeof(RunnableWrapper)
				: typeof(RunnableWrapper<>).MakeGenericType(returnType);
			var methodName = RunnableWrapper.RunPrefix + resultType;
			var resultMethod = runnableWrapperType.GetMethod(methodName);
			if (resultMethod == null)
				throw new InvalidOperationException($"Method {runnableWrapperType}.{methodName} not found.");

			var methodFutureType = typeof(Func<>).MakeGenericType(method.ReturnType);
			var futureArg = Delegate.CreateDelegate(methodFutureType, instance, method);

			if (unrollFactor<=1)
				return Lambda(
						delegateType,
							Call(null, resultMethod, Constant(futureArg))).Compile();

			return Lambda(
				delegateType,
				Block(
					Enumerable.Repeat(resultMethod, unrollFactor)
						.Select(m => Call(null, m, Constant(futureArg)))
					)).Compile();
		}

		/// <summary>Factory method for the setup action.</summary>
		/// <param name="instance">The instance.</param>
		/// <param name="targetInfo">The target information.</param>
		/// <param name="result">The setup action callback.</param>
		internal static void CreateSetupAction(
			object instance, Target targetInfo, out Action result)
		{
			if (targetInfo.SetupMethod == null)
			{
				// "Setup" or "Cleanup" methods are optional, so default to an empty delegate, so there is always something that can be invoked
				result = (Action)CreateIdle(ResultType.Void, typeof(void), true, 1);
				return;
			}

			var resultType = GetResultType(targetInfo.SetupMethod);
			var returnType = GetRunType(resultType, targetInfo.SetupMethod);
			result = (Action)CreateRun(resultType, returnType, instance, targetInfo.SetupMethod, 1);
		}

		/// <summary>Factory method for the cleanup action.</summary>
		/// <param name="instance">The instance.</param>
		/// <param name="targetInfo">The target information.</param>
		/// <param name="result">The cleanup action callback.</param>
		internal static void CreateCleanupAction(
			object instance, Target targetInfo, out Action result)
		{
			if (targetInfo.CleanupMethod == null)
			{
				// "Setup" or "Cleanup" methods are optional, so default to an empty delegate, so there is always something that can be invoked
				result = (Action)CreateIdle(ResultType.Void, typeof(void), true, 1);
				return;
			}

			var resultType = GetResultType(targetInfo.CleanupMethod);
			var returnType = GetRunType(resultType, targetInfo.CleanupMethod);
			result = (Action)CreateRun(resultType, returnType, instance, targetInfo.CleanupMethod, 1);
		}

		// DONTTOUCH: 'out TDelegate' used to enable type inference.
		/// <summary>Factory method for the run callback.</summary>
		/// <param name="instance">The instance.</param>
		/// <param name="targetInfo">The target information.</param>
		/// <param name="unrollFactor">The unroll factor.</param>
		/// <param name="result">The run callback.</param>
		/// <exception cref="InvalidOperationException">Benchmark {targetInfo.Type.FullName}:</exception>
		internal static void CreateRunCallback(
			object instance, Target targetInfo, int unrollFactor, out Action result)
		{
			if (targetInfo.Method == null)
			{
				throw new ArgumentException(
					nameof(targetInfo),
					$"Benchmark {targetInfo.Type.FullName}: no method to run.");
			}
			var resultType = GetResultType(targetInfo.Method);
			var returnType = GetRunType(resultType, targetInfo.Method);
			result = (Action)CreateRun(resultType, returnType, instance, targetInfo.Method, unrollFactor);
		}

		// DONTTOUCH: 'out TDelegate' used to enable type inference;
		/// <summary>Factory method for the run callback.</summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="instance">The instance.</param>
		/// <param name="targetInfo">The target information.</param>
		/// <param name="unrollFactor">The unroll factor.</param>
		/// <param name="result">The run callback.</param>
		internal static void CreateRunCallback<TResult>(
			object instance, Target targetInfo, int unrollFactor, out Func<TResult> result)
		{
			if (targetInfo.Method == null)
			{
				throw new ArgumentException(
					nameof(targetInfo),
					$"Benchmark {targetInfo.Type.FullName}: no method to run.");
			}
			var resultType = GetResultType(targetInfo.Method);
			var returnType = GetRunType(resultType, targetInfo.Method);
			result = (Func<TResult>)CreateRun(resultType, returnType, instance, targetInfo.Method, unrollFactor);

		}

		// DONTTOUCH: 'out TDelegate' used to enable type inference.
		/// <summary>Factory method for the idle callback.</summary>
		/// <param name="targetInfo">The target information.</param>
		/// <param name="unrollFactor">The unroll factor.</param>
		/// <param name="result">The idle callback.</param>
		internal static void CreateIdleCallback(
			Target targetInfo, int unrollFactor, out Action result)
		{
			if (targetInfo.Method == null)
			{
				throw new ArgumentException(
					nameof(targetInfo),
					$"Benchmark {targetInfo.Type.FullName}: no method to run.");
			}
			var resultType = GetResultType(targetInfo.Method);
			var returnType = GetIdleType(resultType, targetInfo.Method);
			result = (Action)CreateIdle(resultType, returnType, targetInfo.Method.IsStatic, unrollFactor);
		}

		// DONTTOUCH: 'out TDelegate' used to enable type inference;
		/// <summary>Factory method for the idle callback.</summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="targetInfo">The target information.</param>
		/// <param name="unrollFactor">The unroll factor.</param>
		/// <param name="result">The idle callback.</param>
		internal static void CreateIdleCallback<TResult>(
			Target targetInfo, int unrollFactor, out Func<TResult> result)
		{
			if (targetInfo.Method == null)
			{
				throw new ArgumentException(
					nameof(targetInfo),
					$"Benchmark {targetInfo.Type.FullName}: no method to run.");
			}
			var resultType = GetResultType(targetInfo.Method);
			var returnType = GetIdleType(resultType, targetInfo.Method);
			result = (Func<TResult>)CreateIdle(resultType, returnType, targetInfo.Method.IsStatic, unrollFactor);
		}

		/// <summary>Fills the properties of the instance of the object used to run the benchmark.</summary>
		/// <param name="instance">The instance.</param>
		/// <param name="benchmark">The benchmark.</param>
		internal static void FillProperties(object instance, Benchmark benchmark)
		{
			foreach (var parameter in benchmark.Parameters.Items)
			{
				var flags = BindingFlags.Public;
				flags |= parameter.IsStatic ? BindingFlags.Static : BindingFlags.Instance;

				var targetType = benchmark.Target.Type;
				var paramProperty = targetType.GetProperty(parameter.Name, flags);

				var setter = paramProperty?.GetSetMethod();
				if (setter == null)
					throw new InvalidOperationException(
						$"Type {targetType.FullName}: no settable property {parameter.Name} found.");

				// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
				if (setter.IsStatic)
				{
					setter.Invoke(null, new[] { parameter.Value });
				}
				else
				{
					setter.Invoke(instance, new[] { parameter.Value });
				}
			}
		}
		#endregion

		#region Factory advanced helpers
		private enum ResultType
		{
			Void,
			Task,
			RefType,
			ValueType,
			TaskOfT,
			ValueTaskOfT
		}

		private static bool IsVoid(this ResultType resultType) =>
			resultType == ResultType.Void || resultType == ResultType.Task;

		private static bool IsTaskLike(this ResultType resultType) =>
			resultType == ResultType.Task || resultType == ResultType.TaskOfT || resultType == ResultType.ValueTaskOfT;

		private static ResultType GetResultType(MethodInfo methodInfo) => GetResultType(methodInfo.ReturnType);

		private static ResultType GetResultType(Type resultType)
		{
			if (resultType == typeof(void))
				return ResultType.Void;
			if (resultType == typeof(Task))
				return ResultType.Task;

			if (resultType.IsGenericType)
			{
				var genericType = resultType.GetGenericTypeDefinition();

				if (typeof(Task<>).IsAssignableFrom(genericType))
					return ResultType.TaskOfT;

				if (typeof(ValueTask<>).IsAssignableFrom(genericType))
					return ResultType.ValueTaskOfT;
			}

			if (resultType.IsValueType)
				return ResultType.ValueType;

			return ResultType.RefType;
		}

		private static Type GetIdleType(ResultType resultType, MethodInfo method) =>
			resultType == ResultType.ValueType
				? typeof(int) // we return int because creating bigger ValueType could take longer than benchmarked method itself
				: GetRunType(resultType, method);

		private static Type GetRunType(ResultType resultType, MethodInfo method)
		{
			switch (resultType)
			{
				case ResultType.Void:
				case ResultType.Task:
					return typeof(void);
				case ResultType.RefType:
				case ResultType.ValueType:
					return method.ReturnType;
				case ResultType.TaskOfT:
				case ResultType.ValueTaskOfT:
					return method.ReturnType.GenericTypeArguments[0];
				default:
					throw new ArgumentOutOfRangeException(nameof(resultType));
			}
		}
		#endregion
	}
}