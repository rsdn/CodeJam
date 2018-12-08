using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;

namespace CodeJam
{
	/// <summary>Exception factory class</summary>
	[PublicAPI]
	public static class CodeExceptions
	{
		#region Behavior setup and implementation helpers
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

		/// <summary>
		/// Formats message or returns <paramref name="messageFormat"/> as it is if <paramref name="args"/> are null or empty.
		/// </summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Formatted string.</returns>
		[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		internal static string FormatMessage([NotNull] string messageFormat, [CanBeNull] params object[] args) =>
			(args == null || args.Length == 0)
				? messageFormat
				: string.Format(CultureInfo.InvariantCulture, messageFormat, args);

		private static string ToInv<T>([NotNull] this T value)
		{
			if (value is IFormattable f)
				return f.ToString(null, CultureInfo.InvariantCulture);
			return value.ToString();
		}

		/// <summary>Returns trace source for code exceptions.</summary>
		/// <value>The code trace source.</value>
		[NotNull] public static TraceSource CodeTraceSource => _codeTraceSource.Value;

		[NotNull][ItemNotNull]
		private static readonly Lazy<TraceSource> _codeTraceSource = new Lazy<TraceSource>(
			() => CreateTraceSource(typeof(Code).Namespace + "." + nameof(CodeTraceSource)));

		[NotNull]
		private static TraceSource CreateTraceSource([NotNull] string sourceName)
		{
			// BASEDON: System.Diagnostics.PresentationTraceSources
			var traceSource = new TraceSource(sourceName);
			if (traceSource.Switch.Level == SourceLevels.Off && Debugger.IsAttached)
			{
				traceSource.Switch.Level = SourceLevels.Warning;
			}
			return traceSource;
		}

		/// <summary>Logs the exception that will be thrown to the <see cref="CodeTraceSource"/>.</summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="exception">The exception.</param>
		/// <returns>The original exception.</returns>
		[NotNull]
		internal static TException LogToCodeTraceSourceBeforeThrow<TException>([NotNull] this TException exception)
			where TException : Exception
		{
			var sb = new StringBuilder();
			exception.ToDiagnosticString(sb);
			if (exception.StackTrace == null)
			{
				sb.Append(new StackTrace());
			}

			CodeTraceSource.TraceEvent(TraceEventType.Error, 0, sb.ToString());
			return exception;
		}

		/// <summary>Logs the caught exception to the <see cref="CodeTraceSource"/>.</summary>
		/// <typeparam name="TException">The type of the exception.</typeparam>
		/// <param name="exception">The exception.</param>
		/// <returns>The original exception.</returns>
		internal static TException LogToCodeTraceSourceCaught<TException>([NotNull] this TException exception)
			where TException : Exception
		{
			var sb = new StringBuilder();
			sb.Append("Swallowed: ");
			exception.ToDiagnosticString(sb);
			if (exception.StackTrace == null)
			{
				sb.Append(new StackTrace());
			}

			CodeTraceSource.TraceEvent(TraceEventType.Warning, 0, sb.ToString());
			return exception;
		}
		#endregion

		#region Argument validation
		/// <summary>Creates <see cref="ArgumentNullException"/>.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>Initialized instance of <see cref="ArgumentNullException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentNullException ArgumentNull([NotNull, InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentNullException(argumentName).LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentNullException"/>.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentException ArgumentItemNull([NotNull, InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentException(
				argumentName, $"All items in '{argumentName}' should not be null.")
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for empty string.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentException ArgumentNullOrEmpty([NotNull, InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"String '{argumentName}' should be neither null nor empty.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for empty (or whitespace) string.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentException ArgumentNullOrWhiteSpace([NotNull, InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"String '{argumentName}' should be neither null nor whitespace.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentOutOfRangeException"/>.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="fromValue">From value (inclusive).</param>
		/// <param name="toValue">To value (inclusive).</param>
		/// <returns>Initialized instance of <see cref="ArgumentOutOfRangeException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentOutOfRange(
			[NotNull, InvokerParameterName] string argumentName,
			int value, int fromValue, int toValue)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				$"The value of '{argumentName}' ({value.ToInv()}) should be between {fromValue.ToInv()} and {toValue.ToInv()}.")
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentOutOfRangeException"/>.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="fromValue">From value (inclusive).</param>
		/// <param name="toValue">To value (inclusive).</param>
		/// <returns>Initialized instance of <see cref="ArgumentOutOfRangeException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentOutOfRange(
			[NotNull, InvokerParameterName] string argumentName,
			double value, double fromValue, double toValue)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				$"The value of '{argumentName}' ({value.ToInv()}) should be between {fromValue.ToInv()} and {toValue.ToInv()}.")
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentOutOfRangeException"/>.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="fromValue">From value (inclusive).</param>
		/// <param name="toValue">To value (inclusive).</param>
		/// <returns>Initialized instance of <see cref="ArgumentOutOfRangeException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentOutOfRange<T>(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] T value,
			[NotNull] T fromValue,
			[NotNull] T toValue)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				$"The value of '{argumentName}' ('{value.ToInv()}') should be between '{fromValue.ToInv()}' and '{toValue.ToInv()}'.")
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="IndexOutOfRangeException"/>.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="length">The length.</param>
		/// <returns>Initialized instance of <see cref="IndexOutOfRangeException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static IndexOutOfRangeException IndexOutOfRange(
			[NotNull, InvokerParameterName] string argumentName,
			int value, int startIndex, int length)
		{
			BreakIfAttached();
			return new IndexOutOfRangeException(
				$"The value of '{argumentName}' ({value.ToInv()}) should be greater than or equal to {startIndex.ToInv()} and less than {length.ToInv()}.")
				.LogToCodeTraceSourceBeforeThrow();
		}
		#endregion

		#region General purpose exceptions
		/// <summary>Creates <see cref="ArgumentException"/>.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static ArgumentException Argument(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new ArgumentException(
				FormatMessage(messageFormat, args), argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="InvalidOperationException"/>.</summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="InvalidOperationException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static InvalidOperationException InvalidOperation(
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new InvalidOperationException(
				FormatMessage(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}
		#endregion

		#region Exceptions for specific scenarios
		/// <summary>
		/// Creates <see cref="ArgumentOutOfRangeException"/>.
		/// Used to be thrown from the default: switch clause
		/// </summary>
		/// <typeparam name="T">The type of the value. Auto-inferred.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <returns>Initialized instance of <see cref="ArgumentOutOfRangeException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentOutOfRangeException UnexpectedArgumentValue<T>(
			[NotNull, InvokerParameterName] string argumentName,
			[CanBeNull] T value)
		{
			BreakIfAttached();
			var valueType = value?.GetType() ?? typeof(T);
			return new ArgumentOutOfRangeException(
				argumentName, value, $"Unexpected value '{value?.ToInv()}' of type '{valueType.FullName}'.")
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="ArgumentOutOfRangeException"/>.
		/// Used to be thrown from default: switch clause
		/// </summary>
		/// <typeparam name="T">The type of the value. Auto-inferred.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="ArgumentOutOfRangeException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static ArgumentOutOfRangeException UnexpectedArgumentValue<T>(
			[NotNull, InvokerParameterName] string argumentName,
			[CanBeNull] T value,
			[NotNull] string messageFormat, [CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName, value,
				FormatMessage(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="InvalidOperationException"/>.
		/// Used to be thrown from the default: switch clause
		/// </summary>
		/// <typeparam name="T">The type of the value. Auto-inferred.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>Initialized instance of <see cref="InvalidOperationException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static InvalidOperationException UnexpectedValue<T>([CanBeNull] T value)
		{
			BreakIfAttached();
			var valueType = value?.GetType() ?? typeof(T);
			return
				new InvalidOperationException(
					$"Unexpected value '{value?.ToInv()}' of type '{valueType.FullName}'.")
					.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="InvalidOperationException"/>.
		/// Used to be thrown from default: switch clause
		/// </summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="InvalidOperationException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static InvalidOperationException UnexpectedValue(
			[NotNull] string messageFormat, [CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new InvalidOperationException(
				FormatMessage(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Throw this if the object is disposed.</summary>
		/// <param name="typeofDisposedObject">The typeof disposed object.</param>
		/// <returns>Initialized instance of <see cref="ObjectDisposedException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ObjectDisposedException ObjectDisposed([CanBeNull] Type typeofDisposedObject)
		{
			BreakIfAttached();
			return new ObjectDisposedException(
				typeofDisposedObject?.FullName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Throw this if the object is disposed.</summary>
		/// <param name="typeofDisposedObject">The typeof disposed object.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="ObjectDisposedException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static ObjectDisposedException ObjectDisposed(
			[CanBeNull] Type typeofDisposedObject,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return
				new ObjectDisposedException(
					typeofDisposedObject?.FullName, FormatMessage(messageFormat, args))
					.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Used to be thrown in places expected to be unreachable.</summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="NotSupportedException"/>.</returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static NotSupportedException Unreachable([NotNull] string messageFormat, [CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new NotSupportedException(
				FormatMessage(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}
		#endregion
	}
}