using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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
	/// Implementation of <seealso cref="IRunnableBenchmark"/> for methods with return values.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target.</typeparam>
	/// <typeparam name="TResult">The type of the result.</typeparam>
	/// <seealso cref="IRunnableBenchmark"/>
	[SuppressMessage("ReSharper", "PassStringInterpolation")]
	public class RunnableBenchmark<TTarget, TResult> : IRunnableBenchmark where TTarget : new()
	{
		private Func<TResult> _runCallback;
		private Func<TResult> _idleCallback;
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private TResult Idle() => default(TResult);

		#region Copy this into InProcessProgram<TTarget>
		private Benchmark _benchmark;
		private TTarget _instance;
		private Action _setupAction;
		private int _operationsPerInvoke;
		private IJob _job;
		private TextWriter _output;

		/// <summary>Initializes the specified benchmark before <see cref="IRunnableBenchmark.Run"/> call.</summary>
		/// <param name="benchmarkToRun">The benchmark that will be run.</param>
		/// <param name="output">The writer to redirect the output.</param>
		public void Init(Benchmark benchmarkToRun, TextWriter output)
		{
			_benchmark = benchmarkToRun;
			var target = _benchmark.Target;
			_instance = new TTarget();
			CreateSetupAction(_instance, target, out _setupAction);
			CreateRunCallback(_instance, target, out _runCallback);
			_idleCallback = Idle;

			_operationsPerInvoke = target.OperationsPerInvoke;
			_job = _benchmark.Job;
			_output = output;
		}

		/// <summary>Runs the benchmark.</summary>
		public void Run()
		{
			if (_benchmark == null)
				throw new InvalidOperationException("Call Init() first.");

			var oldWriter = Console.Out;
			try
			{
				Console.SetOut(_output);
				FillProperties(_instance, _benchmark);
				_setupAction();
				_runCallback();

				_output.WriteLine();
				foreach (var infoLine in EnvironmentInfo.GetCurrent().ToFormattedString())
				{
					_output.WriteLine("// {0}", infoLine);
				}
				_output.WriteLine();

				new MethodInvoker().Invoke(_job, _operationsPerInvoke, _setupAction, _runCallback, _idleCallback);
			}
			catch (Exception ex)
			{
				_output.WriteLine(ex);
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
	/// <summary>
	/// Implementation of <seealso cref="IRunnableBenchmark"/> for methods that DO NOT return value.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target.</typeparam>
	/// <seealso cref="IRunnableBenchmark"/>
	[SuppressMessage("ReSharper", "PassStringInterpolation")]
	public class RunnableBenchmark<TTarget> : IRunnableBenchmark where TTarget : new()
	{
		private Action _runCallback;
		private Action _idleCallback;
		// ReSharper disable once MemberCanBeMadeStatic.Local
		private void Idle() { }

		#region Copied from InProcessProgram<TTarget, TResult>
		private Benchmark _benchmark;
		private TTarget _instance;
		private Action _setupAction;
		private int _operationsPerInvoke;
		private IJob _job;
		private TextWriter _output;

		/// <summary>Initializes the specified benchmark before <see cref="IRunnableBenchmark.Run"/> call.</summary>
		/// <param name="benchmarkToRun">The benchmark that will be run.</param>
		/// <param name="output">The writer to redirect the output.</param>
		public void Init(Benchmark benchmarkToRun, TextWriter output)
		{
			_benchmark = benchmarkToRun;
			var target = _benchmark.Target;
			_instance = new TTarget();
			CreateSetupAction(_instance, target, out _setupAction);
			CreateRunCallback(_instance, target, out _runCallback);
			_idleCallback = Idle;

			_operationsPerInvoke = target.OperationsPerInvoke;
			_job = _benchmark.Job;
			_output = output;
		}

		/// <summary>Runs the benchmark.</summary>
		public void Run()
		{
			if (_benchmark == null)
				throw new InvalidOperationException("Call Init() first.");

			var oldWriter = Console.Out;
			try
			{
				Console.SetOut(_output);
				FillProperties(_instance, _benchmark);
				_setupAction();
				_runCallback();

				_output.WriteLine();
				foreach (var infoLine in EnvironmentInfo.GetCurrent().ToFormattedString())
				{
					_output.WriteLine("// {0}", infoLine);
				}
				_output.WriteLine();

				new MethodInvoker().Invoke(_job, _operationsPerInvoke, _setupAction, _runCallback, _idleCallback);
			}
			catch (Exception ex)
			{
				_output.WriteLine(ex);
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