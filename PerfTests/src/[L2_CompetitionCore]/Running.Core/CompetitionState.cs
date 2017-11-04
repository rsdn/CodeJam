using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// The class holding the state of the competition.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeBraces_lock")]
	public sealed class CompetitionState
	{
		private static readonly object _lockKey = new object();

		private readonly List<IMessage> _messages = new List<IMessage>();
		private readonly List<Summary> _summaries = new List<Summary>();

		private readonly Stopwatch _stopwatch = new Stopwatch();

		/// <summary>Initializes a new instance of the <see cref="CompetitionState"/> class.</summary>
		internal CompetitionState(
			[NotNull] Type benchmarkType,
			[NotNull] ICompetitionConfig competitionConfig)
		{
			Code.NotNull(benchmarkType, nameof(benchmarkType));
			Code.NotNull(competitionConfig, nameof(competitionConfig));

			SummaryFromAllRuns = _summaries.AsReadOnly();

			BenchmarkType = benchmarkType;
			Config = competitionConfig;
			Logger = competitionConfig.GetCompositeLogger();

			RunNumber = 0;
			RunsLeft = 1;

			HighestMessageSeverityInRun = MessageSeverity.Verbose;
			HighestMessageSeverity = MessageSeverity.Verbose;
			LastRunSummary = null;
		}

		/// <summary>Prepare for next run.</summary>
		internal void PrepareForRun()
		{
			lock (_lockKey)
			{
				AssertIsInCompetition();
				Code.AssertState(RunsLeft > 0, "No runs left.");

				RunNumber++;
				RunsLeft--;

				HighestMessageSeverityInRun = MessageSeverity.Verbose;
				MessagesInRun = 0;
				LastRunSummary = null;

				if (!_stopwatch.IsRunning)
					_stopwatch.Start();
			}
		}

		#region State properties

		#region Competition info properties
		/// <summary>The type of the benchmark.</summary>
		/// <value>The type of the benchmark.</value>
		[NotNull]
		public Type BenchmarkType { get; private set; }

		/// <summary>Config for the competition.</summary>
		/// <value>The config.</value>
		[NotNull]
		public ICompetitionConfig Config { get; private set; }

		/// <summary>Competition options.</summary>
		/// <value>Competition options.</value>
		[NotNull]
		public CompetitionOptions Options => Config.Options;

		/// <summary>The logger for the competition.</summary>
		/// <value>The logger.</value>
		[NotNull]
		internal ILogger Logger { get; private set; }

		/// <summary>
		/// The summary for the last completed run.
		/// Is null if the current run is not completed.
		/// Can be null if the run was completed with errors.
		/// </summary>
		/// <value>The summary for the last completed run or <c>null</c> if the run failed.</value>
		[CanBeNull]
		public Summary LastRunSummary { get; private set; }

		/// <summary>List of summaries from all runs.</summary>
		/// <value>The list of summaries from all runs.</value>
		[NotNull]
		public IReadOnlyList<Summary> SummaryFromAllRuns { get; }

		/// <summary>The competition was completed.</summary>
		/// <value><c>true</c> if the competition was completed.</value>
		public bool Completed { get; private set; }

		/// <summary>Time elapsed since start of the competition.</summary>
		/// <value>Time elapsed since start of the competition.</value>
		public TimeSpan Elapsed => _stopwatch.Elapsed;
		#endregion

		#region Run number properties
		/// <summary>The number of the current run.</summary>
		/// <value>The number of the current run.</value>
		public int RunNumber { get; private set; }

		/// <summary>Expected count of runs left.</summary>
		/// <value>Expected count of runs left.</value>
		public int RunsLeft { get; private set; }

		/// <summary>The competition is performing a first run..</summary>
		/// <value><c>true</c> if the competition is performing first run.</value>
		public bool IsFirstRun => RunNumber == 1;

		/// <summary>
		/// The competition has no additional runs requested
		/// or the count of runs is out of limit
		/// or there are critical errors in the run.
		/// </summary>
		/// <value>
		/// <c>true</c> if the run is about to finish.
		/// </value>
		public bool LooksLikeLastRun => RunsLeft <= 0 || RunLimitExceeded || HasCriticalErrorsInRun;

		/// <summary>The count of runs is out of limit.</summary>
		/// <value><c>true</c> if count of runs is out of limit.</value>
		public bool RunLimitExceeded => RunNumber >= Options.RunOptions.MaxRunsAllowed;
		#endregion

		#region Message-related properties
		/// <summary>Count of messages for the current run.</summary>
		/// <value>Count of messages for the current run.</value>
		internal int MessagesInRun { get; set; }

		/// <summary>The highest message severity for entire competition.</summary>
		/// <value>The highest message severity for entire competition.</value>
		public MessageSeverity HighestMessageSeverity { get; private set; }

		/// <summary>The highest message severity for the run.</summary>
		/// <value>The highest message severity for the run.</value>
		public MessageSeverity HighestMessageSeverityInRun { get; private set; }

		/// <summary>There's a critical-severity messages for the current run.</summary>
		/// <value><c>true</c> if there's a critical-severity messages for the current run.</value>
		public bool HasCriticalErrorsInRun => HighestMessageSeverityInRun.IsCriticalError();

		/// <summary>There's a error-severity messages for the current run.</summary>
		/// <value><c>true</c> if there's a error-severity messages for the current run.</value>
		public bool HasTestErrorsInRun => HighestMessageSeverityInRun.IsTestErrorOrHigher();

		/// <summary>The competition completed without warnings and errors.</summary>
		/// <value><c>true</c> if the competition completed without warnings and errors.</value>
		public bool CompletedWithoutWarnings => Completed && !HighestMessageSeverity.IsWarningOrHigher();

		/// <summary>The competition completed without errors.</summary>
		/// <value><c>true</c> if the competition completed without errors.</value>
		public bool CompletedWithoutErrors => Completed && !HighestMessageSeverity.IsTestErrorOrHigher();
		#endregion

		#endregion

		[AssertionMethod]
		private void AssertIsInCompetition() =>
			Code.AssertState(
				!Completed,
				"Could not change competition state as the competition is already completed.");

		#region State modification
		/// <summary>Request additional runs for the competition.</summary>
		/// <param name="additionalRunsCount">Count of additional runs.</param>
		/// <param name="explanationMessage">The explanation message for therequest</param>
		public void RequestReruns(int additionalRunsCount, [NotNull] string explanationMessage)
		{
			Code.InRange(additionalRunsCount, nameof(additionalRunsCount), 1, CompetitionCore.MaxRunLimit);
			Code.NotNullNorEmpty(explanationMessage, nameof(explanationMessage));

			lock (_lockKey)
			{
				AssertIsInCompetition();

				var maxRuns = Options.RunOptions.MaxRunsAllowed;
				var runsLeft = maxRuns - RunNumber;
				if (runsLeft == 0)
				{
					WriteMessageCore(
						MessageSource.Runner,
						MessageSeverity.Informational,
						$"{explanationMessage} No reruns requested as run limit ({maxRuns} run(s)) exceeded.");
				}
				else
				{
					var request = Math.Min(runsLeft, additionalRunsCount);
					WriteMessageCore(
						MessageSource.Runner,
						MessageSeverity.Informational,
						$"{explanationMessage} Requesting {request} run(s).");

					RunsLeft = Math.Max(request, RunsLeft);
				}
			}
		}

		/// <summary>Adds summary for the run.</summary>
		/// <param name="summary">Summary for the run.</param>
		internal void RunCompleted([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));

			lock (_lockKey)
			{
				AssertIsInCompetition();
				Code.BugIf(LastRunSummary != null, "Attempt to add last run summary twice.");

				LastRunSummary = summary;
				_summaries.Add(summary);
			}
		}

		/// <summary>Marks competition state as completed.</summary>
		internal void CompetitionCompleted()
		{
			lock (_lockKey)
			{
				AssertIsInCompetition();

				_stopwatch.Stop();
				Completed = true;
			}
		}
		#endregion

		#region Messages
		/// <summary>Returns all messages for the competition.</summary>
		/// <returns>All messages for the competition</returns>
		public IMessage[] GetMessages()
		{
			lock (_lockKey)
			{
				return _messages.ToArray();
			}
		}

		/// <summary>Adds a message for the competition.</summary>
		/// <param name="messageSource">Source of the message.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="message">Text of the message.</param>
		/// <param name="hint">Hints for the message.</param>
		internal void WriteMessageCore(
			MessageSource messageSource, MessageSeverity messageSeverity,
			[NotNull] string message,
			[CanBeNull] string hint = null)
		{
			Message result;

			lock (_lockKey)
			{
				AssertIsInCompetition();

				result = new Message(
					RunNumber, MessagesInRun + 1,
					Elapsed,
					messageSource, messageSeverity,
					message, hint);

				_messages.Add(result);
				MessagesInRun++;

				if (HighestMessageSeverityInRun < messageSeverity)
				{
					HighestMessageSeverityInRun = messageSeverity;
				}
				if (HighestMessageSeverity < messageSeverity)
				{
					HighestMessageSeverity = messageSeverity;
				}
			}

			Logger.LogMessage(result);
		}
		#endregion
	}
}