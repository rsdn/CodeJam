using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.SourceAnnotations;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Helper class to store <see cref="CompetitionAnalyser"/> results.</summary>
	/// <seealso cref="CodeJam.PerfTests.Analysers.Analysis"/>
	internal class CompetitionAnalysis : Analysis
	{
		/// <summary>Run state slot for the competition targets.</summary>
		public static readonly RunState<CompetitionTargets> TargetsSlot = new RunState<CompetitionTargets>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CompetitionAnalysis"/> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="summary">The summary.</param>
		public CompetitionAnalysis([NotNull] string id, [NotNull] Summary summary) : base(id, summary)
		{
			Targets = TargetsSlot[summary];
		}

		/// <summary>The competition targets.</summary>
		/// <value>The competition targets.</value>
		[NotNull]
		public CompetitionTargets Targets { get; }

		/// <summary>Enumerates the targets as defined by <see cref="IOrderProvider"/>.</summary>
		/// <returns></returns>
		public IEnumerable<CompetitionTarget> GetSummaryOrderTargets() =>
			Summary
				.GetSummaryOrderBenchmarks()
				.Select(b => Targets[b.Target])
				.Where(t => t != null)
				.Distinct();

		/// <summary>Competition validation parameters.</summary>
		/// <value>Competition validation parameters.</value>
		[NotNull]
		public CompetitionCheckMode Checks => RunState.Options.Checks;

		/// <summary>Competition annotation parameters.</summary>
		/// <value>Competition annotation parameters.</value>
		[NotNull]
		public CompetitionAnnotationMode Annotations => RunState.Options.Annotations;

		/// <summary>Competition adjustment parameters.</summary>
		/// <value>Competition adjustment parameters.</value>
		[NotNull]
		public CompetitionAdjustmentMode Adjustments => RunState.Options.Adjustments;

		/// <summary><c>true</c> if rerun should be performed as analysis was failed.</summary>
		/// <value><c>true</c> if rerun should be performed as analysis was failed.</value>
		public bool RerunRequested { get; private set; }

		/// <summary>Mark analysis to request rerun. Sets <see cref="RerunRequested"/> to <c>true</c>.</summary>
		public void MarkForRerun() => RerunRequested = true;
	}
}