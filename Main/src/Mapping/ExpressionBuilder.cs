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

	class ExpressionBuilder
	{
		public ExpressionBuilder(IMapperBuilder mapperBuilder, Tuple<MemberInfo[],LambdaExpression>[] memberMappers)
		{
			_mapperBuilder = mapperBuilder;
			_memberMappers = memberMappers;
			_fromType      = mapperBuilder.FromType;
			_toType        = mapperBuilder.ToType;
		}

		readonly IMapperBuilder                                   _mapperBuilder;
		readonly Type                                             _fromType;
		readonly Type                                             _toType;
		readonly Tuple<MemberInfo[],LambdaExpression>[]           _memberMappers;
		readonly Dictionary<Tuple<Type,Type>,ParameterExpression> _mappers     = new Dictionary<Tuple<Type,Type>,ParameterExpression>();
		readonly List<Expression>                                 _expressions = new List<Expression>();
		readonly List<ParameterExpression>                        _locals      = new List<ParameterExpression>();

		static int _nameCounter;

		#region GetExpressionEx

		public LambdaExpression GetExpressionEx()
		{
			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
				return _mapperBuilder.MappingSchema.GetConvertExpression(_fromType, _toType);

			var pFrom = Expression.Parameter(_fromType, "from");

			Expression expr;

			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
			{
				expr = GetExpressionExImpl(pFrom, _toType);
			}
			else if (_mapperBuilder.ProcessCrossReferences == true)
			{
				var pDic = Expression.Parameter(typeof(IDictionary<object,object>), "dic" + ++_nameCounter);

				expr = Expression.Block(
					new[] { pDic },
					Expression.Assign(pDic, Expression.New(InfoOf.Constructor(() => new Dictionary<object,object>()))),
					new MappingImpl(this, pFrom, Expression.Constant(null, _toType), pDic).GetExpressionWithDic());
			}
			else
				expr = GetExpressionExImpl(pFrom, _toType);

			var l = Expression.Lambda(
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

			var initExpression = BuildCollectionMapper(fromExpression, toType);

			if (initExpression != null)
				return initExpression;

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
						GetExpressionExImpl(getValue, toMember.Type));

					binds.Add(Expression.Bind(toMember.MemberInfo, expr));
				}
			}

			var newExpression = GetNewExpression(toType);

			initExpression = Convert(binds.Count > 0 ? (Expression)Expression.MemberInit(newExpression, binds) : newExpression, toType);

			return initExpression;
		}

		NewExpression GetNewExpression(Type originalType)
		{
			var type = originalType;

			if (type.IsInterface && type.IsGenericType)
			{
				var definition = type.GetGenericTypeDefinition();

				if (definition == typeof(IList<>) || definition == typeof(IEnumerable<>))
				{
					type = typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);
				}
			}

			return Expression.New(type);
		}

		Expression Convert(Expression expr, Type toType)
			=> expr == null ? null : expr.Type == toType ? expr : Expression.Convert(expr, toType);

		Expression BuildCollectionMapper(Expression fromExpression, Type toType)
		{
			var fromType = fromExpression.Type;

			if (toType.IsSubClass(typeof(IEnumerable<>)) && fromType.IsSubClass(typeof(IEnumerable<>)))
				return Convert(ConvertCollection(null, fromExpression, toType), toType);

			return null;
		}

		Expression ConvertCollection(ParameterExpression dic, Expression fromExpression, Type toType)
		{
			var fromType     = fromExpression.Type;
			var fromItemType = fromType.GetItemType();
			var toItemType   = toType.  GetItemType();

			if (toType.IsGenericType && !toType.IsGenericTypeDefinition)
			{
				var toDefinition = toType.GetGenericTypeDefinition();

				if (toDefinition == typeof(List<>) || typeof(List<>).IsSubClass(toDefinition))
				{
					return Convert(ExpressionBuilderHelper.ToList(_mapperBuilder, dic, fromExpression, fromItemType, toItemType), toType);
				}

				if (toDefinition == typeof(HashSet<>) || typeof(HashSet<>).IsSubClass(toDefinition))
				{
					return Convert(ExpressionBuilderHelper.ToHashSet(_mapperBuilder, dic, fromExpression, fromItemType, toItemType), toType);
				}
			}

			if (toType.IsArray)
			{
				return Convert(ExpressionBuilderHelper.ToArray(_mapperBuilder, dic, fromExpression, fromItemType, toItemType), toType);
			}

			throw new NotImplementedException();
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

		public LambdaExpression GetExpression()
		{
			var pFrom = Expression.Parameter(_fromType, "from");
			var pTo   = Expression.Parameter(_toType,   "to");
			var pDic  = Expression.Parameter(typeof(IDictionary<object,object>), "dic" + ++_nameCounter);

			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
			{
				var type = _mapperBuilder.MappingSchema.IsScalarType(_fromType) ? _fromType : _toType;

				return Expression.Lambda(
					//Expression.Throw(
					//	Expression.New(
					//		InfoOf.Constructor(() => new ArgumentException("")),
					//		Expression.Constant($"Type {type.FullName} cannot be a scalar type. To convert scalar types use ConvertTo<TTo>.From(TFrom value).")),
					//	_toType),
					_mapperBuilder.MappingSchema.GetConvertExpression(_fromType, _toType).ReplaceParameters(pFrom),
					pFrom,
					pTo,
					pDic);
			}

			var expr = new MappingImpl(this, pFrom, pTo, pDic).GetExpression();

			var l = Expression.Lambda(
				_locals.Count > 0 || _expressions.Count > 0 ?
					Expression.Block(_locals, _expressions.Concat(expr)) :
					expr,
				pFrom, pTo, pDic);

			return l;
		}

		class MappingImpl
		{
			public MappingImpl(ExpressionBuilder builder, Expression fromExpression, Expression toExpression, ParameterExpression pDic)
			{
				_builder        = builder;
				_fromExpression = fromExpression;
				_toExpression   = toExpression;
				_pDic           = pDic;
				_localObject    = Expression.Parameter(_toExpression.Type, "obj" + ++_nameCounter);
				_fromAccessor   = TypeAccessor.GetAccessor(_fromExpression.Type);
				_toAccessor     = TypeAccessor.GetAccessor(_toExpression.  Type);
				_cacheMapper    = _builder._mapperBuilder.ProcessCrossReferences != false;
			}

			readonly ExpressionBuilder         _builder;
			readonly Expression                _fromExpression;
			readonly Expression                _toExpression;
			readonly ParameterExpression       _pDic;
			readonly ParameterExpression       _localObject;
			readonly TypeAccessor              _fromAccessor;
			readonly TypeAccessor              _toAccessor;
			readonly List<Expression>          _expressions = new List<Expression>();
			readonly List<ParameterExpression> _locals      = new List<ParameterExpression>();
			readonly bool                      _cacheMapper;

			Type _actualLocalObjectType;

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

				if (!BuildArrayMapper())
				{
					var newLocalObjectExpr = _builder.GetNewExpression(_toExpression.Type);

					_actualLocalObjectType = newLocalObjectExpr.Type;

					_expressions.Add(Expression.Assign(
						_localObject,
						Expression.Condition(
							Expression.Equal(_toExpression, Expression.Constant(null, _toExpression.Type)),
							_builder.Convert(newLocalObjectExpr, _toExpression.Type),
							_toExpression)));

					if (_cacheMapper)
					{
						_expressions.Add(
							Expression.Call(
								InfoOf.Method(() => ExpressionBuilderHelper.Add(null, null, null)),
								_pDic,
								_fromExpression,
								_localObject));
					}

					if (!BuildListMapper())
						GetObjectExpression();
				}

				_expressions.Add(_localObject);

				var expr = Expression.Block(_locals, _expressions) as Expression;

				if (_cacheMapper)
					expr = Expression.Convert(
						Expression.Coalesce(
							Expression.Call(
								InfoOf<IDictionary<object,object>>.Method(_ => ExpressionBuilderHelper.GetValue(null, null)),
								_pDic,
								_fromExpression),
							expr),
						_toExpression.Type);

				return expr;
			}

			void GetObjectExpression()
			{
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
								setter.ReplaceParameters(_localObject, BuildClassMapper(getValue, toMember)) :
								setter.ReplaceParameters(_localObject, _builder.GetExpressionExImpl(getValue, toMember.Type)));

						_expressions.Add(expr);
					}
				}
			}

			Expression BuildClassMapper(Expression getValue, MemberAccessor toMember)
			{
				var key   = Tuple.Create(_fromExpression.Type, toMember.Type);
				var pFrom = Expression.Parameter(getValue.Type, "pFrom");
				var pTo   = Expression.Parameter(toMember.Type, "pTo");
				var toObj = toMember.GetterExpression.ReplaceParameters(_localObject);

				ParameterExpression l = null;

				if (_cacheMapper)
				{
					if (_builder._mappers.TryGetValue(key, out l))
						return Expression.Invoke(l, getValue, toObj, _pDic);

					l = Expression.Parameter(Expression.Lambda(Expression.Constant(null, toMember.Type), pFrom, pTo, _pDic).Type);

					_builder._mappers.Add(key, l);
					_builder._locals. Add(l);
				}

				var expr = new MappingImpl(_builder, _cacheMapper ? pFrom : getValue, _cacheMapper ? pTo : toObj, _pDic).GetExpression();

				if (_cacheMapper)
				{
					var lex = Expression.Lambda(expr, pFrom, pTo, _pDic);

					_builder._expressions.Add(Expression.Assign(l, lex));

					expr = Expression.Invoke(l, getValue, toObj, _pDic);
				}

				return expr;
			}

			bool BuildArrayMapper()
			{
				var fromType = _fromExpression.Type;
				var toType   = _localObject.Type;

				if (toType.IsArray && fromType.IsSubClass(typeof(IEnumerable<>)))
				{
					var fromItemType = fromType.GetItemType();
					var toItemType   = toType.  GetItemType();

					var expr = ExpressionBuilderHelper.ToArray(_builder._mapperBuilder, _pDic, _fromExpression, fromItemType, toItemType);

					_expressions.Add(Expression.Assign(_localObject, expr));

					return true;
				}

				return false;
			}

			bool BuildListMapper()
			{
				var fromListType = _fromExpression.Type;
				var toListType   = _localObject.Type;

				if (!toListType.IsSubClass(typeof(IEnumerable<>)) || !fromListType.IsSubClass(typeof(IEnumerable<>)))
					return false;

				var clearMethodInfo = toListType.GetMethod("Clear");

				if (clearMethodInfo != null)
					_expressions.Add(Expression.Call(_localObject, clearMethodInfo));

				var fromItemType = fromListType.GetItemType();
				var toItemType   = toListType.  GetItemType();

				var addRangeMethodInfo = toListType.GetMethod("AddRange");

				if (addRangeMethodInfo != null)
				{
					var selectExpr = ExpressionBuilderHelper.Select(_builder._mapperBuilder, _cacheMapper ? _pDic : null, _fromExpression, fromItemType, toItemType);
					_expressions.Add(Expression.Call(_localObject, addRangeMethodInfo, selectExpr));
				}
				else if (toListType.IsGenericType && !toListType.IsGenericTypeDefinition)
				{
					if (toListType.IsSubClass(typeof(ICollection<>)))
					{
						var selectExpr = ExpressionBuilderHelper.Select(_builder._mapperBuilder, _cacheMapper ? _pDic : null, _fromExpression, fromItemType, toItemType);

						_expressions.Add(
							Expression.Call(
								InfoOf.Method(() => ((ICollection<int>)null).AddRange((IEnumerable<int>)null))
									.GetGenericMethodDefinition()
									.MakeGenericMethod(toItemType),
								_localObject,
								selectExpr));
					}
					else
					{
						_expressions.Add(
							Expression.Assign(
								_localObject,
								_builder.ConvertCollection(_cacheMapper ? _pDic : null, _fromExpression, toListType)));
					}
				}
				else
				{
					throw new NotImplementedException();
				}

				return true;
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

		public static Expression ToList(IMapperBuilder builder, ParameterExpression dic, Expression fromExpression, Type fromItemType, Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => Enumerable.ToList<int>(null)).GetGenericMethodDefinition();
			var expr       = Select(builder, dic, fromExpression, fromItemType, toItemType);

			return Expression.Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		public static Expression ToHashSet(IMapperBuilder builder, ParameterExpression dic, Expression fromExpression, Type fromItemType, Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => EnumerableExtensions.ToHashSet<int>(null)).GetGenericMethodDefinition();
			var expr       = Select(builder, dic, fromExpression, fromItemType, toItemType);

			return Expression.Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		public static Expression ToArray(IMapperBuilder builder, ParameterExpression dic, Expression fromExpression, Type fromItemType, Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => Enumerable.ToArray<int>(null)).GetGenericMethodDefinition();
			var expr       = Select(builder, dic, fromExpression, fromItemType, toItemType);

			return Expression.Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		public static Expression Select(IMapperBuilder builder, ParameterExpression dic, Expression getValue, Type fromItemType, Type toItemType)
		{
			var getBuilderInfo = InfoOf.Method(() => GetBuilder<int,int>(null)).               GetGenericMethodDefinition();
			var selectInfo     = InfoOf.Method(() => Enumerable.Select<int,int>(null, _ => _)).GetGenericMethodDefinition();
			var itemBuilder    = (IMapperBuilder)getBuilderInfo.MakeGenericMethod(fromItemType, toItemType).Invoke(null, new object[] { builder });

			var expr = getValue;

			if (builder.DeepCopy != false || fromItemType != toItemType)
			{
				Expression selector;

				if (dic == null)
				{
					selector = itemBuilder.GetMapperLambdaExpressionEx();
				}
				else
				{
					var p = Expression.Parameter(fromItemType);
					selector = Expression.Lambda(
						Expression.Invoke(itemBuilder.GetMapperLambdaExpression(), p, Expression.Constant(null, toItemType), dic),
						p);
				}

				expr = Expression.Call(selectInfo.MakeGenericMethod(fromItemType, toItemType), getValue, selector);
			}

			return expr;
		}
	}
}
