using System;
using System.Collections.Generic;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.UnitTesting
{
	using CompetitionTargets = IDictionary<Target, CompetitionTarget>;

	/// <summary>
	/// Internal class to manage consequent runs.
	/// DO NOT add this one explicitly
	/// </summary>
	internal class CompetitionState : IAnalyser
	{
		private readonly CompetitionTargets _competitionTargets = new Dictionary<Target, CompetitionTarget>();

		public IEnumerable<IWarning> Analyse(Summary summary)
		{
			var warnings = new List<IWarning>();
			if (summary.GetCompetitionParameters().AnnotateOnRun)
			{
				AnnotateSourceHelper.AnnotateBenchmarkFiles(summary, warnings);
			}

			return warnings;
		}

		public bool LastRun { get; set; }
		public int RerunCount { get; set; }
		public int RunCount { get; set; }

		public CompetitionTargets GetCompetitionTargets(Summary summary)
		{
			if (_competitionTargets.Count == 0)
			{
				CompetitionTargetHelpers.InitCompetitionTargets(_competitionTargets, summary);
			}

			return _competitionTargets;
		}

		public CompetitionTarget[] GetCompetitionTargetsToUpdate(Summary summary)
		{
			var competitionTargets = GetCompetitionTargets(summary);
			return CompetitionTargetHelpers.GetCompetitionTargetsToUpdate(summary, competitionTargets);
		}

		public void ValidateSummary(Summary summary, double defaultMinRatio, double defaultMaxRatio)
		{
			var competitionTargets = GetCompetitionTargets(summary);
			CompetitionTargetHelpers.ValidateSummary(summary, defaultMinRatio, defaultMaxRatio, competitionTargets);
		}
	}
}