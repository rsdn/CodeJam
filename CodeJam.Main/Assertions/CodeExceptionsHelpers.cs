using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace CodeJam.Internal
{
	/// <summary>Helper class for custom code exception factory classes</summary>
	[PublicAPI]
	public static class CodeExceptionsHelpers
	{
		#region Setup
		/// <summary>
		/// If true, breaks execution if debugger is attached and assertion is failed.
		/// Enabled by default.
		/// </summary>
		/// <value><c>true</c> if the execution will break on exception creation; otherwise, <c>false</c>.</value>
		public static bool BreakOnException { get; set; } = true;

		/// <summary>BreaksExecution if debugger attached.</summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		public static void BreakIfAttached()
		{
			if (BreakOnException && Debugger.IsAttached)
				Debugger.Break();
		}
		#endregion

		#region Exception message
		/// <summary>
		/// Formats message or returns <paramref name="messageFormat"/> as it is if <paramref name="args"/> are null or empty.
		/// </summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Formatted string.</returns>
		[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static string FormatExceptionMessage([NotNull] string messageFormat, [CanBeNull] params object[] args) =>
			(args == null || args.Length == 0)
				? messageFormat
				: string.Format(CultureInfo.InvariantCulture, messageFormat, args);

		/// <summary>
		/// Returns culture-invariant representation of passed value.
		/// </summary>
		public static string ToInv<T>([NotNull] this T value)
		{
			if (value is IFormattable f)
				return f.ToString(null, CultureInfo.InvariantCulture);
			return value.ToString();
		}
		#endregion

		#region Logging
		/// <summary>Returns trace source for code exceptions.</summary>
		/// <value>The code trace source.</value>
		[NotNull]
		public static TraceSource CodeTraceSource => _codeTraceSource.Value;

		[NotNull]
		[ItemNotNull]
		private static readonly Lazy<TraceSource> _codeTraceSource = new Lazy<TraceSource>(
			() => CreateTraceSource(typeof(Code).Namespace + "." + nameof(CodeTraceSource)));

		[NotNull]
		private static TraceSource CreateTraceSource([NotNull] string sourceName) =>
			new TraceSource(sourceName) { Switch = { Level = SourceLevels.Warning } };

		private static readonly string _assertionFailedMessageWithStack =
			"Assertion failed: {0}" + Environment.NewLine + "{1}";

		private static readonly string _exceptionCaughtMessage = "Exception caught safely: {0}";
		private static readonly string _exceptionSwallowedMessage = "Exception swallowed: {0}";

		/// <summary>Logs the exception that will be thrown to the <see cref="CodeTraceSource"/>.</summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="exception">The exception.</param>
		/// <returns>The original exception.</returns>
		[NotNull]
		[MustUseReturnValue]
		public static TException LogToCodeTraceSourceBeforeThrow<TException>([NotNull] this TException exception)
			where TException : Exception
		{
			CodeTraceSource.TraceEvent(
				TraceEventType.Verbose,
				0,
				_assertionFailedMessageWithStack,
				exception,
				Environment.StackTrace);

			return exception;
		}

		/// <summary>
		/// Logs the caught exception to the <see cref="CodeTraceSource" />.
		/// </summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="exception">The exception.</param>
		/// <param name="safe">If set to <c>true</c> the exception is expected and can be ignored safely.</param>
		/// <returns>
		/// The original exception.
		/// </returns>
		[NotNull]
		public static TException LogToCodeTraceSourceOnCatch<TException>([NotNull] this TException exception, bool safe)
			where TException : Exception
		{
			if (safe)
				CodeTraceSource.TraceEvent(
					TraceEventType.Verbose,
					0,
					_exceptionCaughtMessage,
					exception);

			else
				CodeTraceSource.TraceEvent(
					TraceEventType.Warning,
					0,
					_exceptionSwallowedMessage,
					exception);

			return exception;
		}
		#endregion
	}
}