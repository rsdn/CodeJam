using System;
using System.Collections.Generic;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;

using CodeJam;
using CodeJam.Collections;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Engines
{
	/// <summary>
	/// Burst mode measurements engine (a lot of runs, measure each).
	/// Recommended for use if call time >> than timer resolution (recommended minimum is 1000 ns).
	/// </summary>
	/// <seealso cref="BenchmarkDotNet.Engines.IEngine"/>
	internal sealed class BurstModeEngine : IEngine
	{
		private readonly EngineParameters _engineParameters;
		private bool _isPreAllocated;
		private bool _isJitted;

		/// <summary>Initializes a new instance of the <see cref="BurstModeEngine"/> class.</summary>
		/// <param name="engineParameters">The engine parameters.</param>
		public BurstModeEngine(EngineParameters engineParameters)
		{
			_engineParameters = engineParameters;

			Resolver = new CompositeResolver(
				EnvResolver.Instance,
				InfrastructureResolver.Instance,
				EngineResolver.Instance);

			var targetJob = engineParameters.TargetJob;
			Clock = targetJob.ResolveValue(InfrastructureMode.ClockCharacteristic, Resolver);
			ForceAllocations = targetJob.ResolveValue(GcMode.ForceCharacteristic, Resolver);
			UnrollFactor = targetJob.ResolveValue(RunMode.UnrollFactorCharacteristic, Resolver);
			Strategy = targetJob.ResolveValue(RunMode.RunStrategyCharacteristic, Resolver);
			EvaluateOverhead = targetJob.ResolveValue(AccuracyMode.EvaluateOverheadCharacteristic, Resolver);
			RemoveOutliers = targetJob.ResolveValue(AccuracyMode.RemoveOutliersCharacteristic, Resolver);

			InvocationCount = targetJob.ResolveValue(RunMode.InvocationCountCharacteristic, Resolver);
			WarmupCount = targetJob.ResolveValueAsNullable(RunMode.WarmupCountCharacteristic) ?? 1;
			TargetCount = targetJob.ResolveValueAsNullable(RunMode.TargetCountCharacteristic) ?? 1;

			IdleWarmupList = new List<Measurement>(WarmupCount);
			WarmupList = new List<Measurement>(WarmupCount);
			IdleTargetList = new List<Measurement>(TargetCount);
			TargetList = new List<Measurement>(TargetCount);

			var resultCount = OperationsPerInvoke * InvocationCount;
			if (resultCount % UnrollFactor != 0)
				throw new ArgumentOutOfRangeException(
					$"InvokeCount({resultCount}) should be a multiple of UnrollFactor({UnrollFactor}).");
			ResultIterationsCount = resultCount / UnrollFactor;
		}

		private IClock Clock { get; }
		private bool ForceAllocations { get; }
		private int UnrollFactor { get; }
		private RunStrategy Strategy { get; }
		private bool EvaluateOverhead { get; }
		private bool RemoveOutliers { get; }

		private int InvocationCount { get; }
		private int WarmupCount { get; }
		private int TargetCount { get; }
		private long ResultIterationsCount { get; }

		private List<Measurement> IdleWarmupList { get; }
		private List<Measurement> WarmupList { get; }
		private List<Measurement> IdleTargetList { get; }
		private List<Measurement> TargetList { get; }

		#region Implementation of IEngine
		/// <summary>
		/// Must provoke all static constructors and perform any other necessary allocations
		/// so Run() has 0 exclusive allocations and our Memory Diagnostics is 100% accurate!
		/// </summary>
		/// <exception cref="Exception">just use this things here to provoke static ctor</exception>
		public void PreAllocate()
		{
			var list = new List<Measurement>
			{
				new Measurement(),
				new Measurement()
			};
			list.Sort(); // provoke JIT, static constructors etc (was allocating 1740 bytes with first call)

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (TimeUnit.All == null || list[0].Nanoseconds != default(double))
				throw new Exception("just use this things here to provoke static ctor");

			IdleWarmupList.Clear();
			IdleWarmupList.Capacity = WarmupCount;
			WarmupList.Clear();
			WarmupList.Capacity = WarmupCount;
			IdleTargetList.Clear();
			IdleTargetList.Capacity = TargetCount;
			TargetList.Clear();
			TargetList.Capacity = TargetCount;

			_isPreAllocated = true;
		}

		/// <summary>
		/// Must perform jitting via warmup calls.
		/// <remarks>is called after first call to Setup, from the auto-generated benchmark process</remarks>
		/// </summary>
		public void Jitting()
		{
			JittingCore();
			_isJitted = true;
		}

		/// <summary>Runs this instance.</summary>
		/// <returns>Run results.</returns>
		/// <exception cref="InvalidOperationException">You must call PreAllocate() and Jitting() first!</exception>
		public RunResults Run()
		{
			if (!_isJitted || !_isPreAllocated)
				throw new InvalidOperationException("You must call PreAllocate() and Jitting() first!");

			_isPreAllocated = false;

			if (Strategy != RunStrategy.ColdStart)
			{
				if (EvaluateOverhead)
				{
					RunCore(IterationMode.IdleWarmup, WarmupCount, IdleWarmupList);
				}
				RunCore(IterationMode.MainWarmup, WarmupCount, WarmupList);
			}

			if (EvaluateOverhead)
			{
				RunCore(IterationMode.IdleTarget, TargetCount, IdleTargetList);
			}
			RunCore(IterationMode.MainTarget, TargetCount, TargetList);

			var results = new RunResults(
				IdleTargetList.IsNullOrEmpty() ? null : IdleTargetList,
				TargetList,
				RemoveOutliers,
				new GcStats());

			if (!IsDiagnoserAttached)
			{
				foreach (var measurement in IdleWarmupList)
				{
					Console.WriteLine(measurement.ToOutputLine());
				}
				foreach (var measurement in WarmupList)
				{
					Console.WriteLine(measurement.ToOutputLine());
				}
				foreach (var measurement in IdleTargetList)
				{
					Console.WriteLine(measurement.ToOutputLine());
				}
				foreach (var measurement in TargetList)
				{
					Console.WriteLine(measurement.ToOutputLine());
				}
			}

			return results;
		}

		/// <summary>Runs the iteration.</summary>
		/// <param name="data">The data.</param>
		/// <returns>Measurement for the iteration</returns>
		/// <exception cref="NotSupportedException"></exception>
		Measurement IEngine.RunIteration(IterationData data)
		{
			throw new NotSupportedException();
		}

		/// <summary>Writes the line.</summary>
		/// <exception cref="NotSupportedException"></exception>
		void IEngine.WriteLine()
		{
			throw new NotSupportedException();
		}

		/// <summary>Writes the line.</summary>
		/// <param name="line">The line content.</param>
		/// <exception cref="NotSupportedException"></exception>
		void IEngine.WriteLine(string line)
		{
			throw new NotSupportedException();
		}

		/// <summary>Gets the target job.</summary>
		/// <value>The target job.</value>
		public Job TargetJob => _engineParameters.TargetJob;
		/// <summary>Gets the operations per invoke.</summary>
		/// <value>The operations per invoke.</value>
		public long OperationsPerInvoke => _engineParameters.OperationsPerInvoke;
		/// <summary>Gets the setup action.</summary>
		/// <value>The setup action.</value>
		public Action SetupAction => _engineParameters.SetupAction;
		/// <summary>Gets the cleanup action.</summary>
		/// <value>The cleanup action.</value>
		public Action CleanupAction => _engineParameters.CleanupAction;
		/// <summary>Gets the idle action.</summary>
		/// <value>The idle action.</value>
		public Action<long> IdleAction => _engineParameters.IdleAction;
		/// <summary>Gets the main action.</summary>
		/// <value>The main action.</value>
		public Action<long> MainAction => _engineParameters.MainAction;
		/// <summary>Gets a value indicating whether this run has diagnoser attached.</summary>
		/// <value>
		/// <c>true</c> if this run has diagnoser attached; otherwise, <c>false</c>.
		/// </value>
		public bool IsDiagnoserAttached => _engineParameters.IsDiagnoserAttached;
		/// <summary>Gets the resolver.</summary>
		/// <value>The resolver.</value>
		public IResolver Resolver { get; }
		#endregion

		private void JittingCore()
		{
			IdleAction(1);
			MainAction(1);
		}

		private static void ForceGcCollect()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		private void RunCore(
			IterationMode iterationMode,
			int repeatCount,
			List<Measurement> measurements)
		{
			DebugCode.BugIf(measurements.Count > 0, "measurements not empty.");
			DebugCode.BugIf(measurements.Capacity < repeatCount, "measurements capacity not set.");

			var action = iterationMode.IsIdle() ? IdleAction : MainAction;
			var resultIterationsCount = ResultIterationsCount;
			// TODO: reenable after https://github.com/dotnet/BenchmarkDotNet/issues/302#issuecomment-262057686
			//if (!iterationMode.IsIdle())
			//	SetupAction?.Invoke();

			ForceGcCollect();
			if (ForceAllocations) // DONTTOUCH: DO NOT merge loops as it will produce inaccurate results for microbenchmarks.
			{
				for (var i = 0; i < repeatCount; i++)
				{
					ForceGcCollect();

					var clock = Clock.Start();
					action(resultIterationsCount);
					var clockSpan = clock.Stop();

					measurements.Add(
						new Measurement(0, iterationMode, i + 1, 1, clockSpan.GetNanoseconds()));
				}
			}
			else
			{
				for (var i = 0; i < repeatCount; i++)
				{
					var clock = Clock.Start();
					action(resultIterationsCount);
					var clockSpan = clock.Stop();

					measurements.Add(
						new Measurement(0, iterationMode, i + 1, 1, clockSpan.GetNanoseconds()));
				}
			}

			ForceGcCollect();
			// TODO: reenable after https://github.com/dotnet/BenchmarkDotNet/issues/302#issuecomment-262057686
			//if (!iterationMode.IsIdle())
			//	CleanupAction?.Invoke();
		}

		/// <summary>Returns a <see cref="String"/> that represents this instance.</summary>
		/// <returns>A <see cref="String"/> that represents this instance.</returns>
		public override string ToString() => GetType().Name;
	}
}