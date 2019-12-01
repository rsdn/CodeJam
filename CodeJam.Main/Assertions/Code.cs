using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;
using CodeJam.Strings;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam
{
	/// <summary>Assertions class.</summary>
	[PublicAPI]
	public static partial class Code
	{
		#region Argument validation
		/// <summary>Ensures that <paramref name="arg"/> != <c>null</c></summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		[ContractAnnotation("arg: null => halt")]
		public static void NotNull<T>(
			[CanBeNull, NoEnumeration, AssertionCondition(AssertionConditionType.IS_NOT_NULL)] T arg,
			[NotNull, InvokerParameterName] string argName) where T : class
		{
			if (arg == null)
				throw CodeExceptions.ArgumentNull(argName);
		}

		/// <summary>Ensures that all items in <paramref name="arg"/> != <c>null</c></summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ItemNotNull<T>(
			[NotNull] IEnumerable<T> arg,
			[NotNull, InvokerParameterName] string argName) where T : class
		{
			foreach (var item in arg)
				if (item == null)
					throw CodeExceptions.ArgumentItemNull(argName);
		}

		/// <summary>Ensures that <paramref name="arg"/> and its all items != <c>null</c></summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullAndItemNotNull<T>(
			[CanBeNull, AssertionCondition(AssertionConditionType.IS_NOT_NULL)] IEnumerable<T> arg,
			[NotNull, InvokerParameterName] string argName) where T : class
		{
			if (arg == null)
				throw CodeExceptions.ArgumentNull(argName);
			ItemNotNull(arg, argName);
		}

		/// <summary>Ensures that <paramref name="arg"/> != <c>null</c></summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		[ContractAnnotation("arg: null => halt")]
		public static void NotNull<T>(
			[CanBeNull, AssertionCondition(AssertionConditionType.IS_NOT_NULL)] T? arg,
			[NotNull, InvokerParameterName] string argName) where T : struct
		{
			if (arg == null)
				throw CodeExceptions.ArgumentNull(argName);
		}

		/// <summary>
		/// Ensures that supplied enumerable is not empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">Enumerable.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotEmpty<T>([NotNull] IEnumerable<T> arg, [NotNull, InvokerParameterName] string argName)
		{
			using var en = arg.GetEnumerator();
			if (!en.MoveNext())
				throw CodeExceptions.ArgumentEmpty(argName);
		}

		/// <summary>
		/// Ensures that supplied collection is not empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">Collection.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotEmpty<T>([NotNull] ICollection<T> arg, [NotNull, InvokerParameterName] string argName)
		{
			if (arg.Count == 0)
				throw CodeExceptions.ArgumentEmpty(argName);
		}

		/// <summary>
		/// Ensures that supplied array is not empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">Array.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotEmpty<T>([NotNull] T[] arg, [NotNull, InvokerParameterName] string argName)
		{
			if (arg.Length == 0)
				throw CodeExceptions.ArgumentEmpty(argName);
		}

		/// <summary>Ensures that <paramref name="arg"/> is not null nor empty</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		[ContractAnnotation("arg:null => stop")]
		public static void NotNullNorEmpty(
			[CanBeNull, AssertionCondition(AssertionConditionType.IS_NOT_NULL)] string arg,
			[NotNull, InvokerParameterName] string argName)
		{
			if (string.IsNullOrEmpty(arg))
				throw CodeExceptions.ArgumentNullOrEmpty(argName);
		}

		/// <summary>Ensures that <paramref name="arg"/> is not null nor white space</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		[ContractAnnotation("arg:null => stop")]
		public static void NotNullNorWhiteSpace(
			[CanBeNull, AssertionCondition(AssertionConditionType.IS_NOT_NULL)] string arg,
			[NotNull, InvokerParameterName] string argName)
		{
			if (arg.IsNullOrWhiteSpace())
				throw CodeExceptions.ArgumentNullOrWhiteSpace(argName);
		}

		/// <summary>Assertion for the argument value</summary>
		/// <param name="condition">The condition to check</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="message">The message.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		[ContractAnnotation("condition: false => halt")]
		public static void AssertArgument(
			[AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition,
			[NotNull, InvokerParameterName] string argName,
			[NotNull] string message)
		{
			if (!condition)
				throw CodeExceptions.Argument(argName, message);
		}

		/// <summary>Assertion for the argument value</summary>
		/// <param name="condition">The condition to check</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		[ContractAnnotation("condition: false => halt")]
		public static void AssertArgument(
			[AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition,
			[NotNull, InvokerParameterName] string argName,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			if (!condition)
				throw CodeExceptions.Argument(argName, messageFormat, args);
		}
		#endregion

		#region Argument validation - in range
		/// <summary>Assertion for the argument in range</summary>
		/// <param name="value">The value.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="fromValue">From value (inclusive).</param>
		/// <param name="toValue">To value (inclusive).</param>
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

		/// <summary>Assertion for the argument in range</summary>
		/// <param name="value">The value.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="fromValue">From value (inclusive).</param>
		/// <param name="toValue">To value (inclusive).</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void InRange(
			double value,
			[NotNull, InvokerParameterName] string argName,
			double fromValue,
			double toValue)
		{
			// DONTTOUCH: handles the NaN values
			if (!(value >= fromValue && value <= toValue))
				throw CodeExceptions.ArgumentOutOfRange(argName, value, fromValue, toValue);
		}

		/// <summary>Assertion for the argument in range</summary>
		/// <typeparam name="T">Type of the value</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="fromValue">From value (inclusive).</param>
		/// <param name="toValue">To value (inclusive).</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void InRange<T>(
			[NotNull] T value,
			[NotNull, InvokerParameterName] string argName,
			[NotNull] T fromValue,
			[NotNull] T toValue)
		{
			// DONTTOUCH: handles the NaN values
			if (!(Operators<T>.GreaterThanOrEqual(value, fromValue) && Operators<T>.LessThanOrEqual(value, toValue)))
				throw CodeExceptions.ArgumentOutOfRange(argName, value, fromValue, toValue);
		}

		#endregion

		#region Argument validation - valid count
		/// <summary>Asserts if the passed value is not a valid count.</summary>
		/// <param name="count">The count value.</param>
		/// <param name="argName">The name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ValidCount(int count, [NotNull, InvokerParameterName] string argName)
			=> InRange(count, argName, 0, int.MaxValue);

		/// <summary>Asserts if the passed value is not a valid count.</summary>
		/// <param name="count">The count value.</param>
		/// <param name="argName">The name of the argument.</param>
		/// <param name="length">The length.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ValidCount(int count,
			[NotNull, InvokerParameterName] string argName,
			int length)
				=> InRange(count, argName, 0, length);
		#endregion

		#region Argument validation - valid index
		/// <summary>Assertion for index in range</summary>
		/// <param name="index">The index.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ValidIndex(
			int index,
			[NotNull, InvokerParameterName] string argName)
		{
			if (index < 0)
				throw CodeExceptions.IndexOutOfRange(argName, index, 0, int.MaxValue);
		}

		/// <summary>Assertion for index in range</summary>
		/// <param name="index">The index.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="length">The length.</param>
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

		/// <summary>Assertion for from-to index pair</summary>
		/// <param name="fromIndex">From index.</param>
		/// <param name="fromIndexName">Name of from index.</param>
		/// <param name="toIndex">To index.</param>
		/// <param name="toIndexName">Name of to index.</param>
		/// <param name="length">The length.</param>
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

		/// <summary>Assertion for startIndex-count pair</summary>
		/// <param name="startIndex">The start index.</param>
		/// <param name="startIndexName">Start name of the index.</param>
		/// <param name="count">The count.</param>
		/// <param name="countName">Name of the count.</param>
		/// <param name="length">The length.</param>
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
		/// <summary>State assertion</summary>
		/// <param name="condition">The condition to check</param>
		/// <param name="message">The message.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		[ContractAnnotation("condition: false => halt")]
		public static void AssertState(
			[AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition,
			[NotNull] string message)
		{
			if (!condition)
				throw CodeExceptions.InvalidOperation(message);
		}

		/// <summary>State assertion</summary>
		/// <param name="condition">The condition to check</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		[ContractAnnotation("condition: false => halt")]
		public static void AssertState(
			[AssertionCondition(AssertionConditionType.IS_TRUE)] bool condition,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			if (!condition)
				throw CodeExceptions.InvalidOperation(messageFormat, args);
		}
		#endregion

		#region Code consistency validation
		/// <summary>Asserts if the given condition is satisfied.</summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="message">The message.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		[ContractAnnotation("condition: true => halt")]
		public static void BugIf(
			[AssertionCondition(AssertionConditionType.IS_FALSE)] bool condition,
			[NotNull] string message)
		{
			if (condition)
				throw CodeExceptions.InvalidOperation(message);
		}

		/// <summary>Asserts if the given condition is satisfied.</summary>
		/// <param name="condition">The condition to check.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		[ContractAnnotation("condition: true => halt")]
		public static void BugIf(
			[AssertionCondition(AssertionConditionType.IS_FALSE)] bool condition,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			if (condition)
				throw CodeExceptions.InvalidOperation(messageFormat, args);
		}
		#endregion
	}
}