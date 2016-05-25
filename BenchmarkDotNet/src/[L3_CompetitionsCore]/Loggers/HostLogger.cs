using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Loggers
{
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public class HostLogger : ILogger
	{
		// DONTTOUCH: Verify that all code uses these constants before changing
		// DONTTOUCH: Check that PreprocessLine works after changing
		// TODO: unittest for that
		public const string LogInfoPrefix = "// ?";
		public const string LogImportantInfoPrefix = "// !";
		public const string LogImportantAreaStart = "// !<--";
		public const string LogImportantAreaEnd = "// !-->";

		#region Fields, .ctor & properties
		private volatile int _importantAreaCount;

		public HostLogger(ILogger wrappedLogger, bool detailedLogging)
		{
			if (wrappedLogger == null)
				throw new ArgumentNullException(nameof(wrappedLogger));

			WrappedLogger = wrappedLogger;
			DetailedLogging = detailedLogging;
		}

		protected ILogger WrappedLogger { get; }
		public bool DetailedLogging { get; }
		#endregion

		// ReSharper disable once VirtualMemberNeverOverriden.Global
		protected virtual bool ShouldWrite(LogKind kind) =>
			_importantAreaCount > 0 ||
				DetailedLogging ||
				kind == LogKind.Error;

		// ReSharper disable once VirtualMemberNeverOverriden.Global
		protected virtual bool PreprocessLine(string text)
		{
			if (string.IsNullOrEmpty(text))
				return false;

			bool shouldWrite = false;
#pragma warning disable 420
			if (text.StartsWith(LogImportantAreaStart, StringComparison.Ordinal))
			{
				shouldWrite = true;
				Interlocked.Increment(ref _importantAreaCount);
			}
			else if (text.StartsWith(LogImportantAreaEnd, StringComparison.Ordinal))
			{
				shouldWrite = true;
				Interlocked.Decrement(ref _importantAreaCount);
			}
#pragma warning restore 420
			else if (text.StartsWith(LogImportantInfoPrefix, StringComparison.Ordinal))
			{
				shouldWrite = true;
			}

			return shouldWrite;
		}

		public virtual void WriteLine()
		{
			if (ShouldWrite(LogKind.Default))
			{
				WrappedLogger.WriteLine();
			}
		}

		public virtual void WriteLine(LogKind logKind, string text)
		{
			// DONTTOUCH: the order of calls in condition is important.
			if (PreprocessLine(text) || ShouldWrite(logKind))
			{
				WrappedLogger.WriteLine(logKind, text);
			}
		}

		public virtual void Write(LogKind logKind, string text)
		{
			if (ShouldWrite(logKind))
			{
				WrappedLogger.Write(logKind, text);
			}
		}
	}
}