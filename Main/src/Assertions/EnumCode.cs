using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;

namespace CodeJam
{
	/// <summary>Enum assertions class.</summary>
	[PublicAPI]
	public static class EnumCode
	{
		#region Defined
		/// <summary>Asserts that specified enum value is defined.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void Defined<TEnum>(
			TEnum value,
			[NotNull, InvokerParameterName] string argName)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!EnumHelper.IsDefined(value))
				throw EnumCodeExceptions.ArgumentNotDefinedException(argName, value);
		}

		/// <summary>Asserts that all bits of the flags combination are defined.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="flags">The flags to check.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void FlagsDefined<TEnum>(
			TEnum flags,
			[NotNull, InvokerParameterName] string argName)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!EnumHelper.AreFlagsDefined(flags))
				throw EnumCodeExceptions.ArgumentNotDefinedException(argName, flags);
		}
		#endregion

		#region Flags
		/// <summary>Asserts that the specified flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="flag">The flag.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void FlagSet<TEnum>(
			TEnum value,
			[NotNull, InvokerParameterName] string argName,
			TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!value.IsFlagSet(flag))
				throw EnumCodeExceptions.ArgumentAnyFlagUnset(argName, value, flag);
		}

		/// <summary>Asserts that any bit from specified flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="flag">The flag.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void AnyFlagUnset<TEnum>(
			TEnum value,
			[NotNull, InvokerParameterName] string argName,
			TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!value.IsAnyFlagUnset(flag))
				throw EnumCodeExceptions.ArgumentFlagSet(argName, value, flag);
		}

		/// <summary>Asserts that any bit from specified flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void AnyFlagSet<TEnum>(
			TEnum value,
			[NotNull, InvokerParameterName] string argName,
			TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!value.IsAnyFlagSet(flags))
				throw EnumCodeExceptions.ArgumentFlagUnset(argName, value, flags);
		}

		/// <summary>Asserts that the specified flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void FlagUnset<TEnum>(
			TEnum value,
			[NotNull, InvokerParameterName] string argName,
			TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (!value.IsFlagUnset(flags))
				throw EnumCodeExceptions.ArgumentAnyFlagSet(argName, value, flags);
		}
		#endregion
	}
}
