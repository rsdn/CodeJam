using System;
using System.Linq;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Limits;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Columns
{
	/// <summary>Displays metric (upper or lower boundary) for the benchmark to baseline comparison.</summary>
	/// <seealso cref="BenchmarkDotNet.Columns.IColumn"/>
	[PublicAPI]
	public class CompetitionLimitColumn : IColumn
	{
		#region Static members & origin enum
		// DONTTOUCH: order of the enum members matches to the order in columns
		/// <summary>Limit column kind</summary>
		public enum Kind
		{
			/// <summary>Min limit.</summary>
			Min,
			/// <summary>Mean for limit.</summary>
			Value,
			/// <summary>Max limit.</summary>
			Max,
			/// <summary>Limit variance.</summary>
			Variance
		}

		private const int PriorityInCategoryOffset = 400;

		/// <summary>Minimum limit column</summary>
		public static readonly CompetitionLimitColumn LimitMin = new CompetitionLimitColumn(Kind.Min);

		/// <summary>Actual value column</summary>
		public static readonly CompetitionLimitColumn LimitValue = new CompetitionLimitColumn(Kind.Value);

		/// <summary>Maximum limit column</summary>
		public static readonly CompetitionLimitColumn LimitMax = new CompetitionLimitColumn(Kind.Max);

		/// <summary>Maximum limit column</summary>
		public static readonly CompetitionLimitColumn LimitVariance = new CompetitionLimitColumn(Kind.Variance);
		#endregion

		#region Fields & .ctor
		private readonly Kind _kind;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompetitionLimitColumn" /> class.
		/// </summary>
		/// <param name="competitionLimitProvider">The competition limit provider.</param>
		/// <param name="kind">The kind.</param>
		public CompetitionLimitColumn([CanBeNull] ICompetitionLimitProvider competitionLimitProvider, Kind kind)
			: this(kind)
		{
			ColumnName = (competitionLimitProvider?.ShortInfo ?? "Lim") + kind;
			CompetitionLimitProvider = competitionLimitProvider;
		}

		private CompetitionLimitColumn(Kind kind)
		{
			DebugEnumCode.Defined(kind, nameof(kind));

			_kind = kind;
			ColumnName = "Lim" + kind;
			PriorityInCategory = PriorityInCategoryOffset + (int)kind;
		}
		#endregion

		#region Properties
		/// <summary>The name of the column.</summary>
		/// <value>The name of the column.</value>
		public string ColumnName { get; }

		/// <summary>
		/// An unique identifier of the column.
		/// <remarks>
		/// If there are several columns with the same Id, only one of them will be shown in the summary.
		/// </remarks>
		/// </summary>
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

		/// <summary>Instance of competition limit provider.</summary>
		/// <value>The competition limit provider.</value>
		[CanBeNull]
		public ICompetitionLimitProvider CompetitionLimitProvider { get; }
		#endregion

		/// <summary>Returns value for the column.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric value (upper or lower boundary) for the benchmark.</returns>
		public string GetValue(Summary summary, Benchmark benchmark)
		{
			var c = HostEnvironmentInfo.MainCultureInfo;
			string result = null;
			if (benchmark.Target.Baseline)
			{
				switch (_kind)
				{
					case Kind.Min:
					case Kind.Value:
					case Kind.Max:
						result = 1.0.ToString("F2", c);
						break;
					case Kind.Variance:
						result = 0.0.ToString("F2", c);
						break;
				}
			}
			else if (CompetitionLimitProvider == null)
			{
				// TODO: !!!
				//var limitProvider = CompetitionCore.RunState[summary].Options.Limits.LimitProvider;
				//switch (_kind)
				//{
				//	case Kind.Min:
				//		result = CompetitionAnalysis.TargetsSlot[summary][benchmark.Target]?.Limits.MinRatioText;
				//		break;
				//	case Kind.Value:
				//		result = limitProvider?.TryGetMeanValue(benchmark, summary)?.ToString("F2", c);
				//		break;
				//	case Kind.Max:
				//		result = CompetitionAnalysis.TargetsSlot[summary][benchmark.Target]?.Limits.MaxRatioText;
				//		break;
				//	case Kind.Variance:
				//		result = limitProvider?.TryGetVariance(benchmark, summary)?.ToString("F2", c);
				//		break;
				//}
			}
			else
			{
				var limitProvider = CompetitionLimitProvider;
				switch (_kind)
				{
					case Kind.Min:
						result = limitProvider.TryGetCompetitionLimit(benchmark, summary).MinRatioText;
						break;
					case Kind.Value:
						result = limitProvider.TryGetMeanValue(benchmark, summary)?.ToString("F2", c);
						break;
					case Kind.Max:
						result = limitProvider.TryGetCompetitionLimit(benchmark, summary).MaxRatioText;
						break;
					case Kind.Variance:
						result = limitProvider.TryGetVariance(benchmark, summary)?.ToString("F2", c);
						break;
				}
			}

			return result ?? "?";
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