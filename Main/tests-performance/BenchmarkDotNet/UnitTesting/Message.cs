namespace BenchmarkDotNet.UnitTesting
{
	public enum MessageSeverity
	{
		ExecutionError,
		Error,
		Warning,
		Informational
	}
	public enum MessageSource
	{
		Other,
		Validator,
		Analyser,
		BenchmarkRunner
	}

	interface IMessage
	{
		MessageSource MessageSource { get; }
		MessageSeverity MessageSeverity { get; }
		string MessageText { get; }
	}
	public class Message: IMessage
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