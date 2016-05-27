using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

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

		#region State properties
		/// <summary>The competition has no additional runs requested.</summary>
		/// <value><c>true</c> if the competition has no additional runs requested..</value>
		public bool LooksLikeLastRun => RunsLeft <= 0;

		/// <summary>The count of runs is out of limit.</summary>
		/// <value><c>true</c> if count of runs is out of limit.</value>
		public bool RunLimitExceeded => RunNumber >= MaxRunsAllowed;

		/// <summary>There's a critical-severity messages for the current run.</summary>
		/// <value><c>true</c> if there's a critical-severity messages for the current run.</value>
		public bool HasCriticalErrorsInRun => HighestMessageSeverityInRun.IsCriticalError();

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
		#endregion

		#region State modification
		/// <summary>Init the competition state.</summary>
		/// <param name="maxRunsAllowed">Max limit for competition reruns.</param>
		/// <param name="logger">The logger for the competition.</param>
		internal void FirstTimeInit(int maxRunsAllowed, [NotNull] ILogger logger)
		{
			Code.NotNull(logger, nameof(logger));

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
			RunNumber++;
			RunsLeft--;

			HighestMessageSeverityInRun = MessageSeverity.Verbose;
			MessagesInRun = 0;
			LastRunSummary = null;
		}

		/// <summary>Marks the run as completed.</summary>
		/// <param name="summary">The summary for the run.</param>
		internal void RunCompleted(Summary summary) => LastRunSummary = summary;

		/// <summary>
		/// Requests additional runs for the competition.
		/// </summary>
		/// <param name="additionalRunsCount">Count of additional runs.</param>
		/// <param name="explanationMessage">The explanation message for therequest</param>
		public void RequestReruns(int additionalRunsCount, [NotNull] string explanationMessage)
		{
			Code.InRange(additionalRunsCount, nameof(additionalRunsCount), 0, 1000);
			Code.NotNullNorEmpty(explanationMessage, nameof(explanationMessage));

			if (additionalRunsCount == 0)
			{
				WriteMessage(
					MessageSource.BenchmarkRunner,
					MessageSeverity.Informational,
					$"No reruns requested: {explanationMessage}");
			}
			else
			{
				WriteMessage(
					MessageSource.BenchmarkRunner,
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
			var message = args == null || args.Length == 0
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
			Message message;

			lock (_messages)
			{
				MessagesInRun++;
				if (HighestMessageSeverityInRun < messageSeverity)
				{
					HighestMessageSeverityInRun = messageSeverity;
				}

				message = new Message(
					RunNumber, MessagesInRun,
					messageSource, messageSeverity, messageText);

				_messages.Add(message);
			}

			Logger?.LogMessage(message);
		}
		#endregion
	}
}