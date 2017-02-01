using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Messages;

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
		private readonly List<IMessage> _messages = new List<IMessage>();
		private readonly List<Summary> _summaries = new List<Summary>();
		private readonly Stopwatch _stopwatch = new Stopwatch();

		/// <summary>Initializes a new instance of the <see cref="CompetitionState"/> class.</summary>
		public CompetitionState()
		{
			SummaryFromAllRuns = _summaries.AsReadOnly();
		}

		#region State properties
		/// <summary>The competition is performing first run..</summary>
		/// <value><c>true</c> if the competition is performing first run.</value>
		public bool FirstRun => RunNumber == 1;

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

		/// <summary>There's a critical-severity messages for the current run.</summary>
		/// <value><c>true</c> if there's a critical-severity messages for the current run.</value>
		public bool HasCriticalErrorsInRun => HighestMessageSeverityInRun.IsCriticalError();

		/// <summary>There's a error-severity messages for the current run.</summary>
		/// <value><c>true</c> if there's a error-severity messages for the current run.</value>
		public bool HasTestErrorsInRun => HighestMessageSeverityInRun.IsTestErrorOrHigher();

		/// <summary>The competition completed without warnings and errors.</summary>
		/// <value><c>true</c> if the competition completed without warnings and errors.</value>
		public bool CompletedSuccessfully => Completed && !HighestMessageSeverity.IsWarningOrHigher();

		/// <summary>Time elapsed since start of the competition.</summary>
		/// <value>Time elapsed since start of the competition.</value>
		public TimeSpan Elapsed => _stopwatch.Elapsed;

		/// <summary>The config for the competition.</summary>
		/// <value>The config.</value>
		public ICompetitionConfig Config { get; private set; }

		/// <summary>The type of the benchmark.</summary>
		/// <value>The type of the benchmark.</value>
		public Type BenchmarkType { get; private set; }

		/// <summary>Competition options.</summary>
		/// <value>Competition options.</value>
		public CompetitionOptions Options => Config.Options;

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

		/// <summary>The highest message severity for entire competition.</summary>
		/// <value>The highest message severity for entire competition.</value>
		public MessageSeverity HighestMessageSeverity { get; private set; }

		/// <summary>Count of messages for the current run.</summary>
		/// <value>Count of messages for the current run.</value>
		public int MessagesInRun { get; private set; }

		/// <summary>
		/// The summary for the last completed run.
		/// Is null if the current run is not completed.
		/// Can be null if the run was completed with errors.
		/// </summary>
		/// <value>The summary for the last completed run.</value>
		[CanBeNull]
		public Summary LastRunSummary { get; private set; }

		/// <summary>List of summaries from all runs.</summary>
		/// <value>The list of summaries from all runs.</value>
		public IReadOnlyList<Summary> SummaryFromAllRuns { get; }

		/// <summary>The competition was completed.</summary>
		/// <value><c>true</c> if the competition was completed.</value>
		public bool Completed { get; private set; }
		#endregion

		[AssertionMethod]
		private void AsserIsInInit() =>
			Code.AssertState(
				BenchmarkType == null && !Completed,
				"Could not init state as the competition is in run or was completed.");

		[AssertionMethod]
		private void AssertIsInCompetition() =>
			Code.AssertState(
				BenchmarkType != null && !Completed,
				"Could not update state as the competition is not running or was completed.");

		#region State modification
		/// <summary>Init the competition state.</summary>
		/// <param name="benchmarkType">Type of the benchmark.</param>
		/// <param name="competitionConfig">The config for the competition.</param>
		internal void FirstTimeInit(
			[NotNull] Type benchmarkType,
			[NotNull] ICompetitionConfig competitionConfig)
		{
			Code.NotNull(benchmarkType, nameof(benchmarkType));
			Code.NotNull(competitionConfig, nameof(competitionConfig));

			lock (_messages)
			{
				AsserIsInInit();

				_stopwatch.Restart();

				BenchmarkType = benchmarkType;
				Config = competitionConfig;
				Logger = competitionConfig.GetCompositeLogger();

				RunNumber = 0;
				RunsLeft = 1;

				HighestMessageSeverityInRun = MessageSeverity.Verbose;
				HighestMessageSeverity = MessageSeverity.Verbose;
				MessagesInRun = 0;
				LastRunSummary = null;

				Completed = false;
				_summaries.Clear();
				_messages.Clear();
			}
		}

		/// <summary>Prepare for next run.</summary>
		internal void PrepareForRun()
		{
			lock (_messages)
			{
				AssertIsInCompetition();
				Code.AssertState(RunsLeft > 0, "No runs left.");

				RunNumber++;
				RunsLeft--;

				HighestMessageSeverityInRun = MessageSeverity.Verbose;
				MessagesInRun = 0;
				LastRunSummary = null;
			}
		}

		/// <summary>Mark the run as completed.</summary>
		/// <param name="summary">Summary for the run.</param>
		internal void RunCompleted([NotNull] Summary summary)
		{
			Code.NotNull(summary, nameof(summary));

			lock (_messages)
			{
				AssertIsInCompetition();

				LastRunSummary = summary;
				_summaries.Add(summary);
			}
		}

		/// <summary>Marks competition state as completed.</summary>
		internal void CompetitionCompleted()
		{
			lock (_messages)
			{
				AssertIsInCompetition();

				_stopwatch.Stop();
				Completed = true;
			}
		}

		/// <summary>Request additional runs for the competition.</summary>
		/// <param name="additionalRunsCount">Count of additional runs.</param>
		/// <param name="explanationMessage">The explanation message for therequest</param>
		public void RequestReruns(int additionalRunsCount, [NotNull] string explanationMessage)
		{
			Code.InRange(additionalRunsCount, nameof(additionalRunsCount), 0, CompetitionRunMode.MaxRunLimit);
			Code.NotNullNorEmpty(explanationMessage, nameof(explanationMessage));

			lock (_messages)
			{
				AssertIsInCompetition();

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
		/// <param name="messageSource">Source of the message.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="messageText">Text of the message.</param>
		public void WriteMessage(
			MessageSource messageSource, MessageSeverity messageSeverity,
			[NotNull] string messageText)
		{
			Message message;

			lock (_messages)
			{
				AssertIsInCompetition();

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
				if (HighestMessageSeverity < messageSeverity)
				{
					HighestMessageSeverity = messageSeverity;
				}
			}

			if (Logger == null)
			{
				throw CodeExceptions.InvalidOperation(
					$"Please call {nameof(FirstTimeInit)}() at first.");
			}
			Logger.LogMessage(message);
		}
		#endregion
	}
}