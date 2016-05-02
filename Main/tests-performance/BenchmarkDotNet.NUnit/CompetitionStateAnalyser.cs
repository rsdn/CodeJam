using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.NUnit
{
	using CompetitionTargets = IDictionary<Target, CompetitionTarget>;

	/// <summary>
	/// Internal class to manage consequent runs.
	/// DO NOT add this one explicitly
	/// </summary>
	internal class CompetitionStateAnalyser : IAnalyser
	{
		private readonly CompetitionTargets _competitionTargets = new Dictionary<Target, CompetitionTarget>();

		public IEnumerable<IWarning> Analyse(Summary summary) => Enumerable.Empty<IWarning>();

		public bool LastRun { get; set; }
		public bool RerunRequested { get; set; }

		public CompetitionTargets GetCompetitionTargets(Summary summary)
		{
			if (_competitionTargets.Count == 0)
			{
				CompetitionTargetHelpers.InitCompetitionTargets(_competitionTargets, summary);
			}

			return _competitionTargets;
		}

		public CompetitionTarget[] GetNewCompetitionTargets(Summary summary)
		{
			var competitionTargets = GetCompetitionTargets(summary);
			return CompetitionTargetHelpers.GetNewCompetitionTargets(summary, competitionTargets);
		}

		public void ValidateSummary(Summary summary, double defaultMinRatio, double defaultMaxRatio)
		{
			var competitionTargets = GetCompetitionTargets(summary);
			CompetitionTargetHelpers.ValidateSummary(summary, defaultMinRatio, defaultMaxRatio, competitionTargets);
		}
	}
}