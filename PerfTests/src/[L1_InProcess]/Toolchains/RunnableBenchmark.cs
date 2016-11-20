using System;
using System.IO;

using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

using static BenchmarkDotNet.Toolchains.RunnableBenchmarkFactory;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains
{
	// DONTTOUCH: please, keep these in a single file.

	// Copy of the code generated for each benchmark
	/// <summary>
	/// Implementation of <see cref="IRunnableBenchmark"/> for methods with return values.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target.</typeparam>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <seealso cref="IRunnableBenchmark"/>
	internal class RunnableBenchmark<TTarget, TResult> : IRunnableBenchmark where TTarget : new()
	{
		private Func<TResult> _runCallback;
		private Func<TResult> _idleCallback;
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private TResult Idle() => default(TResult);

		#region Copy this into InProcessProgram<TTarget>
		private Benchmark _benchmark;
		private TTarget _instance;
		private bool _isDiagnoserAttached;
		private Type _engineFactoryType;
		private Action _setupAction;
		private Action _cleanupAction;
		private int _operationsPerInvoke;
		private Job _job;
		private TextWriter _output;

		/// <summary>Initializes the specified benchmark before <see cref="IRunnableBenchmark.Run"/> call.</summary>
		/// <param name="benchmarkToRun">The benchmark that will be run.</param>
		/// <param name="engineFactoryType">Type of the engine factory.</param>
		/// <param name="output">The writer to redirect the output.</param>
		/// <param name="isDiagnoserAttached"><c>true</c> if there is diagnoser attached.</param>
		public void Init(Benchmark benchmarkToRun, Type engineFactoryType, TextWriter output, bool isDiagnoserAttached)
		{
			// TODO: lazy RunCore! Why:
			// the first thing to do is to let diagnosers hook in before anything happens
			// so all jit-related diagnosers can catch first jit compilation!
			if (_isDiagnoserAttached) // stub code, no diagnoser supported yet.
			{
				_output.WriteLine(Engine.Signals.BeforeAnythingElse);
			}

			_benchmark = benchmarkToRun;
			_isDiagnoserAttached = isDiagnoserAttached;
			_engineFactoryType = engineFactoryType;

			var target = _benchmark.Target;
			var unrollFactor = _benchmark.Job.ResolveValue(RunMode.UnrollFactorCharacteristic, EnvResolver.Instance);
			_instance = new TTarget();
			CreateSetupAction(_instance, target, out _setupAction);
			CreateCleanupAction(_instance, target, out _cleanupAction);
			CreateRunCallback(_instance, target, out _runCallback);
			_idleCallback = Idle;

			_runCallback = Unroll(_runCallback, unrollFactor);
			_idleCallback = Unroll(_idleCallback, unrollFactor);

			_operationsPerInvoke = target.OperationsPerInvoke;
			_job = _benchmark.Job;
			_output = output;
		}

		/// <summary>Runs the benchmark.</summary>
		public void Run()
		{
			if (_benchmark == null)
				throw new InvalidOperationException("Call Init() first.");

			using (BenchmarkHelpers.CaptureConsoleOutput(_output))
			{
				try
				{
					FillProperties(_instance, _benchmark);

					_output.WriteLine();
					foreach (var infoLine in BenchmarkEnvironmentInfo.GetCurrent().ToFormattedString())
					{
						_output.WriteLine("// {0}", infoLine);
					}
					_output.WriteLine("// Job: {0}", _job.DisplayInfo);
					_output.WriteLine();

					var engineParameters = new EngineParameters
					{
						MainAction = MainMultiAction,
						IdleAction = IdleMultiAction,
						SetupAction = _setupAction,
						CleanupAction = _cleanupAction,
						TargetJob = _job,
						OperationsPerInvoke = _operationsPerInvoke,
						IsDiagnoserAttached = _isDiagnoserAttached
					};

					var engine = ((IEngineFactory)Activator.CreateInstance(_engineFactoryType))
						.Create(engineParameters);

					engine.PreAllocate();

					_setupAction();

					engine.Jitting(); // does first call to main action, must be executed after setup()!

					if (_isDiagnoserAttached)
						Console.WriteLine(Engine.Signals.AfterSetup);

					var results = engine.Run();

					if (_isDiagnoserAttached)
						Console.WriteLine(Engine.Signals.BeforeCleanup);

					_cleanupAction();

					results.Print(); // printing costs memory, do this after runs
				}
				catch (Exception ex)
				{
					_output.WriteLine(ex);
					throw;
				}
			}
		}

		private void IdleMultiAction(long invokeCount)
		{
			for (long i = 0; i < invokeCount; i++)
			{
				_idleCallback();
			}
		}

		private void MainMultiAction(long invokeCount)
		{
			for (long i = 0; i < invokeCount; i++)
			{
				_runCallback();
			}
		}
		#endregion
	}

	// Copy of the InProcessProgram<TTarget, TResult>. Func<TResult> => Action.
	/// <summary>
	/// Implementation of <see cref="IRunnableBenchmark"/> for methods that DO NOT return value.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target.</typeparam>
	/// <seealso cref="IRunnableBenchmark"/>
	internal class RunnableBenchmark<TTarget> : IRunnableBenchmark where TTarget : new()
	{
		private Action _runCallback;
		private Action _idleCallback;
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private void Idle() { }

		#region Copied from InProcessProgram<TTarget, TResult>
		private Benchmark _benchmark;
		private TTarget _instance;
		private bool _isDiagnoserAttached;
		private Type _engineFactoryType;
		private Action _setupAction;
		private Action _cleanupAction;
		private int _operationsPerInvoke;
		private Job _job;
		private TextWriter _output;

		/// <summary>Initializes the specified benchmark before <see cref="IRunnableBenchmark.Run"/> call.</summary>
		/// <param name="benchmarkToRun">The benchmark that will be run.</param>
		/// <param name="engineFactoryType">Type of the engine factory.</param>
		/// <param name="output">The writer to redirect the output.</param>
		/// <param name="isDiagnoserAttached"><c>true</c> if there is diagnoser attached.</param>
		public void Init(Benchmark benchmarkToRun, Type engineFactoryType, TextWriter output, bool isDiagnoserAttached)
		{
			// TODO: lazy RunCore! Why:
			// the first thing to do is to let diagnosers hook in before anything happens
			// so all jit-related diagnosers can catch first jit compilation!
			if (_isDiagnoserAttached) // stub code, no diagnoser supported yet.
			{
				_output.WriteLine(Engine.Signals.BeforeAnythingElse);
			}

			_benchmark = benchmarkToRun;
			_isDiagnoserAttached = isDiagnoserAttached;
			_engineFactoryType = engineFactoryType;

			var target = _benchmark.Target;
			var unrollFactor = _benchmark.Job.ResolveValue(RunMode.UnrollFactorCharacteristic, EnvResolver.Instance);
			_instance = new TTarget();
			CreateSetupAction(_instance, target, out _setupAction);
			CreateCleanupAction(_instance, target, out _cleanupAction);
			CreateRunCallback(_instance, target, out _runCallback);
			_idleCallback = Idle;

			_runCallback = Unroll(_runCallback, unrollFactor);
			_idleCallback = Unroll(_idleCallback, unrollFactor);

			_operationsPerInvoke = target.OperationsPerInvoke;
			_job = _benchmark.Job;
			_output = output;
		}

		/// <summary>Runs the benchmark.</summary>
		public void Run()
		{
			if (_benchmark == null)
				throw new InvalidOperationException("Call Init() first.");

			using (BenchmarkHelpers.CaptureConsoleOutput(_output))
			{
				try
				{
					FillProperties(_instance, _benchmark);

					_output.WriteLine();
					foreach (var infoLine in BenchmarkEnvironmentInfo.GetCurrent().ToFormattedString())
					{
						_output.WriteLine("// {0}", infoLine);
					}
					_output.WriteLine("// Job: {0}", _job.DisplayInfo);
					_output.WriteLine();

					var engineParameters = new EngineParameters
					{
						MainAction = MainMultiAction,
						IdleAction = IdleMultiAction,
						SetupAction = _setupAction,
						CleanupAction = _cleanupAction,
						TargetJob = _job,
						OperationsPerInvoke = _operationsPerInvoke,
						IsDiagnoserAttached = _isDiagnoserAttached
					};

					var engine = ((IEngineFactory)Activator.CreateInstance(_engineFactoryType))
						.Create(engineParameters);

					engine.PreAllocate();

					_setupAction();

					engine.Jitting(); // does first call to main action, must be executed after setup()!

					if (_isDiagnoserAttached)
						Console.WriteLine(Engine.Signals.AfterSetup);

					var results = engine.Run();

					if (_isDiagnoserAttached)
						Console.WriteLine(Engine.Signals.BeforeCleanup);

					_cleanupAction();

					results.Print(); // printing costs memory, do this after runs
				}
				catch (Exception ex)
				{
					_output.WriteLine(ex);
					throw;
				}
			}
		}

		private void IdleMultiAction(long invokeCount)
		{
			for (long i = 0; i < invokeCount; i++)
			{
				_idleCallback();
			}
		}

		private void MainMultiAction(long invokeCount)
		{
			for (long i = 0; i < invokeCount; i++)
			{
				_runCallback();
			}
		}
		#endregion
	}
}