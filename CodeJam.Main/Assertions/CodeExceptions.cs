using System;
using System.Diagnostics;

using CodeJam.Internal;

using JetBrains.Annotations;

using static CodeJam.Internal.CodeExceptionsHelper;

namespace CodeJam
{
	/// <summary>Exception factory class</summary>
	[PublicAPI]
	public static class CodeExceptions
	{
		#region Argument validation
		/// <summary>Creates <see cref="ArgumentNullException"/>.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>Initialized instance of <see cref="ArgumentNullException"/>.</returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentNullException ArgumentNull([InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentNullException(argumentName).LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException" /> for default values.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="type">Type of the argument.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException" />.</returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentDefault([InvokerParameterName] string argumentName, Type type)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"The value of '{argumentName}' should be not equal to default({type.Name}).",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for null collection item.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentItemNull([InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"All items in '{argumentName}' should not be null.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="ArgumentException"/>.
		/// </summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/></returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentEmpty([InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"Collection '{argumentName}' must not be empty.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for empty string.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentNullOrEmpty([InvokerParameterName] string argumentName)
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
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentNullOrWhiteSpace([InvokerParameterName] string argumentName)
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
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentOutOfRange(
			[InvokerParameterName] string argumentName,
			int value, int fromValue, int toValue)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				Invariant($"The value of '{argumentName}' ({value}) should be between {fromValue} and {toValue}."))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentOutOfRangeException"/>.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="fromValue">From value (inclusive).</param>
		/// <param name="toValue">To value (inclusive).</param>
		/// <returns>Initialized instance of <see cref="ArgumentOutOfRangeException"/>.</returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentOutOfRange(
			[InvokerParameterName] string argumentName,
			double value, double fromValue, double toValue)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				Invariant($"The value of '{argumentName}' ({value}) should be between {fromValue} and {toValue}."))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentOutOfRangeException"/>.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="fromValue">From value (inclusive).</param>
		/// <param name="toValue">To value (inclusive).</param>
		/// <returns>Initialized instance of <see cref="ArgumentOutOfRangeException"/>.</returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentOutOfRange<T>(
			[InvokerParameterName] string argumentName,
			T value,
			T fromValue,
			T toValue)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				Invariant($"The value of '{argumentName}' ('{value}') should be between '{fromValue}' and '{toValue}'."))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="IndexOutOfRangeException"/>.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="length">The length.</param>
		/// <returns>Initialized instance of <see cref="IndexOutOfRangeException"/>.</returns>
		[DebuggerHidden, MustUseReturnValue]
		public static IndexOutOfRangeException IndexOutOfRange(
			[InvokerParameterName] string argumentName,
			int value, int startIndex, int length)
		{
			BreakIfAttached();
			return new IndexOutOfRangeException(
				Invariant(
					$"The value of '{argumentName}' ({value}) should be greater than or equal to {startIndex} and less than {length}."))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for non-cancellable tokens.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, MustUseReturnValue]
		public static ArgumentException ArgumentWaitCancellationRequired([InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentException(
				Invariant($"This method requires '{argumentName}' to be cancellable; otherwise method may wait indefinitely."),
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}
		#endregion

		#region General purpose exceptions
		/// <summary>Creates <see cref="ArgumentException"/>.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static ArgumentException Argument(
			[InvokerParameterName] string argumentName,
			string messageFormat,
			params object[]? args)
		{
			BreakIfAttached();
			return new ArgumentException(
				InvariantFormat(messageFormat, args),
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="InvalidOperationException"/>.</summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="InvalidOperationException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static InvalidOperationException InvalidOperation(
			[NotNull] string messageFormat,
			params object[]? args)
		{
			BreakIfAttached();
			return new InvalidOperationException(InvariantFormat(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="OverflowException"/>.</summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="OverflowException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static OverflowException Overflow(
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new OverflowException(InvariantFormat(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}
		#endregion

		#region Exceptions for specific scenarios
		/// <summary>Creates <see cref="TimeoutException"/>.</summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="TimeoutException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static TimeoutException Timeout(
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new TimeoutException(InvariantFormat(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="TimeoutException" />.</summary>
		/// <param name="timeout">The timeout.</param>
		/// <returns>Initialized instance of <see cref="TimeoutException" />.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static TimeoutException Timeout(TimeSpan timeout)
		{
			BreakIfAttached();
			return new TimeoutException(
				Invariant($"Operation timed out in {timeout}."))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="ArgumentOutOfRangeException"/>.
		/// Used to be thrown from the default: switch clause
		/// </summary>
		/// <typeparam name="T">The type of the value. Auto-inferred.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <returns>Initialized instance of <see cref="ArgumentOutOfRangeException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static ArgumentOutOfRangeException UnexpectedArgumentValue<T>(
			[NotNull, InvokerParameterName] string argumentName,
			[CanBeNull] T value)
		{
			BreakIfAttached();
			var valueType = value?.GetType() ?? typeof(T);
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				Invariant($"Unexpected value '{value}' of type '{valueType.FullName}'."))
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
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static ArgumentOutOfRangeException UnexpectedArgumentValue<T>(
			[NotNull, InvokerParameterName] string argumentName,
			[CanBeNull] T value,
			[NotNull] string messageFormat, [CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				InvariantFormat(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="InvalidOperationException"/>.
		/// Used to be thrown from the default: switch clause
		/// </summary>
		/// <typeparam name="T">The type of the value. Auto-inferred.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>Initialized instance of <see cref="InvalidOperationException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static InvalidOperationException UnexpectedValue<T>([CanBeNull] T value)
		{
			BreakIfAttached();
			var valueType = value?.GetType() ?? typeof(T);
			return new InvalidOperationException(
				Invariant($"Unexpected value '{value}' of type '{valueType.FullName}'."))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>
		/// Creates <see cref="InvalidOperationException"/>.
		/// Used to be thrown from default: switch clause
		/// </summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="InvalidOperationException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static InvalidOperationException UnexpectedValue(
			[NotNull] string messageFormat, [CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new InvalidOperationException(InvariantFormat(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Throw this if the object is disposed.</summary>
		/// <param name="typeofDisposedObject">The typeof disposed object.</param>
		/// <returns>Initialized instance of <see cref="ObjectDisposedException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static ObjectDisposedException ObjectDisposed(Type? typeofDisposedObject)
		{
			BreakIfAttached();
			return new ObjectDisposedException(typeofDisposedObject?.FullName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Throw this if the object is disposed.</summary>
		/// <param name="typeofDisposedObject">The typeof disposed object.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="ObjectDisposedException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static ObjectDisposedException ObjectDisposed(
			Type? typeofDisposedObject,
			string messageFormat,
			params object[]? args)
		{
			BreakIfAttached();
			return new ObjectDisposedException(
				typeofDisposedObject?.FullName,
				InvariantFormat(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Used to be thrown in places expected to be unreachable.</summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="NotSupportedException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static NotSupportedException Unreachable([NotNull] string messageFormat, [CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new NotSupportedException(InvariantFormat(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}
		#endregion
	}
}
