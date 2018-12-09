using System;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Configs;

using JetBrains.Annotations;

using static BenchmarkDotNet.Loggers.FilteringLogger;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Loggers
{
	/// <summary>
	/// Logger helpers.
	/// </summary>
	public static class LoggerHelpers
	{
		private const int SeparatorLength = 42;

		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		public static void WriteSeparatorLine([NotNull] this ILogger logger) =>
			WriteSeparatorLine(logger, null, false);

		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="header">The separator line text.</param>
		public static void WriteSeparatorLine([NotNull] this ILogger logger, [CanBeNull] string header) =>
			WriteSeparatorLine(logger, header, false);

		/// <summary>Helper method that writes separator log line.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="header">The separator line text.</param>
		/// <param name="topHeader">Write top-level header.</param>
		public static void WriteSeparatorLine(
			[NotNull] this ILogger logger, [CanBeNull] string header, bool topHeader)
		{
			var separatorChar = topHeader ? '=' : '-';
			var logKind = topHeader ? LogKind.Header : LogKind.Help;

			var result = new StringBuilder(SeparatorLength);

			if (!string.IsNullOrEmpty(header))
			{
				var prefixLength = (SeparatorLength - header.Length - 2) / 2;
				if (prefixLength <= 0) prefixLength = 1;
				result.Append(separatorChar, prefixLength).Append(' ').Append(header).Append(' ');
			}

			var suffixLength = SeparatorLength - result.Length;
			if (suffixLength <= 0) suffixLength = 1;
			result.Append(separatorChar, suffixLength);

			logger.WriteLine();
			logger.WriteLine(logKind, result.ToString());
		}

		/// <summary>Writes additional info to the log. Will be written even if <see cref="FilteringLoggerMode"/> filter applied.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="message">Text of the message.</param>
		public static void WriteHintLine(
			[NotNull] this ILogger logger,
			[NotNull] string message) =>
				logger.WriteLineInfo($"{ImportantInfoPrefix} {message}");

		/// <summary>Writes help hint to the log. Will be written even if <see cref="FilteringLoggerMode"/> filter applied.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="message">Text of the message.</param>
		public static void WriteHelpHintLine(
			[NotNull] this ILogger logger,
			[NotNull] string message) =>
				logger.WriteLineHelp($"{InfoPrefix} {message}");

		/// <summary>Writes verbose information to the log.</summary>
		/// <param name="logger">The logger.</param>
		/// <param name="message">Text of the message.</param>
		public static void WriteVerboseLine(
			[NotNull] this ILogger logger,
			[NotNull] string message) =>
				logger.WriteLineInfo($"{VerbosePrefix} {message}");

		/// <summary>Flushes the loggers.</summary>
		/// <param name="config">Config with loggers.</param>
		public static void FlushLoggers(IConfig config)
		{
			foreach (var logger in config.GetLoggers())
			{
				logger.Flush();
			}
		}

		/// <summary>All messages within the scope will be passed to the log.</summary>
		/// <param name="config">Config with loggers.</param>
		/// <returns>Disposable to mark the scope completion.</returns>
		public static IDisposable BeginImportantLogScope(IConfig config)
		{
			var loggers = config.GetLoggers().OfType<FilteringLogger>().ToArray();
			return FilteringLogger.BeginImportantLogScope(loggers);
		}
	}
}