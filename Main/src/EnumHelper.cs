using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;
using CodeJam.Collections;
using CodeJam.Reflection;

using JetBrains.Annotations;

using static CodeJam.PlatformDependent;

using EnumClass =
#if FW35
	CodeJam.Targeting.EnumTargeting
#else
	System.Enum
#endif
	;

namespace CodeJam
{
	/// <summary>
	/// Extension methods for Enum types
	/// </summary>
	[PublicAPI]
	public static class EnumHelper
	{
		private static class Holder<TEnum>
			where TEnum : struct
		{
			#region Static fields
			// DONTTOUCH: the ordering of the fields represents the dependencies between them.
			// DO NOT change it until the initialization logic is changed.

			private static readonly bool _isEnum = typeof(TEnum).IsEnum;
			private static readonly bool _isNullableEnum = typeof(TEnum).IsNullableEnum();

			private static readonly Type _enumType = typeof(TEnum).ToNullableUnderlying();

			// ReSharper disable once StaticMemberInGenericType
			private static readonly bool _isFlagsEnum =
				(_isEnum || _isNullableEnum) &&
					_enumType.GetCustomAttribute<FlagsAttribute>() != null;

			private static readonly HashSet<TEnum> _values = _enumType.IsEnum
				? new HashSet<TEnum>((TEnum[])Enum.GetValues(_enumType))
				: new HashSet<TEnum>();

			private static readonly IReadOnlyDictionary<string, TEnum> _nameValues = GetNameValuesCore(_enumType, false);
			private static readonly IReadOnlyDictionary<string, TEnum> _nameValuesIgnoreCase = GetNameValuesCore(_enumType, true);
			private static readonly IReadOnlyDictionary<TEnum, string> _valueNames = GetValueNamesCore(_enumType);

			private static readonly TEnum _flagsMask = _isFlagsEnum ? GetFlagsMaskCore(_values.ToArray()) : default(TEnum);
			#endregion

			#region Flag operations emit
			[CanBeNull]
			private static readonly Func<TEnum, TEnum, bool> _isFlagSetCallback = _isEnum || _isNullableEnum
				? OperatorsFactory.IsFlagSetOperator<TEnum>()
				: null;

			[CanBeNull]
			private static readonly Func<TEnum, TEnum, bool> _isAnyFlagSetCallback = _isEnum || _isNullableEnum
				? OperatorsFactory.IsAnyFlagSetOperator<TEnum>()
				: null;

			[CanBeNull]
			private static readonly Func<TEnum, TEnum, TEnum> _setFlagCallback = _isEnum || _isNullableEnum
				? OperatorsFactory.SetFlagOperator<TEnum>()
				: null;

			[CanBeNull]
			private static readonly Func<TEnum, TEnum, TEnum> _clearFlagCallback = _isEnum || _isNullableEnum
				? OperatorsFactory.ClearFlagOperator<TEnum>()
				: null;
			#endregion

			#region Init helpers
			private static IReadOnlyDictionary<string, TEnum> GetNameValuesCore(Type enumType, bool ignoreCase)
			{
				var result =
#if FW40
					new DictionaryWithReadOnly<string, TEnum>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
#else
					new Dictionary<string, TEnum>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
#endif

				if (enumType.IsEnum)
				{
					var names = Enum.GetNames(enumType);
					var values = (TEnum[])Enum.GetValues(enumType);
					for (var i = 0; i < names.Length; i++)
					{
						// DONTTOUCH: case may be ignored
						if (result.ContainsKey(names[i]))
							continue;

						result.Add(names[i], values[i]);
					}
				}
				return result;
			}

			private static IReadOnlyDictionary<TEnum, string> GetValueNamesCore(Type enumType)
			{
				var result =
#if FW40
					new DictionaryWithReadOnly<TEnum, string>();
#else
					new Dictionary<TEnum, string>();
#endif

				if (enumType.IsEnum)
				{
					var names = Enum.GetNames(enumType);
					var values = (TEnum[])Enum.GetValues(enumType);
					for (var i = 0; i < names.Length; i++)
					{
						if (result.ContainsKey(values[i]))
							continue;

						result.Add(values[i], names[i]);
					}
				}
				return result;
			}

			// ReSharper disable once SuggestBaseTypeForParameter
			private static TEnum GetFlagsMaskCore(TEnum[] values)
			{
				if (values.Length == 0)
					return default(TEnum);

				var result = values[0];
				for (var i = 1; i < values.Length; i++)
				{
					result = Operators<TEnum>.BitwiseOr(result, values[i]);
				}

				return result;
			}

			[DebuggerHidden, MethodImpl(AggressiveInlining)]
			[AssertionMethod]
			private static void AssertUsage()
			{
				if (!_isEnum && !_isNullableEnum)
				{
					throw CodeExceptions.Argument(
						typeof(TEnum).Name,
						$"The {typeof(TEnum).Name} type is not enum type");
				}
			}
			#endregion

			#region API
			public static IReadOnlyDictionary<string, TEnum> GetNameValues(bool ignoreCase)
			{
				AssertUsage();
				return ignoreCase ? _nameValuesIgnoreCase : _nameValues;
			}

			public static IReadOnlyDictionary<TEnum, string> ValueNames
			{
				get
				{
					AssertUsage();
					return _valueNames;
				}
			}

			public static bool IsFlagsEnum
			{
				get
				{
					AssertUsage();
					return _isFlagsEnum;
				}
			}

			public static TEnum FlagsMask
			{
				get
				{
					AssertUsage();
					return _flagsMask;
				}
			}

			[MethodImpl(AggressiveInlining)]
			public static bool IsDefined(TEnum value)
			{
				AssertUsage();
				return _values.Contains(value);
			}

			[MethodImpl(AggressiveInlining)]
			public static bool AreFlagsDefined(TEnum flags)
			{
				AssertUsage();
				return _values.Contains(flags) ||
					// ReSharper disable once PossibleNullReferenceException
					_isFlagsEnum && IsFlagSetCallback(_flagsMask, flags);
			}

			[NotNull]
			public static Func<TEnum, TEnum, bool> IsFlagSetCallback
			{
				get
				{
					AssertUsage();
					// ReSharper disable once AssignNullToNotNullAttribute
					return _isFlagSetCallback;
				}
			}

			[NotNull]
			public static Func<TEnum, TEnum, bool> IsAnyFlagSetCallback
			{
				get
				{
					AssertUsage();
					// ReSharper disable once AssignNullToNotNullAttribute
					return _isAnyFlagSetCallback;
				}
			}

			[NotNull]
			public static Func<TEnum, TEnum, TEnum> SetFlagCallback
			{
				get
				{
					AssertUsage();
					// ReSharper disable once AssignNullToNotNullAttribute
					return _setFlagCallback;
				}
			}

			[NotNull]
			public static Func<TEnum, TEnum, TEnum> ClearFlagCallback
			{
				get
				{
					AssertUsage();
					// ReSharper disable once AssignNullToNotNullAttribute
					return _clearFlagCallback;
				}
			}
			#endregion
		}

		#region Metadata checks
		/// <summary>Determines whether the specified value is defined.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value to check.</param>
		/// <returns>True, if enum defines the value.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool IsDefined<TEnum>(TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.IsDefined(value);

		/// <summary>Determines whether all bits of the flags combination are defined.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="flags">The flags to check.</param>
		/// <returns>True, if enum defines all bits of the flags combination.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool AreFlagsDefined<TEnum>(TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.AreFlagsDefined(flags);

		/// <summary>Determines whether the enum has flags modifier.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <returns>True, if the enum is flags enum</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool IsFlagsEnum<TEnum>()
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.IsFlagsEnum;

		/// <summary>Returns a combination of all flags declared in the enum.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <returns>
		/// A combination of all flags declared in the enum, or <c>default(TEnum)</c> if <see cref="IsFlagsEnum{TEnum}"/> is false.
		/// </returns>
		[MethodImpl(AggressiveInlining)]
		public static TEnum GetFlagsMask<TEnum>()
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.FlagsMask;

		/// <summary>Try to parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="result">The parsed value.</param>
		/// <returns><c>true</c>, if parsing was successful; <c>false</c> otherwise.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool TryParse<TEnum>(string name, out TEnum result)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				TryParse(name, false, out result);

		/// <summary>Try to parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <param name="result">The parsed value.</param>
		/// <returns><c>true</c>, if parsing was successful; <c>false</c> otherwise.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool TryParse<TEnum>(string name, bool ignoreCase, out TEnum result)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.GetNameValues(ignoreCase).TryGetValue(name, out result) ||
					EnumClass.TryParse(name, ignoreCase, out result);

		/// <summary>Try to parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>Parsed value, if parsing was successful; <c>null</c> otherwise.</returns>
		[MethodImpl(AggressiveInlining)]
		public static TEnum? TryParse<TEnum>(string name, bool ignoreCase = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			TEnum result;
			return TryParse(name, ignoreCase, out result) ? result : (TEnum?)null;
		}

		/// <summary>Parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>Parsed value.</returns>
		[MethodImpl(AggressiveInlining)]
		public static TEnum Parse<TEnum>(string name, bool ignoreCase = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			TEnum result;
			if (Holder<TEnum>.GetNameValues(ignoreCase).TryGetValue(name, out result))
				return result;
			return (TEnum)Enum.Parse(typeof(TEnum), name, ignoreCase);
		}

		/// <summary>Returns a dictionary containing the enum names and their values.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>Returns a dictionary containing the enum names and their values.</returns>
		[MethodImpl(AggressiveInlining)]
		public static IReadOnlyDictionary<string, TEnum> GetNameValues<TEnum>(bool ignoreCase = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.GetNameValues(ignoreCase);

		/// <summary>
		/// Retrieves an array of the names of the constants in a specified enumeration.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="ignoreCase">If set to <c>true</c> the case of the name will be ignored.</param>
		/// <returns>A string array of the names of the constants in enumType.</returns>
		[NotNull]
		[Pure]
		public static string[] GetNames<TEnum>(bool ignoreCase = false)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.GetNameValues(ignoreCase).Keys.ToArray();

		/// <summary>
		/// Retrieves an array of the values of the constants in a specified enumeration.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <returns>An array that contains the values of the constants in enumType.</returns>
		[NotNull]
		[Pure]
		public static TEnum[] GetValues<TEnum>()
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.GetNameValues(false).Values.ToArray();

		/// <summary>Returns the name of the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The enum value.</param>
		/// <returns>The name of the enum value, or <c>null</c> if there is no value defined.</returns>
		[MethodImpl(AggressiveInlining)]
		public static string GetName<TEnum>(TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.ValueNames.GetValueOrDefault(value);
		#endregion

		#region Flag checks
		/// <summary>Determines whether the specified flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns><c>true</c> if the value includes all bits of the flag or the flag is zero.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool IsFlagSet<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.IsFlagSetCallback(value, flag);

		/// <summary>Determines whether any bit from specified flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns><c>true</c> if the value includes any bit of the flags or the flag is zero.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool IsAnyFlagSet<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.IsAnyFlagSetCallback(value, flags);

		/// <summary>Determines whether the specified flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns><c>true</c> if the value does not include all bits of the flag.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool IsFlagUnset<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				!Holder<TEnum>.IsFlagSetCallback(value, flag);

		/// <summary>Determines whether any bit from specified flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns><c>true</c> if the value does not include any bit of the flags.</returns>
		[MethodImpl(AggressiveInlining)]
		public static bool AreAllFlagsUnset<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				!Holder<TEnum>.IsAnyFlagSetCallback(value, flags);
		#endregion

		#region Flag operations
		/// <summary>Sets the flag.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>Bitwise combination of the flag and the value</returns>
		[MethodImpl(AggressiveInlining)]
		public static TEnum SetFlag<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.SetFlagCallback(value, flag);

		/// <summary>Clears the flag.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>The bits of the value excluding the ones from the flag.</returns>
		[MethodImpl(AggressiveInlining)]
		public static TEnum ClearFlag<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
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
		[MethodImpl(AggressiveInlining)]
		public static TEnum SetFlag<TEnum>(this TEnum value, TEnum flag, bool enabled)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				enabled
			? SetFlag(value, flag)
			: ClearFlag(value, flag);
		#endregion
	}
}