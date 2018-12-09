using System;
using System.Linq;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Columns
{
	/// <summary>Displays metric for the benchmark.</summary>
	/// <seealso cref="BenchmarkDotNet.Columns.IColumn"/>
	[PublicAPI]
	public class MetricValueColumn : IColumn
	{
		#region Static members & origin enum
		/// <summary>Metric column value.</summary>
		public enum Kind
		{
			/// <summary>Mean for metric.</summary>
			Mean = MetricValueColumns.Mean,

			/// <summary>Metric value standard deviation.</summary>
			StdDev = MetricValueColumns.StdDev,

			/// <summary>Min metric value.</summary>
			Min = MetricValueColumns.Min,

			/// <summary>Max metric value.</summary>
			Max = MetricValueColumns.Max
		}

		private const int PriorityInCategoryStartValue = 400;
		#endregion

		#region Fields & .ctor
		private readonly Kind _kind;

		/// <summary>Initializes a new instance of the <see cref="MetricValueColumn"/> class.</summary>
		/// <param name="metric">The metric information.</param>
		/// <param name="kind">The kind of value to display.</param>
		public MetricValueColumn([NotNull] MetricInfo metric, Kind kind) :
			this(null, metric, kind)
		{ }

		/// <summary>Initializes a new instance of the <see cref="MetricValueColumn"/> class.</summary>
		/// <param name="name">The  column name.</param>
		/// <param name="metric">The metric information.</param>
		/// <param name="kind">The kind of value to display.</param>
		public MetricValueColumn([CanBeNull] string name, [NotNull] MetricInfo metric, Kind kind)
		{
			DebugEnumCode.Defined(kind, nameof(kind));
			_kind = kind;

			ColumnName = name ?? (metric.DisplayName + (kind == Kind.Mean ? "" : "-" + kind));
			Metric = metric;
			PriorityInCategory = PriorityInCategoryStartValue;
		}
		#endregion

		#region Properties
		/// <summary>Gets name of the column.</summary>
		/// <value>The name of the column.</value>
		[NotNull]
		public string ColumnName { get; }

		/// <summary>Column description.</summary>
		/// <value>The column description.</value>
		public string Legend => "";

		/// <summary>
		/// An unique identifier of the column.
		/// <remarks>
		/// If there are several columns with the same Id, only one of them will be shown in the summary.
		/// </remarks>
		/// </summary>
		/// <value>The unique identificator of the column.</value>
		[NotNull]
		public string Id => ColumnName;

		/// <summary>Should be shown anyway.</summary>
		/// <value><c>true</c> if should be shown anyway.</value>
		public bool AlwaysShow => true;

		/// <summary>Gets category of the column.</summary>
		/// <value>The category of the column.</value>
		public ColumnCategory Category => ColumnCategory.Statistics;

		/// <summary>Gets order of column in the same category.</summary>
		/// <returns>The order of column in the same category.</returns>
		public int PriorityInCategory { get; }

		/// <summary>Gets column kind.</summary>
		/// <value>The column kind.</value>
		public Kind ColumnKind { get; }

		/// <summary>Gets metric info.</summary>
		/// <value>The metric info.</value>
		[NotNull]
		public MetricInfo Metric { get; }

		/// <summary>Defines if the column's value represents a number.</summary>
		/// <value><c>true</c> if the column's value represents a number.</value>
		public bool IsNumeric => true;

		/// <summary>Defines how to format column's value.</summary>
		/// <value>Format column mode.</value>
		public UnitType UnitType => UnitType.Dimensionless;
		#endregion

		/// <summary>Returns value for the column.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric value (upper or lower boundary) for the benchmark.</returns>
		public string GetValue(Summary summary, BenchmarkCase benchmark) => GetValue(summary, benchmark, null);

		/// <summary>Returns value for the column.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <param name="style">The summary style.</param>
		/// <returns>The value for the column</returns>
		public string GetValue(Summary summary, BenchmarkCase benchmark, ISummaryStyle style)
		{
			double result;
			var metric = Metric;
			if (metric.IsRelative && benchmark.Descriptor.Baseline)
			{
				switch (_kind)
				{
					case Kind.Min:
					case Kind.Mean:
					case Kind.Max:
						result = 1.0;
						break;
					case Kind.StdDev:
						result = 0.0;
						break;
					default:
						result = double.NaN;
						break;
				}
			}
			else
			{
				var valuesProvider = metric.ValuesProvider;
				switch (_kind)
				{
					case Kind.Min:
						result = valuesProvider.TryGetLimitValues(benchmark, summary).Min;
						break;
					case Kind.Mean:
						result = valuesProvider.TryGetMeanValue(benchmark, summary) ?? double.NaN;
						break;
					case Kind.Max:
						result = valuesProvider.TryGetLimitValues(benchmark, summary).Max;
						break;
					case Kind.StdDev:
						result = valuesProvider.TryGetVariance(benchmark, summary) ?? double.NaN;
						break;
					default:
						result = double.NaN;
						break;
				}
			}

			return double.IsNaN(result) ? "?" : result.ToString(metric.MetricUnits);
		}

		/// <summary>Determines whether the specified summary is default.</summary>
		/// <param name="summary">The summary.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns><c>true</c> if the specified summary is default; otherwise, <c>false</c>.</returns>
		public bool IsDefault(Summary summary, BenchmarkCase benchmark) => true;

		/// <summary>Can provide values for the specified summary.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns><c>true</c> if can provide values for the specified summary.</returns>
		public bool IsAvailable(Summary summary) =>
			!Metric.IsRelative || summary.BenchmarksCases.Any(b => b.Descriptor.Baseline);

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => ColumnName;
	}
}