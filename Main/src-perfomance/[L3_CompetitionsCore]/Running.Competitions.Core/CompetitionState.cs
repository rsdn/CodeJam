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

		#region Messages
		public IMessage[] GetMessages() => _messages.ToArray();

		public void WriteMessage(MessageSource messageSource, MessageSeverity messageSeverity, string message) =>
			_messages.Add(new Message(messageSource, messageSeverity, message));

		public void WriteMessage(
			MessageSource messageSource, MessageSeverity messageSeverity, string messageFormat, params object[] args) =>
				WriteMessage(messageSource, messageSeverity, string.Format(messageFormat, args));
		#endregion

		#region State properties
		public bool LastRun { get; private set; }
		public bool RerunRequested { get; private set; }
		public int RunCount { get; private set; }
		public Summary LastRunSummary { get; private set; }
		#endregion

		#region State modification
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
		#endregion
	}
}