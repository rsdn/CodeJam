using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Strings;

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
		/// <summary>Creates read-only wrapper for the config.</summary>
		/// <param name="config">The config to wrap.</param>
		/// <returns>Read-only wrapper for the config.</returns>
		public static IConfig AsReadOnly(this IConfig config) => new ReadOnlyConfig(config);

		#region Selects
		/// <summary>Returns jobs from the benchmarks.</summary>
		/// <param name="benchmarks">The benchmarks to select jobs from.</param>
		/// <returns>Jobs from the benchmarks.</returns>
		// ReSharper disable once ReturnTypeCanBeEnumerable.Global
		public static IOrderedEnumerable<IJob> GetJobs([NotNull] this IEnumerable<Benchmark> benchmarks) =>
			benchmarks
				.Select(d => d.Job)
				.Distinct()
				.OrderBy(d => d.GetShortInfo());

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
					.Where(b => b.Job == benchmark.Job && b.Parameters == benchmark.Parameters)
					.FirstOrDefault(b => b.Target.Baseline);

		/// <summary>Returns the report for the benchmark or <c>null</c> if there's no report.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>The report for the benchmark or <c>null</c> if there's no report.</returns>
		public static BenchmarkReport TryGetBenchmarkReport(
			[NotNull] this Summary summary,
			[NotNull] Benchmark benchmark) =>
				summary.Reports.SingleOrDefault(r => r.Benchmark == benchmark);
		#endregion

		/// <summary>Gets the value of the current TimeSpan structure expressed in nanoseconds.</summary>
		/// <param name="timeSpan">The timespan.</param>
		/// <returns>The total number of nanoseconds represented by this instance.</returns>
		public static double TotalNanoseconds(this TimeSpan timeSpan) => timeSpan.Ticks * (1.0e9 / TimeSpan.TicksPerSecond);
		#endregion

		#region Loggers
		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		public static void WriteSeparatorLine([NotNull] this ILogger logger) =>
			WriteSeparatorLine(logger, null);

		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="prefix">The separator line prefix.</param>
		public static void WriteSeparatorLine([NotNull] this ILogger logger, [CanBeNull] string prefix)
		{
			logger.WriteLine();
			if (prefix.IsNullOrEmpty())
			{
				logger.WriteLine(LogKind.Header, new string('=', 40));
			}
			else
			{
				logger.WriteLine($"{prefix}{new string('=', 40 - prefix.Length)}");
			}
		}
		#endregion

		#region Reflection
		/// <summary>Checks that the assembly is build in debug mode.</summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns><c>true</c> if the assembly was build with optimizations disabled.</returns>
		public static bool IsDebugAssembly([NotNull] this Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			var optAtt = (DebuggableAttribute)Attribute.GetCustomAttribute(assembly, typeof(DebuggableAttribute));
			return optAtt != null && optAtt.IsJITOptimizerDisabled;
		}
		#endregion

		#region Process
		/// <summary>Changes the priority of the process.</summary>
		/// <param name="process">The target process.</param>
		/// <param name="priority">The priority.</param>
		/// <param name="logger">The logger.</param>
		public static void SetPriority(
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
			}
			catch (Exception ex)
			{
				logger.WriteLineError(
					string.Format(
						"// !Failed to set up priority {1}. Make sure you have the right permissions. Message: {0}", ex.Message,
						priority));
			}
		}

		/// <summary>Changes the cpu affinity of the process.</summary>
		/// <param name="process">The target process.</param>
		/// <param name="processorAffinity">The processor affinity.</param>
		/// <param name="logger">The logger.</param>
		public static void SetAffinity(
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
				process.ProcessorAffinity = processorAffinity;
			}
			catch (Exception ex)
			{
				logger.WriteLineError(
					string.Format(
						"// !Failed to set up processor affinity 0x{1:X}. Make sure you have the right permissions. Message: {0}",
						ex.Message,
						(long)processorAffinity));
			}
		}
		#endregion

		#region IO
		// TODO: test for it
		/// <summary>Writes file content without empty line at the end.</summary>
		/// <param name="path">The path.</param>
		/// <param name="lines">The lines to write.</param>
		// THANKSTO: http://stackoverflow.com/a/11689630
		public static void WriteFileContent(string path, string[] lines)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (lines == null)
				throw new ArgumentNullException(nameof(lines));

			using (var writer = File.CreateText(path))
			{
				if (lines.Length > 0)
				{
					for (var i = 0; i < lines.Length - 1; i++)
					{
						writer.WriteLine(lines[i]);
					}
					writer.Write(lines[lines.Length - 1]);
				}
			}
		}

		/// <summary>Tries to obtain text from the given URI.</summary>
		/// <param name="uri">The URI to geth the text from.</param>
		/// <returns>The text reader or <c>null</c> if none.</returns>
		public static TextReader TryGetTextFromUri(string uri) =>
			TryGetTextFromUri(uri, null);

		/// <summary>Tries to obtain text from the given URI.</summary>
		/// <param name="uri">The URI to geth the text from.</param>
		/// <param name="timeOut">The timeout.</param>
		/// <returns>The text reader or <c>null</c> if none.</returns>
		public static TextReader TryGetTextFromUri(string uri, TimeSpan? timeOut)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			var uriInst = new Uri(uri, UriKind.RelativeOrAbsolute);
			if (uriInst.IsAbsoluteUri && !uriInst.IsFile)
			{
				try
				{
					var webRequest = WebRequest.Create(uriInst);
					if (timeOut != null)
					{
						webRequest.Timeout = (int)timeOut.Value.TotalMilliseconds;
					}
					using (var response = webRequest.GetResponse())
					using (var content = response.GetResponseStream())
					{
						if (content == null)
							return null;

						using (var reader = new StreamReader(content))
						{
							return new StringReader(reader.ReadToEnd());
						}
					}
				}
				catch (WebException)
				{
					return null;
				}
			}

			var path = uriInst.IsAbsoluteUri ? uriInst.LocalPath : uri;

			return File.Exists(path) ? File.OpenText(path) : null;
		}
		#endregion
	}
}