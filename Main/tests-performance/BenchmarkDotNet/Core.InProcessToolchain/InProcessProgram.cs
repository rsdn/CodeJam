using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

using static BenchmarkDotNet.Toolchains.ProgramFactory;

// ReSharper disable CheckNamespace
namespace BenchmarkDotNet.Toolchains
{
	public interface IProgram
	{
		void Init(Benchmark benchmarkToRun);
		void Run();
	}

	public static class ProgramFactory
	{
		public static IProgram CreateInProcessProgram(Benchmark benchmark)
		{
			var target = benchmark.Target;
			var targetMethodReturnType = target.Method.ReturnType;
			if (targetMethodReturnType == typeof(void))
			{
				var programType = typeof(InProcessProgram<>).MakeGenericType(
					target.Type);
				var program = Activator.CreateInstance(programType, benchmark);
				return (IProgram)program;
			}
			else
			{
				var programType = typeof(InProcessProgram<,>).MakeGenericType(
					target.Type, targetMethodReturnType);
				var program = Activator.CreateInstance(programType, benchmark);
				return (IProgram)program;
			}
		}

		#region Init helpers
		// DONTTOUCH: 'out TDelegate' used to enable type inference;
		private static void Create<TDelegate>(
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
			Create(instance, targetInfo.SetupMethod, out result);
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
			Create(instance, targetInfo.Method, out result);
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
			Create(instance, targetInfo.Method, out result);
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

				if (paramProperty == null || paramProperty.SetMethod == null)
					throw new InvalidOperationException(
						$"Type {targetType.FullName}: no settable property {parameter.Name} found.");

				if (paramProperty.SetMethod.IsStatic)
				{
					paramProperty.SetValue(null, parameter.Value);
				}
				else
				{
					paramProperty.SetValue(instance, parameter.Value);
				}
			}
		}
		#endregion
	}

	// Copy of the code generated for each benchmark
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class InProcessProgram<TTarget, TResult> : IProgram where TTarget : new()
	{
		private Func<TResult> runCallback;
		private Func<TResult> idleCallback;
		private TResult Idle() => default(TResult);

		#region Copy this into InProcessProgram<TTarget>
		private Benchmark benchmark;
		private TTarget instance;
		private Action setupAction;
		private int operationsPerInvoke;
		private IJob job;

		public void Init(Benchmark benchmarkToRun)
		{
			benchmark = benchmarkToRun;
			var target = benchmark.Target;
			instance = new TTarget();
			CreateSetupAction(instance, target, out setupAction);
			CreateRunCallback(instance, target, out runCallback);
			idleCallback = Idle;

			operationsPerInvoke = target.OperationsPerInvoke;
			job = benchmark.Job;

			SetupProperties(instance, benchmarkToRun);
		}

		public void Run()
		{
			try
			{
				SetupProperties(instance, benchmark);
				setupAction();
				runCallback();

				Console.WriteLine();
				foreach (var infoLine in EnvironmentInfo.GetCurrent().ToFormattedString())
				{
					Console.WriteLine("// {0}", infoLine);
				}
				Console.WriteLine();

				new MethodInvoker().Invoke(job, operationsPerInvoke, setupAction, runCallback, idleCallback);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		} 
		#endregion
	}

	// Copy of the InProcessProgram<TTarget, TResult>. Func<TResult> => Action.
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class InProcessProgram<TTarget> : IProgram where TTarget : new()
	{
		private Action runCallback;
		private Action idleCallback;
		private void Idle() { }

		#region Copied from InProcessProgram<TTarget, TResult>
		private Benchmark benchmark;
		private TTarget instance;
		private Action setupAction;
		private int operationsPerInvoke;
		private IJob job;

		public void Init(Benchmark benchmarkToRun)
		{
			benchmark = benchmarkToRun;
			var target = benchmark.Target;
			instance = new TTarget();
			CreateSetupAction(instance, target, out setupAction);
			CreateRunCallback(instance, target, out runCallback);
			idleCallback = Idle;

			operationsPerInvoke = target.OperationsPerInvoke;
			job = benchmark.Job;

			SetupProperties(instance, benchmarkToRun);
		}

		public void Run()
		{
			try
			{
				SetupProperties(instance, benchmark);
				setupAction();
				runCallback();

				Console.WriteLine();
				foreach (var infoLine in EnvironmentInfo.GetCurrent().ToFormattedString())
				{
					Console.WriteLine("// {0}", infoLine);
				}
				Console.WriteLine();

				new MethodInvoker().Invoke(job, operationsPerInvoke, setupAction, runCallback, idleCallback);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				throw;
			}
		}
		#endregion

	}
}