using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using CodeJam.Arithmetic;
using CodeJam.Collections;
using CodeJam.Reflection;
// ReSharper disable once RedundantUsingDirective
using CodeJam.Targeting;

using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsExt;

#if !LESSTHAN_NET40
using EnumEx = System.Enum;
#endif

namespace CodeJam
{
	/// <summary>
	/// Extension methods for Enum types
	/// </summary>
	[PublicAPI]
	public static class EnumHelper
	{
#region Perf-critical metadata checks
		/// <summary>Determines whether the specified value is defined.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value to check.</param>
		/// <returns>True, if enum defines the value.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsDefined<TEnum>(TEnum value)
			where TEnum : struct, Enum =>
				Holder<TEnum>.IsDefined(value);

		/// <summary>Determines whether the string representation of value is defined.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">String representation of value to check.</param>
		/// <returns>True, if enum defines the value.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsDefined<TEnum>([NotNull] string value)
			where TEnum : struct, Enum =>
				TryParse<TEnum>(value, out var parsed) && Holder<TEnum>.IsDefined(parsed);

		/// <summary>Determines whether all bits of the flags combination are defined.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="flags">The flags to check.</param>
		/// <returns>True, if enum defines all bits of the flags combination.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool AreFlagsDefined<TEnum>(TEnum flags)
			where TEnum : struct, Enum =>
				Holder<TEnum>.AreFlagsDefined(flags);

		/// <summary>Determines whether the enum has flags modifier.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <returns>True, if the enum is flags enum</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsFlagsEnum<TEnum>()
			where TEnum : struct, Enum =>
				Holder<TEnum>.IsFlagsEnum;

		/// <summary>Returns a combination of all flags declared in the enum.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <returns>
		/// A combination of all flags declared in the enum, or <c>default(TEnum)</c> if <see cref="IsFlagsEnum{TEnum}"/> is false.
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public static TEnum GetFlagsMask<TEnum>()
			where TEnum : struct, Enum =>
				Holder<TEnum>.FlagsMask;

		/// <summary>Try to parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="result">The parsed value.</param>
		/// <returns><c>true</c>, if parsing was successful; <c>false</c> otherwise.</returns>
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
		[MethodImpl(AggressiveInlining)]
		public static bool TryParse<TEnum>([NotNull] string name, bool ignoreCase, out TEnum result)
			where TEnum : struct, Enum =>
				Holder<TEnum>.GetNameValues(ignoreCase).TryGetValue(name, out result) ||
					EnumEx.TryParse(name, ignoreCase, out result);

		/// <summary>Try to parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>Parsed value, if parsing was successful; <c>null</c> otherwise.</returns>
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
			if (Holder<TEnum>.GetNameValues(ignoreCase).TryGetValue(name, out var result))
				return result;
			return (TEnum)Enum.Parse(typeof(TEnum), name, ignoreCase);
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
				Holder<TEnum>.IsFlagSetCallback(value, flag);

		/// <summary>Determines whether any bit from specified flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns><c>true</c> if the value does not include all bits of the flag.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsAnyFlagUnset<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, Enum =>
				!Holder<TEnum>.IsFlagSetCallback(value, flag);

		/// <summary>Determines whether any bit from specified flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns><c>true</c> if the value includes any bit of the flags or the flag is zero.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsAnyFlagSet<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, Enum =>
				Holder<TEnum>.IsAnyFlagSetCallback(value, flags);

		/// <summary>Determines whether the specified flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns><c>true</c> if the value does not include any bit of the flags.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static bool IsFlagUnset<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, Enum =>
				!Holder<TEnum>.IsAnyFlagSetCallback(value, flags);
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
				Holder<TEnum>.SetFlagCallback(value, flag);

		/// <summary>Clears the flag.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>The bits of the value excluding the ones from the flag.</returns>
		[Pure]
		[MethodImpl(AggressiveInlining)]
		public static TEnum ClearFlag<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, Enum =>
				Holder<TEnum>.ClearFlagCallback(value, flag);

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

#region Enum values

		/// <summary>Returns a dictionary containing the enum names and their values.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>Returns a dictionary containing the enum names and their values.</returns>
		[Pure, NotNull]
		[MethodImpl(AggressiveInlining)]
		public static IReadOnlyDictionary<string, TEnum> GetNameValues<TEnum>(bool ignoreCase = false)
			where TEnum : struct, Enum =>
				Holder<TEnum>.GetNameValues(ignoreCase);

		/// <summary>
		/// Retrieves an array of the names of the constants in a specified enumeration.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>A string array of the names of the constants in enumType.</returns>
		[Pure, NotNull]
		public static string[] GetNames<TEnum>(bool ignoreCase = false)
			where TEnum : struct, Enum =>
				Holder<TEnum>.GetNameValues(ignoreCase).Keys.ToArray();

		/// <summary>
		/// Retrieves an array of the values of the constants in a specified enumeration.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <returns>An array that contains the values of the constants in enumType.</returns>
		[Pure, NotNull]
		public static TEnum[] GetValues<TEnum>()
			where TEnum : struct, Enum =>
				Holder<TEnum>.GetNameValues(false).Values.ToArray();
#endregion

#region DisplayName/Description
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


		/// <summary>Returns name of the enum value.</summary>
		/// <typeparam name="TEnum">Enum type</typeparam>
		/// <param name="value">The enum value.</param>
		/// <returns>The name of the enum value.</returns>
		[Pure, NotNull]
		[MethodImpl(AggressiveInlining)]
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
		[Pure, NotNull]
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
		[CanBeNull]
		public static string GetDescription<TEnum>(TEnum value)
			where TEnum : struct, Enum =>
				GetEnumValue(value).Description;


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

#region Holder struct
		[NotNull]
		private static readonly ILazyDictionary<Type, EnumValues> _enumValuesCache = LazyDictionary.Create(
			(Type enumType) => new EnumValues(enumType.ToNullableUnderlying()),
			LazyThreadSafetyMode.ExecutionAndPublication);

		private static class Holder<TEnum>
			where TEnum : struct, Enum
		{
#region Static fields
			[NotNull]
			private static readonly HashSet<TEnum> _values = new HashSet<TEnum>((TEnum[])Enum.GetValues(typeof(TEnum)));
			[NotNull]
			private static readonly IReadOnlyDictionary<string, TEnum> _nameValues = GetNameValuesCore(ignoreCase: false);
			[NotNull]
			private static readonly IReadOnlyDictionary<string, TEnum> _nameValuesIgnoreCase = GetNameValuesCore(ignoreCase: true);
#endregion

#region Init helpers
			[NotNull]
			private static IReadOnlyDictionary<string, TEnum> GetNameValuesCore(bool ignoreCase)
			{
				var result =
#if LESSTHAN_NET45
					new DictionaryEx<string, TEnum>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
#else
					new Dictionary<string, TEnum>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
#endif

				var names = Enum.GetNames(typeof(TEnum));
				var values = (TEnum[])Enum.GetValues(typeof(TEnum));
				for (var i = 0; i < names.Length; i++)
				{
					// DONTTOUCH: case may be ignored
					if (result.ContainsKey(names[i]))
						continue;

					result.Add(names[i], values[i]);
				}

				return result;
			}

			private static TEnum GetFlagsMaskCore()
			{
				var result = default(TEnum);

				if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() != null)
				{
					var values = (TEnum[])Enum.GetValues(typeof(TEnum));
					foreach (var value in values)
						result = Operators<TEnum>.BitwiseOr(result, value);
				}

				return result;
			}
#endregion

#region API
			public static bool IsFlagsEnum { get; } = typeof(TEnum).GetCustomAttribute<FlagsAttribute>() != null;
			public static TEnum FlagsMask { get; } = GetFlagsMaskCore();
			[NotNull]
			public static Func<TEnum, TEnum, bool> IsFlagSetCallback { get; } = OperatorsFactory.IsFlagSetOperator<TEnum>();
			[NotNull]
			public static Func<TEnum, TEnum, bool> IsAnyFlagSetCallback { get; } = OperatorsFactory.IsAnyFlagSetOperator<TEnum>();
			[NotNull]
			public static Func<TEnum, TEnum, TEnum> SetFlagCallback { get; } = OperatorsFactory.SetFlagOperator<TEnum>();
			[NotNull]
			public static Func<TEnum, TEnum, TEnum> ClearFlagCallback { get; } = OperatorsFactory.ClearFlagOperator<TEnum>();

			[NotNull]
			public static IReadOnlyDictionary<string, TEnum> GetNameValues(bool ignoreCase) =>
				ignoreCase ? _nameValuesIgnoreCase : _nameValues;

			[MethodImpl(AggressiveInlining)]
			public static bool IsDefined(TEnum value) => _values.Contains(value);

			[MethodImpl(AggressiveInlining)]
			public static bool AreFlagsDefined(TEnum flags) =>
				_values.Contains(flags) || IsFlagsEnum && IsFlagSetCallback(FlagsMask, flags);
#endregion
		}
#endregion
	}
}