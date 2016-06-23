#if !FW35
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	using Collections;
	using Expressions;
	using Metadata;
	using Reflection;

	/// <summary>
	/// Providers object / value mapping support.
	/// </summary>
	[PublicAPI]
	public class MappingSchema
	{
		#region Init

		/// <summary>
		/// Create an instance of <seealso cref="MappingSchema"/>.
		/// </summary>
		public MappingSchema()
			: this(null, (MappingSchema[])null)
		{
		}

		/// <summary>
		/// Create an instance of <seealso cref="MappingSchema"/>.
		/// </summary>
		/// <param name="schemas">Base schemas.</param>
		public MappingSchema(params MappingSchema[] schemas)
			: this(null, schemas)
		{
		}

		/// <summary>
		/// Create an instance of <seealso cref="MappingSchema"/>.
		/// </summary>
		/// <param name="configuration">Configuration name.</param>
		public MappingSchema(string configuration/* ??? */)
			: this(configuration, null)
		{
		}

		/// <summary>
		/// Create an instance of <seealso cref="MappingSchema"/>.
		/// </summary>
		/// <param name="configuration">Configuration name.</param>
		/// <param name="schemas">Base schemas.</param>
		public MappingSchema(string configuration, params MappingSchema[] schemas)
		{
			var schemaInfo = new MappingSchemaInfo(configuration);

			if (schemas == null || schemas.Length == 0)
			{
				Schemas = new[] { schemaInfo, Default.Schemas[0] };
			}
			else if (schemas.Length == 1)
			{
				Schemas = new MappingSchemaInfo[1 + schemas[0].Schemas.Length];
				Schemas[0] = schemaInfo;
				Array.Copy(schemas[0].Schemas, 0, Schemas, 1, schemas[0].Schemas.Length);
			}
			else
			{
				var schemaList = new List<MappingSchemaInfo>(10) { schemaInfo };

				foreach (var schema in schemas)
				{
					foreach (var sc in schema.Schemas)
					{
						if (schemaList.Contains(sc))
							schemaList.Remove(sc);
						schemaList.Add(sc);
					}
				}

				Schemas = schemaList.ToArray();
			}
		}

		internal readonly MappingSchemaInfo[] Schemas;

		#endregion

		#region Default Values
		private const FieldAttributes _enumLookup =
			FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal;

		/// <summary>
		/// Gets default value for provided <see cref="Type"/>.
		/// </summary>
		/// <param name="type"><see cref="Type"/> to get default value.</param>
		/// <returns>Default value of the provided <see cref="Type"/></returns>
		[Pure]
		public object GetDefaultValue([NotNull] Type type)
		{
			Code.NotNull(type, nameof(type));

			foreach (var info in Schemas)
			{
				var o = info.GetDefaultValue(type);
				if (o.HasValue)
					return o.Value;
			}

			if (type.IsEnum)
			{
				var mapValues = GetMapValues(type);

				if (mapValues != null)
				{
					var fields =
						from f in mapValues
						where f.MapValues.Any(a => a.Value == null)
						select f.OrigValue;

					var value = fields.FirstOrDefault();

					if (value != null)
					{
						SetDefaultValue(type, value);
						return value;
					}
				}
			}

			return DefaultValue.GetValue(type, this);
		}

		/// <summary>
		/// Sets default value for provided <see cref="Type"/>.
		/// </summary>
		/// <param name="type">Type to set default value for.</param>
		/// <param name="value">Value to set.</param>
		public void SetDefaultValue([NotNull] Type type, object value)
		{
			Code.NotNull(type, nameof(type));
			Schemas[0].SetDefaultValue(type, value);
		}

		#endregion

		#region GenericConvertProvider

		// Should be public.
		//
		private void InitGenericConvertProvider<T>() => InitGenericConvertProvider(typeof(T));

		private bool InitGenericConvertProvider(params Type[] types) =>
			Schemas.Aggregate(false, (cur, info) => cur || info.InitGenericConvertProvider(types, this));

		private void SetGenericConvertProvider(Type type)
		{
			if (!type.IsGenericTypeDefinition)
				throw new CodeJamMappingException($"'{type}' must be a generic type.");

			Schemas[0].SetGenericConvertProvider(type);
		}

		#endregion

		#region Convert

		/// <summary>
		/// Returns an object of a specified type whose value is equivalent to a specified object.
		/// </summary>
		/// <param name="value">An object to convert.</param>
		/// <typeparam name="T">The type of object to return.</typeparam>
		/// <returns>An object whose type is <i>conversionType</i> and whose value is equivalent to <i>value</i>.</returns>
		public T ChangeTypeTo<T>(object value)
			=> Converter.ChangeTypeTo<T>(value, this);

		/// <summary>
		/// Returns an object of a specified type whose value is equivalent to a specified object.
		/// </summary>
		/// <param name="value">An object to convert.</param>
		/// <param name="conversionType">The type of object to return.</param>
		/// <returns>An object whose type is <i>conversionType</i> and whose value is equivalent to <i>value</i>.</returns>
		public object ChangeType(object value, Type conversionType)
			=> Converter.ChangeType(value, conversionType, this);

		/// <summary>
		/// Converts enum to its map value.
		/// </summary>
		/// <param name="value">Value to convert.</param>
		/// <returns>Mapped value.</returns>
		public object EnumToValue(Enum value)
		{
			var toType = ConvertBuilder.GetDefaultMappingFromEnumType(this, value.GetType());
			return Converter.ChangeType(value, toType, this);
		}

		/// <summary>
		/// Returns an expression that converts a value of type <i>TFrom</i> to <i>TTo</i> or null.
		/// </summary>
		/// <param name="from">Type to convert from.</param>
		/// <param name="to">Type to convert to.</param>
		/// <returns>Convert expression.</returns>
		// ReSharper disable once VirtualMemberNeverOverriden.Global
		protected internal virtual LambdaExpression TryGetConvertExpression(Type from, Type to)
		{
			var li = GetConverter(from, to, false);
			return li == null ? null : (LambdaExpression)ReduceDefaultValue(li.CheckNullLambda);
		}

		internal ConcurrentDictionary<object,Func<object,object>> Converters
			=> Schemas[0].Converters;

		/// <summary>
		/// Returns an expression that converts a value of type <i>TFrom</i> to <i>TTo</i>.
		/// </summary>
		/// <typeparam name="TFrom">Type to convert from.</typeparam>
		/// <typeparam name="TTo">Type to convert to.</typeparam>
		/// <returns>Convert expression.</returns>
		public Expression<Func<TFrom,TTo>> GetConvertExpression<TFrom,TTo>()
		{
			var li = GetConverter(typeof(TFrom), typeof(TTo), true);
			return (Expression<Func<TFrom,TTo>>)ReduceDefaultValue(li.CheckNullLambda);
		}

		/// <summary>
		/// Returns an expression that converts a value of type <i>from</i> to <i>to</i>.
		/// </summary>
		/// <param name="from">Type to convert from.</param>
		/// <param name="to">Type to convert to.</param>
		/// <param name="checkNull">If <i>true</i>, created expression checks input value for <i>null</i>.</param>
		/// <param name="createDefault">If <i>true</i>, new expression is created.</param>
		/// <returns>Convert expression.</returns>
		public LambdaExpression GetConvertExpression(
			[NotNull] Type from,
			[NotNull] Type to,
			bool checkNull = true,
			bool createDefault = true)
		{
			Code.NotNull(from, nameof(from));
			Code.NotNull(to,   nameof(to));

			var li = GetConverter(from, to, createDefault);
			return li == null ? null : (LambdaExpression)ReduceDefaultValue(checkNull ? li.CheckNullLambda : li.Lambda);
		}

		/// <summary>
		/// Returns converter from a value of type <i>TFrom</i> to <i>TTo</i>.
		/// </summary>
		/// <typeparam name="TFrom">Type to convert from.</typeparam>
		/// <typeparam name="TTo">Type to convert to.</typeparam>
		/// <returns>Convert function.</returns>
		public Func<TFrom,TTo> GetConverter<TFrom,TTo>()
		{
			var li = GetConverter(typeof(TFrom), typeof(TTo), true);

			if (li.Delegate == null)
			{
				var rex = (Expression<Func<TFrom,TTo>>)ReduceDefaultValue(li.CheckNullLambda);
				var l   = rex.Compile();

				Schemas[0].SetConvertInfo(typeof(TFrom), typeof(TTo), new ConvertInfo.LambdaInfo(li.CheckNullLambda, null, l, li.IsSchemaSpecific));

				return l;
			}

			return (Func<TFrom,TTo>)li.Delegate;
		}

		/// <summary>
		/// Adds an expression that converts a value of type <i>fromType</i> to <i>toType</i>.
		/// </summary>
		/// <param name="fromType">Type to convert from.</param>
		/// <param name="toType">Type to convert to.</param>
		/// <param name="expr">Expression to set.</param>
		/// <param name="addNullCheck">If <i>true</i>, adds an expression to check null value.</param>
		public void SetConvertExpression(
			[NotNull] Type fromType,
			[NotNull] Type toType,
			[NotNull] LambdaExpression expr,
			bool addNullCheck = true)
		{
			Code.NotNull(fromType, nameof(fromType));
			Code.NotNull(toType,   nameof(toType));
			Code.NotNull(expr,     nameof(expr));

			var ex = addNullCheck && expr.Find(Converter.IsDefaultValuePlaceHolder) == null?
				AddNullCheck(expr) :
				expr;

			Schemas[0].SetConvertInfo(fromType, toType, new ConvertInfo.LambdaInfo(ex, expr, null, false));
		}

		/// <summary>
		/// Adds an expression that converts a value of type <i>fromType</i> to <i>toType</i>.
		/// </summary>
		/// <typeparam name="TFrom">Type to convert from.</typeparam>
		/// <typeparam name="TTo">Type to convert to.</typeparam>
		/// <param name="expr">Expression to set.</param>
		/// <param name="addNullCheck">If <i>true</i>, adds an expression to check null value.</param>
		public void SetConvertExpression<TFrom,TTo>(
			[NotNull] Expression<Func<TFrom,TTo>> expr,
			bool addNullCheck = true)
		{
			Code.NotNull(expr, nameof(expr));

			var ex = addNullCheck && expr.Find(Converter.IsDefaultValuePlaceHolder) == null?
				AddNullCheck(expr) :
				expr;

			Schemas[0].SetConvertInfo(typeof(TFrom), typeof(TTo), new ConvertInfo.LambdaInfo(ex, expr, null, false));
		}

		/// <summary>
		/// Adds an expression that converts a value of type <i>fromType</i> to <i>toType</i>.
		/// </summary>
		/// <typeparam name="TFrom">Type to convert from.</typeparam>
		/// <typeparam name="TTo">Type to convert to.</typeparam>
		/// <param name="checkNullExpr">Null check expression.</param>
		/// <param name="expr">Convert expression.</param>
		public void SetConvertExpression<TFrom,TTo>(
			[NotNull] Expression<Func<TFrom,TTo>> checkNullExpr,
			[NotNull] Expression<Func<TFrom,TTo>> expr)
		{
			Code.NotNull(expr, nameof(expr));

			Schemas[0].SetConvertInfo(typeof(TFrom), typeof(TTo), new ConvertInfo.LambdaInfo(checkNullExpr, expr, null, false));
		}

		/// <summary>
		/// Adds a function expression that converts a value of type <i>fromType</i> to <i>toType</i>.
		/// </summary>
		/// <typeparam name="TFrom">Type to convert from.</typeparam>
		/// <typeparam name="TTo">Type to convert to.</typeparam>
		/// <param name="func">Convert function.</param>
		public void SetConverter<TFrom,TTo>([NotNull] Func<TFrom,TTo> func)
		{
			Code.NotNull(func, nameof(func));

			var p  = Expression.Parameter(typeof(TFrom), "p");
			var ex = Expression.Lambda<Func<TFrom,TTo>>(Expression.Invoke(Expression.Constant(func), p), p);

			Schemas[0].SetConvertInfo(typeof(TFrom), typeof(TTo), new ConvertInfo.LambdaInfo(ex, null, func, false));
		}

		private LambdaExpression AddNullCheck(LambdaExpression expr)
		{
			var p = expr.Parameters[0];

			if (p.Type.IsNullable())
				return Expression.Lambda(
					Expression.Condition(
						Expression.PropertyOrField(p, "HasValue"),
						expr.Body,
						new DefaultValueExpression(this, expr.Body.Type)),
					expr.Parameters);

			if (p.Type.IsClass)
				return Expression.Lambda(
					Expression.Condition(
						Expression.NotEqual(p, Expression.Constant(null, p.Type)),
						expr.Body,
						new DefaultValueExpression(this, expr.Body.Type)),
					expr.Parameters);

			return expr;
		}

		private ConvertInfo.LambdaInfo GetConverter(Type from, Type to, bool create)
		{
			for (var i = 0; i < Schemas.Length; i++)
			{
				var info = Schemas[i];
				var li   = info.GetConvertInfo(from, to);

				if (li != null && (i == 0 || !li.IsSchemaSpecific))
					return i == 0 ? li : new ConvertInfo.LambdaInfo(li.CheckNullLambda, li.CheckNullLambda, null, false);
			}

			var isFromGeneric = from.IsGenericType && !from.IsGenericTypeDefinition;
			var isToGeneric   = to.  IsGenericType && !to.  IsGenericTypeDefinition;

			if (isFromGeneric || isToGeneric)
			{
				var empty =
#if (!FW452)
						Array.Empty<Type>()
#else
					Array<Type>.Empty
#endif
					;
				var fromGenericArgs = isFromGeneric ? from.GetGenericArguments() : empty;
				var toGenericArgs   = isToGeneric   ? to.  GetGenericArguments() : empty;

				var args = fromGenericArgs.SequenceEqual(toGenericArgs) ?
					fromGenericArgs : fromGenericArgs.Concat(toGenericArgs).ToArray();

				if (InitGenericConvertProvider(args))
					// ReSharper disable once TailRecursiveCall
					return GetConverter(from, to, create);
			}

			if (create)
			{
				var ufrom = from.ToNullableUnderlying();
				var uto   = to.ToNullableUnderlying();

				LambdaExpression ex;
				var             ss = false;

				if (from != ufrom)
				{
					var li = GetConverter(ufrom, to, false);

					if (li != null)
					{
						var b  = li.CheckNullLambda.Body;
						var ps = li.CheckNullLambda.Parameters;

						// For int? -> byte try to find int -> byte and convert int to int?
						//
						var p = Expression.Parameter(from, ps[0].Name);

						ss = li.IsSchemaSpecific;
						ex = Expression.Lambda(
							b.Transform(e => e == ps[0] ? Expression.Convert(p, ufrom) : e),
							p);
					}
					else if (to != uto)
					{
						li = GetConverter(ufrom, uto, false);

						if (li != null)
						{
							var b  = li.CheckNullLambda.Body;
							var ps = li.CheckNullLambda.Parameters;

							// For int? -> byte? try to find int -> byte and convert int to int? and result to byte?
							//
							var p = Expression.Parameter(from, ps[0].Name);

							ss = li.IsSchemaSpecific;
							ex = Expression.Lambda(
								Expression.Convert(
									b.Transform(e => e == ps[0] ? Expression.Convert(p, ufrom) : e),
									to),
								p);
						}
						else
							ex = null;
					}
					else
						ex = null;
				}
				else if (to != uto)
				{
					// For int? -> byte? try to find int -> byte and convert int to int? and result to byte?
					//
					var li = GetConverter(from, uto, false);

					if (li != null)
					{
						var b  = li.CheckNullLambda.Body;
						var ps = li.CheckNullLambda.Parameters;

						ss = li.IsSchemaSpecific;
						ex = Expression.Lambda(Expression.Convert(b, to), ps);
					}
					else
						ex = null;
				}
				else
					ex = null;

				if (ex != null)
					return new ConvertInfo.LambdaInfo(AddNullCheck(ex), ex, null, ss);

				var d = ConvertInfo.Default.Get(from, to);

				if (d == null || d.IsSchemaSpecific)
					d = ConvertInfo.Default.Create(this, from, to);

				return new ConvertInfo.LambdaInfo(d.CheckNullLambda, d.Lambda, null, d.IsSchemaSpecific);
			}

			return null;
		}

		private Expression ReduceDefaultValue(Expression expr) =>
			expr.Transform(e =>
				Converter.IsDefaultValuePlaceHolder(e)
					? Expression.Constant(GetDefaultValue(e.Type), e.Type)
					: e);

		/// <summary>
		/// Initializes culture specific converters.
		/// </summary>
		/// <param name="info">Instance of <seealso cref="CultureInfo"/></param>
		public void SetCultureInfo([NotNull] CultureInfo info)
		{
			Code.NotNull(info, nameof(info));

			SetConvertExpression((sbyte     v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((sbyte?    v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>             sbyte.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) =>     (sbyte?)sbyte.Parse(s, info.NumberFormat));

			SetConvertExpression((short     v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((short?    v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>             short.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) =>     (short?)short.Parse(s, info.NumberFormat));

			SetConvertExpression((int       v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((int?      v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>               int.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) =>         (int?)int.Parse(s, info.NumberFormat));

			SetConvertExpression((long      v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((long?     v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>              long.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) =>       (long?)long.Parse(s, info.NumberFormat));

			SetConvertExpression((byte      v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((byte?     v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>              byte.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) =>       (byte?)byte.Parse(s, info.NumberFormat));

			SetConvertExpression((ushort    v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((ushort?   v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>            ushort.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) =>   (ushort?)ushort.Parse(s, info.NumberFormat));

			SetConvertExpression((uint      v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((uint?     v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>              uint.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) =>       (uint?)uint.Parse(s, info.NumberFormat));

			SetConvertExpression((ulong     v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((ulong?    v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>             ulong.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) =>     (ulong?)ulong.Parse(s, info.NumberFormat));

			SetConvertExpression((float     v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((float?    v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>             float.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) =>     (float?)float.Parse(s, info.NumberFormat));

			SetConvertExpression((double    v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((double?   v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>            double.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) =>   (double?)double.Parse(s, info.NumberFormat));

			SetConvertExpression((decimal   v) =>           v.      ToString(info.NumberFormat));
			SetConvertExpression((decimal?  v) =>           v.Value.ToString(info.NumberFormat));
			SetConvertExpression((string    s) =>           decimal.Parse(s, info.NumberFormat));
			SetConvertExpression((string    s) => (decimal?)decimal.Parse(s, info.NumberFormat));

			SetConvertExpression((DateTime  v) =>                       v.      ToString(info.DateTimeFormat));
			SetConvertExpression((DateTime? v) =>                       v.Value.ToString(info.DateTimeFormat));
			SetConvertExpression((string    s) =>                      DateTime.Parse(s, info.DateTimeFormat));
			SetConvertExpression((string    s) =>           (DateTime?)DateTime.Parse(s, info.DateTimeFormat));

			SetConvertExpression((DateTimeOffset  v) =>                 v.      ToString(info.DateTimeFormat));
			SetConvertExpression((DateTimeOffset? v) =>                 v.Value.ToString(info.DateTimeFormat));
			SetConvertExpression((string  s) =>                  DateTimeOffset.Parse(s, info.DateTimeFormat));
			SetConvertExpression((string  s) => (DateTimeOffset?)DateTimeOffset.Parse(s, info.DateTimeFormat));
		}

		#endregion

		#region MetadataReader

		/// <summary>
		/// Gets or sets metadata reader.
		/// </summary>
		public IMetadataReader MetadataReader
		{
			get { return Schemas[0].MetadataReader; }
			set
			{
				Schemas[0].MetadataReader = value;
				_metadataReaders = null;
			}
		}

		/// <summary>
		/// Adds metadata reader.
		/// </summary>
		/// <param name="reader">Instance of <see cref="IMetadataReader"/></param>
		public void AddMetadataReader([NotNull] IMetadataReader reader)
		{
			Code.NotNull(reader, nameof(reader));

			MetadataReader = MetadataReader == null ? reader : new MetadataReader(reader, MetadataReader);
		}

		private IMetadataReader[] _metadataReaders;
		private IMetadataReader[] MetadataReaders
		{
			get
			{
				if (_metadataReaders == null)
				{
					var hash = new HashSet<IMetadataReader>();
					var list = new List<IMetadataReader>();

					foreach (var s in Schemas)
						if (s.MetadataReader != null && hash.Add(s.MetadataReader))
							list.Add(s.MetadataReader);

					_metadataReaders = list.ToArray();
				}

				return _metadataReaders;
			}
		}

		/// <summary>
		/// Returns custom attributes applied to provided type.
		/// </summary>
		/// <param name="type">Object type</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this type are returned.</typeparam>
		/// <returns>Array of custom attributes.</returns>
		public T[] GetAttributes<T>(Type type, bool inherit = true)
			where T : Attribute
		{
			var q =
				from mr in MetadataReaders
				from a in mr.GetAttributes<T>(type, inherit)
				select a;

			return q.ToArray();
		}

		/// <summary>
		/// Returns custom attributes applied to provided type member.
		/// </summary>
		/// <param name="memberInfo">Type member.</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this member are returned.</typeparam>
		/// <returns>Array of custom attributes.</returns>
		public T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit = true)
			where T : Attribute
		{
			var q =
				from mr in MetadataReaders
				from a in mr.GetAttributes<T>(memberInfo, inherit)
				select a;

			return q.ToArray();
		}

		/// <summary>
		/// Returns custom attribute applied to provided type.
		/// </summary>
		/// <param name="type">Object type</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this type are returned.</typeparam>
		/// <returns>A custom attribute or <i>null</i>.</returns>
		public T GetAttribute<T>(Type type, bool inherit = true)
			where T : Attribute
		{
			var attrs = GetAttributes<T>(type, inherit);
			return attrs.Length == 0 ? null : attrs[0];
		}

		/// <summary>
		/// Returns custom attribute applied to provided type member.
		/// </summary>
		/// <param name="memberInfo">Type member.</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this member are returned.</typeparam>
		/// <returns>A custom attribute or <i>null</i>.</returns>
		public T GetAttribute<T>(MemberInfo memberInfo, bool inherit = true)
			where T : Attribute
		{
			var attrs = GetAttributes<T>(memberInfo, inherit);
			return attrs.Length == 0 ? null : attrs[0];
		}

		/// <summary>
		/// Returns custom attributes applied to provided type.
		/// </summary>
		/// <param name="type">Object type</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <param name="configGetter">A function that returns configuration value is supported by the attribute.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this type are returned.</typeparam>
		/// <returns>Array of custom attributes.</returns>
		public T[] GetAttributes<T>(Type type, Func<T,string> configGetter, bool inherit = true)
			where T : Attribute
		{
			var list  = new List<T>();
			var attrs = GetAttributes<T>(type, inherit);

			foreach (var c in ConfigurationList)
				foreach (var a in attrs)
					if (configGetter(a) == c)
						list.Add(a);

			return list.Concat(attrs.Where(a => string.IsNullOrEmpty(configGetter(a)))).ToArray();
		}

		/// <summary>
		/// Returns custom attributes applied to provided type member.
		/// </summary>
		/// <param name="memberInfo">Type member.</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <param name="configGetter">A function that returns configuration value is supported by the attribute.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this member are returned.</typeparam>
		/// <returns>Array of custom attributes.</returns>
		public T[] GetAttributes<T>(MemberInfo memberInfo, Func<T,string> configGetter, bool inherit = true)
			where T : Attribute
		{
			var list  = new List<T>();
			var attrs = GetAttributes<T>(memberInfo, inherit);

			foreach (var c in ConfigurationList)
				foreach (var a in attrs)
					if (configGetter(a) == c)
						list.Add(a);

			return list.Concat(attrs.Where(a => string.IsNullOrEmpty(configGetter(a)))).ToArray();
		}

		/// <summary>
		/// Returns custom attribute applied to provided type.
		/// </summary>
		/// <param name="type">Object type</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <param name="configGetter">A function that returns configuration value is supported by the attribute.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this type are returned.</typeparam>
		/// <returns>A custom attribute or <i>null</i>.</returns>
		public T GetAttribute<T>(Type type, Func<T,string> configGetter, bool inherit = true)
			where T : Attribute
		{
			var attrs = GetAttributes(type, configGetter, inherit);
			return attrs.Length == 0 ? null : attrs[0];
		}
		
		/// <summary>
		/// Returns custom attribute applied to provided type member.
		/// </summary>
		/// <param name="memberInfo">Type member.</param>
		/// <param name="inherit"><b>true</b> to search this member's inheritance chain to find the attributes; otherwise, <b>false</b>.</param>
		/// <param name="configGetter">A function that returns configuration value is supported by the attribute.</param>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this member are returned.</typeparam>
		/// <returns>A custom attribute or <i>null</i>.</returns>
		public T GetAttribute<T>(MemberInfo memberInfo, Func<T,string> configGetter, bool inherit = true)
			where T : Attribute
		{
			var attrs = GetAttributes(memberInfo, configGetter, inherit);
			return attrs.Length == 0 ? null : attrs[0];
		}

		#endregion

		#region Configuration

		private string _configurationID;

		/// <summary>
		/// Gets configuration ID.
		/// </summary>
		public  string  ConfigurationID => _configurationID ?? (_configurationID = string.Join(".", ConfigurationList));

		private string[] _configurationList;

		/// <summary>
		/// Configuration list.
		/// </summary>
		public  string[]  ConfigurationList
		{
			get
			{
				if (_configurationList == null)
				{
					var hash = new HashSet<string>();
					var list = new List<string>();

					foreach (var s in Schemas)
						if (!string.IsNullOrEmpty(s.Configuration) && hash.Add(s.Configuration))
							list.Add(s.Configuration);

					_configurationList = list.ToArray();
				}

				return _configurationList;
			}
		}

		#endregion

		#region DefaultMappingSchema

		internal MappingSchema(MappingSchemaInfo mappingSchemaInfo)
		{
			Schemas = new[] { mappingSchemaInfo };
		}

		/// <summary>
		/// Default mapping schema.
		/// </summary>
		public static MappingSchema Default = new DefaultMappingSchema();

		private class DefaultMappingSchema : MappingSchema
		{
			public DefaultMappingSchema()
				: base(new MappingSchemaInfo("") { MetadataReader = Metadata.MetadataReader.Default })
			{
				SetScalarType(typeof(string), true);
			}
		}

		#endregion

		#region Scalar Types

		/// <summary>
		/// <i>true</i> if value type is considered as scalar type.
		/// </summary>
		public bool IsStructIsScalarType { get; set; } = true;

		/// <summary>
		/// Returns <i>true</i> if provided type is considered as a scalar type.
		/// </summary>
		/// <param name="type">Type to check.</param>
		/// <returns></returns>
		public bool IsScalarType(Type type)
		{
			foreach (var info in Schemas)
			{
				var o = info.GetScalarType(type);
				if (o.HasValue)
					return o.Value;
			}

			var attr = GetAttribute<ScalarTypeAttribute>(type, a => a.Configuration);
			var ret  = false;

			if (attr != null)
			{
				ret = attr.IsScalar;
			}
			else
			{
				type = type.ToNullableUnderlying();

				if (type.IsEnum || type.IsPrimitive || IsStructIsScalarType && type.IsValueType)
					ret = true;
			}

			SetScalarType(type, ret);

			return ret;
		}

		/// <summary>
		/// Sets an scalar type indicator scalar for provided type.
		/// </summary>
		/// <param name="type">Type to set.</param>
		/// <param name="isScalarType">Acalar type indicator.</param>
		public void SetScalarType(Type type, bool isScalarType = true)
			=> Schemas[0].SetScalarType(type, isScalarType);

		/// <summary>
		/// Adds scalar type and its default value.
		/// </summary>
		/// <param name="type">Type to add</param>
		/// <param name="defaultValue">Default value.</param>
		public void AddScalarType(Type type, object defaultValue)
		{
			SetScalarType  (type);
			SetDefaultValue(type, defaultValue);
		}

		#endregion

		#region GetMapValues
		private ConcurrentDictionary<Type,MapValue[]> _mapValues;

		/// <summary>
		/// Returns mapping values for provided enum type.
		/// </summary>
		/// <param name="type">Type to get mapping values.</param>
		/// <returns>Array of mapping values.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="type" /> is null.</exception>
		// ReSharper disable once VirtualMemberNeverOverriden.Global
		public virtual MapValue[] GetMapValues([NotNull] Type type)
		{
			Code.NotNull(type, nameof(type));

			if (_mapValues == null)
				_mapValues = new ConcurrentDictionary<Type,MapValue[]>();

			MapValue[] mapValues;

			if (_mapValues.TryGetValue(type, out mapValues))
				return mapValues;

			var underlyingType = type.ToNullableUnderlying();

			if (underlyingType.IsEnum)
			{
				var fields =
				(
					from f in underlyingType.GetFields()
					where (f.Attributes & _enumLookup) == _enumLookup
					let attrs = GetAttributes<MapValueAttribute>(f, a => a.Configuration)
					select new MapValue(Enum.Parse(underlyingType, f.Name, false), attrs)
				).ToArray();

				if (fields.Any(f => f.MapValues.Length > 0))
					mapValues = fields;
			}

			_mapValues[type] = mapValues;

			return mapValues;
		}

		#endregion
	}
}
#endif