using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;

using CodeJam;
using CodeJam.Collections;

using JetBrains.Annotations;

#pragma warning disable 1591

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Engines
{
	/// <summary>
	/// Engine that measures each call in benchmark (base class).
	/// Should be used only if call time >> than timer resolution (recommended minimum is 1500 ns).
	/// </summary>
	/// <seealso cref="BenchmarkDotNet.Engines.IEngine" />
	internal abstract class BurstModeEngineBase : IEngine
	{
		private readonly EngineParameters _engineParameters;
		private bool _isPreAllocated;
		private bool _isJitted;

		/// <summary>Initializes a new instance of the <see cref="BurstModeEngineBase"/> class.</summary>
		/// <param name="engineParameters">The engine parameters.</param>
		protected BurstModeEngineBase(EngineParameters engineParameters)
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
		}

		/// <summary>The clock.</summary>
		/// <value>The clock.</value>
		protected IClock Clock { get; }
		/// <summary>Force GC before each run.</summary>
		/// <value><c>true</c> GC should be performed before each call.</value>
		protected bool ForceAllocations { get; }
		private int UnrollFactor { get; }
		private RunStrategy Strategy { get; }
		private bool EvaluateOverhead { get; }
		private bool RemoveOutliers { get; }

		private int InvocationCount { get; }
		private int WarmupCount { get; }
		private int TargetCount { get; }

		private List<Measurement> IdleWarmupList { get; }
		private List<Measurement> WarmupList { get; }
		private List<Measurement> IdleTargetList { get; }
		private List<Measurement> TargetList { get; }

		#region Implementation of IEngine
		/// <summary>
		/// Must provoke all static .ctors and perform any other necessary allocations
		/// so Run() has 0 exclusive allocations and our Memory Diagnostics is 100% accurate!
		/// </summary>
		/// <exception cref="Exception">just use this things here to provoke static ctor</exception>
		public void PreAllocate()
		{
			var list = new List<Measurement> { new Measurement(), new Measurement() };
			list.Sort(); // provoke JIT, static ctors etc (was allocating 1740 bytes with first call)

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
				RemoveOutliers);

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

		/// <summary>
		/// Returns the result count with <see cref="OperationsPerInvoke"/> and <see cref="UnrollFactor"/> applied.
		/// </summary>
		/// <param name="original">The original count.</param>
		/// <returns>Result count.</returns>
		/// <exception cref="ArgumentOutOfRangeException">InvokeCount should be a multiple of UnrollFactor({UnrollFactor}).</exception>
		protected long GetResultCount(int original)
		{
			var resultCount = original * OperationsPerInvoke * InvocationCount;
			if (resultCount % UnrollFactor != 0)
				throw new ArgumentOutOfRangeException(
					$"InvokeCount({resultCount}) should be a multiple of UnrollFactor({UnrollFactor}).");

			return resultCount / UnrollFactor;
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
		Action<long> IEngine.MainAction => _engineParameters.MainAction;
		Action<long> IEngine.IdleAction => _engineParameters.IdleAction;
		/// <summary>Gets a value indicating whether this run has diagnoser attached.</summary>
		/// <value>
		/// <c>true</c> if this run has diagnoser attached; otherwise, <c>false</c>.
		/// </value>
		public bool IsDiagnoserAttached => _engineParameters.IsDiagnoserAttached;
		/// <summary>Gets the resolver.</summary>
		/// <value>The resolver.</value>
		public IResolver Resolver { get; }
		#endregion

		/// <summary>Jitting method.</summary>
		protected abstract void JittingCore();

		/// <summary>Forces the GC collection.</summary>
		protected static void ForceGcCollect()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		/// <summary>Core runs logic.</summary>
		/// <param name="iterationMode">The iteration mode.</param>
		/// <param name="iterationCount">The iteration count.</param>
		/// <param name="measurements">The measurements.</param>
		protected abstract void RunCore(
			IterationMode iterationMode,
			int iterationCount,
			List<Measurement> measurements);
	}

	/// <summary>
	/// Engine that measures each call in benchmark.
	/// Should be used only if call time >> than timer resolution (recommended minimum is 1500 ns).
	/// </summary>
	/// <seealso cref="BenchmarkDotNet.Engines.BurstModeEngineBase" />
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	internal class BurstModeEngine : BurstModeEngineBase
	{
		private readonly Action _idleAction;
		private readonly Action _mainAction;

		/// <summary>Initializes a new instance of the <see cref="BurstModeEngine"/> class.</summary>
		/// <param name="engineParameters">The engine parameters.</param>
		/// <param name="idleAction">The idle action.</param>
		/// <param name="mainAction">The main action.</param>
		public BurstModeEngine(EngineParameters engineParameters, [NotNull] Action idleAction, [NotNull] Action mainAction)
			: base(engineParameters)
		{
			_idleAction = idleAction;
			_mainAction = mainAction;
		}

		/// <summary>Jitting method.</summary>
		protected override void JittingCore()
		{
			// first signal about jitting is raised from auto-generated Program.cs, look at BenchmarkProgram.txt
			_idleAction.Invoke();
			_mainAction.Invoke();
		}

		/// <summary>Core runs logic.</summary>
		/// <param name="iterationMode">The iteration mode.</param>
		/// <param name="iterationCount">The iteration count.</param>
		/// <param name="measurements">The measurements.</param>
		protected override void RunCore(IterationMode iterationMode, int iterationCount, List<Measurement> measurements)
		{
			DebugCode.BugIf(measurements.Count>0, "measurements not empty.");
			DebugCode.BugIf(measurements.Capacity < iterationCount, "measurements capacity not set.");

			var resultCount = GetResultCount(iterationCount);
			var action = iterationMode.IsIdle() ? _idleAction : _mainAction;

			if (!iterationMode.IsIdle())
				SetupAction?.Invoke();

			ForceGcCollect();
			if (ForceAllocations) // DONTTOUCH: DO NOT merge loops as it will produce inaccurate results for microbenchmarks.
			{
				for (int i = 0; i < resultCount; i++)
				{
					ForceGcCollect();

					var clock = Clock.Start();
					action();
					var clockSpan = clock.Stop();

					measurements.Add(
						new Measurement(0, iterationMode, i + 1, 1, clockSpan.GetNanoseconds()));
				}
			}
			else
			{
				for (int i = 0; i < resultCount; i++)
				{
					var clock = Clock.Start();
					action();
					var clockSpan = clock.Stop();

					measurements.Add(
						new Measurement(0, iterationMode, i + 1, 1, clockSpan.GetNanoseconds()));
				}
			}

			ForceGcCollect();
			if (!iterationMode.IsIdle())
				CleanupAction?.Invoke();
		}
	}

	/// <summary>
	/// Engine that measures each call in benchmark.
	/// Should be used only if call time >> than timer resolution (recommended minimum is 1500 ns).
	/// </summary>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <seealso cref="BenchmarkDotNet.Engines.BurstModeEngineBase" />
	internal class BurstModeEngine<TResult> : BurstModeEngineBase
	{
		private readonly Func<TResult> _idleAction;
		private readonly Func<TResult> _mainAction;

		/// <summary>Initializes a new instance of the <see cref="BurstModeEngine{TResult}"/> class.</summary>
		/// <param name="engineParameters">The engine parameters.</param>
		/// <param name="idleAction">The idle action.</param>
		/// <param name="mainAction">The main action.</param>
		public BurstModeEngine(
			EngineParameters engineParameters,
			[NotNull] Func<TResult> idleAction,
			[NotNull] Func<TResult> mainAction)
			: base(engineParameters)
		{
			_idleAction = idleAction;
			_mainAction = mainAction;
		}

		/// <summary>Jitting method.</summary>
		protected override void JittingCore()
		{
			// first signal about jitting is raised from auto-generated Program.cs, look at BenchmarkProgram.txt
			_mainAction.Invoke();
			_idleAction.Invoke();
		}

		/// <summary>Core runs logic.</summary>
		/// <param name="iterationMode">The iteration mode.</param>
		/// <param name="iterationCount">The iteration count.</param>
		/// <param name="measurements">The measurements.</param>
		protected override void RunCore(IterationMode iterationMode, int iterationCount, List<Measurement> measurements)
		{
			var resultCount = GetResultCount(iterationCount);
			var action = iterationMode.IsIdle() ? _idleAction : _mainAction;

			if (!iterationMode.IsIdle())
				SetupAction?.Invoke();

			ForceGcCollect();
			if (ForceAllocations) // DONTTOUCH: DO NOT merge loops as it will produce inaccurate results for microbenchmarks.
			{
				for (int i = 0; i < resultCount; i++)
				{
					ForceGcCollect();

					var clock = Clock.Start();
					action();
					var clockSpan = clock.Stop();

					measurements.Add(
						new Measurement(0, iterationMode, i + 1, 1, clockSpan.GetNanoseconds()));
				}
			}
			else
			{
				for (int i = 0; i < resultCount; i++)
				{
					var clock = Clock.Start();
					action();
					var clockSpan = clock.Stop();

					measurements.Add(
						new Measurement(0, iterationMode, i + 1, 1, clockSpan.GetNanoseconds()));
				}
			}

			ForceGcCollect();
			if (!iterationMode.IsIdle())
				CleanupAction?.Invoke();
		}
	}
}