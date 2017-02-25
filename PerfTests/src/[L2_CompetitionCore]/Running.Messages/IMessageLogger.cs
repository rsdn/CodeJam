using BenchmarkDotNet.Loggers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Messages
{
	/// <summary>
	/// The message logger.
	/// </summary>
	public interface IMessageLogger
	{
		/// <summary>Gets logger (can be used for direct log output).</summary>
		/// <value>The logger.</value>
		ILogger Logger { get; }

	/// <summary>Adds a message for the competition.</summary>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="message">Text of the message.</param>
		/// <param name="hint">Hints for the message.</param>
		void WriteMessage(
			MessageSeverity messageSeverity,
			[NotNull] string message,
			[CanBeNull] string hint = null);
	}
}