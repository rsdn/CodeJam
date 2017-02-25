using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Helpers
{
	// TODO: move to different classes
	/// <summary>
	/// Helper methods for benchmark infrastructure.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	public static class BenchmarkHelpers
	{
		#region Benchmark-related
		/// <summary>Returns benchmark types from the assembly.</summary>
		/// <param name="assembly">The assembly to get benchmarks from.</param>
		/// <returns>Benchmark types from the assembly</returns>
		public static Type[] GetBenchmarkTypes([NotNull] Assembly assembly) =>
			// Use reflection for a more maintainable way of creating the benchmark switcher,
			// Benchmarks are listed in namespace order first (e.g. BenchmarkDotNet.Samples.CPU,
			// BenchmarkDotNet.Samples.IL, etc) then by name, so the output is easy to understand.
			assembly
				.GetTypes()
				.Where(
					t => t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
						.Any(m => m.GetCustomAttributes(true).OfType<BenchmarkAttribute>().Any()))
				.Where(t => !t.GetTypeInfo().IsGenericType && !t.IsAbstract)
				.OrderBy(t => t.Namespace)
				.ThenBy(t => t.Name)
				.ToArray();

		/// <summary>Creates read-only wrapper for the config.</summary>
		/// <param name="config">The config to wrap.</param>
		/// <returns>Read-only wrapper for the config.</returns>
		public static IConfig AsReadOnly(this IConfig config) => new ReadOnlyConfig(config);

		#region Selects
		/// <summary>Returns benchmarks for the summary sorted by execution order.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Benchmarks for the summary.</returns>
		public static IEnumerable<Benchmark> GetExecutionOrderBenchmarks([NotNull] this Summary summary)
		{
			var orderProvider = summary.Config.GetOrderProvider() ?? DefaultOrderProvider.Instance;
			return orderProvider.GetExecutionOrder(summary.Benchmarks);
		}

		/// <summary>Returns benchmarks for the summary sorted by display order.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Benchmarks for the summary.</returns>
		public static IEnumerable<Benchmark> GetSummaryOrderBenchmarks([NotNull] this Summary summary)
		{
			var orderProvider = summary.Config.GetOrderProvider() ?? DefaultOrderProvider.Instance;
			return orderProvider.GetSummaryOrder(summary.Benchmarks, summary);
		}

		/// <summary>Returns the baseline for the benchmark.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Baseline for the benchmark</returns>
		public static Benchmark TryGetBaseline(
			[NotNull] this Summary summary,
			[NotNull] Benchmark benchmark) =>
				summary.Benchmarks
					.Where(b => b.Job.DisplayInfo == benchmark.Job.DisplayInfo)
					.Where(b => b.Parameters.DisplayInfo == benchmark.Parameters.DisplayInfo)
					.FirstOrDefault(b => b.Target.Baseline);
		#endregion

		/// <summary>
		/// Determines whether the characteristic has influence on job execution.
		/// </summary>
		/// <param name="characteristic">The characteristic.</param>
		/// <param name="includeIgnoreOnApply">Include ignorable.</param>
		/// <returns>
		/// <c>true</c> if the characteristic has influence on job execution.
		/// </returns>
		public static bool DeterminesBehavior(this Characteristic characteristic, bool includeIgnoreOnApply = false) =>
			!characteristic.HasChildCharacteristics && (includeIgnoreOnApply || !characteristic.IgnoreOnApply);

		/// <summary>Gets the value of the current TimeSpan structure expressed in nanoseconds.</summary>
		/// <param name="timeSpan">The timespan.</param>
		/// <returns>The total number of nanoseconds represented by this instance.</returns>
		public static double TotalNanoseconds(this TimeSpan timeSpan) => timeSpan.Ticks * (1.0e9 / TimeSpan.TicksPerSecond);
		#endregion

		#region Priority & affinity
		/// <summary>Changes the priority of the thread.</summary>
		/// <param name="thread">The target thread.</param>
		/// <param name="priority">The priority.</param>
		/// <param name="logger">The logger.</param>
		public static bool SetPriority(
			[NotNull] this Thread thread,
			ThreadPriority priority,
			[NotNull] ILogger logger)
		{
			if (thread == null)
				throw new ArgumentNullException(nameof(thread));
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			try
			{
				thread.Priority = priority;
				return true;
			}
			catch (Exception ex)
			{
				logger.WriteLineError(
					$"// ! Failed to set up priority {priority} for thread {thread}. Make sure you have the right permissions. Message: {ex.Message}");
			}
			return false;
		}

		/// <summary>Changes the priority of the process.</summary>
		/// <param name="process">The target process.</param>
		/// <param name="priority">The priority.</param>
		/// <param name="logger">The logger.</param>
		public static bool SetPriority(
			[NotNull] this Process process,
			ProcessPriorityClass priority,
			[NotNull] ILogger logger)
		{
			if (process == null)
				throw new ArgumentNullException(nameof(process));
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			try
			{
				process.PriorityClass = priority;
				return true;
			}
			catch (Exception ex)
			{
				logger.WriteLineError(
					$"// ! Failed to set up priority {priority} for process {process}. Make sure you have the right permissions. Message: {ex.Message}");
			}
			return false;
		}

		/// <summary>Changes the cpu affinity of the process.</summary>
		/// <param name="process">The target process.</param>
		/// <param name="processorAffinity">The processor affinity.</param>
		/// <param name="logger">The logger.</param>
		public static bool SetAffinity(
			[NotNull] this Process process,
			IntPtr processorAffinity,
			[NotNull] ILogger logger)
		{
			if (process == null)
				throw new ArgumentNullException(nameof(process));
			if (logger == null)
				throw new ArgumentNullException(nameof(logger));

			try
			{
				var cpuMask = (1 << Environment.ProcessorCount) - 1;
				process.ProcessorAffinity = new IntPtr(processorAffinity.ToInt32() & cpuMask);
				return true;
			}
			catch (Exception ex)
			{
				logger.WriteLineError(
					$"// ! Failed to set up processor affinity 0x{(long)processorAffinity:X} for process {process}. Make sure you have the right permissions. Message: {ex.Message}");
			}
			return false;
		}

		/// <summary>Sets highest possible priority and Changes the cpu affinity of the process.</summary>
		/// <param name="processorAffinity">The processor affinity.</param>
		/// <param name="logger">The logger.</param>
		public static IDisposable SetupHighestPriorityScope(
			[CanBeNull] IntPtr? processorAffinity,
			[NotNull] ILogger logger)
		{
			var process = Process.GetCurrentProcess();
			var thread = Thread.CurrentThread;

			var oldThreadPriority = thread.Priority;
			var oldProcessPriority = process.PriorityClass;
			var oldAffinity = process.ProcessorAffinity;

			process.SetPriority(ProcessPriorityClass.RealTime, logger);
			if (processorAffinity.HasValue)
			{
				process.SetAffinity(processorAffinity.Value, logger);
			}
			thread.SetPriority(ThreadPriority.Highest, logger);

			return Disposable.Create(
				() =>
				{
					thread.SetPriority(oldThreadPriority, logger);
					if (processorAffinity.HasValue)
					{
						process.SetAffinity(oldAffinity, logger);
					}
					process.SetPriority(oldProcessPriority, logger);
				});
		}
		#endregion
	}
}