using System;
using System.Collections.Generic;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;

// TODO: rewrite, add support for diagnosers.
// Copied from Benchmark.Net.
// ReSharper disable All

namespace BenchmarkDotNet.Running
{
	internal class MethodInvokerLight
	{
		private const int MinInvokeCount = 4;
		private const int MinIterationTimeMs = 200;
		private const int WarmupAutoMinIterationCount = 5;
		private const int TargetAutoMinIterationCount = 10;
		private const double TargetIdleAutoMaxAcceptableError = 0.05;
		private const double TargetIdleAutoMaxIterationCount = 30;
		private const double TargetMainAutoMaxAcceptableError = 0.01;

		public struct Measurement
		{
			public IterationMode IterationMode { get; }
			public int LaunchIndex { get; }
			public int IterationIndex { get; }
			public long Operations { get; }
			public double Nanoseconds { get; }

			public Measurement(
				int launchIndex, IterationMode iterationMode, int iterationIndex, long operations, double nanoseconds)
			{
				IterationMode = iterationMode;
				IterationIndex = iterationIndex;
				Operations = operations;
				Nanoseconds = nanoseconds;
				LaunchIndex = launchIndex;
			}

			public string ToOutputLine() => $"{IterationMode} {IterationIndex}: {GetDisplayValue()}";
			private string GetDisplayValue() => $"{Operations} op, {Nanoseconds.ToStr()} ns, {GetAverageTime()}";
			private string GetAverageTime() => $"{(Nanoseconds / Operations).ToTimeStr()}/op";

			public override string ToString()
				=> $"#{LaunchIndex}/{IterationMode} {IterationIndex}: {Operations} op, {Nanoseconds.ToTimeStr()}";
		}

		private struct MultiInvokeInput
		{
			public IterationMode IterationMode { get; }
			public int Index { get; }
			public long InvokeCount { get; }

			public MultiInvokeInput(IterationMode iterationMode, int index, long invokeCount)
			{
				IterationMode = iterationMode;
				Index = index;
				InvokeCount = invokeCount;
			}
		}

		private readonly List<Measurement> _allMeasurements;

		public MethodInvokerLight(IJob job)
		{
			_allMeasurements = new List<Measurement>(job.WarmupCount + 2 * job.TargetCount);
		}

		private static bool InvokeBaseImplementation(IJob job, long operationsPerInvoke) =>
			job.Mode != Mode.SingleRun ||
				job.LaunchCount.IsAuto ||
				job.WarmupCount.IsAuto ||
				job.TargetCount.IsAuto ||
				operationsPerInvoke > 1;

		public void Invoke(IJob job, long operationsPerInvoke, Action setupAction, Action targetAction, Action idleAction)
		{
			if (InvokeBaseImplementation(job, operationsPerInvoke))
			{
				CodeJam.DebugCode.AssertState(false, "please check the call");
				new MethodInvoker().Invoke(job, operationsPerInvoke, setupAction, targetAction, idleAction);
				return;
			}

			// Jitting
			setupAction();
			targetAction();
			idleAction();

			// Run
			RunCore(setupAction, targetAction, IterationMode.MainWarmup, job.WarmupCount);
			RunCore(setupAction, targetAction, IterationMode.MainTarget, job.TargetCount);

			PrintResult();
		}

		public void Invoke<T>(
			IJob job, long operationsPerInvoke, Action setupAction, Func<T> targetAction, Func<T> idleAction)
		{
			if (InvokeBaseImplementation(job, operationsPerInvoke))
			{
				CodeJam.DebugCode.AssertState(false, "please check the call");
				new MethodInvoker().Invoke(job, operationsPerInvoke, setupAction, targetAction, idleAction);
				return;
			}

			// Jitting
			setupAction();
			targetAction();
			idleAction();

			// Run
			RunCore(setupAction, targetAction, IterationMode.MainWarmup, job.WarmupCount);
			RunCore(setupAction, targetAction, IterationMode.MainTarget, job.TargetCount);

			PrintResult();
		}

		private void RunCore<T>(
			Action setupAction, Func<T> targetAction, IterationMode iterationMode, Count iterationCount)
		{
			ForceGcCollect();

			setupAction();
			for (int i = 0; i < iterationCount; i++)
			{
				var chronometer = Chronometer.Start();
				targetAction();
				var clockSpan = chronometer.Stop();
				var measurement = new Measurement(0, iterationMode, i + 1, 1, clockSpan.GetNanoseconds());
				_allMeasurements.Add(measurement);
			}

			ForceGcCollect();
		}

		private void RunCore(
			Action setupAction, Action targetAction, IterationMode iterationMode, Count iterationCount)
		{
			setupAction();
			ForceGcCollect();

			for (int i = 0; i < iterationCount; i++)
			{
				var chronometer = Chronometer.Start();
				targetAction();
				var clockSpan = chronometer.Stop();

				var measurement = new Measurement(0, iterationMode, i + 1, 1, clockSpan.GetNanoseconds());
				_allMeasurements.Add(measurement);
			}

			ForceGcCollect();
		}

		private void PrintResult()
		{
			var tmp = new List<Measurement>();
			int resultIndex = 0;
			foreach (var measurement in _allMeasurements)
			{
				if (measurement.IterationMode == IterationMode.MainTarget)
					tmp.Add(
						new Measurement(
							measurement.LaunchIndex,
							IterationMode.Result,
							++resultIndex,
							measurement.Operations,
							measurement.Nanoseconds));
			}

			foreach (var measurement in _allMeasurements)
			{
				Console.WriteLine(measurement.ToOutputLine());
			}
			Console.WriteLine();

			foreach (var measurement in tmp)
			{
				Console.WriteLine(measurement.ToOutputLine());
			}
			Console.WriteLine();

			_allMeasurements.Clear();
		}

		private static void ForceGcCollect()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}
	}

	internal static class CommonExtensions
	{
		public static string ToTimeStr(this double value, TimeUnit unit = null, int unitNameWidth = 1)
		{
			unit = unit ?? TimeUnit.GetBestTimeUnit(value);
			var unitValue = TimeUnit.Convert(value, TimeUnit.Nanoseconds, unit);
			var unitName = unit.Name.PadLeft(unitNameWidth);
			return $"{unitValue.ToStr("N4")} {unitName}";
		}

		public static string ToStr(this double value, string format = "0.##")
		{
			// Here we should manually create an object[] for string.Format
			// If we write something like
			//     string.Format(EnvironmentInfo.MainCultureInfo, $"{{0:{format}}}", value)
			// it will be resolved to:
			//     string.Format(System.IFormatProvider, string, params object[]) // .NET 4.5
			//     string.Format(System.IFormatProvider, string, object)          // .NET 4.6
			// Unfortunately, Mono doesn't have the second overload (with object instead of params object[]).            
			var args = new object[] { value };
			return string.Format(EnvironmentInfo.MainCultureInfo, $"{{0:{format}}}", args);
		}
	}
}