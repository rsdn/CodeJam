using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using SDC = System.Diagnostics.CodeAnalysis;

using System.Linq;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;
using CodeJam.Strings;

using JetBrains.Annotations;

using Microsoft.VisualStudio;

using static CodeJam.Targeting.MethodImplOptionsEx;

// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess

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
		public static void NotNull<T>(
			[SDC.NotNull, NoEnumeration, ValidatedNotNull] T? arg,
			[InvokerParameterName] string argName)
			where T : class
		{
			if (arg == null)
				throw CodeExceptions.ArgumentNull(argName);
		}

		/// <summary>Ensures that <paramref name="arg"/> != <c>null</c></summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <remarks>
		/// This version enables not-null assertions for generic methods without
		/// <code>where T : class</code> constraint.
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void GenericNotNull<T>(
			[SDC.NotNull, NoEnumeration, ValidatedNotNull] T? arg,
			[InvokerParameterName] string argName)
		{
			if (arg == null)
				throw CodeExceptions.ArgumentNull(argName);
		}

		/// <summary>Ensures that <paramref name="arg" /> != <see cref="Guid.Empty" />.</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">The name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotDefault(Guid arg, [InvokerParameterName] string argName)
		{
			if (arg == Guid.Empty)
				throw CodeExceptions.ArgumentDefault(argName, typeof(Guid));
		}

		/// <summary>Ensures that <paramref name="arg"/> != default(<typeparamref name="T"/>)</summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <remarks>
		/// This version enables not-default assertions for generic methods
		/// </remarks>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void GenericNotDefault<T>(
			[NoEnumeration] T? arg,
			[InvokerParameterName] string argName)
		{
			if (Operators<T>.AreEqual(arg, default))
				throw CodeExceptions.ArgumentDefault(argName, typeof(T));
		}

		/// <summary>Ensures that <paramref name="arg"/> != <c>null</c></summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNull<T>(
			[SDC.NotNull, ValidatedNotNull] T? arg,
			[InvokerParameterName] string argName) where T : struct
		{
			if (arg == null)
				throw CodeExceptions.ArgumentNull(argName);
		}

		/// <summary>
		/// Ensures that supplied enumerable is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">Enumerable.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullNorEmpty<T>(
			[SDC.NotNull, InstantHandle, ValidatedNotNull] IEnumerable<T>? arg,
			[InvokerParameterName] string argName)
		{
			NotNull(arg, argName);
			if (!arg.Any())
				throw CodeExceptions.ArgumentEmpty(argName);
		}

		/// <summary>
		/// Ensures that supplied collection is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">Collection.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullNorEmpty<T>(
			[SDC.NotNull, ValidatedNotNull] ICollection<T>? arg,
			[InvokerParameterName] string argName)
		{
			NotNull(arg, argName);
			if (arg.Count == 0)
				throw CodeExceptions.ArgumentEmpty(argName);
		}

		/// <summary>
		/// Ensures that supplied array is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">Array.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullNorEmpty<T>(
			[SDC.NotNull, ValidatedNotNull] T?[]? arg,
			[InvokerParameterName] string argName)
		{
			NotNull(arg, argName);
			if (arg.Length == 0)
				throw CodeExceptions.ArgumentEmpty(argName);
		}

		/// <summary>Ensures that <paramref name="arg"/> is not null nor empty</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullNorEmpty(
			[SDC.NotNull, ValidatedNotNull] string? arg,
			[InvokerParameterName] string argName)
		{
			if (arg.IsNullOrEmpty())
				throw CodeExceptions.ArgumentNullOrEmpty(argName);
		}

		/// <summary>Ensures that <paramref name="arg"/> is not null nor white space</summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullNorWhiteSpace(
			[SDC.NotNull, ValidatedNotNull] string? arg,
			[InvokerParameterName] string argName)
		{
			if (arg.IsNullOrWhiteSpace())
				throw CodeExceptions.ArgumentNullOrWhiteSpace(argName);
		}

		/// <summary>Ensures that <paramref name="arg"/> and its all items != <c>null</c></summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullAndItemNotNull<T>(
			[SDC.NotNull, InstantHandle, ValidatedNotNull] IEnumerable<T?>? arg,
			[InvokerParameterName] string argName) where T : class
		{
			NotNull(arg, argName);
			ItemNotNull(arg, argName);
		}

		/// <summary>
		/// Ensures that supplied enumerable is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">Enumerable.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullNorEmptyAndItemNotNull<T>(
			[SDC.NotNull, InstantHandle, ValidatedNotNull] IEnumerable<T?>? arg,
			[InvokerParameterName] string argName)
			where T : class
		{
			NotNull(arg, argName);

			var hasItems = false;

			foreach (var item in arg)
			{
				if (item == null)
					throw CodeExceptions.ArgumentItemNull(argName);
				hasItems = true;
			}

			if (!hasItems)
				throw CodeExceptions.ArgumentEmpty(argName);
		}

		/// <summary>
		/// Ensures that supplied collection is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">Collection.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullNorEmptyAndItemNotNull<T>(
			[SDC.NotNull, ValidatedNotNull] ICollection<T?>? arg,
			[InvokerParameterName] string argName)
			where T : class
		{
			NotNullNorEmpty(arg, argName);
			ItemNotNull(arg, argName);
		}

		/// <summary>
		/// Ensures that supplied array is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">Array.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void NotNullNorEmptyAndItemNotNull<T>(
			[SDC.NotNull, ValidatedNotNull] T?[]? arg,
			[InvokerParameterName] string argName)
			where T : class
		{
			NotNullNorEmpty(arg, argName);
			ItemNotNull(arg, argName);
		}

		/// <summary>Ensures that all items in <paramref name="arg"/> != <c>null</c></summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ItemNotNull<T>(
			[InstantHandle] IEnumerable<T?> arg,
			[InvokerParameterName] string argName) where T : class
		{
			NotNull(arg, argName);

			foreach (var item in arg)
				if (item == null)
					throw CodeExceptions.ArgumentItemNull(argName);
		}

		/// <summary>Assertion for the argument value</summary>
		/// <param name="condition">The condition to check</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="message">The message.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void AssertArgument(
			[DoesNotReturnIf(false)] bool condition,
			[InvokerParameterName] string argName,
			string message)
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
		public static void AssertArgument(
			[DoesNotReturnIf(false)] bool condition,
			[InvokerParameterName] string argName,
			string messageFormat,
			params object[]? args)
		{
			if (!condition)
				throw CodeExceptions.Argument(argName, messageFormat, args);
		}

		/// <summary>Ensures that <paramref name="arg"/> != <c>null</c></summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static T ReturnIfNotNull<T>(
			[SDC.NotNull, NoEnumeration, ValidatedNotNull] T? arg,
			[InvokerParameterName] string argName)
			where T : class
		{
			NotNull(arg, argName);
			return arg;
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
			[InvokerParameterName] string argName,
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
			[InvokerParameterName] string argName,
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
			[DisallowNull] T value,
			[InvokerParameterName] string argName,
			[DisallowNull] T fromValue,
			[DisallowNull] T toValue)
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
		public static void ValidCount(int count, [InvokerParameterName] string argName) =>
			InRange(count, argName, 0, int.MaxValue);

		/// <summary>Asserts if the passed value is not a valid count.</summary>
		/// <param name="count">The count value.</param>
		/// <param name="argName">The name of the argument.</param>
		/// <param name="length">The length.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ValidCount(
			int count,
			[InvokerParameterName] string argName,
			int length) =>
				InRange(count, argName, 0, length);
		#endregion

		#region Argument validation - valid index
		/// <summary>Assertion for index in range</summary>
		/// <param name="index">The index.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void ValidIndex(
			int index,
			[InvokerParameterName] string argName)
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
			[InvokerParameterName] string argName,
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
			[InvokerParameterName] string fromIndexName,
			int toIndex,
			[InvokerParameterName] string toIndexName,
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
			[InvokerParameterName] string startIndexName,
			int count,
			[InvokerParameterName] string countName,
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
		public static void AssertState(
			[DoesNotReturnIf(false)] bool condition,
			string message)
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
		public static void AssertState(
			[DoesNotReturnIf(false)] bool condition,
			string messageFormat,
			params object[]? args)
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
			[DoesNotReturnIf(true)] bool condition,
			string message)
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
		public static void BugIf(
			[DoesNotReturnIf(true)] bool condition,
			string messageFormat,
			params object[]? args)
		{
			if (condition)
				throw CodeExceptions.InvalidOperation(messageFormat, args);
		}
		#endregion
	}
}