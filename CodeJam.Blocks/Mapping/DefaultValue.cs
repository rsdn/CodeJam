#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

using CodeJam.Reflection;
using CodeJam.Targeting;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// Provides default value service.
	/// </summary>
	[PublicAPI]
	public static class DefaultValue
	{
		private static readonly ConcurrentDictionary<Type, object?> _defaultValues = new()
		{
			[typeof(int)] = default(int),
			[typeof(uint)] = default(uint),
			[typeof(byte)] = default(byte),
			[typeof(char)] = default(char),
			[typeof(bool)] = default(bool),
			[typeof(sbyte)] = default(sbyte),
			[typeof(short)] = default(short),
			[typeof(long)] = default(long),
			[typeof(ushort)] = default(ushort),
			[typeof(ulong)] = default(ulong),
			[typeof(float)] = default(float),
			[typeof(double)] = default(double),
			[typeof(decimal)] = default(decimal),
			[typeof(DateTime)] = default(DateTime),
			[typeof(TimeSpan)] = default(TimeSpan),
			[typeof(DateTimeOffset)] = default(DateTimeOffset),
			[typeof(Guid)] = default(Guid),
			[typeof(string)] = default(string)
		};

		/// <summary>
		/// Gets default value for provided <see cref="Type"/>.
		/// </summary>
		/// <param name="type"><see cref="Type"/> to get default value.</param>
		/// <param name="mappingSchema">An instance of <see cref="MappingSchema"/>.</param>
		/// <returns>Default value of the provided <see cref="Type"/></returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static object? GetValue(Type type, MappingSchema? mappingSchema = null)
		{
			Code.NotNull(type, nameof(type));

			var ms = mappingSchema ?? MappingSchema.Default;

			if (_defaultValues.TryGetValue(type, out var value))
				return value;

			if (type.GetIsEnum())
			{
				var mapValues = ms.GetMapValues(type);

				if (mapValues != null)
				{
					var fields =
						from f in mapValues
						where f.MapValues.Any(a => a.Value == null)
						select f.OrigValue;

					value = fields.FirstOrDefault();
				}
			}

			if (value == null && !type.GetIsClass() && !type.IsNullable())
			{
				var mi = InfoOf.Method(() => GetValue<int>());

				value =
					Expression.Lambda<Func<object>>(
						Expression.Convert(
							Expression.Call(mi.GetGenericMethodDefinition().MakeGenericMethod(type)),
							typeof(object)))
						.Compile()();
			}

			_defaultValues[type] = value;

			return value;
		}

		/// <summary>
		/// Gets default value for provided <see cref="Type"/>.
		/// </summary>
		/// <typeparam name="T">Type to get default value.</typeparam>
		/// <returns>Default value of the provided <see cref="Type"/></returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static T? GetValue<T>()
		{
			if (_defaultValues.TryGetValue(typeof(T), out var value))
				return (T)value;

			_defaultValues[typeof(T)] = default(T);

			return default;
		}

		/// <summary>
		/// Sets default value for provided <see cref="Type"/>.
		/// </summary>
		/// <typeparam name="T">Type to set default value for.</typeparam>
		/// <param name="value">Value to set.</param>
		public static void SetValue<T>(T? value)
			=> _defaultValues[typeof(T)] = value;
	}
}

#endif