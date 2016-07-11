using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Messages
{
	/// <summary>Competition message.</summary>
	/// <seealso cref="IMessage"/>
	public class Message : IMessage
	{
		/// <summary>Initializes a new instance of the <see cref="Message"/> class.</summary>
		/// <param name="runNumber">Number of the run the message belongs to.</param>
		/// <param name="runMessageNumber">Number of the message in the run.</param>
		/// <param name="elapsed">Time elapsed from the start of the benchmark.</param>
		/// <param name="messageSource">The source of the message.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="messageText">>Text of the message.</param>
		public Message(
			int runNumber,
			int runMessageNumber,
			TimeSpan elapsed,
			MessageSource messageSource, MessageSeverity messageSeverity,
			[NotNull] string messageText)
		{
			Code.NotNullNorEmpty(messageText, nameof(messageText));

			RunNumber = runNumber;
			RunMessageNumber = runMessageNumber;
			Elapsed = elapsed;
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

		/// <summary>Time elapsed since start of the competition.</summary>
		/// <value>Time elapsed since start of the competition.</value>
		public TimeSpan Elapsed { get; }

		/// <summary>The source of the message.</summary>
		/// <value>The source of the message.</value>
		public MessageSource MessageSource { get; }

		/// <summary>Severity of the message.</summary>
		/// <value>The severity of the message.</value>
		public MessageSeverity MessageSeverity { get; }

		/// <summary>Text of the message.</summary>
		/// <value>The text of the message.</value>
		public string MessageText { get; }
	}
}