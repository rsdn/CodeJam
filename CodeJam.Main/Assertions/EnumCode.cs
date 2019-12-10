using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam
{
	/// <summary>Enum assertions class.</summary>
	[PublicAPI]
	public static class EnumCode
	{
		#region Defined
		/// <summary>Asserts that specified argument enum value is defined.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void Defined<TEnum>(
			TEnum value,
			[NotNull, InvokerParameterName] string argName)
			where TEnum : struct, Enum
		{
			if (!EnumHelper.IsDefined(value))
				throw EnumCodeExceptions.ArgumentNotDefined(argName, value);
		}

		/// <summary>Asserts that all bits of the flags combination are defined.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argFlags">The bitwise combinations of the flags to check.</param>
		/// <param name="argName">Name of the argument.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void FlagsDefined<TEnum>(
			TEnum argFlags,
			[NotNull, InvokerParameterName] string argName)
			where TEnum : struct, Enum
		{
			if (!EnumHelper.AreFlagsDefined(argFlags))
				throw EnumCodeExceptions.ArgumentNotDefined(argName, argFlags);
		}
		#endregion

		#region Flags
		/// <summary>Asserts that the specified argument flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="flag">The flag.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void FlagSet<TEnum>(
			TEnum value,
			[NotNull, InvokerParameterName] string argName,
			TEnum flag)
			where TEnum : struct, Enum
		{
			if (!value.IsFlagSet(flag))
				throw EnumCodeExceptions.ArgumentAnyFlagUnset(argName, value, flag);
		}

		/// <summary>Asserts that any bit from specified argument flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void AnyFlagUnset<TEnum>(
			TEnum value,
			[NotNull, InvokerParameterName] string argName,
			TEnum flags)
			where TEnum : struct, Enum
		{
			if (!value.IsAnyFlagUnset(flags))
				throw EnumCodeExceptions.ArgumentFlagSet(argName, value, flags);
		}

		/// <summary>Asserts that any bit from specified argument flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void AnyFlagSet<TEnum>(
			TEnum value,
			[NotNull, InvokerParameterName] string argName,
			TEnum flags)
			where TEnum : struct, Enum
		{
			if (!value.IsAnyFlagSet(flags))
				throw EnumCodeExceptions.ArgumentFlagUnset(argName, value, flags);
		}

		/// <summary>Asserts that the specified argument flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="flag">The flag.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void FlagUnset<TEnum>(
			TEnum value,
			[NotNull, InvokerParameterName] string argName,
			TEnum flag)
			where TEnum : struct, Enum
		{
			if (!value.IsFlagUnset(flag))
				throw EnumCodeExceptions.ArgumentAnyFlagSet(argName, value, flag);
		}
		#endregion

		#region StateFlags
		/// <summary>Asserts that the specified state flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <param name="message">The message.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void StateFlagSet<TEnum>(TEnum value, TEnum flag, [NotNull] string message)
			where TEnum : struct, Enum
		{
			if (!value.IsFlagSet(flag))
				throw CodeExceptions.InvalidOperation(message);
		}
		/// <summary>Asserts that the specified state flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		public static void StateFlagSet<TEnum>(
			TEnum value, TEnum flag,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
			where TEnum : struct, Enum
		{
			if (!value.IsFlagSet(flag))
				throw CodeExceptions.InvalidOperation(messageFormat, args);
		}

		/// <summary>Asserts that any bit from specified state flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <param name="message">The message.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void AnyStateFlagUnset<TEnum>(TEnum value, TEnum flags, [NotNull] string message)
			where TEnum : struct, Enum
		{
			if (!value.IsAnyFlagUnset(flags))
				throw CodeExceptions.InvalidOperation(message);
		}

		/// <summary>Asserts that any bit from specified state flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		public static void AnyStateFlagUnset<TEnum>(
			TEnum value, TEnum flags,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
			where TEnum : struct, Enum
		{
			if (!value.IsAnyFlagUnset(flags))
				throw CodeExceptions.InvalidOperation(messageFormat, args);
		}

		/// <summary>Asserts that any bit from specified state flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <param name="message">The message.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void AnyStateFlagSet<TEnum>(TEnum value, TEnum flags, [NotNull] string message)
			where TEnum : struct, Enum
		{
			if (!value.IsAnyFlagSet(flags))
				throw CodeExceptions.InvalidOperation(message);
		}

		/// <summary>Asserts that any bit from specified state flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		public static void AnyStateFlagSet<TEnum>(
			TEnum value, TEnum flags,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
			where TEnum : struct, Enum
		{
			if (!value.IsAnyFlagSet(flags))
				throw CodeExceptions.InvalidOperation(messageFormat, args);
		}

		/// <summary>Asserts that the specified state flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <param name="message">The message.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod]
		public static void StateFlagUnset<TEnum>(TEnum value, TEnum flag, [NotNull] string message)
			where TEnum : struct, Enum
		{
			if (!value.IsFlagUnset(flag))
				throw CodeExceptions.InvalidOperation(message);
		}

		/// <summary>Asserts that the specified state flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		[DebuggerHidden, MethodImpl(AggressiveInlining)]
		[AssertionMethod, StringFormatMethod("messageFormat")]
		public static void StateFlagUnset<TEnum>(
			TEnum value, TEnum flag,
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
			where TEnum : struct, Enum
		{
			if (!value.IsFlagUnset(flag))
				throw CodeExceptions.InvalidOperation(messageFormat, args);
		}
		#endregion
	}
}