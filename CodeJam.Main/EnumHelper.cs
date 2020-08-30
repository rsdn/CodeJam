using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using CodeJam.Collections;
using CodeJam.Reflection;
// ReSharper disable once RedundantUsingDirective
using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
using EnumEx = System.Enum;
#else
using EnumEx = System.EnumEx;
#endif

namespace CodeJam
{
	/// <summary>
	/// Extension methods for Enum types
	/// </summary>
	[PublicAPI]
	public static partial class EnumHelper
	{
		[NotNull]
		private static readonly ILazyDictionary<Type, EnumValues> _enumValuesCache = LazyDictionary.Create(
			(Type enumType) => new EnumValues(enumType.ToNullableUnderlying()),
			LazyThreadSafetyMode.ExecutionAndPublication);

		#region Enum values
		/// <summary>
		/// Retrieves an array of the values of the constants in a specified enumeration.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <returns>An array that contains the values of the constants in enumType.</returns>
		[Pure, NotNull]
		public static TEnum[] GetValues<TEnum>()
			where TEnum : struct, Enum =>
				MetaHolder<TEnum>.GetNameValues(false).Values.ToArray();

		/// <summary>
		/// Retrieves an array of the names of the constants in a specified enumeration.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>A string array of the names of the constants in enumType.</returns>
		[Pure, NotNull]
		public static string[] GetNames<TEnum>(bool ignoreCase = false)
			where TEnum : struct, Enum =>
				MetaHolder<TEnum>.GetNameValues(ignoreCase).Keys.ToArray();

		/// <summary>Returns a dictionary containing the enum names and their values.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>Returns a dictionary containing the enum names and their values.</returns>
		[Pure, NotNull]
		public static IReadOnlyDictionary<string, TEnum> GetNameValues<TEnum>(bool ignoreCase = false)
			where TEnum : struct, Enum =>
				MetaHolder<TEnum>.GetNameValues(ignoreCase);

		/// <summary>Gets enum values collection that contains information about enum type and its values.</summary>
		/// <typeparam name="TEnum">Enum type</typeparam>
		/// <returns>The enum values collection.</returns>
		[Pure, NotNull]
		public static EnumValues GetEnumValues<TEnum>()
			where TEnum : struct, Enum =>
			_enumValuesCache[typeof(TEnum)];

		/// <summary>Returns enum values collection that contains information about enum type and its values.</summary>
		/// <param name="enumType">Type of the enum.</param>
		/// <returns>The enum values collection.</returns>
		[Pure, NotNull]
		public static EnumValues GetEnumValues([NotNull] Type enumType) => _enumValuesCache[enumType];

		/// <summary>Gets metadata about enum value.</summary>
		/// <typeparam name="TEnum">Enum type</typeparam>
		/// <param name="value">Enum value.</param>
		/// <returns>The enum values collection.</returns>
		[Pure, NotNull]
		public static EnumValue GetEnumValue<TEnum>(TEnum value)
			where TEnum : struct, Enum => GetEnumValues<TEnum>().GetByValue(value);

		/// <summary>
		/// Gets field info of enum value.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The enum value.</param>
		/// <returns><see cref="FieldInfo"/> corresponding to <paramref name="value"/>.</returns>
		[Pure, NotNull]
		public static FieldInfo GetField<TEnum>(TEnum value)
			where TEnum : struct, Enum =>
				GetEnumValue(value).UnderlyingField;
		#endregion

		#region DisplayName/Description
		/// <summary>Returns name of the enum value.</summary>
		/// <typeparam name="TEnum">Enum type</typeparam>
		/// <param name="value">The enum value.</param>
		/// <returns>The name of the enum value.</returns>
		[Pure, NotNull]
		public static string GetName<TEnum>(TEnum value)
			where TEnum : struct, Enum =>
				GetEnumValue(value).Name;

		/// <summary>
		/// Returns description of enum value.
		/// </summary>
		/// <typeparam name="TEnum">Enum type</typeparam>
		/// <param name="value">Enum value.</param>
		/// <returns>Enum value display name.</returns>
		/// <remarks>
		/// Returns description of enum value specified by <see cref="DisplayAttribute"/>, or <c>null</c> if no attribute
		/// specified.
		/// </remarks>
		[Pure, CanBeNull]
		public static string GetDisplayName<TEnum>(TEnum value)
			where TEnum : struct, Enum =>
				GetEnumValue(value).GetDisplayName();

		/// <summary>
		/// Returns description of enum value.
		/// </summary>
		/// <typeparam name="TEnum">Enum type</typeparam>
		/// <param name="value">Enum value.</param>
		/// <returns>Enum value description.</returns>
		/// <remarks>
		/// Returns description of enum value specified by <see cref="DisplayAttribute"/>, or <c>null</c> if no attribute
		/// specified.
		/// </remarks>
		[Pure]
		public static string? GetDescription<TEnum>(TEnum value)
			where TEnum : struct, Enum =>
				GetEnumValue(value).Description;
		#endregion

		#region Perf-critical metadata checks
		/// <summary>Try to parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="result">The parsed value.</param>
		/// <returns><c>true</c>, if parsing was successful; <c>false</c> otherwise.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool TryParse<TEnum>([NotNull] string name, out TEnum result)
			where TEnum : struct, Enum =>
				TryParse(name, false, out result);

		/// <summary>Try to parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <param name="result">The parsed value.</param>
		/// <returns><c>true</c>, if parsing was successful; <c>false</c> otherwise.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool TryParse<TEnum>([NotNull] string name, bool ignoreCase, out TEnum result)
			where TEnum : struct, Enum =>
				MetaHolder<TEnum>.GetNameValues(ignoreCase).TryGetValue(name, out result) ||
					EnumEx.TryParse(name, ignoreCase, out result);

		/// <summary>Try to parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>Parsed value, if parsing was successful; <c>null</c> otherwise.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static TEnum? TryParse<TEnum>([NotNull] string name, bool ignoreCase = false)
			where TEnum : struct, Enum =>
				TryParse(name, ignoreCase, out TEnum result) ? result : (TEnum?)null;

		/// <summary>Parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>Parsed value.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static TEnum Parse<TEnum>([NotNull] string name, bool ignoreCase = false)
			where TEnum : struct, Enum
		{
			if (MetaHolder<TEnum>.GetNameValues(ignoreCase).TryGetValue(name, out var result))
				return result;
			return (TEnum)Enum.Parse(typeof(TEnum), name, ignoreCase);
		}

		/// <summary>Determines whether the enum has flags modifier.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <returns>True, if the enum is flags enum</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsFlagsEnum<TEnum>()
			where TEnum : struct, Enum =>
				MetaHolder<TEnum>.IsFlagsEnum;

		/// <summary>Returns a bitwise combination of all values declared in the enum.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <returns>A bitwise combination of all values declared in the enum. </returns>
		[MethodImpl(AggressiveInlining)]
		public static TEnum GetValuesMask<TEnum>()
			where TEnum : struct, Enum =>
				MetaHolder<TEnum>.ValuesMask;

		/// <summary>Determines whether the specified value is defined.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value to check.</param>
		/// <returns>True, if enum defines the value.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsDefined<TEnum>(TEnum value)
			where TEnum : struct, Enum =>
				MetaHolder<TEnum>.ValuesSet.Contains(value);

		/// <summary>Determines whether the string representation of value is defined.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">String representation of value to check.</param>
		/// <returns>True, if enum defines the value.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsDefined<TEnum>([NotNull] string value)
			where TEnum : struct, Enum =>
				TryParse<TEnum>(value, out var parsed) && IsDefined(parsed);

		/// <summary>Determines whether all bits of the flags combination are defined.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="flags">The flags to check.</param>
		/// <returns>True, if enum defines all bits of the flags combination.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool AreFlagsDefined<TEnum>(TEnum flags)
			where TEnum : struct, Enum =>
				IsDefined(flags)
				|| MetaHolder<TEnum>.IsFlagsEnum && OpHolder<TEnum>.IsFlagSetCallback(MetaHolder<TEnum>.ValuesMask, flags);

		/// <summary>Determines whether all bits of the flags combination are defined.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">String representation of value to check.</param>
		/// <returns>True, if enum defines the value.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool AreFlagsDefined<TEnum>([NotNull] string value)
			where TEnum : struct, Enum =>
				TryParse<TEnum>(value, out var parsed) && AreFlagsDefined(parsed);

		/// <summary>Returns all defined enum values that match specified flag mask.</summary>
		/// <remarks>
		/// This method will include all matching flags excluding duplicates
		/// <code>
		/// [Flags] enum Samples { None = 0x0, A = 0x1, A2 = 0x1, B = 0x2, BX = B | 0x10, All = A|BX }
		/// var flags = Samples.All.GetDefinedFlags(); // A, B, BX, All
		/// </code>
		/// </remarks>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>All declared values that match specified flag mask.</returns>
		[Pure, NotNull]
		public static TEnum[] GetDefinedFlags<TEnum>(TEnum value)
			where TEnum : struct, Enum
		{
			var equals = OpHolder<TEnum>.AreEqualCallback;
			if (!MetaHolder<TEnum>.IsFlagsEnum)
			{
				return equals(value, default) || !IsDefined(value)
					? Array<TEnum>.Empty
					: new[] { value };
			}

			var definedFlags = MetaHolder<TEnum>.NonDefaultFlags;
			var result = new List<TEnum>(definedFlags.Length);
			foreach (var flag in definedFlags)
			{
				if (equals(flag, default))
					continue;

				if (value.IsFlagSet(flag))
					result.Add(flag);
			}
			return result.ToArray();
		}

		/// <summary>Returns combination of flags that represent specified enum value.</summary>
		/// <remarks>
		/// This method will not include duplicates of flag combinations
		/// <code>
		/// [Flags] enum Samples { None = 0x0, A = 0x1, A2 = 0x1, B = 0x2, BX = B | 0x10, All = A|BX }
		/// var flags = Samples.All.ToFlags(); // A, B, BX
		/// </code>
		/// </remarks>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <returns>Combination of flags that represents specified enum value.</returns>
		[Pure, NotNull]
		public static TEnum[] ToFlags<TEnum>(this TEnum value)
			where TEnum : struct, Enum
		{
			var equals = OpHolder<TEnum>.AreEqualCallback;
			if (!MetaHolder<TEnum>.IsFlagsEnum)
			{
				return equals(value, default) || !IsDefined(value)
					? Array<TEnum>.Empty
					: new[] { value };
			}

			var uniqueFlags = MetaHolder<TEnum>.NonDefaultUniqueFlags;
			var result = new List<TEnum>(uniqueFlags.Length);
			foreach (var flag in uniqueFlags)
			{
				if (value.IsFlagSet(flag))
					result.Add(flag);
			}
			return result.ToArray();
		}
		#endregion

		#region Flag checks
		/// <summary>Determines whether the specified flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns><c>true</c> if the value includes all bits of the flag or the flag is zero.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsFlagSet<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, Enum =>
				OpHolder<TEnum>.IsFlagSetCallback(value, flag);

		/// <summary>Determines whether any bit from specified flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns><c>true</c> if the value does not include all bits of the flag.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsAnyFlagUnset<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, Enum =>
				!OpHolder<TEnum>.IsFlagSetCallback(value, flag);

		/// <summary>Determines whether any bit from specified flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns><c>true</c> if the value includes any bit of the flags or the flag is zero.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsAnyFlagSet<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, Enum =>
				OpHolder<TEnum>.IsAnyFlagSetCallback(value, flags);

		/// <summary>Determines whether the specified flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns><c>true</c> if the value does not include any bit of the flags.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsFlagUnset<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, Enum =>
				!OpHolder<TEnum>.IsAnyFlagSetCallback(value, flags);
		#endregion

		#region Flag operations
		/// <summary>Sets the flag.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>Bitwise combination of the flag and the value</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static TEnum SetFlag<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, Enum =>
				OpHolder<TEnum>.SetFlagCallback(value, flag);

		/// <summary>Clears the flag.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>The bits of the value excluding the ones from the flag.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static TEnum ClearFlag<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, Enum =>
				OpHolder<TEnum>.ClearFlagCallback(value, flag);

		/// <summary>Sets or clears the flag depending on <paramref name="enabled"/> value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <param name="enabled">Determines whether the flag should be set or cleared.</param>
		/// <returns>
		/// Bitwise combination of the flag and the value if the <paramref name="enabled"/> is <c>true</c>;
		/// otherwise, the result is the bits of the value excluding the ones from the flag.
		/// </returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static TEnum SetFlag<TEnum>(this TEnum value, TEnum flag, bool enabled)
			where TEnum : struct, Enum =>
				enabled
					? SetFlag(value, flag)
					: ClearFlag(value, flag);
		#endregion
	}
}