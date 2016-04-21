using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;

namespace CodeJam
{
	/// <summary>
	/// Exception factory class
	/// </summary>
	[PublicAPI]
	public static class CodeExceptions
	{
		#region Behavior setup and implementation helpers
		/// <summary>
		/// If true, breaks execution on assertion failure.
		/// Enabled by default.
		/// </summary>
		public static bool BreakOnException { get; set; } = true;

		/// <summary>
		/// BreaksExecution if debugger attached
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		public static void BreakIfAttached()
		{
			if (BreakOnException && Debugger.IsAttached)
				Debugger.Break();
		}

		/// <summary>
		/// Formats message or returns <paramref name="messageFormat"/> as it is if <paramref name="args"/> are null or empty
		/// </summary>
		[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		private static string FormatMessage([NotNull] string messageFormat, [CanBeNull] params object[] args) =>
			(args == null || args.Length == 0) ? messageFormat : string.Format(messageFormat, args);
		#endregion

		#region Argument validation
		/// <summary>
		/// Creates <seealso cref="ArgumentNullException"/>
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentNullException ArgumentNull([NotNull, InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentNullException(argumentName);
		}

		/// <summary>
		/// Creates <seealso cref="ArgumentException"/> for empty string
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentException ArgumentNullOrEmpty([NotNull, InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"String '{argumentName}' should be neither null nor empty",
				argumentName);
		}

		/// <summary>
		/// Creates <seealso cref="ArgumentException"/> for empty (or whitespace) string
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentException ArgumentNullOrWhiteSpace([NotNull, InvokerParameterName] string argumentName)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"String '{argumentName}' should be neither null nor whitespace",
				argumentName);
		}

		/// <summary>
		/// Creates <seealso cref="ArgumentOutOfRangeException"/>
		/// </summary>
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
				$"The value of '{argumentName}' ({value}) should be between {fromValue} and {toValue}");
		}

		/// <summary>
		/// Creates <seealso cref="ArgumentOutOfRangeException"/>
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentOutOfRange(
			[NotNull, InvokerParameterName] string argumentName,
			int value, int fromValue)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				$"The value of '{argumentName}' ({value}) should be greater than {fromValue}");
		}

		/// <summary>
		/// Creates <seealso cref="ArgumentOutOfRangeException"/>
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentOutOfRange<T>(
			[NotNull, InvokerParameterName] string argumentName,
			T value, T fromValue, T toValue)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				$"The value of '{argumentName}' ('{value}') should be between '{fromValue}' and '{toValue}'");
		}

		/// <summary>
		/// Creates <seealso cref="ArgumentOutOfRangeException"/>
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentOutOfRange<T>(
			[NotNull, InvokerParameterName] string argumentName,
			T value,
			T fromValue)
		{
			BreakIfAttached();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				$"The value of '{argumentName}' ('{value}') should be greater than '{fromValue}'.");
		}

		/// <summary>
		/// Creates <seealso cref="IndexOutOfRangeException"/>
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static IndexOutOfRangeException IndexOutOfRange(
			[NotNull, InvokerParameterName] string argumentName,
			int value, int startIndex, int length)
		{
			BreakIfAttached();
			return new IndexOutOfRangeException(
				$"The value of '{argumentName}' ({value}) should be greater than or equal to {startIndex} and less than {length}.");
		}
		#endregion

		#region General purpose exceptions
		/// <summary>
		/// Creates <seealso cref="ArgumentException"/>
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static ArgumentException Argument(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new ArgumentException(FormatMessage(messageFormat, args), argumentName);
		}

		/// <summary>
		/// Creates <seealso cref="InvalidOperationException"/>
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static InvalidOperationException InvalidOperation(
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new InvalidOperationException(FormatMessage(messageFormat, args));
		}
		#endregion

		#region Exceptions for specific scenarios
		/// <summary>
		/// Creates <seealso cref="ArgumentOutOfRangeException"/>.
		/// Used to be thrown from the default: switch clause
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentOutOfRangeException UnexpectedArgumentValue<T>(
			[NotNull, InvokerParameterName] string argumentName,
			[CanBeNull] T value)
		{
			BreakIfAttached();
			var valueType = value?.GetType() ?? typeof(T);
			return new ArgumentOutOfRangeException(
				argumentName, value, $"Unexpected value '{value}' of type '{valueType.FullName}'");
		}

		/// <summary>
		/// Creates <seealso cref="ArgumentOutOfRangeException"/>.
		/// Used to be thrown from default: switch clause
		/// </summary>
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
				FormatMessage(messageFormat, args));
		}

		/// <summary>
		/// Creates <seealso cref="InvalidOperationException"/>.
		/// Used to be thrown from the default: switch clause
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static InvalidOperationException UnexpectedValue<T>([CanBeNull] T value)
		{
			BreakIfAttached();
			var valueType = value?.GetType() ?? typeof(T);
			return new InvalidOperationException($"Unexpected value '{value}' of type '{valueType.FullName}'");
		}

		/// <summary>
		/// Creates <seealso cref="InvalidOperationException"/>.
		/// Used to be thrown from default: switch clause
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static InvalidOperationException UnexpectedValue(
			[NotNull] string messageFormat, [CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new InvalidOperationException(FormatMessage(messageFormat, args));
		}

		/// <summary>
		/// Throw this if the object is disposed.
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static ObjectDisposedException ObjectDisposed([CanBeNull] Type typeofDisposedObject)
		{
			BreakIfAttached();
			return new ObjectDisposedException(typeofDisposedObject?.FullName);
		}

		/// <summary>
		/// Throw this if the object is disposed.
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static ObjectDisposedException ObjectDisposed(
			[CanBeNull] Type typeofDisposedObject, [NotNull] string messageFormat, [CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new ObjectDisposedException(typeofDisposedObject?.FullName, FormatMessage(messageFormat, args));
		}

		/// <summary>
		/// Used to be thrown in places expected to be unreachable.
		/// </summary>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[StringFormatMethod("messageFormat")]
		[MustUseReturnValue]
		public static NotSupportedException Unreachable([NotNull] string messageFormat, [CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new NotSupportedException(FormatMessage(messageFormat, args));
		}
		#endregion
	}
}