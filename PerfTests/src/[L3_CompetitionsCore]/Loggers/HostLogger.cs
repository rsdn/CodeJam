using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using BenchmarkDotNet.Loggers;

using CodeJam.Threading;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Loggers
{
	/// <summary>
	/// Basic logger implementation for unit test runners
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global")]
	public class HostLogger : ILogger
	{
		// DONTTOUCH: Check that all code does not hardcode content of the constants before changing

		#region Log line prefixes (constants)
		/// <summary>
		/// The prefix for informational log lines.
		/// Lines with this prefix will be written even if <seealso cref="DetailedLogging"/> set to <c>false</c>.
		/// </summary>
		public const string LogInfoPrefix = "// ?";

		/// <summary>
		/// The prefix for important log lines.
		/// Lines with this prefix will be written even if <seealso cref="DetailedLogging"/> set to <c>false</c>.
		/// </summary>
		public const string LogImportantInfoPrefix = "// !";

		/// <summary>
		/// The start prefix for important log area.
		/// Lines between start and end prefixes will be written even if <seealso cref="DetailedLogging"/> set to <c>false</c>.
		/// </summary>
		public const string LogImportantAreaStart = "// !<--";

		/// <summary>
		/// The end prefix for important log area.
		/// Lines between start and end prefixes will be written even if <seealso cref="DetailedLogging"/> set to <c>false</c>.
		/// </summary>
		public const string LogImportantAreaEnd = "// !-->";
		#endregion

		#region Fields, .ctor & properties
		private volatile int _importantAreaCount;

		/// <summary>Initializes a new instance of the <see cref="HostLogger"/> class.</summary>
		/// <param name="wrappedLogger">The logger to redirect the output. Cannot be null.</param>
		public HostLogger([NotNull] ILogger wrappedLogger) : this(wrappedLogger, false) { }

		/// <summary>Initializes a new instance of the <see cref="HostLogger"/> class.</summary>
		/// <param name="wrappedLogger">The logger to redirect the output. Cannot be null.</param>
		/// <param name="detailedLogging">
		/// Detailed logging mode.
		/// If set to <c>false</c> only messages with LogImportant* prefixes wil be written.
		/// </param>
		public HostLogger([NotNull] ILogger wrappedLogger, bool detailedLogging)
		{
			Code.NotNull(wrappedLogger, nameof(wrappedLogger));

			WrappedLogger = wrappedLogger;
			DetailedLogging = detailedLogging;
		}

		/// <summary>The logger to redirect the output.</summary>
		/// <value>TThe logger to redirect the output.</value>
		protected ILogger WrappedLogger { get; }

		/// <summary>
		/// Detailed logging mode.
		/// If disabled, only messages with LogImportant* prefixes are logged.
		/// </summary>
		/// <value><c>true</c> if detailed logging mode is enabled.</value>
		public bool DetailedLogging { get; }
		#endregion

		/// <summary>Checks if the line should be written.</summary>
		/// <param name="logKind">The kind of log message.</param>
		/// <returns><c>true</c> if the line should be written.</returns>
		// ReSharper disable once VirtualMemberNeverOverriden.Global
		protected virtual bool ShouldWrite(LogKind logKind) =>
			DetailedLogging ||
				_importantAreaCount > 0 ||
				logKind == LogKind.Error;

		/// <summary>Handles well-known prefixes for the line.</summary>
		/// <param name="text">The text of the log line.</param>
		/// <returns><c>true</c> if the line should be written.</returns>
		// ReSharper disable once VirtualMemberNeverOverriden.Global
		protected virtual bool PreprocessLine(string text)
		{
			if (DetailedLogging)
				return true;

			if (string.IsNullOrEmpty(text))
				return false;

			bool shouldWrite;
#pragma warning disable 420
			if (text.StartsWith(LogImportantAreaStart, StringComparison.Ordinal))
			{
				shouldWrite = true;
				Interlocked.Increment(ref _importantAreaCount);
			}
			else if (text.StartsWith(LogImportantAreaEnd, StringComparison.Ordinal))
			{
				shouldWrite = true;
				// Decrement if value > 0
				InterlockedOperations.Update(ref _importantAreaCount, i => Math.Max(i - 1, 0));
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

		/// <summary>Write the empty line.</summary>
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

		/// <summary>Write the test.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public virtual void Write(LogKind logKind, string text)
		{
			if (ShouldWrite(logKind))
			{
				WrappedLogger.Write(logKind, text);
			}
		}
	}
}