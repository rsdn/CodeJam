using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

using static BenchmarkDotNet.Toolchains.RunnableBenchmarkFactory;

namespace BenchmarkDotNet.Toolchains
{
	// DONTTOUCH: please, keep these in a single file.

	// Copy of the code generated for each benchmark
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "PassStringInterpolation")]
	public class RunnableBenchmark<TTarget, TResult> : IRunnableBenchmark where TTarget : new()
	{
		private Func<TResult> runCallback;
		private Func<TResult> idleCallback;
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private TResult Idle() => default(TResult);

		#region Copy this into InProcessProgram<TTarget>
		private Benchmark benchmark;
		private TTarget instance;
		private Action setupAction;
		private int operationsPerInvoke;
		private IJob job;
		private TextWriter output;

		public void Init(Benchmark benchmarkToRun, TextWriter textualOutput)
		{
			benchmark = benchmarkToRun;
			var target = benchmark.Target;
			instance = new TTarget();
			CreateSetupAction(instance, target, out setupAction);
			CreateRunCallback(instance, target, out runCallback);
			idleCallback = Idle;

			operationsPerInvoke = target.OperationsPerInvoke;
			job = benchmark.Job;
			output = textualOutput;

			SetupProperties(instance, benchmarkToRun);
		}

		public void Run()
		{
			if (benchmark == null)
				throw new InvalidOperationException("Call Init() first.");

			var oldWriter = Console.Out;
			try
			{
				Console.SetOut(output);
				SetupProperties(instance, benchmark);
				setupAction();
				runCallback();

				output.WriteLine();
				foreach (var infoLine in EnvironmentInfo.GetCurrent().ToFormattedString())
				{
					output.WriteLine("// {0}", infoLine);
				}
				output.WriteLine();

				new MethodInvoker().Invoke(job, operationsPerInvoke, setupAction, runCallback, idleCallback);
			}
			catch (Exception ex)
			{
				output.WriteLine(ex);
				throw;
			}
			finally
			{
				Console.SetOut(oldWriter);
			}
		}
		#endregion
	}

	// Copy of the InProcessProgram<TTarget, TResult>. Func<TResult> => Action.
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "PassStringInterpolation")]
	public class RunnableBenchmark<TTarget> : IRunnableBenchmark where TTarget : new()
	{
		private Action runCallback;
		private Action idleCallback;
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private void Idle() { }

		#region Copied from InProcessProgram<TTarget, TResult>
		private Benchmark benchmark;
		private TTarget instance;
		private Action setupAction;
		private int operationsPerInvoke;
		private IJob job;
		private TextWriter output;

		public void Init(Benchmark benchmarkToRun, TextWriter textualOutput)
		{
			benchmark = benchmarkToRun;
			var target = benchmark.Target;
			instance = new TTarget();
			CreateSetupAction(instance, target, out setupAction);
			CreateRunCallback(instance, target, out runCallback);
			idleCallback = Idle;

			operationsPerInvoke = target.OperationsPerInvoke;
			job = benchmark.Job;
			output = textualOutput;

			SetupProperties(instance, benchmarkToRun);
		}

		public void Run()
		{
			if (benchmark == null)
				throw new InvalidOperationException("Call Init() first.");

			var oldWriter = Console.Out;
			try
			{
				Console.SetOut(output);
				SetupProperties(instance, benchmark);
				setupAction();
				runCallback();

				output.WriteLine();
				foreach (var infoLine in EnvironmentInfo.GetCurrent().ToFormattedString())
				{
					output.WriteLine("// {0}", infoLine);
				}
				output.WriteLine();

				new MethodInvoker().Invoke(job, operationsPerInvoke, setupAction, runCallback, idleCallback);
			}
			catch (Exception ex)
			{
				output.WriteLine(ex);
				throw;
			}
			finally
			{
				Console.SetOut(oldWriter);
			}
		}
		#endregion
	}
}