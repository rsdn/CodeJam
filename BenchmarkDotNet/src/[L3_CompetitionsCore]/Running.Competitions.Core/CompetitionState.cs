using System;
using System.Collections.Generic;

using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running.Messages;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Running.Competitions.Core
{
	[PublicAPI]
	public class CompetitionState
	{
		private readonly List<IMessage> _messages = new List<IMessage>();

		#region State properties
		public bool LastRun => RunNumber >= MaxRunsAllowed;
		public bool LooksLikeLastRun => RunsLeft <= 0;

		public int MaxRunsAllowed { get; private set; }

		public int RunNumber { get; private set; }
		public int RunsLeft { get; private set; }

		public int MessagesInRun { get; private set; }
		public Summary LastRunSummary { get; private set; }
		#endregion

		#region State modification
		internal void FirstTimeInit(int maxRunsAllowed)
		{
			MaxRunsAllowed = maxRunsAllowed;

			RunsLeft = 1;
			RunNumber = 0;

			MessagesInRun = 0;
			LastRunSummary = null;
		}

		internal void PrepareForRun()
		{
			RunsLeft--;
			RunNumber++;

			MessagesInRun = 0;
			LastRunSummary = null;
		}

		internal void RunCompleted(Summary summary) => LastRunSummary = summary;

		public void RequestReruns(int additionalRunsCount, string explanationMessage)
		{
			if (additionalRunsCount < 0)
				throw new ArgumentOutOfRangeException(
					nameof(additionalRunsCount), additionalRunsCount, null);

			if (string.IsNullOrEmpty(explanationMessage))
				throw new ArgumentNullException(nameof(explanationMessage));

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
		public IMessage[] GetMessages() => _messages.ToArray();

		public void WriteMessage(
			MessageSource messageSource, MessageSeverity messageSeverity,
			string message)
		{
			MessagesInRun++;
			_messages.Add(
				new Message(
					RunNumber,
					MessagesInRun,
					messageSource,
					messageSeverity,
					message));
		}

		public void WriteMessage(
			MessageSource messageSource, MessageSeverity messageSeverity,
			string messageFormat, params object[] args) =>
				WriteMessage(
					messageSource, messageSeverity,
					string.Format(messageFormat, args));
		#endregion
	}
}