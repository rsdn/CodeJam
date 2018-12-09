using CodeJam;
using CodeJam.Threading;
using JetBrains.Annotations;
using System;
using System.Threading;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Loggers
{
	/// <summary>Basic logger implementation that supports message filtering.</summary>
	/// <seealso cref="ILogger"/>
	[PublicAPI]
	public class FilteringLogger : ILogger
	{
		// DONTTOUCH: Check that all code does not hardcode content of the constants before changing

		#region Log line prefixes (constants)
		/// <summary>
		/// The prefix for verbose log lines.
		/// Lines with this prefix will be written only if <see cref="FilteringLoggerMode.AllMessages"/> mode set.
		/// </summary>
		public const string VerbosePrefix = "//  ";

		/// <summary>
		/// The prefix for informational log lines.
		/// Lines with this prefix will be written even if <see cref="FilteringLoggerMode"/> filter applied.
		/// </summary>
		public const string InfoPrefix = "// ?";

		/// <summary>
		/// The prefix for important log lines.
		/// Lines with this prefix will be written even if <see cref="FilteringLoggerMode"/> filter applied.
		/// </summary>
		public const string ImportantInfoPrefix = "// !";

		/// <summary>
		/// The start prefix for important log scope.
		/// Lines between start and end prefixes will be written even if <see cref="FilteringLoggerMode"/> filter applied.
		/// </summary>
		public const string ImportantLogScopeStartPrefix = "// !<--";

		/// <summary>
		/// The end prefix for important log scope.
		/// Lines between start and end prefixes will be written even if <see cref="FilteringLoggerMode"/> filter applied.
		/// </summary>
		public const string ImportantLogScopeEndPrefix = "// !-->";
		#endregion

		/// <summary>Begins 'Log all' scope for the loggers.</summary>
		/// <param name="loggers">The loggers.</param>
		/// <returns>Disposable to mark the scope completion.</returns>
		internal static IDisposable BeginImportantLogScope(FilteringLogger[] loggers)
		{
			if (loggers.Length == 0)
				return Disposable.Empty;

			foreach (var filteringLogger in loggers)
			{
				filteringLogger.IncrementScopeCount();
			}

			return Disposable.Create(
				() =>
				{
					foreach (var filteringLogger in loggers)
					{
						filteringLogger.DecrementScopeCount();
					}
				});
		}

		#region Fields, .ctor & properties
		private volatile int _importantScopeCount;

		/// <summary>Initializes a new instance of the <see cref="FilteringLogger"/> class.</summary>
		/// <param name="wrappedLogger">The logger to redirect the output. Cannot be null.</param>
		/// <param name="filteringMode">The log filtering mode.</param>
		public FilteringLogger([NotNull] ILogger wrappedLogger, FilteringLoggerMode filteringMode)
		{
			if (wrappedLogger == null)
				throw new ArgumentNullException(nameof(wrappedLogger));

			WrappedLogger = wrappedLogger;
			FilteringMode = filteringMode;
		}

		/// <summary>Gets logger that consumes filtered text.</summary>
		/// <value>The logger that consumes filtered text.</value>
		protected ILogger WrappedLogger { get; }

		/// <summary>Gets log filtering mode.</summary>
		/// <value>The log filtering mode.</value>
		public FilteringLoggerMode FilteringMode { get; }
		#endregion

#pragma warning disable 420
		private void IncrementScopeCount() =>
			Interlocked.Increment(ref _importantScopeCount);

		// Decrement if value > 0
		private void DecrementScopeCount() =>
			InterlockedOperations.Update(ref _importantScopeCount, i => Math.Max(i - 1, 0));
#pragma warning restore 420

		/// <summary>Checks if the line should be written.</summary>
		/// <param name="logKind">The kind of log message.</param>
		/// <returns><c>true</c> if the line should be written.</returns>
		protected virtual bool ShouldWrite(LogKind logKind) =>
			FilteringMode == FilteringLoggerMode.AllMessages ||
				_importantScopeCount > 0 ||
				(logKind == LogKind.Error && FilteringMode == FilteringLoggerMode.PrefixedOrErrors);

		/// <summary>Handles well-known prefixes for the line.</summary>
		/// <param name="text">The text of the log line.</param>
		/// <returns><c>true</c> if the line should be written.</returns>
		protected virtual bool PreprocessLine(string text)
		{
			if (FilteringMode == FilteringLoggerMode.AllMessages)
				return true;

			if (string.IsNullOrEmpty(text))
				return false;

			bool shouldWrite;
			if (text.StartsWith(ImportantLogScopeStartPrefix, StringComparison.Ordinal))
			{
				IncrementScopeCount();
				shouldWrite = true;
			}
			else if (text.StartsWith(ImportantLogScopeEndPrefix, StringComparison.Ordinal))
			{
				// Decrement if value > 0
				DecrementScopeCount();
				shouldWrite = true;
			}
			else if (text.StartsWith(ImportantInfoPrefix, StringComparison.Ordinal)
				|| text.StartsWith(InfoPrefix, StringComparison.Ordinal))
			{
				shouldWrite = true;
			}
			else
			{
				shouldWrite = false;
			}

			return shouldWrite;
		}

		/// <summary>Write empty line.</summary>
		public virtual void WriteLine()
		{
			if (ShouldWrite(LogKind.Default))
			{
				WrappedLogger.WriteLine();
			}
		}

		/// <summary>Writes line.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public virtual void WriteLine(LogKind logKind, string text)
		{
			// DONTTOUCH: the order of calls in condition is important.
			// Preprocess call can change the _importantScopeCount value.
			if (PreprocessLine(text) || ShouldWrite(logKind))
			{
				WrappedLogger.WriteLine(logKind, text);
			}
		}

		/// <summary>Writes text.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public virtual void Write(LogKind logKind, string text)
		{
			if (ShouldWrite(logKind))
			{
				WrappedLogger.Write(logKind, text);
			}
		}

		/// <summary>Flushes the log.</summary>
		public void Flush() => WrappedLogger.Flush();
	}
}