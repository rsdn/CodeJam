using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.CodeExceptions;
using static CodeJam.PlatformDependent;

namespace CodeJam
{
	/// <summary>Enum exception factory class</summary>
	[PublicAPI]
	public static class EnumCodeExceptions
	{
		#region Defined
		/// <summary>Creates <seealso cref="ArgumentOutOfRangeException"/> for undefined enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <returns>Initialized instance of <seealso cref="ArgumentOutOfRangeException"/></returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentNotDefinedException<TEnum>(
			[NotNull, InvokerParameterName] string argumentName,
			TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			BreakIfAttached();
			var valueType = value.GetType();
			return new ArgumentOutOfRangeException(
				argumentName, value, $"Unexpected value '{value}' of type '{valueType.FullName}'.");
		}
		#endregion

		#region Flags
		/// <summary>Creates <seealso cref="ArgumentException"/> for flag is set case.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>Initialized instance of <seealso cref="ArgumentException"/></returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentException ArgumentFlagSet<TEnum>(
			[NotNull, InvokerParameterName] string argumentName,
			TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			BreakIfAttached();
			return new ArgumentException(
				$"The value of the {argumentName} argument ('{value}') should not include flag '{flag}'.", argumentName);
		}

		/// <summary>Creates <seealso cref="ArgumentException"/> for any bit from flag is not set case.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns>Initialized instance of <seealso cref="ArgumentException"/></returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentException ArgumentAnyFlagUnset<TEnum>(
			[NotNull, InvokerParameterName] string argumentName,
			TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			BreakIfAttached();
			return new ArgumentException(
				$"The value of the {argumentName} argument ('{value}') should include flag '{flags}'.", argumentName);
		}

		/// <summary>Creates <seealso cref="ArgumentException"/> for any bit from flag is set case.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns>Initialized instance of <seealso cref="ArgumentException"/></returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentException ArgumentAnyFlagSet<TEnum>(
			[NotNull, InvokerParameterName] string argumentName,
			TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			BreakIfAttached();
			return new ArgumentException(
				$"The value of the {argumentName} argument ('{value}') should not include any flag from '{flags}'.", argumentName);
		}

		/// <summary>Creates <seealso cref="ArgumentException"/> for flag is not set case.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>Initialized instance of <seealso cref="ArgumentException"/></returns>
		[DebuggerHidden, NotNull, MethodImpl(AggressiveInlining)]
		[MustUseReturnValue]
		public static ArgumentException ArgumentFlagUnset<TEnum>(
			[NotNull, InvokerParameterName] string argumentName,
			TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			BreakIfAttached();
			return new ArgumentException(
				$"The value of the {argumentName} argument ('{value}') should include any flag from '{flag}'.", argumentName);
		}
		#endregion
	}
}