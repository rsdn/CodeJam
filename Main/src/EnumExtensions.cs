using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;
using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Extension methods for Enum types
	/// </summary>
	[PublicAPI]
	public static class EnumExtensions
	{
		private static class Holder<TEnum>
			where TEnum : struct
		{
			#region Static fields
			// DONTTOUCH: the ordering of the fields represents the dependencies between them.
			// DO NOT change it until the initialization logic is changed.

			private static readonly bool _isEnum = typeof(TEnum).IsEnum;
			private static readonly bool _isNullableEnum = typeof(TEnum).IsNullableEnum();

			private static readonly Type _enumType = typeof(TEnum).IsNullableEnum()
				? Nullable.GetUnderlyingType(typeof(TEnum))
				: typeof(TEnum);

			// ReSharper disable once StaticMemberInGenericType
			private static readonly bool _isFlagsEnum =
				(_isEnum || _isNullableEnum) &&
					_enumType.GetCustomAttribute<FlagsAttribute>() != null;

			private static readonly HashSet<TEnum> _values = _enumType.IsEnum
				? new HashSet<TEnum>((TEnum[])Enum.GetValues(_enumType))
				: new HashSet<TEnum>();

			private static readonly IReadOnlyDictionary<string, TEnum> _nameValues = GetNameValuesCore(_enumType);

			private static readonly TEnum _flagsMask = _isFlagsEnum ? GetFlagsMaskCore(_values.ToArray()) : default(TEnum);
			#endregion

			#region Flag operations emit
			[CanBeNull]
			private static readonly Func<TEnum, TEnum, bool> _isFlagSetCallback = _isEnum || _isNullableEnum
				? OperatorsFactory.IsFlagSetOperator<TEnum>()
				: null;

			[CanBeNull]
			private static readonly Func<TEnum, TEnum, bool> _isFlagMatchCallback = _isEnum || _isNullableEnum
				? OperatorsFactory.IsFlagMatchOperator<TEnum>()
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
			private static IReadOnlyDictionary<string, TEnum> GetNameValuesCore(Type enumType)
			{
				var result =
#if FW40
				new DictionaryWithReadOnly<string, TEnum>();
#else
					new Dictionary<string, TEnum>();
#endif

				if (enumType.IsEnum)
				{
					var names = Enum.GetNames(enumType);
					var values = (TEnum[])Enum.GetValues(enumType);
					for (var i = 0; i < names.Length; i++)
					{
						result.Add(names[i], values[i]);
					}
				}
				return result;
			}

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

			[DebuggerHidden, MethodImpl(PlatformDependent.AggressiveInlining)]
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
			public static IReadOnlyDictionary<string, TEnum> NameValues
			{
				[MethodImpl(PlatformDependent.AggressiveInlining)]
				get
				{
					AssertUsage();
					return _nameValues;
				}
			}

			[MethodImpl(PlatformDependent.AggressiveInlining)]
			public static bool IsDefined(TEnum value)
			{
				AssertUsage();
				return _values.Contains(value);
			}

			[MethodImpl(PlatformDependent.AggressiveInlining)]
			public static bool IsFlagsDefined(TEnum flags)
			{
				AssertUsage();
				return _values.Contains(flags) ||
					// ReSharper disable once PossibleNullReferenceException
					(_isFlagsEnum && IsFlagSetCallback(_flagsMask, flags));
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
			public static Func<TEnum, TEnum, bool> IsFlagMatchCallback
			{
				get
				{
					AssertUsage();
					// ReSharper disable once AssignNullToNotNullAttribute
					return _isFlagMatchCallback;
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
		/// <summary>Determines whether the specified value is defined</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value to check.</param>
		/// <returns>True, if enum defines the value.</returns>
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static bool IsDefined<TEnum>(TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.IsDefined(value);

		/// <summary>Determines whether all bits of the flags combination are defined</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="flags">The flags to check.</param>
		/// <returns>True, if enum defines all bits of the flags combination.</returns>
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static bool AreFlagsDefined<TEnum>(TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.IsFlagsDefined(flags);

		/// <summary>Try to parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <returns><c>true</c>, if parsing was successful; <c>false</c> otherwise.</returns>
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static bool TryParse<TEnum>(string name, out TEnum value)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.NameValues.TryGetValue(name, out value) ||
					Enum.TryParse(name, out value);

		/// <summary>Try to parse the enum value.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="name">The name.</param>
		/// <returns>Parsed value, if parsing was successful; <c>null</c> otherwise.</returns>
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static TEnum? TryParse<TEnum>(string name)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			TEnum value;
			return (Holder<TEnum>.NameValues.TryGetValue(name, out value) || Enum.TryParse(name, out value))
				? value
				: (TEnum?)null;
		}
		#endregion

		#region Flag checks
		/// <summary>Determines whether the specified flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns><c>true</c> if the value includes all bits of the flag or the flag is zero.</returns>
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static bool IsFlagSet<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.IsFlagSetCallback(value, flag);

		/// <summary>Determines whether any bit from specified flag is set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns><c>true</c> if the value includes any bit of the flags or the flag is zero.</returns>
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static bool IsFlagMatch<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.IsFlagMatchCallback(value, flags);

		/// <summary>Determines whether the specified flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns><c>true</c> if the value does not include all bits of the flag.</returns>
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static bool IsFlagNotSet<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				!Holder<TEnum>.IsFlagSetCallback(value, flag);

		/// <summary>Determines whether any bit from specified flag is not set.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flags">The bitwise combinations of the flags.</param>
		/// <returns><c>true</c> if the value does not include any bit of the flags.</returns>
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static bool IsFlagNotMatch<TEnum>(this TEnum value, TEnum flags)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				!Holder<TEnum>.IsFlagMatchCallback(value, flags);
		#endregion

		#region Flag operations
		/// <summary>Sets the flag.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>Bitwise combination of the flag and the value</returns>
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static TEnum SetFlag<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.SetFlagCallback(value, flag);


		/// <summary>Clears the flag.</summary>
		/// <typeparam name="TEnum">The type of the enum.</typeparam>
		/// <param name="value">The value.</param>
		/// <param name="flag">The flag.</param>
		/// <returns>The bits of the value excluding the ones from the flag.</returns>
		[MethodImpl(PlatformDependent.AggressiveInlining)]
		public static TEnum ClearFlag<TEnum>(this TEnum value, TEnum flag)
			where TEnum : struct, IComparable, IFormattable, IConvertible =>
				Holder<TEnum>.ClearFlagCallback(value, flag);
		#endregion
	}
}