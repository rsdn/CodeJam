using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Parameters;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.Helpers
{
	/// <summary>
	/// Helper methods for benchmark infrastructure
	/// </summary>
	public static class BenchmarkHelpers
	{
		#region Common
		/// <summary>
		/// Calculates the Nth percentile from the set of values
		/// </summary>
		/// <remarks>
		/// The implementation is expected to be consistent with the one from Excel.
		/// It's a quite common to export bench output into .csv for further analysis 
		/// And it's a good idea to have same results from all tools being used.
		/// </remarks>
		/// <param name="values">Sequence of the values to be calculated</param>
		/// <param name="percentileRatio">Value in range 0.0..1.0</param>
		/// <returns>Percentile from the set of values</returns>
		// BASEDON: http://stackoverflow.com/a/8137526
		public static double Percentile(IEnumerable<double> values, double percentileRatio)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (percentileRatio < 0 || percentileRatio > 1)
			{
				throw new ArgumentOutOfRangeException(
					nameof(percentileRatio), "The percentileRatio arg should be in range of 0.0 - 1.0.");
			}

			var elements = values.ToArray();
			if (elements.Length == 0)
				return 0;

			// DONTTOUCH: the following code was taken from http://stackoverflow.com/a/8137526 and it is proven
			// to work in the same way the excel counterpart does.
			// So it's better to leave it as it is unless you do not want to reimplement it from scratch:)
			// ReSharper disable SuggestVarOrType_BuiltInTypes
			Array.Sort(elements);
			double realIndex = percentileRatio * (elements.Length - 1);
			int index = (int)realIndex;
			double frac = realIndex - index;
			if (index + 1 < elements.Length)
				return elements[index] * (1 - frac) + elements[index + 1] * frac;

			return elements[index];
			// ReSharper restore SuggestVarOrType_BuiltInTypes
		}

		/// <summary>
		/// Retrieves the typed attribute value for the specified method
		/// </summary>
		/// <typeparam name="TAttribute">Type of the attribute</typeparam>
		/// <param name="method">Method annotated with the attribute</param>
		/// <returns>The instance of attribute or <c>null</c> if none</returns>
		public static TAttribute TryGetAttribute<TAttribute>(this MethodInfo method) where TAttribute : Attribute
			=> (TAttribute)Attribute.GetCustomAttribute(method, typeof(TAttribute));
		#endregion

		#region Column helpers
		public static string ToTimeStr(this double value, TimeUnit unit = null, int unitNameWidth = 1)
		{
			unit = unit ?? TimeUnit.GetBestTimeUnit(value);
			var unitValue = TimeUnit.Convert(value, TimeUnit.Nanoseconds, unit);
			var unitName = unit.Name.PadLeft(unitNameWidth);
			return $"{unitValue.ToStr("N4")} {unitName}";
		}

		public static string ToStr(this double value, string format = "0.##") =>
			string.Format(EnvironmentHelper.MainCultureInfo, $"{{0:{format}}}", value);
		#endregion

		#region Statistics-related
		/// <summary>
		/// Returns the baseline for the benchmark
		/// </summary>
		public static Benchmark GetBaseline(this Summary summary, Benchmark benchmark) =>
			summary.Benchmarks
				.Where(b => b.Job == benchmark.Job && b.Parameters == benchmark.Parameters)
				.FirstOrDefault(b => b.Target.Baseline);

		/// <summary>
		/// Groups benchmarks being run under same conditions (job+parameters)
		/// </summary>
		public static ILookup<KeyValuePair<IJob, ParameterInstances>, Benchmark> SameConditionBenchmarks(this Summary summary)
			=> summary.Benchmarks.ToLookup(b => new KeyValuePair<IJob, ParameterInstances>(b.Job, b.Parameters));

		/// <summary>
		/// Calculates the Nth percentile for the benchmark
		/// </summary>
		public static double GetPercentile(this Summary summary, Benchmark benchmark, double percentileRatio)
		{
			var values = summary.Reports[benchmark]
				.GetResultRuns()
				.Select(r => r.GetAverageNanoseconds());
			return Percentile(values, percentileRatio);
		}
		#endregion

		#region Benchmark configuration
		/// <summary>
		/// Removes the default console logger, removes all exporters but default markdown exporter.
		/// </summary>
		// TODO: do not filter the exporters?
		public static ManualConfig CreateUnitTestConfig(IConfig template)
		{
			var result = new ManualConfig();
			result.Add(template.GetColumns().ToArray());
			//result.Add(template.GetExporters().ToArray());
			result.Add(template.GetExporters().Where(l => l == MarkdownExporter.Default).ToArray());
			result.Add(template.GetLoggers().Where(l => l != ConsoleLogger.Default).ToArray());
			result.Add(template.GetDiagnosers().ToArray());
			result.Add(template.GetAnalysers().ToArray());
			result.Add(template.GetJobs().ToArray());

			return result;
		}

		/// <summary>
		/// Checks that the assembly is build in debug mode.
		/// </summary>
		public static bool IsDebugAssembly(this Assembly assembly)
		{
			var optAtt = (DebuggableAttribute)Attribute.GetCustomAttribute(assembly, typeof(DebuggableAttribute));
			return optAtt != null && optAtt.IsJITOptimizerDisabled;
		}
		#endregion

		#region IO
		/// <summary>
		/// Writes file content without empty line at the end
		/// </summary>
		// BASEDON: http://stackoverflow.com/a/11689630
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
		#endregion
	}
}