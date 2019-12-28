﻿#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Xml;

using CodeJam.Targeting;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	using Expressions;

	/// <summary>
	/// Provides data convertion functionality.
	/// </summary>
	[PublicAPI]
	public static class Converter
	{
		[NotNull]
		private static readonly ConcurrentDictionary<object, LambdaExpression> _expressions =
			new ConcurrentDictionary<object, LambdaExpression>();

		static Converter()
		{
			SetConverter<string, char>(v => v.Length == 0 ? '\0' : v[0]);
			//			SetConverter<string,         Binary>     (v => new Binary(Convert.FromBase64String(v)));
			//			SetConverter<Binary,         string>     (v => Convert.ToBase64String(v.ToArray()));
			//			SetConverter<Binary,         byte[]>     (v => v.ToArray());
			SetConverter<bool, decimal>(v => v ? 1m : 0m);
			SetConverter<DateTimeOffset, DateTime>(v => v.LocalDateTime);
			SetConverter<string, XmlDocument>(v => CreateXmlDocument(v));
			SetConverter<string, byte[]>(v => Convert.FromBase64String(v));
			SetConverter<byte[], string>(v => Convert.ToBase64String(v));
			SetConverter<TimeSpan, DateTime>(v => DateTime.MinValue + v);
			SetConverter<DateTime, TimeSpan>(v => v - DateTime.MinValue);
			SetConverter<string, DateTime>(v => DateTime.Parse(v, null, DateTimeStyles.NoCurrentDateDefault));
			SetConverter<char, bool>(v => ToBoolean(v));
			SetConverter<string, bool>(v => v.Length == 1 ? ToBoolean(v[0]) : bool.Parse(v));
		}

		[NotNull]
		private static XmlDocument CreateXmlDocument([NotNull] string str)
		{
			var xml = new XmlDocument();
			xml.LoadXml(str);
			return xml;
		}

		private static bool ToBoolean(char ch)
		{
			switch (ch)
			{
				case '\x0': // Allow int <=> Char <=> Boolean
				case '0':
				case 'n':
				case 'N':
				case 'f':
				case 'F': return false;

				case '\x1': // Allow int <=> Char <=> Boolean
				case '1':
				case 'y':
				case 'Y':
				case 't':
				case 'T': return true;
			}

			throw new InvalidCastException("Invalid cast from System.String to System.Bool");
		}

		/// <summary>
		/// Adds a custom converter from <i>TFrom</i> to <i>TTo</i> types.
		/// </summary>
		/// <typeparam name="TFrom">Type to convert from.</typeparam>
		/// <typeparam name="TTo">Type to convert to.</typeparam>
		/// <param name="expr">Convert expression.</param>
		public static void SetConverter<TFrom, TTo>(Expression<Func<TFrom, TTo>> expr)
			=> _expressions[new { from = typeof(TFrom), to = typeof(TTo) }] = expr;

		[CanBeNull]
		internal static LambdaExpression GetConverter([NotNull] Type from, [NotNull] Type to)
		{
			_expressions.TryGetValue(new { from, to }, out var l);
			return l;
		}

		[NotNull]
		private static readonly ConcurrentDictionary<object, Func<object, object>> _converters =
			new ConcurrentDictionary<object, Func<object, object>>();

		/// <summary>
		/// Returns an object of a specified type whose value is equivalent to a specified object.
		/// </summary>
		/// <param name="value">An object to convert.</param>
		/// <param name="conversionType">The type of object to return.</param>
		/// <param name="mappingSchema">A mapping schema that defines custom converters.</param>
		/// <returns>An object whose type is <i>conversionType</i> and whose value is equivalent to <i>value</i>.</returns>
		public static object ChangeType([CanBeNull] object value, [NotNull] Type conversionType, MappingSchema mappingSchema = null)
		{
			Code.NotNull(conversionType, nameof(conversionType));

			if (value == null || value is DBNull)
				return mappingSchema == null ?
					DefaultValue.GetValue(conversionType) :
					mappingSchema.GetDefaultValue(conversionType);

			if (value.GetType() == conversionType)
				return value;

			var from = value.GetType();
			var to = conversionType;
			var key = new { from, to };

			var converters = mappingSchema == null ? _converters : mappingSchema.Converters;

			if (!converters.TryGetValue(key, out var l))
			{
				var li =
					ConvertInfo.Default.Get(value.GetType(), to) ??
					ConvertInfo.Default.Create(mappingSchema, value.GetType(), to);

				var b = li.CheckNullLambda.Body;
				var ps = li.CheckNullLambda.Parameters;

				var p = Expression.Parameter(typeof(object), "p");
				var ex = Expression.Lambda<Func<object, object>>(
					Expression.Convert(
						b.Transform(e =>
							e == ps[0] ?
								Expression.Convert(p, e.Type) :
							IsDefaultValuePlaceHolder(e) ?
								new DefaultValueExpression(mappingSchema, e.Type) :
								e),
						typeof(object)),
					p);

				l = ex.Compile();

				converters[key] = l;
			}

			return l(value);
		}

		private static class ExprHolder<T>
		{
			[NotNull]
			public static readonly ConcurrentDictionary<Type, Func<object, T>> Converters =
				new ConcurrentDictionary<Type, Func<object, T>>();
		}

		/// <summary>
		/// Returns an object of a specified type whose value is equivalent to a specified object.
		/// </summary>
		/// <typeparam name="T">The type of object to return.</typeparam>
		/// <param name="value">An object to convert.</param>
		/// <param name="mappingSchema">A mapping schema that defines custom converters.</param>
		/// <returns>An object whose type is <i>conversionType</i> and whose value is equivalent to <i>value</i>.</returns>
		public static T ChangeTypeTo<T>(object value, MappingSchema mappingSchema = null)
		{
			if (value == null || value is DBNull)
				return mappingSchema == null ?
					DefaultValue<T>.Value :
					(T)mappingSchema.GetDefaultValue(typeof(T));

			if (value.GetType() == typeof(T))
				return (T)value;

			var from = value.GetType();
			var to = typeof(T);

			if (!ExprHolder<T>.Converters.TryGetValue(from, out var l))
			{
				var li = ConvertInfo.Default.Get(from, to) ?? ConvertInfo.Default.Create(mappingSchema, from, to);
				var b = li.CheckNullLambda.Body;
				var ps = li.CheckNullLambda.Parameters;

				var p = Expression.Parameter(typeof(object), "p");
				var ex = Expression.Lambda<Func<object, T>>(
					b.Transform(e =>
						e == ps[0] ?
							Expression.Convert(p, e.Type) :
							IsDefaultValuePlaceHolder(e) ?
								new DefaultValueExpression(mappingSchema, e.Type) :
								e),
					p);

				l = ex.Compile();

				ExprHolder<T>.Converters[from] = l;
			}

			return l(value);
		}

		internal static bool IsDefaultValuePlaceHolder(Expression expr) =>
			expr is MemberExpression me
					&& me.Member.Name == "Value"
					&& me.Member.DeclaringType?.GetIsGenericType() == true
				? me.Member.DeclaringType.GetGenericTypeDefinition() == typeof(DefaultValue<>)
				: expr is DefaultValueExpression;

		//		public static Type GetDefaultMappingFromEnumType(MappingSchema mappingSchema, Type enumType)
		//			=> ConvertBuilder.GetDefaultMappingFromEnumType(mappingSchema, enumType);
	}
}
#endif