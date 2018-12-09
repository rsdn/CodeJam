using System;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

using CodeJam.Strings;

using JetBrains.Annotations;

using static BenchmarkDotNet.Loggers.FilteringLogger;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// Message-related extensions
	/// </summary>
	public static class MessageExtensions
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

		#region Format messages
		/// <summary>Formats the message.</summary>
		/// <param name="message">The message.</param>
		/// <param name="ex">The ex.</param>
		/// <returns>Formatted message.</returns>
		[NotNull]
		internal static string FormatMessage([NotNull] string message, [NotNull] Exception ex) =>
			$"{message} Exception: {ex.Message}";

		/// <summary>Formats the message.</summary>
		/// <param name="descriptor">The descriptor.</param>
		/// <param name="message">The message.</param>
		/// <returns>Formatted message.</returns>
		[NotNull]
		internal static string FormatMessage([NotNull] Descriptor descriptor, [NotNull] string message) =>
			$".Descriptor {descriptor.WorkloadMethodDisplayInfo}. {message}";

		/// <summary>Formats the message.</summary>
		/// <param name="descriptor">The descriptor.</param>
		/// <param name="message">The message.</param>
		/// <param name="ex">The ex.</param>
		/// <returns>Formatted message.</returns>
		[NotNull]
		internal static string FormatMessage([NotNull] Descriptor descriptor, [NotNull] string message, [NotNull] Exception ex) =>
			$".Descriptor {descriptor.WorkloadMethodDisplayInfo}. {message} Exception: {ex.Message}";

		/// <summary>Formats the hint text.</summary>
		/// <param name="ex">The ex.</param>
		/// <param name="hint">The hint.</param>
		/// <returns>Formatted hint text.</returns>
		[NotNull]
		internal static string FormatHintText([NotNull] Exception ex, [CanBeNull] string hint = null) =>
			hint == null
				? ex.ToDiagnosticString()
				: $"{hint}: {ex.ToDiagnosticString()}";
		#endregion

		#region Logger extensions
		/// <summary>Helper method to dump the content of the message into logger.</summary>
		/// <param name="logger">The logger the message will be dumped to.</param>
		/// <param name="message">The message to log.</param>
		internal static void LogMessage([NotNull] this ILogger logger, [NotNull] IMessage message)
		{
			Code.NotNull(logger, nameof(logger));
			Code.NotNull(message, nameof(message));

			var messageLogKind = message.MessageSeverity.IsCriticalError()
				? LogKind.Error
				: LogKind.Info;
			var prefix = message.MessageSeverity.IsWarningOrHigher()
				? ImportantInfoPrefix
				: InfoPrefix;

			logger.WriteLine(messageLogKind, $"{prefix} {message.ToLogString()}");
			if (message.HintText.NotNullNorEmpty())
			{
				logger.WriteLineInfo($"{prefix} Hint: {message.HintText}");
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