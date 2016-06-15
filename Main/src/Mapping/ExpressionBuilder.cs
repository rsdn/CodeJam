using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using CodeJam.Collections;

namespace CodeJam.Mapping
{
	using Expressions;
	using Reflection;

	class ExpressionBuilder<TFrom,TTo>
	{
		public ExpressionBuilder(MapperBuilder<TFrom,TTo> mapperBuilder, Tuple<MemberInfo[],LambdaExpression>[] memberMappers)
		{
			_mapperBuilder = mapperBuilder;
			_memberMappers = memberMappers;
		}

		readonly MapperBuilder<TFrom,TTo>                         _mapperBuilder;
		readonly Tuple<MemberInfo[],LambdaExpression>[]           _memberMappers;
		readonly Dictionary<Tuple<Type,Type>,ParameterExpression> _mappers     = new Dictionary<Tuple<Type,Type>,ParameterExpression>();
		readonly List<Expression>                                 _expressions = new List<Expression>();
		readonly List<ParameterExpression>                        _locals      = new List<ParameterExpression>();

		#region GetExpressionEx

		public Expression<Func<TFrom,TTo>> GetExpressionEx()
		{
			if (_mapperBuilder.MappingSchema.IsScalarType(typeof(TFrom)) || _mapperBuilder.MappingSchema.IsScalarType(typeof(TTo)))
				return _mapperBuilder.MappingSchema.GetConvertExpression<TFrom, TTo>();

			var pFrom = Expression.Parameter(typeof(TFrom), "from");

			Expression expr;

			if (_mapperBuilder.MappingSchema.IsScalarType(typeof(TFrom)) || _mapperBuilder.MappingSchema.IsScalarType(typeof(TTo)))
				expr = GetExpressionExImpl(pFrom, typeof(TTo));
			else if (_mapperBuilder.ProcessCrossReferences == true)
				expr = new MappingImpl(
					this,
					pFrom,
					Expression.Constant(null, typeof(TTo)),
					Expression.Parameter(typeof(IDictionary<object,object>), "dic"))
					.GetExpressionWithDic();
			else
				expr = GetExpressionExImpl(pFrom, typeof(TTo));

			var l = Expression.Lambda<Func<TFrom,TTo>>(
				_locals.Count > 0 || _expressions.Count > 0 ?
					Expression.Block(_locals, _expressions.Concat(expr)) :
					expr,
				pFrom);

			return l;
		}

		Expression GetExpressionExImpl(Expression fromExpression, Type toType)
		{
			var fromAccessor = TypeAccessor.GetAccessor(fromExpression.Type);
			var toAccessor   = TypeAccessor.GetAccessor(toType);
			var binds        = new List<MemberAssignment>();

			foreach (var toMember in toAccessor.Members.Where(_mapperBuilder.MemberFilter))
			{
				if (!toMember.HasSetter)
					continue;

				if (_memberMappers != null)
				{
					var processed = false;

					foreach (var item in _memberMappers)
					{
						if (item.Item1.Length == 1 && item.Item1[0] == toMember.MemberInfo)
						{
							binds.Add(BuildAssignment(item.Item2, fromExpression, item.Item2.Type, toMember));
							processed = true;
							break;
						}
					}

					if (processed)
						continue;
				}

				Dictionary<string,string> mapDic;
				string toName;

				if (_mapperBuilder.ToMappingDictionary == null ||
					!_mapperBuilder.ToMappingDictionary.TryGetValue(toType, out mapDic) ||
					!mapDic.TryGetValue(toMember.Name, out toName))
					toName = toMember.Name;

				var fromMember = fromAccessor.Members.FirstOrDefault(mi =>
				{
					string fromName;
					if (_mapperBuilder.FromMappingDictionary == null ||
						!_mapperBuilder.FromMappingDictionary.TryGetValue(fromExpression.Type, out mapDic) ||
						!mapDic.TryGetValue(mi.Name, out fromName))
						fromName = mi.Name;
					return fromName == toName;
				});

				if (fromMember == null || !fromMember.HasGetter)
					continue;

				var getter = fromMember.GetterExpression;

				if (_mapperBuilder.MappingSchema.IsScalarType(fromMember.Type) || _mapperBuilder.MappingSchema.IsScalarType(toMember.Type))
				{
					binds.Add(BuildAssignment(getter, fromExpression, fromMember.Type, toMember));
				}
				else if (fromMember.Type == toMember.Type && _mapperBuilder.DeepCopy == false)
				{
					binds.Add(Expression.Bind(toMember.MemberInfo, getter.ReplaceParameters(fromExpression)));
				}
				else
				{
					var getValue = getter.ReplaceParameters(fromExpression);
					var expr     = Expression.Condition(
						Expression.Equal(getValue, Expression.Constant(null, getValue.Type)),
						Expression.Constant(_mapperBuilder.MappingSchema.GetDefaultValue(toMember.Type), toMember.Type),
						BuildClassMapper(getValue, toMember));

					binds.Add(Expression.Bind(toMember.MemberInfo, expr));
				}
			}

			var newExpression  = Expression.New(toType.GetDefaultConstructor());
			var initExpression = binds.Count > 0 ? (Expression)Expression.MemberInit(newExpression, binds) : newExpression;

			return initExpression;
		}

		Expression BuildClassMapper(Expression getValue, MemberAccessor toMember)
		{
			var fromType = getValue.Type;
			var toType   = toMember.Type;

			if (toType.IsSubClass(typeof(IEnumerable<>)) && fromType.IsSubClass(typeof(IEnumerable<>)))
			{
				var fromItemType = fromType.GetItemType();
				var toItemType   = toType.  GetItemType();

				if (toType.IsGenericType && !toType.IsGenericTypeDefinition)
				{
					var toDefinition = toType.GetGenericTypeDefinition();

					if (toDefinition == typeof(List<>))
					{
						return ExpressionBuilderHelper.ToList(_mapperBuilder, getValue, fromItemType, toItemType);
					}
				}
			}

			return GetExpressionExImpl(getValue, toMember.Type);
		}

		MemberAssignment BuildAssignment(LambdaExpression getter, Expression fromExpression, Type fromMemberType, MemberAccessor toMember)
		{
			var getValue = getter.ReplaceParameters(fromExpression);
			var expr     = _mapperBuilder.MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
			var convert  = expr.ReplaceParameters(getValue);

			return Expression.Bind(toMember.MemberInfo, convert);
		}

		#endregion

		#region GetExpression

		public Expression<Func<TFrom,TTo,IDictionary<object,object>,TTo>> GetExpression()
		{
			var pFrom = Expression.Parameter(typeof(TFrom), "from");
			var pTo   = Expression.Parameter(typeof(TTo),   "to");
			var pDic  = Expression.Parameter(typeof(IDictionary<object,object>), "dic");

			if (_mapperBuilder.MappingSchema.IsScalarType(typeof(TFrom)) || _mapperBuilder.MappingSchema.IsScalarType(typeof(TTo)))
			{
				var type = _mapperBuilder.MappingSchema.IsScalarType(typeof(TFrom)) ? typeof(TFrom) : typeof(TTo);

				return Expression.Lambda<Func<TFrom,TTo,IDictionary<object,object>,TTo>>(
					Expression.Throw(
						Expression.New(
							InfoOf.Constructor(() => new ArgumentException("")),
							Expression.Constant($"Type {type.FullName} cannot be a scalar type. To convert scalar types use ConvertTo<TTo>.From(TFrom value).")),
						typeof(TTo)),
					pFrom,
					pTo,
					pDic);
			}

			var expr = new MappingImpl(this, pFrom, pTo, pDic).GetExpression();

			var l = Expression.Lambda<Func<TFrom,TTo,IDictionary<object,object>,TTo>>(
				_locals.Count > 0 || _expressions.Count > 0 ?
					Expression.Block(_locals, _expressions.Concat(expr)) :
					expr,
				pFrom, pTo, pDic);

			return l;
		}

		class MappingImpl
		{
			public MappingImpl(ExpressionBuilder<TFrom,TTo> builder, Expression fromExpression, Expression toExpression, ParameterExpression pDic)
			{
				_builder        = builder;
				_fromExpression = fromExpression;
				_toExpression   = toExpression;
				_pDic           = pDic;
				_localObject    = Expression.Parameter(_toExpression.Type, "obj" + ++_nameCounter);
				_fromAccessor   = TypeAccessor.GetAccessor(_fromExpression.Type);
				_toAccessor     = TypeAccessor.GetAccessor(_toExpression.  Type);
			}

			static int _nameCounter;

			readonly ExpressionBuilder<TFrom,TTo> _builder;
			readonly Expression                   _fromExpression;
			readonly Expression                   _toExpression;
			readonly ParameterExpression          _pDic;
			readonly ParameterExpression          _localObject;
			readonly TypeAccessor                 _fromAccessor;
			readonly TypeAccessor                 _toAccessor;
			readonly List<Expression>             _expressions = new List<Expression>();
			readonly List<ParameterExpression>    _locals      = new List<ParameterExpression>();

			public Expression GetExpressionWithDic()
			{
				_locals.Add(_pDic);

				_expressions.Add(Expression.Assign(
					_pDic,
					Expression.New(InfoOf.Constructor(() => new Dictionary<object,object>()))));

				return GetExpression();
			}

			public Expression GetExpression()
			{
				_locals.Add(_localObject);

				_expressions.Add(Expression.Assign(
					_localObject,
					Expression.Condition(
						Expression.Equal(_toExpression, Expression.Constant(null, _toExpression.Type)),
						Expression.New(_toExpression.Type.GetDefaultConstructor(true)),
						_toExpression)));

				if (_builder._mapperBuilder.ProcessCrossReferences != false)
				{
					_expressions.Add(
						Expression.Call(
							InfoOf.Method(() => ExpressionBuilderHelper.Add(null, null, null)),
							_pDic,
							_fromExpression,
							_localObject));
				}

				foreach (var toMember in _toAccessor.Members.Where(_builder._mapperBuilder.MemberFilter))
				{
					if (!toMember.HasSetter)
						continue;

					var setter = toMember.SetterExpression;

					if (_builder._memberMappers != null)
					{
						var processed = false;

						foreach (var item in _builder._memberMappers)
						{
							if (item.Item1.Length == 1 && item.Item1[0] == toMember.MemberInfo)
							{
								_expressions.Add(BuildAssignment(item.Item2, setter, item.Item2.Type, _localObject, toMember));
								processed = true;
								break;
							}
						}

						if (processed)
							continue;
					}

					Dictionary<string,string> mapDic;
					string toName;

					if (_builder._mapperBuilder.ToMappingDictionary == null ||
						!_builder._mapperBuilder.ToMappingDictionary.TryGetValue(_toExpression.Type, out mapDic) ||
						!mapDic.TryGetValue(toMember.Name, out toName))
						toName = toMember.Name;

					var fromMember = _fromAccessor.Members.FirstOrDefault(mi =>
					{
						string fromName;
						if (_builder._mapperBuilder.FromMappingDictionary == null ||
							!_builder._mapperBuilder.FromMappingDictionary.TryGetValue(_fromExpression.Type, out mapDic) ||
							!mapDic.TryGetValue(mi.Name, out fromName))
							fromName = mi.Name;
						return fromName == toName;
					});

					if (fromMember == null || !fromMember.HasGetter)
						continue;

					var getter = fromMember.GetterExpression;

					if (_builder._mapperBuilder.MappingSchema.IsScalarType(fromMember.Type) || _builder._mapperBuilder.MappingSchema.IsScalarType(toMember.Type))
					{
						_expressions.Add(BuildAssignment(getter, setter, fromMember.Type, _localObject, toMember));
					}
					else if (fromMember.Type == toMember.Type && _builder._mapperBuilder.DeepCopy == false)
					{
						_expressions.Add(setter.ReplaceParameters(_localObject, getter.ReplaceParameters(_fromExpression)));
					}
					else
					{
						var getValue = getter.ReplaceParameters(_fromExpression);
						var expr     = Expression.IfThenElse(
							// if (from == null)
							Expression.Equal(getValue, Expression.Constant(null, getValue.Type)),
							//   localObject = null;
							setter.ReplaceParameters(
								_localObject,
								Expression.Constant(_builder._mapperBuilder.MappingSchema.GetDefaultValue(toMember.Type), toMember.Type)),
							// else
							toMember.HasGetter ?
								setter.ReplaceParameters(_localObject, BuildClassMapper(setter, getValue, toMember)) :
								setter.ReplaceParameters(_localObject, _builder.GetExpressionExImpl(getValue, toMember.Type)));

						_expressions.Add(expr);
					}
				}

				_expressions.Add(_localObject);

				return Expression.Block(_locals, _expressions);
			}

			Expression BuildClassMapper(LambdaExpression setter, Expression getValue, MemberAccessor toMember)
			{
				var cacheMapper = _builder._mapperBuilder.ProcessCrossReferences != false;
				var key         = Tuple.Create(_fromExpression.Type, toMember.Type);
				var pFrom       = Expression.Parameter(getValue.Type, "pFrom");
				var pTo         = Expression.Parameter(toMember.Type, "pTo");
				var toObj       = toMember.GetterExpression.ReplaceParameters(_localObject);

				ParameterExpression l = null;

				if (cacheMapper)
				{
					if (_builder._mappers.TryGetValue(key, out l))
						return Expression.Invoke(l, getValue, toObj, _pDic);

					l = Expression.Parameter(Expression.Lambda(Expression.Constant(null, toMember.Type), pFrom, pTo, _pDic).Type);

					_builder._mappers.Add(key, l);
					_builder._locals. Add(l);
				}

				var expr = BuildListMapper(cacheMapper, getValue, toMember);

				if (expr == null)
					expr = new MappingImpl(_builder, cacheMapper? pFrom : getValue, cacheMapper? pTo : toObj, _pDic).GetExpression();

				if (cacheMapper)
				{
					var lex =
						Expression.Lambda(
							Expression.Convert(
								Expression.Coalesce(
									Expression.Call(
										InfoOf<IDictionary<object,object>>.Method(_ => ExpressionBuilderHelper.GetValue(null, null)),
										_pDic,
										pFrom),
									expr),
								toMember.Type),
							pFrom,
							pTo,
							_pDic);

					_builder._expressions.Add(Expression.Assign(l, lex));

					expr = Expression.Invoke(l, getValue, toObj, _pDic);
				}

				return expr;
			}

			Expression BuildListMapper(bool cacheMapper, Expression getValue, MemberAccessor toMember)
			{
				var fromListType = getValue.Type;
				var toListType   = toMember.Type;

				Expression expr = null;

				if (toListType.IsSubClass(typeof(IEnumerable<>)) && fromListType.IsSubClass(typeof(IEnumerable<>)))
				{
					var fromItemType = fromListType.GetItemType();
					var toItemType   = toListType.  GetItemType();

					if (toListType.IsGenericType && !toListType.IsGenericTypeDefinition)
					{
						var toDefinition = toListType.GetGenericTypeDefinition();

						if (toDefinition == typeof(List<>))
						{
							expr = ExpressionBuilderHelper.ToList(_builder._mapperBuilder, getValue, fromItemType, toItemType);
						}
					}
				}

				if (expr != null && cacheMapper)
				{
					expr = Expression.IfThenElse(
						// if (from == null)
						Expression.Equal(getValue, Expression.Constant(null, getValue.Type)),
						//   localObject = null;
						Expression.Constant(_builder._mapperBuilder.MappingSchema.GetDefaultValue(expr.Type), expr.Type),
						// else
						expr);
				}

				return expr;
			}

			Expression BuildAssignment(
				LambdaExpression getter,
				LambdaExpression setter,
				Type fromMemberType,
				Expression toExpression,
				MemberAccessor toMember)
			{
				var getValue = getter.ReplaceParameters(_fromExpression);
				var expr     = _builder._mapperBuilder.MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
				var convert  = expr.ReplaceParameters(getValue);

				return setter.ReplaceParameters(toExpression, convert);
			}
		}

		#endregion
	}

	static class ExpressionBuilderHelper
	{
		public static object GetValue(IDictionary<object,object> dic, object key)
		{
			object result;
			return dic != null && dic.TryGetValue(key, out result) ? result : null;
		}

		public static void Add(IDictionary<object,object> dic, object key, object value)
		{
			if (key != null && dic != null)
				dic[key] = value;
		}

		static IMapperBuilder GetBuilder<TFrom,TTo>(IMapperBuilder builder)
			=> new MapperBuilder<TFrom,TTo>
			{
				MappingSchema          = builder.MappingSchema,
				MemberMappers          = builder.MemberMappers,
				FromMappingDictionary  = builder.FromMappingDictionary,
				ToMappingDictionary    = builder.ToMappingDictionary,
				MemberFilter           = builder.MemberFilter,
				ProcessCrossReferences = builder.ProcessCrossReferences,
				DeepCopy               = builder.DeepCopy,
			};

		public static Expression ToList(IMapperBuilder builder, Expression getValue, Type fromItemType, Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => Enumerable.ToList<int>(null)).GetGenericMethodDefinition();
			var expr       = Select(builder, getValue, fromItemType, toItemType);

			return Expression.Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		public static Expression ToArray(IMapperBuilder builder, Expression getValue, Type fromItemType, Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => Enumerable.ToArray<int>(null)).GetGenericMethodDefinition();
			var expr       = Select(builder, getValue, fromItemType, toItemType);

			return Expression.Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		public static Expression Select(IMapperBuilder builder, Expression getValue, Type fromItemType, Type toItemType)
		{
			var getBuilderInfo = InfoOf.Method(() => GetBuilder<int,int>(null)).               GetGenericMethodDefinition();
			var selectInfo     = InfoOf.Method(() => Enumerable.Select<int,int>(null, _ => _)).GetGenericMethodDefinition();
			var itemBuilder    = (IMapperBuilder)getBuilderInfo.MakeGenericMethod(fromItemType, toItemType).Invoke(null, new object[] { builder });

			var expr = getValue;

			if (builder.DeepCopy != false || fromItemType != toItemType)
			{
				expr = Expression.Call(
					selectInfo.MakeGenericMethod(fromItemType, toItemType),
					getValue,
					itemBuilder.GetMapperLambdaExpressionEx());
			}

			return expr;
		}
	}
}
