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
		private readonly List<IMessage> _messages = new List<IMessage>();
		public IEnumerable<IMessage> GetMessages() => _messages.ToArray();

		public void WriteMessage(MessageSource messageSource, MessageSeverity messageSeverity, string message) => 
			_messages.Add(new Message(messageSource, messageSeverity, message));

		public void WriteMessage(MessageSource messageSource, MessageSeverity messageSeverity, string messageFormat, params object[] args) => 
			WriteMessage(messageSource, messageSeverity, string.Format(messageFormat, args));

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
		public bool RerunRequested { get; set; }
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

		public void ValidatePreconditions(Summary summary) => CompetitionTargetHelpers.ValidatePreconditions(summary);

		public void ValidateSummary(Summary summary, double defaultMinRatio, double defaultMaxRatio)
		{
			var competitionTargets = GetCompetitionTargets(summary);
			CompetitionTargetHelpers.ValidateSummary(summary, defaultMinRatio, defaultMaxRatio, competitionTargets);
		}

		public void ValidatePostconditions(Summary summary) => CompetitionTargetHelpers.ValidatePostconditions(summary);
	}
}