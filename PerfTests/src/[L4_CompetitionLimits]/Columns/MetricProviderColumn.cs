using System;
using System.Linq;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Columns
{
	[PublicAPI]
	public class MetricProviderColumn : IColumn
	{
		public MetricProviderColumn(ILimitMetricProvider metricProvider, bool useUpperBoundary)
		{
			Code.NotNull(metricProvider, nameof(metricProvider));
			MetricProvider = metricProvider;
			UseUpperBoundary = useUpperBoundary;
		}

		public bool UseUpperBoundary { get; }
		public ILimitMetricProvider MetricProvider { get; }

		public string GetValue(Summary summary, Benchmark benchmark)
		{
			double lower, upper;
			if (MetricProvider.TryGetMetrics(benchmark, summary, out lower, out upper))
			{
				var value = UseUpperBoundary ? lower : upper;
				return value.ToString("N2", EnvironmentInfo.MainCultureInfo);
			}

			return "?";
		}

		public string ColumnName => MetricProvider.ShortInfo + (UseUpperBoundary ? "(upper)" : "(lower)");

		public bool IsAvailable(Summary summary) => summary.Benchmarks.Any(b => b.Target.Baseline);

		public bool AlwaysShow => true;

		public ColumnCategory Category => ColumnCategory.Statistics;

		public override string ToString() => ColumnName;
	}
}