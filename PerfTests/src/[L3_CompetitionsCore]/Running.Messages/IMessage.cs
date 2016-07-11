using System;

namespace CodeJam.PerfTests.Running.Messages
{
	/// <summary>Common interface for competition messages.</summary>
	public interface IMessage
	{
		/// <summary>Number of the run the message belongs to.</summary>
		/// <value>The number of the run the message belongs to.</value>
		int RunNumber { get; }

		/// <summary>Number of the message in the run.</summary>
		/// <value>The number of the message in the run.</value>
		int RunMessageNumber { get; }

		/// <summary>Time elapsed since start of the competition.</summary>
		/// <value>Time elapsed since start of the competition.</value>
		TimeSpan Elapsed { get; }

		/// <summary>The source of the message.</summary>
		/// <value>The source of the message.</value>
		MessageSource MessageSource { get; }

		/// <summary>Severity of the message.</summary>
		/// <value>The severity of the message.</value>
		MessageSeverity MessageSeverity { get; }

		/// <summary>Text of the message.</summary>
		/// <value>The text of the message.</value>
		string MessageText { get; }
	}
}