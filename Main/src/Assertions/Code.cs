using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;

namespace CodeJam
{
	/// <summary>
	/// Assertions class.
	/// </summary>
	[PublicAPI]
	public static partial class Code
	{
		#region Argument validation
		/// <summary>
		/// Ensures that <paramref name="arg" /> != <c>null</c>
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNull<T>(
			[CanBeNull, NoEnumeration] T arg,
			[NotNull, InvokerParameterName] string argName) where T : class
		{
			if (arg == null)
				throw CodeExceptions.ArgumentNull(argName);
		}

		/// <summary>
		/// Ensures that <paramref name="arg" /> != <c>null</c>
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNull<T>(
			[CanBeNull] T? arg,
			[NotNull, InvokerParameterName] string argName) where T : struct
		{
			if (arg == null)
				throw CodeExceptions.ArgumentNull(argName);
		}

		/// <summary>
		/// Ensures that <paramref name="arg" /> is not null nor empty
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullNorEmpty(
			[CanBeNull] string arg,
			[NotNull, InvokerParameterName] string argName)
		{
			if (string.IsNullOrEmpty(arg))
				throw CodeExceptions.ArgumentNullOrEmpty(argName);
		}

		/// <summary>
		/// Ensures that <paramref name="arg" /> is not null nor white space
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullNorWhiteSpace(
			[CanBeNull] string arg,
			[NotNull, InvokerParameterName] string argName)
		{
			if (string.IsNullOrWhiteSpace(arg))
				throw CodeExceptions.ArgumentNullOrWhiteSpace(argName);
		}

		/// <summary>
		/// Assertion for the argument value
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void AssertArgument(
			bool condition,
			[NotNull, InvokerParameterName] string argName,
			[NotNull] string message)
		{
			if (!condition)
				throw CodeExceptions.Argument(argName, message);
		}

		/// <summary>
		/// Assertion for the argument value
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		public static void AssertArgument(
			bool condition,
			[NotNull, InvokerParameterName] string argName,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			if (!condition)
				throw CodeExceptions.Argument(argName, messageFormat, args);
		}
		#endregion

		#region Argument validation - in range
		/// <summary>
		/// Assertion for the argument in range
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void InRange(
			int value,
			[NotNull, InvokerParameterName] string argName,
			int fromValue,
			int toValue)
		{
			if (value < fromValue || value > toValue)
				throw CodeExceptions.ArgumentOutOfRange(argName, value, fromValue, toValue);
		}

		/// <summary>
		/// Assertion for the argument in range
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void InRange<T>(
			T value,
			[NotNull, InvokerParameterName] string argName,
			T fromValue,
			T toValue)
		{
			if (Operators<T>.LessThan(value, fromValue) || Operators<T>.GreaterThan(value, toValue))
				throw CodeExceptions.ArgumentOutOfRange(argName, value, fromValue, toValue);
		}
		#endregion

		#region Argument validation - valid index
		/// <summary>
		/// Assertion for index in range
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ValidIndex(
			int index,
			[NotNull, InvokerParameterName] string argName)
		{
			if (index < 0)
				throw CodeExceptions.IndexOutOfRange(argName, index, 0, int.MaxValue);
		}

		/// <summary>
		/// Assertion for index in range
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ValidIndex(
			int index,
			[NotNull, InvokerParameterName] string argName,
			int length)
		{
			if (index < 0 || index >= length)
				throw CodeExceptions.IndexOutOfRange(argName, index, 0, length);
		}

		/// <summary>
		/// Assertion for from-to index pair
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ValidIndexPair(
			int fromIndex,
			[NotNull, InvokerParameterName] string fromIndexName,
			int toIndex,
			[NotNull, InvokerParameterName] string toIndexName,
			int length)
		{
			ValidIndex(fromIndex, fromIndexName, length);

			if (toIndex < fromIndex || toIndex >= length)
				throw CodeExceptions.IndexOutOfRange(toIndexName, toIndex, fromIndex, length);
		}

		/// <summary>
		/// Assertion for startIndex-count pair
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ValidIndexAndCount(
			int startIndex,
			[NotNull, InvokerParameterName] string startIndexName,
			int count,
			[NotNull, InvokerParameterName] string countName,
			int length)
		{
			ValidIndex(startIndex, startIndexName, length);

			InRange(count, countName, 0, length - startIndex);
		}
		#endregion

		#region State validation
		/// <summary>
		/// State assertion
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void AssertState(
			bool condition,
			[NotNull] string message)
		{
			if (!condition)
				throw CodeExceptions.InvalidOperation(message);
		}

		/// <summary>
		/// State assertion
		/// </summary>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		public static void AssertState(
			bool condition,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			if (!condition)
				throw CodeExceptions.InvalidOperation(messageFormat, args);
		}
		#endregion
	}
}