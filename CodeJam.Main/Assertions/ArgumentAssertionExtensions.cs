using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam
{
	/// <summary>
	/// <see cref="ArgumentAssertion{T}"/> extension methods.
	/// </summary>
	[PublicAPI]
	public static class ArgumentAssertionExtensions
	{
		/// <summary>
		/// Ensures that <paramref name="arg"/> != <c>null</c>
		/// </summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <returns><paramref name="arg"/></returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static ArgumentAssertion<T> NotNull<T>([NoEnumeration] this ArgumentAssertion<T> arg) where T : class
		{
			Code.NotNull(arg.Argument, arg.ArgumentName);
			return arg;
		}

		/// <summary>
		/// Ensures that <paramref name="arg"/> != <c>null</c>
		/// </summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <returns><paramref name="arg"/></returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static ArgumentAssertion<T?> NotNull<T>([NoEnumeration] this ArgumentAssertion<T?> arg) where T : struct
		{
			Code.NotNull(arg.Argument, arg.ArgumentName);
			return arg;
		}

		/// <summary>Ensures that all items in <paramref name="arg"/> != <c>null</c></summary>
		/// <typeparam name="T">Type of the value. Auto-inferred in most cases</typeparam>
		/// <param name="arg">The argument.</param>
		/// <returns><paramref name="arg"/></returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static ArgumentAssertion<IEnumerable<T>> ItemNotNull<T>(this ArgumentAssertion<IEnumerable<T>> arg) where T : class
		{
			Code.ItemNotNull(arg.Argument, arg.ArgumentName);
			return arg;
		}

		/// <summary>
		/// Ensures that supplied enumerable is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">The argument.</param>
		/// <returns><paramref name="arg"/></returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static ArgumentAssertion<IEnumerable<T>> NotNullNorEmpty<T>(this ArgumentAssertion<IEnumerable<T>> arg)
		{
			Code.NotNullNorEmpty(arg.Argument, arg.ArgumentName);
			return arg;
		}

		/// <summary>
		/// Ensures that supplied collection is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">The argument.</param>
		/// <returns><paramref name="arg"/></returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static ArgumentAssertion<ICollection<T>> NotNullNorEmpty<T>(this ArgumentAssertion<ICollection<T>> arg)
		{
			Code.NotNullNorEmpty(arg.Argument, arg.ArgumentName);
			return arg;
		}

		/// <summary>
		/// Ensures that supplied array is not null nor empty.
		/// </summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="arg">The argument.</param>
		/// <returns><paramref name="arg"/></returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static ArgumentAssertion<T[]> NotNullNorEmpty<T>(this ArgumentAssertion<T[]> arg)
		{
			Code.NotNullNorEmpty(arg.Argument, arg.ArgumentName);
			return arg;
		}

		/// <summary>Ensures that <paramref name="arg"/> is not null nor empty</summary>
		/// <param name="arg">The argument.</param>
		/// <returns><paramref name="arg"/></returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static ArgumentAssertion<string> NotNullNorEmpty(this ArgumentAssertion<string> arg)
		{
			Code.NotNullNorEmpty(arg.Argument, arg.ArgumentName);
			return arg;
		}

		/// <summary>Ensures that <paramref name="arg"/> is not null nor white space</summary>
		/// <param name="arg">The argument.</param>
		/// <returns><paramref name="arg"/></returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static ArgumentAssertion<string> NotNullNorWhiteSpace(this ArgumentAssertion<string> arg)
		{
			Code.NotNullNorWhiteSpace(arg.Argument, arg.ArgumentName);
			return arg;
		}

		/// <summary>Assertion for the argument value</summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="arg">The argument.</param>
		/// <param name="condition">The condition to check</param>
		/// <param name="message">The message.</param>
		/// <returns><paramref name="arg"/></returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		[ContractAnnotation("condition: false => halt")]
		public static ArgumentAssertion<T> Assert<T>(
			this ArgumentAssertion<T> arg,
			bool condition,
			[NotNull] string message)
		{
			Code.AssertArgument(condition, arg.ArgumentName, message);
			return arg;
		}

		/// <summary>Assertion for the argument value</summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="arg">Argument.</param>
		/// <param name="condition">The condition to check</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">Format string arguments.</param>
		/// <returns><paramref name="arg"/></returns>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		[ContractAnnotation("condition: false => halt")]
		public static ArgumentAssertion<T> Assert<T>(
			this ArgumentAssertion<T> arg,
			bool condition,
			[NotNull] string messageFormat,
			params object[]? args)
		{
			Code.AssertArgument(condition, arg.ArgumentName, messageFormat, args);
			return arg;
		}
	}
}