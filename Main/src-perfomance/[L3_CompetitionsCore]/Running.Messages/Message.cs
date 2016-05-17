using System;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Running.Messages
{
	// DONTTOUCH: DO NOT change the order of the enum values.
	// It is used to compare the severities.
	public enum MessageSeverity
	{
		[UsedImplicitly]
		Verbose,
		Informational,
		Warning,
		TestError,
		SetupError,
		ExecutionError
	}

	// TODO: define more sources.
	public enum MessageSource
	{
		[UsedImplicitly]
		Other,
		Validator,
		BenchmarkRunner,
		Analyser
	}

	public interface IMessage
	{
		int RunNumber { get; }
		int RunMessageNumber { get; }
		MessageSource MessageSource { get; }
		MessageSeverity MessageSeverity { get; }
		string MessageText { get; }
	}

	public class Message : IMessage
	{
		public Message(
			int runNumber,
			int runMessageNumber,
			MessageSource messageSource, MessageSeverity messageSeverity, string messageText)
		{
			RunNumber = runNumber;
			RunMessageNumber = runMessageNumber;
			MessageSeverity = messageSeverity;
			MessageText = messageText;
			MessageSource = messageSource;
		}

		public int RunNumber { get; }
		public int RunMessageNumber { get; }
		public MessageSource MessageSource { get; }
		public MessageSeverity MessageSeverity { get; }
		public string MessageText { get; }
	}
}