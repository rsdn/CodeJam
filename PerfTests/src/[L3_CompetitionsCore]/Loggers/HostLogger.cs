using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using BenchmarkDotNet.Loggers;

using CodeJam.Threading;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Loggers
{
	/// <summary>Basic logger implementation for unit test runners</summary>
	/// <seealso cref="BenchmarkDotNet.Loggers.ILogger"/>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
	public class HostLogger : IFlushableLogger
	{
		// DONTTOUCH: Check that all code does not hardcode content of the constants before changing

		#region Log line prefixes (constants)
		/// <summary>
		/// The prefix for informational log lines.
		/// Lines with this prefix will be written even if <see cref="HostLogMode"/> filter applied.
		/// </summary>
		public const string LogInfoPrefix = "// ?";

		/// <summary>
		/// The prefix for important log lines.
		/// Lines with this prefix will be written even if <see cref="HostLogMode"/> filter applied.
		/// </summary>
		public const string LogImportantInfoPrefix = "// !";

		/// <summary>
		/// The start prefix for important log area.
		/// Lines between start and end prefixes will be written even if <see cref="HostLogMode"/> filter applied.
		/// </summary>
		public const string LogImportantAreaStart = "// !<--";

		/// <summary>
		/// The end prefix for important log area.
		/// Lines between start and end prefixes will be written even if <see cref="HostLogMode"/> filter applied.
		/// </summary>
		public const string LogImportantAreaEnd = "// !-->";
		#endregion

		#region Fields, .ctor & properties
		private volatile int _importantAreaCount;

		/// <summary>Initializes a new instance of the <see cref="HostLogger"/> class.</summary>
		/// <param name="wrappedLogger">The logger to redirect the output. Cannot be null.</param>
		/// <param name="logMode">Host logging mode.</param>
		public HostLogger([NotNull] ILogger wrappedLogger, HostLogMode logMode)
		{
			Code.NotNull(wrappedLogger, nameof(wrappedLogger));

			WrappedLogger = wrappedLogger;
			LogMode = logMode;
		}

		/// <summary>The logger to redirect the output.</summary>
		/// <value>The logger to redirect the output.</value>
		protected ILogger WrappedLogger { get; }

		/// <summary>Host logging mode.</summary>
		/// <value>The host logging mode.</value>
		public HostLogMode LogMode { get; }
		#endregion

		/// <summary>Checks if the line should be written.</summary>
		/// <param name="logKind">The kind of log message.</param>
		/// <returns><c>true</c> if the line should be written.</returns>
		// ReSharper disable once VirtualMemberNeverOverriden.Global
		protected virtual bool ShouldWrite(LogKind logKind) =>
			LogMode == HostLogMode.AllMessages ||
				_importantAreaCount > 0 ||
				(logKind == LogKind.Error && LogMode == HostLogMode.PrefixedOrErrors);

		/// <summary>Handles well-known prefixes for the line.</summary>
		/// <param name="text">The text of the log line.</param>
		/// <returns><c>true</c> if the line should be written.</returns>
		// ReSharper disable once VirtualMemberNeverOverriden.Global
		protected virtual bool PreprocessLine(string text)
		{
			if (LogMode == HostLogMode.AllMessages)
				return true;

			if (string.IsNullOrEmpty(text))
				return false;

			bool shouldWrite;
#pragma warning disable 420
			if (text.StartsWith(LogImportantAreaStart, StringComparison.Ordinal))
			{
				Interlocked.Increment(ref _importantAreaCount);
				shouldWrite = true;
			}
			else if (text.StartsWith(LogImportantAreaEnd, StringComparison.Ordinal))
			{
				// Decrement if value > 0
				InterlockedOperations.Update(ref _importantAreaCount, i => Math.Max(i - 1, 0));
				shouldWrite = true;
			}
#pragma warning restore 420
			else if (text.StartsWith(LogImportantInfoPrefix, StringComparison.Ordinal) ||
				text.StartsWith(LogInfoPrefix, StringComparison.Ordinal))
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

		/// <summary>Write the line.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public virtual void WriteLine(LogKind logKind, string text)
		{
			// DONTTOUCH: the order of calls in condition is important.
			if (PreprocessLine(text) || ShouldWrite(logKind))
			{
				WrappedLogger.WriteLine(logKind, text);
			}
		}

		/// <summary>Write the text.</summary>
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
		void IFlushableLogger.Flush() =>
			(WrappedLogger as IFlushableLogger)?.Flush();
	}
}