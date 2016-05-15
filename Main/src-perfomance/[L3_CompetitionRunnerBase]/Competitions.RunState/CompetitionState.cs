using System;
using System.Collections.Generic;

using BenchmarkDotNet.Reports;

namespace BenchmarkDotNet.Competitions.RunState
{
	public class CompetitionState
	{
		public static readonly StateSlot<CompetitionState> StateSlot = new StateSlot<CompetitionState>();

		private readonly List<IMessage> _messages = new List<IMessage>();
		public IMessage[] GetMessages() => _messages.ToArray();

		public void WriteMessage(MessageSource messageSource, MessageSeverity messageSeverity, string message) =>
			_messages.Add(new Message(messageSource, messageSeverity, message));

		public void WriteMessage(
			MessageSource messageSource, MessageSeverity messageSeverity, string messageFormat, params object[] args) =>
				WriteMessage(messageSource, messageSeverity, string.Format(messageFormat, args));

		public bool LastRun { get; private set; }
		public bool RerunRequested { get; private set; }
		public int RunCount { get; private set; }
		public Summary LastRunSummary { get; private set; }

		internal void InitOnRun(bool lastRun)
		{
			LastRun = lastRun;
			LastRunSummary = null;
			RerunRequested = false;
		}

		internal void RunSucceed(Summary summary)
		{
			LastRunSummary = summary;
			RunCount++;
		}

		public void RequestRerun(string explanationMessage)
		{
			WriteMessage(MessageSource.BenchmarkRunner, MessageSeverity.Informational, explanationMessage);
			RerunRequested = true;
		}
	}
}