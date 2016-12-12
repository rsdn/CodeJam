using System;
using System.Collections.Generic;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Helper class to store analyser pass results.</summary>
	[PublicAPI]
	public class Analysis
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Analysis"/> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="summary">The summary.</param>
		public Analysis([NotNull] string id, [NotNull] Summary summary)
		{
			Code.NotNullNorEmpty(id, nameof(id));
			Code.NotNull(summary, nameof(summary));

			Id = id;
			Summary = summary;
			RunState = CompetitionCore.RunState[summary];
		}

		#region Properties
		/// <summary>Gets the analysis identifier.</summary>
		/// <value>The analysis identifier.</value>
		[NotNull]
		public string Id { get; }

		/// <summary>The state of the competition.</summary>
		/// <value>The state of the competition.</value>
		[NotNull]
		public CompetitionState RunState { get; }

		/// <summary>The summary.</summary>
		/// <value>The summary.</value>
		[NotNull]
		public Summary Summary { get; }

		/// <summary>Analysis conclusions.</summary>
		/// <value>Analysis conclusions.</value>
		[NotNull]
		public IReadOnlyCollection<Conclusion> Conclusions => ConclusionsList;

		/// <summary>The conclusions list.</summary>
		/// <value>The conclusions list.</value>
		[NotNull]
		protected List<Conclusion> ConclusionsList { get; } = new List<Conclusion>();

		/// <summary>Analysis has no execution or setup errors so far and can be safely performed.</summary>
		/// <value><c>true</c> if analysis has no errors; otherwise, <c>false</c>.</value>
		public bool SafeToContinue => !RunState.HasCriticalErrorsInRun;
		#endregion

		#region Messages
		/// <summary>Adds test execution failure message.</summary>
		/// <param name="message">Message text.</param>
		public void WriteExecutonErrorMessage([NotNull] string message) =>
			RunState.WriteMessage(MessageSource.Analyser, MessageSeverity.ExecutionError, message);

		/// <summary>Adds test setup failure message.</summary>
		/// <param name="message">Message text.</param>
		public void WriteSetupErrorMessage([NotNull] string message) =>
			RunState.WriteMessage(MessageSource.Analyser, MessageSeverity.SetupError, message);

		/// <summary>Adds test error message.</summary>
		/// <param name="message">Message text.</param>
		public void WriteTestErrorMessage([NotNull] string message) =>
			RunState.WriteMessage(MessageSource.Analyser, MessageSeverity.TestError, message);

		/// <summary>Adds warning message.</summary>
		/// <param name="message">Message text.</param>
		public void WriteWarningMessage([NotNull] string message) =>
			RunState.WriteMessage(MessageSource.Analyser, MessageSeverity.Warning, message);

		/// <summary>Adds an info message.</summary>
		/// <param name="message">Message text.</param>
		public void WriteInfoMessage([NotNull] string message) =>
			RunState.WriteMessage(MessageSource.Analyser, MessageSeverity.Informational, message);
		#endregion

		#region Warnings
		/// <summary>Reports test error conclusion.</summary>
		/// <param name="message">Message text.</param>
		/// <param name="report">The report the message belongs to.</param>
		public void AddTestErrorConclusion([NotNull] string message, BenchmarkReport report = null)
		{
			WriteTestErrorMessage(message);
			ConclusionsList.Add(Conclusion.CreateWarning(Id, message, report));
		}

		/// <summary>Reports analyser warning conclusion.</summary>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hint how to fix the warning.</param>
		/// <param name="report">The report the message belongs to.</param>
		public void AddWarningConclusion([NotNull] string message, [NotNull] string hint, BenchmarkReport report = null)
		{
			WriteWarningMessage(message);
			RunState.WriteVerboseHint(hint);
			ConclusionsList.Add(Conclusion.CreateWarning(Id, message, report));
		}
		#endregion
	}
}