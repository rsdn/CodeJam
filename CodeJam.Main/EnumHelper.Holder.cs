﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using CodeJam.Arithmetic;
// ReSharper disable once RedundantUsingDirective
using JetBrains.Annotations;

using static CodeJam.Targeting.MethodImplOptionsEx;

namespace CodeJam
{
	/// <summary>
	/// Extension methods for Enum types
	/// </summary>
	partial class EnumHelper
	{
		private static class OpHolder<TEnum>
			where TEnum : struct, Enum
		{
			[NotNull]
			public static Func<TEnum, TEnum, bool> AreEqualCallback { get; } = OperatorsFactory.ComparisonOperator<TEnum>(ExpressionType.Equal);

			[NotNull]
			public static Func<TEnum, TEnum, bool> IsFlagSetCallback { get; } = OperatorsFactory.IsFlagSetOperator<TEnum>();

			[NotNull]
			public static Func<TEnum, TEnum, bool> IsAnyFlagSetCallback { get; } = OperatorsFactory.IsAnyFlagSetOperator<TEnum>();

			[NotNull]
			public static Func<TEnum, TEnum, TEnum> SetFlagCallback { get; } = OperatorsFactory.SetFlagOperator<TEnum>();

			[NotNull]
			public static Func<TEnum, TEnum, TEnum> ClearFlagCallback { get; } = OperatorsFactory.ClearFlagOperator<TEnum>();
		}

		private static class MetaHolder<TEnum>
			where TEnum : struct, Enum
		{
			#region Init helpers
			private static bool IsFlagsEnumCore() => typeof(TEnum).GetTypeInfo().GetCustomAttribute<FlagsAttribute>() != null;

			[NotNull]
			private static TEnum[] GetValuesCore() => (TEnum[])Enum.GetValues(typeof(TEnum));

			private static IReadOnlyCollection<TEnum> GetValuesSetCore() =>
#if NET46_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
				new HashSet<TEnum>(GetValuesCore());
#else
				new HashSetEx<TEnum>(GetValuesCore());
#endif

			private static TEnum GetValuesMaskCore()
			{
				var result = default(TEnum);

				var values = GetValuesCore();
				foreach (var value in values)
					result = OpHolder<TEnum>.SetFlagCallback(result, value);

				return result;
			}

			[NotNull]
			private static IReadOnlyDictionary<string, TEnum> GetNameValuesCore(bool ignoreCase)
			{
#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
				var result = new Dictionary<string, TEnum>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
#else
				var result = new DictionaryEx<string, TEnum>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
#endif

				var names = Enum.GetNames(typeof(TEnum));
				var values = GetValuesCore();
				for (var i = 0; i < names.Length; i++)
				{
					// DONTTOUCH: case may be ignored
					if (result.ContainsKey(names[i]))
						continue;

					result.Add(names[i], values[i]);
				}

				return result;
			}

			private static TEnum[] GetNonDefaultFlagsCore() =>
				GetValuesCore()
					.Where(f => !OpHolder<TEnum>.AreEqualCallback(f, default))
					.Distinct()
					.ToArray();

			// NB: simple implementation here
			// as result of method call is cached.
			private static TEnum[] GetNonDefaultUniqueFlagsCore()
			{
				// THANKSTO: Maciej Hehl, https://stackoverflow.com/a/2709523
				static int GetNumberOfSetBits(TEnum value)
				{
					var i = Convert.ToUInt64(value);
					i = i - ((i >> 1) & 0x5555555555555555UL);
					i = (i & 0x3333333333333333UL) + ((i >> 2) & 0x3333333333333333UL);
					return (int)(unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
				}

				// At first, we take all non-default enum values and sort them by count of bits in flag
				// to ensure that all simple flags will be checked before flag combinations
				var allFlags = GetValuesCore()
					.Where(f => !OpHolder<TEnum>.AreEqualCallback(f, default))
					.Select((flag, index) => (flag, index, bitsCount: GetNumberOfSetBits(flag)))
					.OrderBy(data => data.bitsCount)
					.ThenBy(data => data.index);

				// Next, we collect flags with unique bits
				// (value skipped if all bits were added earlier).
				var result = new List<(TEnum value, int index)>();
				TEnum addedBits = default;
				foreach (var (flag, index, _) in allFlags)
				{
					if (OpHolder<TEnum>.IsFlagSetCallback(addedBits, flag))
						continue;

					result.Add((flag, index));
					addedBits = OpHolder<TEnum>.SetFlagCallback(addedBits, flag);
				}

				// Finally, we restore original order of enum values and return collected values.
				result.Sort((t1, t2) => t1.index.CompareTo(t2.index));
				return result.Select(t => t.value).ToArray();
			}
			#endregion

			#region Cached values

			[NotNull]
			private static readonly IReadOnlyDictionary<string, TEnum> _nameValues = GetNameValuesCore(ignoreCase: false);

			[NotNull]
			private static readonly IReadOnlyDictionary<string, TEnum> _nameValuesIgnoreCase = GetNameValuesCore(ignoreCase: true);

			// ReSharper disable once StaticMemberInGenericType // result depends on TEnum
			public static bool IsFlagsEnum { get; } = IsFlagsEnumCore();

			public static TEnum ValuesMask { get; } = GetValuesMaskCore();

			[NotNull]
			public static IReadOnlyCollection<TEnum> ValuesSet { get; } = GetValuesSetCore();

			[NotNull]
			[MethodImpl(AggressiveInlining)]
			public static IReadOnlyDictionary<string, TEnum> GetNameValues(bool ignoreCase) =>
				ignoreCase ? _nameValuesIgnoreCase : _nameValues;

			[NotNull]
			public static TEnum[] NonDefaultFlags { get; } = GetNonDefaultFlagsCore();

			[NotNull]
			public static TEnum[] NonDefaultUniqueFlags { get; } = GetNonDefaultUniqueFlagsCore();
			#endregion
		}
	}
}