using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// The class holding the state of the competition.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeBraces_lock")]
	public class CompetitionState
	{
		private readonly List<IMessage> _messages = new List<IMessage>();
		private readonly List<Summary> _summaries = new List<Summary>();
		private readonly Stopwatch _stopwatch = new Stopwatch();

		/// <summary>Initializes a new instance of the <see cref="CompetitionState"/> class.</summary>
		public CompetitionState()
		{
			SummaryFromAllRuns = _summaries.AsReadOnly();
		}

		#region State properties
		/// <summary>The competition is in it's first run.</summary>
		/// <value><c>true</c> if the competition is in it's first run.</value>
		public bool InFirstRun => RunNumber == 1;

		/// <summary>The competition has no additional runs requested or the count of runs is out of limit.</summary>
		/// <value><c>true</c> if the competition has no additional runs requested or the count of runs is out of limit.</value>
		public bool LooksLikeLastRun => RunsLeft <= 0 || RunLimitExceeded;

		/// <summary>The count of runs is out of limit.</summary>
		/// <value><c>true</c> if count of runs is out of limit.</value>
		public bool RunLimitExceeded => RunNumber >= MaxRunsAllowed;

		/// <summary>There's a error-severity messages for the current run.</summary>
		/// <value><c>true</c> if there's a error-severity messages for the current run.</value>
		public bool HasTestErrorsInRun => HighestMessageSeverityInRun.IsTestErrorOrHigher();

		/// <summary>There's a critical-severity messages for the current run.</summary>
		/// <value><c>true</c> if there's a critical-severity messages for the current run.</value>
		public bool HasCriticalErrorsInRun => HighestMessageSeverityInRun.IsCriticalError();

		/// <summary>Time elapsed since start of the competition.</summary>
		/// <value>Time elapsed since start of the competition.</value>
		public TimeSpan Elapsed => _stopwatch.Elapsed;

		/// <summary>Max limit for competition reruns.</summary>
		/// <value>Max count of runs allowed.</value>
		public int MaxRunsAllowed { get; private set; }

		/// <summary>The logger for the competition.</summary>
		/// <value>The logger.</value>
		public ILogger Logger { get; private set; }

		/// <summary>The number of the current run.</summary>
		/// <value>The number of the current run.</value>
		public int RunNumber { get; private set; }

		/// <summary>Expected count of runs left.</summary>
		/// <value>Expected count of runs left.</value>
		public int RunsLeft { get; private set; }

		/// <summary>The highest message severity for the run.</summary>
		/// <value>The highest message severity for the run.</value>
		public MessageSeverity HighestMessageSeverityInRun { get; private set; }

		/// <summary>Count of messages for the current run.</summary>
		/// <value>Count of messages for the current run.</value>
		public int MessagesInRun { get; private set; }

		/// <summary>
		/// The summary for the last completed run.
		/// Is null if the current run is not completed.
		/// Can be null if the run was completed with errors.
		/// </summary>
		/// <value>The summary for the last completed run.</value>
		public Summary LastRunSummary { get; private set; }

		/// <summary>The competition was completed.</summary>
		/// <value><c>true</c> if the competition was completed.</value>
		public bool Completed { get; private set; }

		/// <summary>Summaries from all runs.</summary>
		/// <value>The list of summary from all runs.</value>
		public IReadOnlyList<Summary> SummaryFromAllRuns { get; }
		#endregion

		[AssertionMethod]
		private void AssertIsInCompetition() =>
			Code.AssertState(!Completed, "Could not update the state as the competition was completed.");

		#region State modification
		/// <summary>Init the competition state.</summary>
		/// <param name="maxRunsAllowed">Max limit for competition reruns.</param>
		/// <param name="logger">The logger for the competition.</param>
		internal void FirstTimeInit(int maxRunsAllowed, [NotNull] ILogger logger)
		{
			AssertIsInCompetition();

			Code.NotNull(logger, nameof(logger));
			_stopwatch.Restart();

			MaxRunsAllowed = maxRunsAllowed;
			Logger = logger;

			RunNumber = 0;
			RunsLeft = 1;

			HighestMessageSeverityInRun = MessageSeverity.Verbose;
			MessagesInRun = 0;
			LastRunSummary = null;
		}

		/// <summary>Prepare for next run.</summary>
		internal void PrepareForRun()
		{
			AssertIsInCompetition();

			RunNumber++;
			RunsLeft--;

			HighestMessageSeverityInRun = MessageSeverity.Verbose;
			MessagesInRun = 0;
			LastRunSummary = null;
		}

		/// <summary>Marks the run as completed.</summary>
		/// <param name="summary">Summary for the run.</param>
		internal void RunCompleted([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));
			AssertIsInCompetition();

			LastRunSummary = summary;
			_summaries.Add(summary);
		}

		/// <summary>Marks competition state as completed.</summary>
		internal void MarkAsCompleted()
		{
			AssertIsInCompetition();

			_stopwatch.Stop();
			Completed = true;
		}

		/// <summary>
		/// Requests additional runs for the competition.
		/// </summary>
		/// <param name="additionalRunsCount">Count of additional runs.</param>
		/// <param name="explanationMessage">The explanation message for therequest</param>
		public void RequestReruns(int additionalRunsCount, [NotNull] string explanationMessage)
		{
			AssertIsInCompetition();

			Code.InRange(additionalRunsCount, nameof(additionalRunsCount), 0, 1000);
			Code.NotNullNorEmpty(explanationMessage, nameof(explanationMessage));

			if (additionalRunsCount == 0)
			{
				WriteMessage(
					MessageSource.Runner,
					MessageSeverity.Informational,
					$"No reruns requested: {explanationMessage}");
			}
			else
			{
				WriteMessage(
					MessageSource.Runner,
					MessageSeverity.Informational,
					$"Requesting {additionalRunsCount} run(s): {explanationMessage}");

				RunsLeft = Math.Max(additionalRunsCount, RunsLeft);
			}
		}
		#endregion

		#region Messages
		/// <summary>Returns all messages for the competition.</summary>
		/// <returns>All messages for the competition</returns>
		public IMessage[] GetMessages()
		{
			lock (_messages)
			{
				return _messages.ToArray();
			}
		}

		/// <summary>Adds a message for the competition.</summary>
		/// <param name="messageSource">The source of the message.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="messageFormat">Message string format.</param>
		/// <param name="args">The arguments for message.</param>
		[StringFormatMethod("messageFormat")]
		public void WriteMessage(
			MessageSource messageSource, MessageSeverity messageSeverity,
			string messageFormat, params object[] args)
		{
			var message = args.IsNullOrEmpty()
				? messageFormat
				: string.Format(EnvironmentInfo.MainCultureInfo, messageFormat, args);

			WriteMessage(messageSource, messageSeverity, message);
		}

		/// <summary>Adds a message for the competition.</summary>
		/// <param name="messageSource">The source of the message.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="messageText">Text of the message.</param>
		public void WriteMessage(
			MessageSource messageSource, MessageSeverity messageSeverity,
			string messageText)
		{
			AssertIsInCompetition();

			Message message;

			lock (_messages)
			{
				message = new Message(
					RunNumber, MessagesInRun + 1,
					Elapsed,
					messageSource, messageSeverity, messageText);

				_messages.Add(message);
				MessagesInRun++;
				if (HighestMessageSeverityInRun < messageSeverity)
				{
					HighestMessageSeverityInRun = messageSeverity;
				}
			}

			Logger?.LogMessage(message);
		}
		#endregion
	}
}