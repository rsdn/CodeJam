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
		MessageSource MessageSource { get; }
		MessageSeverity MessageSeverity { get; }
		string MessageText { get; }
	}

	public class Message : IMessage
	{
		public Message(MessageSource messageSource, MessageSeverity messageSeverity, string messageText)
		{
			MessageSeverity = messageSeverity;
			MessageText = messageText;
			MessageSource = messageSource;
		}

		public MessageSource MessageSource { get; }
		public MessageSeverity MessageSeverity { get; }
		public string MessageText { get; }
	}
}