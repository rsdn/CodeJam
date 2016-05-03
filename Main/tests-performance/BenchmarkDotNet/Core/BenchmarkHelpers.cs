using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Jobs;
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
			string.Format(EnvironmentInfo.MainCultureInfo, $"{{0:{format}}}", value);
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
			var values = summary.Reports.Single(r => r.Benchmark == benchmark)
				.GetResultRuns()
				.Select(r => r.GetAverageNanoseconds());
			return Percentile(values, percentileRatio);
		}
		#endregion
	}
}