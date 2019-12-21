using System;
using System.Diagnostics;

using JetBrains.Annotations;

using static CodeJam.Internal.CodeExceptionsHelper;

namespace CodeJam
{
	/// <summary>Enum exception factory class</summary>
	[PublicAPI]
	public static class EnumCodeExceptions
	{
		#region Defined
		/// <summary>Creates <see cref="ArgumentOutOfRangeException"/> for undefined enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <returns>Initialized instance of <see cref="ArgumentOutOfRangeException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static ArgumentOutOfRangeException ArgumentNotDefined<TEnum>(
			[NotNull, InvokerParameterName] string argumentName,
			TEnum value)
			where TEnum : struct, Enum
		{
			BreakIfAttached();
			var valueType = value.GetType();
			return new ArgumentOutOfRangeException(
				argumentName,
				value,
				Invariant($"Unexpected value '{value}' of type '{valueType.FullName}'."))
				.LogToCodeTraceSourceBeforeThrow();
		}
		#endregion

		#region Flags
		/// <summary>Creates <see cref="ArgumentException"/> for flag is set case.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static ArgumentException ArgumentFlagSet<TEnum>(
			[NotNull, InvokerParameterName] string argumentName,
			TEnum value, TEnum flag)
			where TEnum : struct, Enum
		{
			BreakIfAttached();
			return new ArgumentException(
				Invariant($"The value of the {argumentName} argument ('{value}') should not include flag '{flag}'."),
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for any bit from flag is not set case.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static ArgumentException ArgumentAnyFlagUnset<TEnum>(
			[NotNull, InvokerParameterName] string argumentName,
			TEnum value, TEnum flags)
			where TEnum : struct, Enum
		{
			BreakIfAttached();
			return new ArgumentException(
				Invariant($"The value of the {argumentName} argument ('{value}') should include flag '{flags}'."),
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for any bit from flag is set case.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static ArgumentException ArgumentAnyFlagSet<TEnum>(
			[NotNull, InvokerParameterName] string argumentName,
			TEnum value, TEnum flags)
			where TEnum : struct, Enum
		{
			BreakIfAttached();
			return new ArgumentException(
				Invariant($"The value of the {argumentName} argument ('{value}') should not include any flag from '{flags}'."),
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for flag is not set case.</summary>
		/// <typeparam name="TEnum">The type of the enum value.</typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static ArgumentException ArgumentFlagUnset<TEnum>(
			[NotNull, InvokerParameterName] string argumentName,
			TEnum value, TEnum flag)
			where TEnum : struct, Enum
		{
			BreakIfAttached();
			return new ArgumentException(
				Invariant($"The value of the {argumentName} argument ('{value}') should include any flag from '{flag}'."),
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}
		#endregion
	}
}