using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Competition message.</summary>
	/// <seealso cref="IMessage"/>
	public class Message : IMessage
	{
		/// <summary>Initializes a new instance of the <see cref="Message" /> class.</summary>
		/// <param name="runNumber">Number of the run the message belongs to.</param>
		/// <param name="runMessageNumber">Number of the message in the run.</param>
		/// <param name="elapsed">Time elapsed from the start of the benchmark.</param>
		/// <param name="messageSource">Source of the message.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="messageText">Text of the message.</param>
		/// <param name="hintText">Hints for the message.</param>
		public Message(
			int runNumber,
			int runMessageNumber,
			TimeSpan elapsed,
			MessageSource messageSource, MessageSeverity messageSeverity,
			[NotNull] string messageText,
			[CanBeNull] string hintText)
		{
			DebugCode.ValidCount(runNumber, nameof(runNumber));
			DebugCode.ValidCount(runMessageNumber, nameof(runMessageNumber));
			DebugCode.AssertArgument(elapsed > TimeSpan.Zero, nameof(messageSeverity), "Elapsed time should be positive.");
			DebugEnumCode.Defined(messageSource, nameof(messageSource));
			DebugEnumCode.Defined(messageSeverity, nameof(messageSeverity));
			Code.NotNullNorEmpty(messageText, nameof(messageText));

			RunNumber = runNumber;
			RunMessageNumber = runMessageNumber;
			Elapsed = elapsed;
			MessageSource = messageSource;
			MessageSeverity = messageSeverity;
			MessageText = messageText;
			HintText = hintText;
		}

		/// <summary>Gets number of the run the message belongs to.</summary>
		/// <value>The number of the run the message belongs to.</value>
		public int RunNumber { get; }

		/// <summary>Gets number of the message in the run.</summary>
		/// <value>The number of the message in the run.</value>
		public int RunMessageNumber { get; }

		/// <summary>Gets time elapsed since start of the competition.</summary>
		/// <value>Time elapsed since start of the competition.</value>
		public TimeSpan Elapsed { get; }

		/// <summary>Gets source of the message.</summary>
		/// <value>The source of the message.</value>
		public MessageSource MessageSource { get; }

		/// <summary>Gets severity of the message.</summary>
		/// <value>The severity of the message.</value>
		public MessageSeverity MessageSeverity { get; }

		/// <summary>Gets text of the message.</summary>
		/// <value>The text of the message.</value>
		public string MessageText { get; }

		/// <summary>Gets text that describes possible solution of the problem or additional varbose info.</summary>
		/// <value>Hints for the message.</value>
		public string HintText { get; }
	}
}