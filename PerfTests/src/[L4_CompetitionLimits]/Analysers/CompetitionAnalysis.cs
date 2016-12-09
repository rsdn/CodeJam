using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;

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
		private static readonly RunState<CompetitionTargets> _targetsSlot = new RunState<CompetitionTargets>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CompetitionAnalysis"/> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="summary">The summary.</param>
		public CompetitionAnalysis([NotNull] string id, [NotNull] Summary summary) : base(id, summary)
		{
			Targets = _targetsSlot[summary];
		}

		/// <summary>The competition targets.</summary>
		/// <value>The competition targets.</value>
		[NotNull]
		public CompetitionTargets Targets { get; }

		/// <summary>Enumerates the targets as defined by <see cref="IOrderProvider"/>.</summary>
		/// <returns></returns>
		public IEnumerable<CompetitionTarget> SummaryOrderTargets() =>
			Summary
				.GetSummaryOrderBenchmarks()
				.Select(b => Targets[b.Target])
				.Where(t => t != null)
				.Distinct();

		/// <summary>The limit parameters.</summary>
		/// <value>The limit parameters.</value>
		[NotNull]
		public CompetitionLimitsMode Limits => RunState.Options.Limits;

		/// <summary>The source annotation parameters.</summary>
		/// <value>The source annotation parameters.</value>
		[NotNull]
		public SourceAnnotationsMode Annotations => RunState.Options.SourceAnnotations;

		/// <summary><c>true</c> if rerun should be performed as analysis was failed.</summary>
		/// <value><c>true</c> if rerun should be performed as analysis was failed.</value>
		public bool RerunRequested { get; private set; }

		/// <summary>Mark analysis to request rerun. Sets <see cref="RerunRequested"/> to <c>true</c>.</summary>
		public void MarkForRerun() => RerunRequested = true;
	}
}