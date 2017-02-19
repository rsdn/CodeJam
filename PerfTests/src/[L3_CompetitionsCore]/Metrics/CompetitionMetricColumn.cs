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
	public class CompetitionMetricColumn : IColumn
	{
		#region Static members & origin enum
		// DONTTOUCH: order of the enum members matches to the order in columns
		/// <summary>Limit column kind</summary>
		public enum Kind
		{
			/// <summary>Min metric value.</summary>
			Min,

			/// <summary>Mean for metric.</summary>
			Value,

			/// <summary>Max metric value.</summary>
			Max,

			/// <summary>Metric value variance.</summary>
			Variance
		}

		private const int PriorityInCategoryOffset = 400;
		#endregion

		#region Fields & .ctor
		private readonly Kind _kind;

		/// <summary>Initializes a new instance of the <see cref="CompetitionMetricColumn"/> class.</summary>
		/// <param name="competitionMetric">The competitionMetric.</param>
		/// <param name="kind">The kind of value to display.</param>
		public CompetitionMetricColumn([NotNull] CompetitionMetricInfo competitionMetric, Kind kind) :
			this(null, competitionMetric, kind) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionMetricColumn"/> class.</summary>
		/// <param name="name">The  column name.</param>
		/// <param name="competitionMetric">The competitionMetric.</param>
		/// <param name="kind">The kind of value to display.</param>
		public CompetitionMetricColumn([CanBeNull] string name, [NotNull] CompetitionMetricInfo competitionMetric, Kind kind)
		{
			DebugEnumCode.Defined(kind, nameof(kind));
			_kind = kind;

			ColumnName = name ?? (competitionMetric.Name + (kind == Kind.Value ? "" : kind.ToString()));
			CompetitionMetric = competitionMetric;
			PriorityInCategory = PriorityInCategoryOffset + (int)kind;
		}
		#endregion

		#region Properties
		/// <summary>The name of the column.</summary>
		/// <value>The name of the column.</value>
		[NotNull]
		public string ColumnName { get; }

		/// <summary>
		/// An unique identifier of the column.
		/// <remarks>
		/// If there are several columns with the same Id, only one of them will be shown in the summary.
		/// </remarks>
		/// </summary>
		[NotNull]
		public string Id => ColumnName;

		/// <summary>Should be shown anyway.</summary>
		/// <value><c>true</c> if should be shown anyway.</value>
		public bool AlwaysShow => true;

		/// <summary>Category of the column.</summary>
		/// <value>The category of the column.</value>
		public ColumnCategory Category => ColumnCategory.Statistics;

		/// <summary>Defines order of column in the same category.</summary>
		/// <returns>Order of column in the same category.</returns>
		public int PriorityInCategory { get; }

		/// <summary>Limit column kind.</summary>
		/// <value>Limit column kind.</value>
		public Kind ColumnKind { get; }

		/// <summary>Instance of competition metric info.</summary>
		/// <value>Competition metric info.</value>
		[NotNull]
		public CompetitionMetricInfo CompetitionMetric { get; }
		#endregion

		/// <summary>Returns value for the column.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric value (upper or lower boundary) for the benchmark.</returns>
		public string GetValue(Summary summary, Benchmark benchmark)
		{
			double result;
			var metric = CompetitionMetric;
			if (metric.IsRelative && benchmark.Target.Baseline)
			{
				switch (_kind)
				{
					case Kind.Min:
					case Kind.Value:
					case Kind.Max:
						result = 1.0;
						break;
					case Kind.Variance:
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
					case Kind.Value:
						result = valuesProvider.TryGetMeanValue(benchmark, summary) ?? double.NaN;
						break;
					case Kind.Max:
						result = valuesProvider.TryGetLimitValues(benchmark, summary).Max;
						break;
					case Kind.Variance:
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
		public bool IsDefault(Summary summary, Benchmark benchmark) => true;

		/// <summary>Can provide values for the specified summary.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns><c>true</c> if can provide values for the specified summary.</returns>
		public bool IsAvailable(Summary summary) => summary.Benchmarks.Any(b => b.Target.Baseline);

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => ColumnName;
	}
}