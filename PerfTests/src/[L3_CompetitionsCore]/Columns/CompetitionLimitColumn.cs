using System;
using System.Linq;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Limits;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Columns
{
	// TODO: column with limits from source annotations?
	/// <summary>Displays metric (upper or lower boundary) for the benchmark to baseline comparison.</summary>
	/// <seealso cref="BenchmarkDotNet.Columns.IColumn"/>
	[PublicAPI]
	public class CompetitionLimitColumn : IColumn
	{
		/// <summary>Minimum limit column</summary>
		public static readonly CompetitionLimitColumn Min = new CompetitionLimitColumn(null, false);

		/// <summary>Maximum limit column</summary>
		public static readonly CompetitionLimitColumn Max = new CompetitionLimitColumn(null, true);

		/// <summary>Initializes a new instance of the <see cref="CompetitionLimitColumn"/> class.</summary>
		/// <param name="competitionLimitProvider">The competition limit provider.</param>
		/// <param name="useMaxRatio">
		/// Use maximum timing ratio relative to the baseline. if set to <c>false</c> the minimum one used.
		/// </param>
		public CompetitionLimitColumn([CanBeNull] ICompetitionLimitProvider competitionLimitProvider, bool useMaxRatio)
		{
			ColumnName = competitionLimitProvider?.ShortInfo
				?? ("Limit" + (UseMaxRatio ? "(max)" : "(min)"));
			CompetitionLimitProvider = competitionLimitProvider;
			UseMaxRatio = useMaxRatio;
		}

		/// <summary>The name of the column.</summary>
		/// <value>The name of the column.</value>
		public string ColumnName { get; }

		/// <summary>Instance of competition limit provider.</summary>
		/// <value>The competition limit provider.</value>
		[CanBeNull]
		public ICompetitionLimitProvider CompetitionLimitProvider { get; }

		/// <summary>
		/// Use maximum timing ratio relative to the baseline. if set to <c>false</c> the minimum one used.
		/// </summary>
		/// <value>
		/// If <c>true</c>: use maximum timing ratio relative to the baseline. if set to <c>false</c> the minimum one used.
		/// </value>
		public bool UseMaxRatio { get; }

		/// <summary>Returns value for the column.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric value (upper or lower boundary) for the benchmark.</returns>
		public string GetValue(Summary summary, Benchmark benchmark)
		{
			var limitProvider = CompetitionLimitProvider
				?? CompetitionCore.RunState[summary].Options.Limits.LimitProvider;

			var result = limitProvider?.TryGetActualValues(benchmark, summary);
			if (result == null)
				return "?";

			return UseMaxRatio ? result.Value.MaxRatioText : result.Value.MinRatioText;
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

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => ColumnName;
	}
}