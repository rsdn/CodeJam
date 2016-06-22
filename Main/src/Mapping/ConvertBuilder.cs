#if !FW35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

using CodeJam.Collections;
using CodeJam.Expressions;

namespace CodeJam.Mapping
{
	using Reflection;

	internal static class ConvertBuilder
	{
		private static readonly MethodInfo _defaultConverter = InfoOf.Method(() => ConvertDefault(null, typeof(int)));

		private static object ConvertDefault(object value, Type conversionType)
		{
			try
			{
				return Convert.ChangeType(value, conversionType, Thread.CurrentThread.CurrentCulture);
			}
			catch (Exception ex)
			{
				throw new CodeJamConvertException($"Cannot convert value '{value}' to type '{conversionType.FullName}'", ex);
			}
		}

		private static Expression GetCtor(Type from, Type to, Expression p)
		{
			var ctor = to.GetConstructor(new[] { from });

			if (ctor == null)
				return null;

			var ptype = ctor.GetParameters()[0].ParameterType;

			if (ptype != from)
				p = Expression.Convert(p, ptype);

			return Expression.New(ctor, p);
		}

		private static Expression GetValue(Type from, Type to, Expression p)
		{
			var pi = from.GetProperty("Value");

			if (pi == null)
			{
				var fi = from.GetField("Value");

				if (fi != null && fi.FieldType == to)
					return Expression.Field(p, fi);

				return null;
			}

			return pi.PropertyType == to ? Expression.Property(p, pi) : null;
		}

		private const BindingFlags MethodLookup =
			BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private static Expression GetOperator(Type from, Type to, Expression p)
		{
			var op =
				to.GetMethod("op_Implicit", MethodLookup, null, new[] { from }, null) ??
				to.GetMethod("op_Explicit", MethodLookup, null, new[] { from }, null);

			return op != null ? Expression.Convert(p, to, op) : null;
		}

		private static bool IsConvertible(Type type)
		{
			if (type.IsEnum)
				return false;

			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean :
				case TypeCode.Byte    :
				case TypeCode.SByte   :
				case TypeCode.Int16   :
				case TypeCode.Int32   :
				case TypeCode.Int64   :
				case TypeCode.UInt16  :
				case TypeCode.UInt32  :
				case TypeCode.UInt64  :
				case TypeCode.Single  :
				case TypeCode.Double  :
				case TypeCode.Decimal :
				case TypeCode.Char    : return true;
				default               : return false;
			}
		}

		private static Expression GetConvertion(Type from, Type to, Expression p)
		{
			if (IsConvertible(from) && IsConvertible(to) && to != typeof(bool) ||
				from.IsAssignableFrom(to) && to.IsAssignableFrom(from))
				return Expression.ConvertChecked(p, to);

		 	return null;
		}

		private static Expression GetParse(Type from, Type to, Expression p)
		{
			if (from == typeof(string))
			{
				var mi = to.GetMethod("Parse", MethodLookup, null, new [] { from }, null);

				if (mi != null)
				{
					return Expression.Convert(p, to, mi);
				}

				//mi = to.GetMethodEx("Parse", typeof(SqlString));
				//
				//if (mi != null)
				//{
				//	p = GetCtor(from, typeof(SqlString), p);
				//	return Expression.Convert(p, to, mi);
				//}

				return null;
			}

			return null;
		}

		private static Expression GetToString(Type from, Type to, Expression p)
		{
			if (to == typeof(string) && !from.IsNullable())
			{
				var mi =
					from.GetMethod(
						"ToString",
						MethodLookup,
						null,
#if (!FW452)
						Array.Empty<Type>(),
#else
						Array<Type>.Empty,
#endif
						null);
				return mi != null ? Expression.Call(p, mi) : null;
			}

			return null;
		}

		private static Expression GetParseEnum(Type from, Type to, Expression p)
		{
			if (from == typeof(string) && to.IsEnum)
			{
				var values = Enum.GetValues(to);
				var names  = Enum.GetNames (to);

				var dic = new Dictionary<string,object>();

				for (var i = 0; i < values.Length; i++)
				{
					var val = values.GetValue(i);
					var lv  = (long)Convert.ChangeType(val, typeof(long), Thread.CurrentThread.CurrentCulture);

					dic[lv.ToString()] = val;

					if (lv > 0)
						dic["+" + lv] = val;
				}

				for (var i = 0; i < values.Length; i++)
					dic[names[i].ToLowerInvariant()] = values.GetValue(i);

				for (var i = 0; i < values.Length; i++)
					dic[names[i]] = values.GetValue(i);

				var cases =
					from v in dic
					group v.Key by v.Value
					into g
					select Expression.SwitchCase(Expression.Constant(g.Key), g.Select(Expression.Constant));

				var expr = Expression.Switch(
					p,
					Expression.Convert(
						Expression.Call(_defaultConverter,
							Expression.Convert(p, typeof(string)),
							Expression.Constant(to)),
						to),
					cases.ToArray());

				return expr;
			}

			return null;
		}

		private const FieldAttributes _enumField =
			FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal;

		private static object ThrowLinqToDBException(string text)
		{
			throw new CodeJamConvertException(text);
		}

		private static readonly MethodInfo _throwLinqToDBConvertException = InfoOf.Method(() => ThrowLinqToDBException(null));

		private static Expression GetToEnum(Type from, Type to, Expression expression, MappingSchema mappingSchema)
		{
			if (to.IsEnum)
			{
				var toFields = mappingSchema.GetMapValues(to);

				if (toFields == null)
					return null;

				var fromTypeFields = toFields
					.Select(f => new { f.OrigValue, attrs = f.MapValues.Where(a => a.Value == null || a.Value.GetType() == from).ToList() })
					.ToList();

				if (fromTypeFields.All(f => f.attrs.Count != 0))
				{
					var cases = fromTypeFields
						.Select(f => new
							{
								value = f.OrigValue,
								attrs = f.attrs
									.Where (a => a.Configuration == f.attrs[0].Configuration)
									.Select(a => a.Value ?? mappingSchema.GetDefaultValue(from))
									.ToList()
							})
						.ToList();

					var ambiguityMappings =
						from c in cases
						from a in c.attrs
						group c by a into g
						where g.Count() > 1
						select g;

					var ambiguityMapping = ambiguityMappings.FirstOrDefault();

					if (ambiguityMapping != null)
					{
						var enums = ambiguityMapping.ToArray();

						return Expression.Convert(
							Expression.Call(
								_throwLinqToDBConvertException,
								Expression.Constant(
									$"Mapping ambiguity. MapValue({ambiguityMapping.Key}) attribute is defined for both '{to.FullName}.{enums[0].value}' and '{to.FullName}.{enums[1].value}'.")),
								to);
					}

					var expr = Expression.Switch(
						expression,
						Expression.Convert(
							Expression.Call(_defaultConverter,
								Expression.Convert(expression, typeof(object)),
								Expression.Constant(to)),
							to),
						cases
							.Select(f =>
								Expression.SwitchCase(
									Expression.Constant(f.value),
									f.attrs.Select(a => Expression.Constant(a, from))))
							.ToArray());

					return expr;
				}

				if (fromTypeFields.Any(f => f.attrs.Count(a => a.Value != null) != 0))
				{
					var field = fromTypeFields.First(f => f.attrs.Count == 0);

					return Expression.Convert(
						Expression.Call(
							_throwLinqToDBConvertException,
							Expression.Constant(
								$"Inconsistent mapping. '{to.FullName}.{field.OrigValue}' does not have MapValue(<{from.FullName}>) attribute.")),
							to);
				}
			}

			return null;
		}

		private class EnumValues
		{
			public FieldInfo           Field;
			public MapValueAttribute[] Attrs;
		}

		private static Expression GetFromEnum(Type from, Type to, Expression expression, MappingSchema mappingSchema)
		{
			if (from.IsEnum)
			{
				var fromFields = from.GetFields()
					.Where (f => (f.Attributes & _enumField) == _enumField)
					.Select(f => new EnumValues { Field = f, Attrs = mappingSchema.GetAttributes<MapValueAttribute>(f, a => a.Configuration) })
					.ToList();

				{
					var toTypeFields = fromFields
						.Select(f => new { f.Field, Attrs = f.Attrs
							.OrderBy(a =>
							{
								var idx = a.Configuration == null ?
									int.MaxValue :
									Array.IndexOf(mappingSchema.ConfigurationList, a.Configuration);
								return idx < 0 ? int.MaxValue : idx;
							})
							.ThenBy(a => !a.IsDefault)
							.ThenBy(a => a.Value == null)
							.FirstOrDefault(a => a.Value == null || a.Value.GetType() == to) })
						.ToList();

					if (toTypeFields.All(f => f.Attrs != null))
					{
						var cases = toTypeFields.Select(f => Expression.SwitchCase(
							Expression.Constant(f.Attrs.Value ?? mappingSchema.GetDefaultValue(to), to),
							Expression.Constant(Enum.Parse(from, f.Field.Name, false))));

						var expr = Expression.Switch(
							expression,
							Expression.Convert(
								Expression.Call(_defaultConverter,
									Expression.Convert(expression, typeof(object)),
									Expression.Constant(to)),
								to),
							cases.ToArray());

						return expr;
					}

					if (toTypeFields.Any(f => f.Attrs != null))
					{
						var field = toTypeFields.First(f => f.Attrs == null);

						return Expression.Convert(
							Expression.Call(
								_throwLinqToDBConvertException,
								Expression.Constant(
									$"Inconsistent mapping. '{from.FullName}.{field.Field.Name}' does not have MapValue(<{to.FullName}>) attribute.")),
								to);
					}
				}

				if (to.IsEnum)
				{
					var toFields = to.GetFields()
						.Where (f => (f.Attributes & _enumField) == _enumField)
						.Select(f => new EnumValues { Field = f, Attrs = mappingSchema.GetAttributes<MapValueAttribute>(f, a => a.Configuration) })
						.ToList();

					var dic = new Dictionary<EnumValues,EnumValues>();
					var cl  = mappingSchema.ConfigurationList.Concat("", null).Select((c,i) => new { c, i }).ToArray();

					foreach (var toField in toFields)
					{
						if (toField.Attrs == null || toField.Attrs.Length == 0)
							return null;

						var toAttr = toField.Attrs.First();

						toAttr = toField.Attrs.FirstOrDefault(a => a.Configuration == toAttr.Configuration && a.IsDefault) ?? toAttr;

						var fromAttrs = fromFields.Where(f => f.Attrs.Any(a =>
							a.Value?.Equals(toAttr.Value) ?? (toAttr.Value == null))).ToList();

						if (fromAttrs.Count == 0)
							return null;

						if (fromAttrs.Count > 1)
						{
							var fattrs =
								from f in fromAttrs
								select new
								{
									f,
									a = f.Attrs.First(a => a.Value?.Equals(toAttr.Value) ?? (toAttr.Value == null))
								} into fa
								from c in cl
								where fa.a.Configuration == c.c
								orderby c.i
								select fa.f;

							fromAttrs = fattrs.Take(1).ToList();
						}

						var prev =
						(
							from a in dic
							where a.Value.Field == fromAttrs[0].Field
							select new { To = a.Key, From = a.Value }
						)
						.FirstOrDefault();

						if (prev != null)
						{
							return Expression.Convert(
								Expression.Call(
									_throwLinqToDBConvertException,
									Expression.Constant(
										$"Mapping ambiguity. '{from.FullName}.{fromAttrs[0].Field.Name}' can be mapped to either '{to.FullName}.{prev.To.Field.Name}' or '{to.FullName}.{toField.Field.Name}'.")),
									to);
						}

						dic.Add(toField, fromAttrs[0]);
					}

					if (dic.Count > 0)
					{
						var cases = dic.Select(f => Expression.SwitchCase(
							Expression.Constant(Enum.Parse(to,   f.Key.  Field.Name, false)),
							Expression.Constant(Enum.Parse(from, f.Value.Field.Name, false))));

						var expr = Expression.Switch(
							expression,
							Expression.Convert(
								Expression.Call(_defaultConverter,
									Expression.Convert(expression, typeof(object)), 
									Expression.Constant(to)),
								to),
							cases.ToArray());

						return expr;
					}
				}
			}

			return null;
		}

		private static ValueTuple<Expression,bool>? GetConverter(
			MappingSchema mappingSchema,
			Expression expr,
			Type from,
			Type to)
		{
			if (from == to)
				return ValueTuple.Create(expr, false);

			var le = Converter.GetConverter(from, to);

			if (le != null)
				return ValueTuple.Create(le.ReplaceParameters(expr), false);

			var lex = mappingSchema.TryGetConvertExpression(from, to);

			if (lex != null)
				return ValueTuple.Create(lex.ReplaceParameters(expr), true);

			var ex =
				GetFromEnum  (from, to, expr, mappingSchema) ??
				GetToEnum    (from, to, expr, mappingSchema);

			if (ex != null)
				return ValueTuple.Create(ex, true);

			ex =
				GetConvertion(from, to, expr) ??
				GetCtor      (from, to, expr) ??
				GetValue     (from, to, expr) ??
				GetOperator  (from, to, expr) ??
				GetParse     (from, to, expr) ??
				GetToString  (from, to, expr) ??
				GetParseEnum (from, to, expr);

			return ex != null ? ValueTuple.Create(ex, false) : (ValueTuple<Expression, bool>?)null;
		}

		private static ValueTuple<Expression,bool>? ConvertUnderlying(
			MappingSchema mappingSchema,
			Expression    expr,
			Type from, Type ufrom,
			Type to,   Type uto)
		{
			ValueTuple<Expression,bool>? ex = null;

			if (from != ufrom)
			{
				var cp = Expression.Convert(expr, ufrom);

				ex = GetConverter(mappingSchema, cp, ufrom, to);

				if (ex == null && to != uto)
				{
					ex = GetConverter(mappingSchema, cp, ufrom, uto);

					if (ex != null)
						ex = ValueTuple.Create(Expression.Convert(ex.Value.Item1, to) as Expression, ex.Value.Item2);
				}
			}

			if (ex == null && to != uto)
			{
				ex = GetConverter(mappingSchema, expr, from, uto);

				if (ex != null)
					ex = ValueTuple.Create(Expression.Convert(ex.Value.Item1, to) as Expression, ex.Value.Item2);
			}

			return ex;
		}

		public static ValueTuple<LambdaExpression,LambdaExpression,bool> GetConverter(
			MappingSchema mappingSchema,
			Type from,
			Type to)
		{
			if (mappingSchema == null)
				mappingSchema = MappingSchema.Default;

			var p  = Expression.Parameter(from, "p");
			var ne = null as LambdaExpression;

			if (from == to)
				return ValueTuple.Create(Expression.Lambda(p, p), ne, false);

			if (to == typeof(object))
				return ValueTuple.Create(Expression.Lambda(Expression.Convert(p, typeof(object)), p), ne, false);

			var ex =
				GetConverter     (mappingSchema, p, from, to) ??
				ConvertUnderlying(mappingSchema, p, from, from.ToNullableUnderlying(), to, to.ToNullableUnderlying()) ??
				ConvertUnderlying(mappingSchema, p, from, from.ToUnderlying(),         to, to.ToUnderlying());

			if (ex != null)
			{
				ne = Expression.Lambda(ex.Value.Item1, p);

				if (from.IsNullable())
					ex = ValueTuple.Create(
						Expression.Condition(
							Expression.PropertyOrField(p, "HasValue"),
							ex.Value.Item1,
							new DefaultValueExpression(mappingSchema, to)) as Expression,
						ex.Value.Item2);
				else if (from.IsClass)
					ex = ValueTuple.Create(
						Expression.Condition(Expression.NotEqual(p, Expression.Constant(null, from)), ex.Value.Item1, new DefaultValueExpression(mappingSchema, to)) as Expression,
						ex.Value.Item2);
			}

			if (ex != null)
				return ValueTuple.Create(Expression.Lambda(ex.Value.Item1, p), ne, ex.Value.Item2);

			if (to.IsNullable())
			{
				var uto = to.ToNullableUnderlying();

				var defex = Expression.Call(_defaultConverter,
					Expression.Convert(p, typeof(object)),
					Expression.Constant(uto)) as Expression;

				if (defex.Type != uto)
					defex = Expression.Convert(defex, uto);

				defex = GetCtor(uto, to, defex);

				return ValueTuple.Create(Expression.Lambda(defex, p), ne, false);
			}
			else
			{
				var defex = Expression.Call(_defaultConverter,
					Expression.Convert(p, typeof(object)),
					Expression.Constant(to)) as Expression;

				if (defex.Type != to)
					defex = Expression.Convert(defex, to);

				return ValueTuple.Create(Expression.Lambda(defex, p), ne, false);
			}
		}

#region Default Enum Mapping Type

		public static Type GetDefaultMappingFromEnumType(MappingSchema mappingSchema, Type enumType)
		{
			var type = enumType.ToNullableUnderlying();

			if (!type.IsEnum)
				return null;

			var fields =
			(
				from f in type.GetFields()
				where (f.Attributes & _enumField) == _enumField
				let attrs = mappingSchema.GetAttributes<MapValueAttribute>(f, a => a.Configuration)
				select
				(
					from a in attrs
					where a.Configuration == attrs[0].Configuration
					orderby !a.IsDefault
					select a
				).ToList()
			).ToList();

			Type defaultType = null;

			if (fields.All(attrs => attrs.Count != 0))
			{
				var attr = fields.FirstOrDefault(attrs => attrs[0].Value != null);

				if (attr != null)
				{
					var valueType = attr[0].Value.GetType();

					if (fields.All(attrs => attrs[0].Value == null || attrs[0].Value.GetType() == valueType))
						defaultType = valueType;
				}
			}

			if (defaultType == null)
				defaultType = Enum.GetUnderlyingType(type);

			if (enumType.IsNullable() && !defaultType.IsClass && !defaultType.IsNullable())
				defaultType = typeof(Nullable<>).MakeGenericType(defaultType);

			return defaultType;
		}

#endregion
	}
}
#endif