using System;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Horology;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.Columns
{
	[PublicAPI]
	public class PercentileColumn : IColumn
	{
		public static readonly IColumn P0Column = new PercentileColumn(0.00);
		public static readonly IColumn P50Column = new PercentileColumn(0.50);
		public static readonly IColumn P75Column = new PercentileColumn(0.75);
		public static readonly IColumn P85Column = new PercentileColumn(0.85);
		public static readonly IColumn P95Column = new PercentileColumn(0.95);
		public static readonly IColumn P100Column = new PercentileColumn(1);

		public PercentileColumn(double percentileRatio)
		{
			PercentileRatio = percentileRatio;
		}

		public double PercentileRatio { get; }

		public virtual bool AlwaysShow => true;
		public ColumnCategory Category => ColumnCategory.Statistics;
		public virtual string ColumnName => "P" + PercentileRatio * 100;
		public virtual bool IsAvailable(Summary summary) => true;
		public override string ToString() => ColumnName;

		protected virtual bool IsTimeColumn => true;

		protected string Format(double value, TimeUnit timeUnit) =>
			IsTimeColumn ? value.ToTimeStr(timeUnit) : value.ToStr();

		public virtual string GetValue(Summary summary, Benchmark benchmark)
		{
			var percentile = summary.GetPercentile(benchmark, PercentileRatio);
			return Format(percentile, summary.TimeUnit);
		}
	}

	public class ScaledPercentileColumn : PercentileColumn
	{
		public static readonly IColumn S0Column = new ScaledPercentileColumn(0.00);
		public static readonly IColumn S50Column = new ScaledPercentileColumn(0.50);
		public static readonly IColumn S75Column = new ScaledPercentileColumn(0.75);
		public static readonly IColumn S85Column = new ScaledPercentileColumn(0.85);
		public static readonly IColumn S95Column = new ScaledPercentileColumn(0.95);
		public static readonly IColumn S100Column = new ScaledPercentileColumn(1);

		public ScaledPercentileColumn(double percentileRatio) : base(percentileRatio) { }

		public override string ColumnName => "S" + PercentileRatio * 100;
		protected override bool IsTimeColumn => false;

		public override string GetValue(Summary summary, Benchmark benchmark)
		{
			var baselineBench = summary.GetBaseline(benchmark);
			if (baselineBench == null)
				return "N/A";

			var baselinePercentile = summary.GetPercentile(baselineBench, PercentileRatio);
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (baselinePercentile == 0)
				return "N/A";

			var percentile = summary.GetPercentile(benchmark, PercentileRatio);
			var ratio = percentile / baselinePercentile;

			return Format(ratio, summary.TimeUnit);
		}
	}
}