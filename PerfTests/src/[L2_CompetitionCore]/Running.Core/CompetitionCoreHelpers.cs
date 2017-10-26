using System;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Loggers;

using CodeJam.PerfTests.Running.Messages;
using CodeJam.Strings;

using JetBrains.Annotations;

using static BenchmarkDotNet.Loggers.FilteringLogger;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Helpers to use during competition run
	/// </summary>
	public static class CompetitionCoreHelpers
	{
		#region Message severity
		/// <summary>The message severity is setup error or higher.</summary>
		/// <param name="severity">The severity to check.</param>
		/// <returns><c>true</c> if the severity is setup error or higher.</returns>
		public static bool IsCriticalError(this MessageSeverity severity) => severity >= MessageSeverity.SetupError;

		/// <summary>The message severity is test error or higher.</summary>
		/// <param name="severity">The severity to check.</param>
		/// <returns><c>true</c> if the severity is test error or higher.</returns>
		public static bool IsTestErrorOrHigher(this MessageSeverity severity) => severity >= MessageSeverity.TestError;

		/// <summary>The message severity is warning or higher.</summary>
		/// <param name="severity">The severity to check.</param>
		/// <returns><c>true</c> if the severity is warning or higher.</returns>
		public static bool IsWarningOrHigher(this MessageSeverity severity) => severity >= MessageSeverity.Warning;
		#endregion

		#region Messages
		/// <summary>Writes exception message.</summary>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="messageSource">Source of the message.</param>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="message">The explanation for the exception.</param>
		/// <param name="ex">The exception to write.</param>
		public static void WriteExceptionMessage(
			[NotNull] this CompetitionState competitionState,
			MessageSource messageSource, MessageSeverity messageSeverity,
			[NotNull] string message,
			[NotNull] Exception ex)
		{
			Code.NotNullNorEmpty(message, nameof(message));
			Code.NotNull(ex, nameof(ex));

			competitionState.WriteMessage(
				messageSource, messageSeverity,
				$"{message} Exception: {ex.Message}",
				ex.ToDiagnosticString());
		}


		/// <summary>Writes the verbose hint message. Logged, but not reported to user.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="message">Text of the message.</param>
		public static void WriteVerboseHint(
			[NotNull] this ILogger logger,
			[NotNull] string message) =>
				logger.WriteLineInfo($"{LogImportantInfoPrefix} {message}");

		/// <summary>Writes the verbose message.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="message">Text of the message.</param>
		public static void WriteVerbose(
			[NotNull] this ILogger logger,
			[NotNull] string message) =>
				logger.WriteLineInfo($"{LogVerbosePrefix} {message}");

		/// <summary>Helper method to dump the content of the message into logger.</summary>
		/// <param name="logger">The logger the message will be dumped to.</param>
		/// <param name="message">The message to log.</param>
		internal static void LogMessage([NotNull] this ILogger logger, [NotNull] IMessage message)
		{
			Code.NotNull(logger, nameof(logger));
			Code.NotNull(message, nameof(message));

			if (message.MessageSeverity.IsCriticalError())
			{
				logger.WriteLineError($"{LogImportantInfoPrefix} {message.ToLogString()}");
			}
			else if (message.MessageSeverity.IsWarningOrHigher())
			{
				logger.WriteLineInfo($"{LogImportantInfoPrefix} {message.ToLogString()}");
			}
			else
			{
				logger.WriteLineInfo($"{LogInfoPrefix} {message.ToLogString()}");
			}
			if (message.HintText.NotNullNorEmpty())
			{
				logger.WriteLineInfo($"{LogImportantInfoPrefix} Hint: {message.HintText}");
			}
		}

		private static string ToLogString(this IMessage message) => string.Format(
			HostEnvironmentInfo.MainCultureInfo,
			"#{0}.{1,-2} {2:00.000}s, {3,-23} {4}",
			message.RunNumber,
			message.RunMessageNumber,
			message.Elapsed.TotalSeconds,
			message.MessageSeverity + "@" + message.MessageSource + ":",
			message.MessageText);
		#endregion

	}
}