using System;

namespace CodeJam.PerfTests.Running.Messages
{
	/// <summary>
	/// Common interface for benchmark messages
	/// </summary>
	public interface IMessage
	{
		/// <summary>Number of the run the massage belongs to.</summary>
		/// <value>The number of the run the massage belongs to.</value>
		int RunNumber { get; }

		/// <summary>Number of the message in the run.</summary>
		/// <value>The number of the message in the run.</value>
		int RunMessageNumber { get; }

		/// <summary>The source of the message.</summary>
		/// <value>The source of the message.</value>
		MessageSource MessageSource { get; }

		/// <summary>Severity of the message.</summary>
		/// <value>The severity of the message.</value>
		MessageSeverity MessageSeverity { get; }

		/// <summary>Text of the message.</summary>
		/// <value>The text of the message.</value>
		string MessageText { get; }

		/// <summary>Returns a <see cref="string" /> that represents this instance.</summary>
		/// <returns>A <see cref="string" /> that represents this instance.</returns>
		string ToString();
	}
}