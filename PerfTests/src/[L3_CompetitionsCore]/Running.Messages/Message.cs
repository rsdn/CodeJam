using System;

namespace CodeJam.PerfTests.Running.Messages
{
	/// <summary>
	/// Benchmark message
	/// </summary>
	/// <seealso cref="IMessage"/>
	public class Message : IMessage
	{
		/// <summary>Initializes a new instance of the <see cref="Message"/> class.</summary>
		/// <param name="runNumber">Number of the run the message belongs to.</param>
		/// <param name="runMessageNumber">Number of the message in the run.</param>
		/// <param name="messageSource">The source of the message.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="messageText">>Text of the message.</param>
		public Message(
			int runNumber,
			int runMessageNumber,
			MessageSource messageSource, MessageSeverity messageSeverity,
			string messageText)
		{
			Code.NotNullNorEmpty(messageText, nameof(messageText));

			RunNumber = runNumber;
			RunMessageNumber = runMessageNumber;
			MessageSeverity = messageSeverity;
			MessageText = messageText;
			MessageSource = messageSource;
		}

		/// <summary>Number of the run the message belongs to.</summary>
		/// <value>The number of the run the message belongs to.</value>
		public int RunNumber { get; }

		/// <summary>Number of the message in the run.</summary>
		/// <value>The number of the message in the run.</value>
		public int RunMessageNumber { get; }

		/// <summary>The source of the message.</summary>
		/// <value>The source of the message.</value>
		public MessageSource MessageSource { get; }

		/// <summary>Severity of the message.</summary>
		/// <value>The severity of the message.</value>
		public MessageSeverity MessageSeverity { get; }

		/// <summary>Text of the message.</summary>
		/// <value>The text of the message.</value>
		public string MessageText { get; }

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() =>
			$"#{RunNumber}.{RunMessageNumber}, {MessageSeverity}@{MessageSource}: {MessageText}";
	}
}