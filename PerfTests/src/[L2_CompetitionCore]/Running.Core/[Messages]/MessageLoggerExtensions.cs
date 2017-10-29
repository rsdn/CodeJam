using System;

using BenchmarkDotNet.Running;

using JetBrains.Annotations;

using static CodeJam.PerfTests.Running.Core.MessageExtensions;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Extension methods for <see cref="IMessageLogger"/>
	/// </summary>
	public static class MessageLoggerExtensions
	{
		/// <summary>Writes message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">The explanation for the exception.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteMessage(
			[NotNull] this IMessageLogger messageLogger,
			MessageSeverity messageSeverity,
			[NotNull] Target target,
			[NotNull] string message,
			[CanBeNull] string hint = null)
		{
			Code.NotNullNorEmpty(message, nameof(message));

			messageLogger.WriteMessage(
				messageSeverity,
				FormatMessage(target, message), hint);
		}

		/// <summary>Writes exception message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="message">The explanation for the exception.</param>
		/// <param name="ex">The exception to write.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteExceptionMessage(
			[NotNull] this IMessageLogger messageLogger,
			MessageSeverity messageSeverity,
			[NotNull] string message,
			[NotNull] Exception ex,
			[CanBeNull] string hint = null)
		{
			Code.NotNullNorEmpty(message, nameof(message));
			Code.NotNull(ex, nameof(ex));

			messageLogger.WriteMessage(
				messageSeverity,
				FormatMessage(message, ex),
				FormatHintText(ex, hint));
		}

		/// <summary>Writes exception message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">The explanation for the exception.</param>
		/// <param name="ex">The exception to write.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteExceptionMessage(
			[NotNull] this IMessageLogger messageLogger,
			MessageSeverity messageSeverity,
			[NotNull] Target target,
			[NotNull] string message,
			[NotNull] Exception ex,
			[CanBeNull] string hint = null)
		{
			Code.NotNullNorEmpty(message, nameof(message));
			Code.NotNull(ex, nameof(ex));

			messageLogger.WriteMessage(
				messageSeverity,
				FormatMessage(target, message, ex),
				FormatHintText(ex, hint));
		}

		/// <summary>Adds test execution failure message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteExecutionErrorMessage(
			[NotNull] this IMessageLogger messageLogger,
			[NotNull] string message,
			[CanBeNull] string hint = null) =>
				messageLogger.WriteMessage(MessageSeverity.ExecutionError, message, hint);

		/// <summary>Adds test execution failure message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteExecutionErrorMessage(
			[NotNull] this IMessageLogger messageLogger,
			[NotNull] Target target,
			[NotNull] string message,
			[CanBeNull] string hint = null) =>
				messageLogger.WriteMessage(MessageSeverity.ExecutionError, target, message, hint);

		/// <summary>Adds test setup failure message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteSetupErrorMessage(
			[NotNull] this IMessageLogger messageLogger,
			[NotNull] string message,
			[CanBeNull] string hint = null) =>
				messageLogger.WriteMessage(MessageSeverity.SetupError, message, hint);

		/// <summary>Adds test setup failure message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteSetupErrorMessage(
			[NotNull] this IMessageLogger messageLogger,
			[NotNull] Target target,
			[NotNull] string message,
			[CanBeNull] string hint = null) =>
				messageLogger.WriteMessage(MessageSeverity.SetupError, target, message, hint);

		/// <summary>Adds test error message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteTestErrorMessage(
			[NotNull] this IMessageLogger messageLogger,
			[NotNull] string message,
			[CanBeNull] string hint = null) =>
				messageLogger.WriteMessage(MessageSeverity.TestError, message, hint);

		/// <summary>Adds test error message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteTestErrorMessage(
			[NotNull] this IMessageLogger messageLogger,
			[NotNull] Target target,
			[NotNull] string message,
			[CanBeNull] string hint = null) =>
				messageLogger.WriteMessage(MessageSeverity.TestError, target, message, hint);

		/// <summary>Adds warning message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteWarningMessage(
			[NotNull] this IMessageLogger messageLogger,
			[NotNull] string message,
			[CanBeNull] string hint = null) =>
				messageLogger.WriteMessage(MessageSeverity.Warning, message, hint);

		/// <summary>Adds warning message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteWarningMessage(
			[NotNull] this IMessageLogger messageLogger,
			[NotNull] Target target,
			[NotNull] string message,
			[CanBeNull] string hint = null) =>
				messageLogger.WriteMessage(MessageSeverity.Warning, target, message, hint);

		/// <summary>Adds an info message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteInfoMessage(
			[NotNull] this IMessageLogger messageLogger,
			[NotNull] string message,
			[CanBeNull] string hint = null) =>
				messageLogger.WriteMessage(MessageSeverity.Informational, message, hint);

		/// <summary>Adds an info message.</summary>
		/// <param name="messageLogger">The message logger.</param>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hints for the message.</param>
		public static void WriteInfoMessage(
			[NotNull] this IMessageLogger messageLogger,
			[NotNull] Target target,
			[NotNull] string message,
			[CanBeNull] string hint = null) =>
				messageLogger.WriteMessage(MessageSeverity.Informational, target, message, hint);
	}
}