using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Common interface for competition messages.</summary>
	public interface IMessage
	{
		/// <summary>Gets number of the run the message belongs to.</summary>
		/// <value>The number of the run the message belongs to.</value>
		int RunNumber { get; }

		/// <summary>Gets number of the message in the run.</summary>
		/// <value>The number of the message in the run.</value>
		int RunMessageNumber { get; }

		/// <summary>Gets time elapsed since start of the competition.</summary>
		/// <value>Time elapsed since start of the competition.</value>
		TimeSpan Elapsed { get; }

		/// <summary>Gets source of the message.</summary>
		/// <value>The source of the message.</value>
		MessageSource MessageSource { get; }

		/// <summary>Gets severity of the message.</summary>
		/// <value>The severity of the message.</value>
		MessageSeverity MessageSeverity { get; }

		/// <summary>Gets text of the message.</summary>
		/// <value>The text of the message.</value>
		[NotNull]
		string MessageText { get; }

		/// <summary>Gets text that describes possible solution of the problem or additional varbose info.</summary>
		/// <value>Hints for the message.</value>
		[CanBeNull]
		string HintText { get; }
	}
}