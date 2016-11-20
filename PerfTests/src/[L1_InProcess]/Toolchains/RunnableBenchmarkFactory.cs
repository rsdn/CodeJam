using System;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Running;

using static System.Linq.Expressions.Expression;

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
			// TODO: Task/Task<T> support!!!
			if (benchmark == null)
				throw new ArgumentNullException(nameof(benchmark));

			var target = benchmark.Target;
			if (target.Type.IsAbstract || target.Type.IsValueType)
				throw new ArgumentException(
					$"The benchmark target type {target.Type.FullName} should be instantiable (not abstract and not static) class.",
					nameof(benchmark));

			var targetMethodReturnType = target.Method.ReturnType;
			if (targetMethodReturnType == typeof(void))
			{
				var programType = typeof(RunnableBenchmark<>).MakeGenericType(
					target.Type);
				var program = Activator.CreateInstance(programType);
				return (IRunnableBenchmark)program;
			}
			else
			{
				var programType = typeof(RunnableBenchmark<,>).MakeGenericType(
					target.Type, targetMethodReturnType);
				var program = Activator.CreateInstance(programType);
				return (IRunnableBenchmark)program;
			}
		}

		#region RunnableBenchmark helpers
		// DONTTOUCH: 'out TDelegate' used to enable type inference.
		private static void CreateCallback<TDelegate>(
			object instance, MethodInfo method, out TDelegate result)
			where TDelegate : class
		{
			if (method == null)
			{
				result = null;
			}
			else if (method.IsStatic)
			{
				result = (TDelegate)(object)Delegate.CreateDelegate(
					typeof(TDelegate), method);
			}
			else
			{
				result = (TDelegate)(object)Delegate.CreateDelegate(
					typeof(TDelegate), instance, method);
			}
		}

		/// <summary>Factory method for the setup action.</summary>
		/// <param name="instance">The instance.</param>
		/// <param name="targetInfo">The target information.</param>
		/// <param name="result">The setup action callback.</param>
		internal static void CreateSetupAction(
			object instance, Target targetInfo, out Action result)
		{
			CreateCallback(instance, targetInfo.SetupMethod, out result);
			if (result == null)
			{
				// "Setup" or "Cleanup" methods are optional, so default to an empty delegate, so there is always something that can be invoked
				result = () => { };
			}
		}

		/// <summary>Factory method for the cleanup action.</summary>
		/// <param name="instance">The instance.</param>
		/// <param name="targetInfo">The target information.</param>
		/// <param name="result">The cleanup action callback.</param>
		internal static void CreateCleanupAction(
			object instance, Target targetInfo, out Action result)
		{
			CreateCallback(instance, targetInfo.CleanupMethod, out result);
			if (result == null)
			{
				// "Setup" or "Cleanup" methods are optional, so default to an empty delegate, so there is always something that can be invoked
				result = () => { };
			}
		}

		// DONTTOUCH: 'out TDelegate' used to enable type inference.
		/// <summary>Factory method for the run callback.</summary>
		/// <param name="instance">The instance.</param>
		/// <param name="targetInfo">The target information.</param>
		/// <param name="result">The run callback.</param>
		internal static void CreateRunCallback(
			object instance, Target targetInfo, out Action result)
		{
			CreateCallback(instance, targetInfo.Method, out result);
			if (result == null)
			{
				throw new InvalidOperationException(
					$"Benchmark {targetInfo.Type.FullName}: no method to run.");
			}
		}

		// DONTTOUCH: 'out TDelegate' used to enable type inference;
		/// <summary>Factory method for the run callback.</summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="instance">The instance.</param>
		/// <param name="targetInfo">The target information.</param>
		/// <param name="result">The run callback.</param>
		internal static void CreateRunCallback<TResult>(
			object instance, Target targetInfo, out Func<TResult> result)
		{
			CreateCallback(instance, targetInfo.Method, out result);
			if (result == null)
			{
				throw new InvalidOperationException(
					$"Benchmark {targetInfo.Type.FullName}: no method to run.");
			}
		}

		/// <summary>Unrolls the specified callback.</summary>
		/// <param name="callback">The callback.</param>
		/// <param name="unrollFactor">The unroll factor.</param>
		/// <returns>Unrolled callback.</returns>
		public static Action Unroll(Action callback, int unrollFactor)
		{
			if (unrollFactor <= 1)
				return callback;

			// TODO: combine delegates if .net native
			var target = callback.Method.IsStatic ? null : Constant(callback.Target);
			return Lambda<Action>(
				Block(
					Enumerable.Repeat(callback, unrollFactor)
						.Select(c => Call(target, callback.Method))
					)).Compile();
		}

		/// <summary>Unrolls the specified callback.</summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="callback">The callback.</param>
		/// <param name="unrollFactor">The unroll factor.</param>
		/// <returns>Unrolled callback.</returns>
		public static Func<TResult> Unroll<TResult>(Func<TResult> callback, int unrollFactor)
		{
			if (unrollFactor <= 1)
				return callback;

			// TODO: combine delegates if .net native
			var target = callback.Method.IsStatic ? null : Constant(callback.Target);
			return Lambda<Func<TResult>>(
				Block(
					Enumerable.Repeat(callback, unrollFactor)
						.Select(c => Call(target, callback.Method))
					)).Compile();
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
	}
}