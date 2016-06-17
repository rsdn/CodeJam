using System;
using System.Linq;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Running.CompetitionLimits;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Columns
{
	/// <summary>Displays metric (upper or lower boundary) for the benchmark to baseline comparison.</summary>
	/// <seealso cref="BenchmarkDotNet.Columns.IColumn" />
	[PublicAPI]
	public class CompetitionLimitColumn : IColumn
	{
		/// <summary>Initializes a new instance of the <see cref="CompetitionLimitColumn"/> class.</summary>
		/// <param name="competitionLimitProvider">The competition limit provider.</param>
		/// <param name="useMaxRatio">Use maximum timing ratio relative to the baseline. if set to <c>false</c> the minimum one used.</param>
		public CompetitionLimitColumn(ICompetitionLimitProvider competitionLimitProvider, bool useMaxRatio)
		{
			Code.NotNull(competitionLimitProvider, nameof(competitionLimitProvider));

			CompetitionLimitProvider = competitionLimitProvider;
			UseMaxRatio = useMaxRatio;
		}

		/// <summary>Use maximum timing ratio relative to the baseline. if set to <c>false</c> the minimum one used.</summary>
		/// <value>If <c>true</c>: use maximum timing ratio relative to the baseline. if set to <c>false</c> the minimum one used.</value>
		public bool UseMaxRatio { get; }

		/// <summary>Instance of competition limit provider.</summary>
		/// <value>The competition limit provider.</value>
		[NotNull]
		public ICompetitionLimitProvider CompetitionLimitProvider { get; }

		/// <summary>Returns value for the column.</summary>
		/// <param name="summary">The summary.</param>
		/// <param name="benchmark">The benchmark.</param>
		/// <returns>Metric value (upper or lower boundary) for the benchmark.</returns>
		public string GetValue(Summary summary, Benchmark benchmark)
		{
			var result = CompetitionLimitProvider.TryGetActualValues(benchmark, summary);
			if (result == null)
				return "?";

			return UseMaxRatio ? result.MaxRatioText : result.MinRatioText;
		}

		/// <summary>The name of the column.</summary>
		/// <value>The name of the column.</value>
		public string ColumnName => CompetitionLimitProvider.ShortInfo + (UseMaxRatio ? "(max)" : "(min)");

		/// <summary>Has value for the specified summary.</summary>
		/// <param name="summary">The summary.</param>
		/// <returns><c>True</c> if has a value.</returns>
		public bool IsAvailable(Summary summary) => summary.Benchmarks.Any(b => b.Target.Baseline);

		/// <summary>Should be shown anyway.</summary>
		/// <value><c>true</c> if should be shown anyway.</value>
		public bool AlwaysShow => true;

		/// <summary>Category of the column.</summary>
		/// <value>The category of the column.</value>
		public ColumnCategory Category => ColumnCategory.Statistics;

		/// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
		/// <returns>A <see cref="string" /> that represents this instance.</returns>
		public override string ToString() => ColumnName;
	}
}