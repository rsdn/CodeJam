using System;
using System.IO;
using System.Reflection;

using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Toolchains
{

	// Copy of the code generated for each benchmark
	/// <summary> runnable benchmark controller.</summary>
	internal class RunnableBenchmark
	{
		/// <summary>Fills the properties of the instance of the object used to run the benchmark.</summary>
		/// <param name="instance">The instance.</param>
		/// <param name="benchmark">The benchmark.</param>
		private static void FillProperties(object instance, Benchmark benchmark)
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

		private BenchmarkAction _runCallback;
		private BenchmarkAction _idleCallback;
		private BenchmarkAction _setupCallback;
		private BenchmarkAction _cleanupCallback;

		private Benchmark _benchmark;
		private object _instance;
		private bool _isDiagnoserAttached;
		private Type _engineFactoryType;
		private int _operationsPerInvoke;
		private Job _job;
		private TextWriter _output;

		/// <summary>Initializes the specified benchmark before <see cref="Run"/> call.</summary>
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
			_instance = Activator.CreateInstance(target.Type);

			_runCallback = BenchmarkActionFactory.CreateRun(target, _instance, unrollFactor);
			_idleCallback = BenchmarkActionFactory.CreateIdle(target, _instance, unrollFactor);
			_cleanupCallback = BenchmarkActionFactory.CreateCleanup(target, _instance);
			_setupCallback = BenchmarkActionFactory.CreateSetup(target, _instance);

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
						MainAction = _runCallback.InvokeMultiple,
						IdleAction = _idleCallback.InvokeMultiple,
						SetupAction = _setupCallback.InvokeSingle,
						CleanupAction = _cleanupCallback.InvokeSingle,
						TargetJob = _job,
						OperationsPerInvoke = _operationsPerInvoke,
						IsDiagnoserAttached = _isDiagnoserAttached
					};

					var engine = ((IEngineFactory)Activator.CreateInstance(_engineFactoryType))
						.Create(engineParameters);

					engine.PreAllocate();

					_setupCallback.InvokeSingle();

					engine.Jitting(); // does first call to main action, must be executed after setup()!

					if (_isDiagnoserAttached)
						Console.WriteLine(Engine.Signals.AfterSetup);

					var results = engine.Run();

					if (_isDiagnoserAttached)
						Console.WriteLine(Engine.Signals.BeforeCleanup);

					_cleanupCallback.InvokeSingle();

					results.Print(); // printing costs memory, do this after runs
				}
				catch (Exception ex)
				{
					_output.WriteLine(ex);
					throw;
				}
			}
		}
	}
}