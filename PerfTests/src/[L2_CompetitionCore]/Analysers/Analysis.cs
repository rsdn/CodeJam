using System;
using System.Collections.Generic;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Helper class to store competition analysis results.</summary>
	[PublicAPI]
	public class Analysis : IMessageLogger
	{
		/// <summary>Initializes a new instance of the <see cref="Analysis"/> class.</summary>
		/// <param name="id">The identifier.</param>
		/// <param name="summary">The summary.</param>
		public Analysis([NotNull] string id, [NotNull] Summary summary)
			: this(id, summary, MessageSource.Analyser) { }

		/// <summary>Initializes a new instance of the <see cref="Analysis"/> class.</summary>
		/// <param name="id">The identifier.</param>
		/// <param name="summary">The summary.</param>
		/// <param name="messageSource">Source for the messages.</param>
		public Analysis([NotNull] string id, [NotNull] Summary summary, MessageSource messageSource)
		{
			Code.NotNullNorEmpty(id, nameof(id));
			Code.NotNull(summary, nameof(summary));
			DebugEnumCode.Defined(messageSource, nameof(messageSource));

			Id = id;
			Summary = summary;
			RunState = CompetitionCore.RunState[summary];
			MessageSource = messageSource;
		}

		#region Properties
		/// <summary>Gets the analysis identifier.</summary>
		/// <value>The analysis identifier.</value>
		[NotNull]
		public string Id { get; }

		/// <summary>Source for the messages.</summary>
		/// <value>Source for the messages.</value>
		public MessageSource MessageSource { get; }

		/// <summary>The state of the competition.</summary>
		/// <value>The state of the competition.</value>
		[NotNull]
		public CompetitionState RunState { get; }

		/// <summary>The summary.</summary>
		/// <value>The summary.</value>
		[NotNull]
		public Summary Summary { get; }

		/// <summary>Config for the competition.</summary>
		/// <value>The config.</value>
		public ICompetitionConfig Config => RunState.Config;

		/// <summary>Competition options.</summary>
		/// <value>Competition options.</value>
		public CompetitionOptions Options => RunState.Config.Options;

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

		#region Warnings
		/// <summary>Reports test error conclusion.</summary>
		/// <param name="message">Message text.</param>
		/// <param name="report">The report the message belongs to.</param>
		public void AddTestErrorConclusion(
			[NotNull] string message,
			BenchmarkReport report = null)
		{
			this.WriteTestErrorMessage(message);
			ConclusionsList.Add(Conclusion.CreateWarning(Id, message, report));
		}

		/// <summary>Reports test error conclusion.</summary>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">Message text.</param>
		/// <param name="report">The report the message belongs to.</param>
		public void AddTestErrorConclusion(
			[NotNull] Target target,
			[NotNull] string message,
			BenchmarkReport report = null)
		{
			this.WriteTestErrorMessage(target, message);
			ConclusionsList.Add(
				Conclusion.CreateWarning(
					Id, $"Target {target.MethodDisplayInfo}. {message}",
					report));
		}

		/// <summary>Reports analyser warning conclusion.</summary>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hint how to fix the warning.</param>
		/// <param name="report">The report the message belongs to.</param>
		public void AddWarningConclusion(
			[NotNull] string message,
			[NotNull] string hint,
			BenchmarkReport report = null)
		{
			this.WriteWarningMessage(message, hint);
			ConclusionsList.Add(Conclusion.CreateWarning(Id, message, report));
		}

		/// <summary>Reports analyser warning conclusion.</summary>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hint how to fix the warning.</param>
		/// <param name="report">The report the message belongs to.</param>
		public void AddWarningConclusion(
			[NotNull] Target target,
			[NotNull] string message,
			[NotNull] string hint,
			BenchmarkReport report = null)
		{
			this.WriteWarningMessage(target, message, hint);
			ConclusionsList.Add(
				Conclusion.CreateWarning(
					Id, $"Target {target.MethodDisplayInfo}. {message}",
					report));
		}
		#endregion

		#region IMessageLogger implementation
		/// <summary>Gets logger (can be used for direct log output).</summary>
		/// <value>The logger.</value>
		public ILogger Logger => RunState.Logger;

		/// <summary>Adds a message for the competition.</summary>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="message">Text of the message.</param>
		/// <param name="hint">Hints for the message.</param>
		public void WriteMessage(MessageSeverity messageSeverity, string message, string hint = null) => RunState.WriteMessage(MessageSource, messageSeverity, message, hint);
		#endregion
	}
}