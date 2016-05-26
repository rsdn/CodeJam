using System;
using System.Reflection;

using BenchmarkDotNet.Portability;
using BenchmarkDotNet.Running;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	public static class RunnableBenchmarkFactory
	{
		public static IRunnableBenchmark Create(Benchmark benchmark)
		{
			var target = benchmark.Target;
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
		// DONTTOUCH: 'out TDelegate' used to enable type inference;
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

		// DONTTOUCH: 'out TDelegate' used to enable type inference;
		internal static void CreateSetupAction(
			object instance, Target targetInfo, out Action result)
		{
			CreateCallback(instance, targetInfo.SetupMethod, out result);
			if (result == null)
			{
				// setupMethod is optional, so default to an empty delegate, so there is always something that can be invoked
				result = () => { };
			}
		}

		// DONTTOUCH: 'out TDelegate' used to enable type inference;
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

		internal static void SetupProperties(object instance, Benchmark benchmark)
		{
			foreach (var parameter in benchmark.Parameters.Items)
			{
				var flags = BindingFlags.Public;
				flags |= parameter.IsStatic ? BindingFlags.Static : BindingFlags.Instance;

				var targetType = benchmark.Target.Type;
				var paramProperty = targetType.GetProperty(parameter.Name, flags);

				var setter = paramProperty?.GetSetter();
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