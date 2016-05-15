using System;

namespace BenchmarkDotNet.Competitions.RunState
{
	// DONTTOUCH: order is important
	public enum MessageSeverity
	{
		Informational,
		Warning,
		TestError,
		SetupError,
		ExecutionError
	}

	public enum MessageSource
	{
		Other,
		Validator,
		Analyser,
		BenchmarkRunner
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