using System;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Running.Messages
{
	// DONTTOUCH: DO NOT change the order of the enum values.
	// It is used to compare the severities.
	// DO Check usages before  changing the enum values
	[PublicAPI]
	public enum MessageSeverity
	{
		Verbose,
		Informational,
		Warning,
		TestError,
		SetupError,
		ExecutionError
	}

	// TODO: define more sources.
	[PublicAPI]
	public enum MessageSource
	{
		Other,
		BenchmarkRunner,
		Validator,
		Analyser,
		Diagnoser
	}

	public interface IMessage
	{
		int RunNumber { get; }
		int RunMessageNumber { get; }
		MessageSource MessageSource { get; }
		MessageSeverity MessageSeverity { get; }
		string MessageText { get; }
		string ToString();
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

		public override string ToString()
			=> $"#{RunNumber}.{RunMessageNumber}, {MessageSeverity}@{MessageSource}: {MessageText}";
	}
}