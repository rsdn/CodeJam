#if !FW35
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using CodeJam.Collections;

using static System.Linq.Expressions.Expression;

namespace CodeJam.Mapping
{
	using Expressions;
	using Reflection;

	internal class ExpressionBuilder
	{
		public ExpressionBuilder(IMapperBuilder mapperBuilder, ValueTuple<MemberInfo[],LambdaExpression>[] memberMappers)
		{
			_mapperBuilder = mapperBuilder;
			_memberMappers = memberMappers;
			_fromType      = mapperBuilder.FromType;
			_toType        = mapperBuilder.ToType;
		}

		private readonly IMapperBuilder                                        _mapperBuilder;
		private readonly Type                                                  _fromType;
		private readonly Type                                                  _toType;
		private readonly ValueTuple<MemberInfo[],LambdaExpression>[]           _memberMappers;
		private          Dictionary<ValueTuple<Type,Type>,ParameterExpression> _mappers     = new Dictionary<ValueTuple<Type,Type>,ParameterExpression>();
		private          HashSet<ValueTuple<Type,Type>>                        _mapperTypes = new HashSet<ValueTuple<Type, Type>>();
		private readonly List<Expression>                                      _expressions = new List<Expression>();
		private readonly List<ParameterExpression>                             _locals      = new List<ParameterExpression>();

		private int _nameCounter;
		private int _restartCounter;

		#region GetExpressionEx

		public LambdaExpression GetExpressionEx()
		{
			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
				return _mapperBuilder.MappingSchema.GetConvertExpression(_fromType, _toType);

			var pFrom = Parameter(_fromType, "from");

			Expression expr;

			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
			{
				expr = GetExpressionExImpl(pFrom, _toType);
			}
			else if (_mapperBuilder.ProcessCrossReferences == true)
			{
				var pDic = Parameter(typeof(IDictionary<object,object>), "dic" + ++_nameCounter);

				expr = Block(
					new[] { pDic },
					Assign(pDic, New(InfoOf.Constructor(() => new Dictionary<object,object>()))),
					new MappingImpl(this, pFrom, Constant(_mapperBuilder.MappingSchema.GetDefaultValue(_toType), _toType), pDic).GetExpressionWithDic());
			}
			else
			{
				expr = GetExpressionExImpl(pFrom, _toType);
			}

			if (_restartCounter > 10)
			{
				_restartCounter = 0;
				_mapperBuilder.ProcessCrossReferences = true;
				return GetExpressionEx();
			}

			var l = Lambda(
				_locals.Count > 0 || _expressions.Count > 0 ?
					Block(_locals, _expressions.Concat(expr)) :
					expr,
				pFrom);

			return l;
		}

		private Expression GetExpressionExImpl(Expression fromExpression, Type toType)
		{
			var fromAccessor = TypeAccessor.GetAccessor(fromExpression.Type);
			var toAccessor   = TypeAccessor.GetAccessor(toType);
			var binds        = new List<MemberAssignment>();
			var key          = new ValueTuple<Type,Type>(fromExpression.Type, toType);

			if (_mapperTypes.Contains(key))
			{
				_restartCounter++;

				if (_restartCounter > 10)
					return null;
			}

			_mapperTypes.Add(key);

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
					binds.Add(Bind(toMember.MemberInfo, getter.ReplaceParameters(fromExpression)));
				}
				else
				{
					var getValue = getter.ReplaceParameters(fromExpression);
					var exExpr   = GetExpressionExImpl(getValue, toMember.Type);

					if (_restartCounter > 10)
						return null;

					var expr     = Condition(
						Equal(getValue, Constant(_mapperBuilder.MappingSchema.GetDefaultValue(getValue.Type), getValue.Type)),
						Constant(_mapperBuilder.MappingSchema.GetDefaultValue(toMember.Type), toMember.Type),
						exExpr);

					binds.Add(Bind(toMember.MemberInfo, expr));
				}
			}

			var newExpression = GetNewExpression(toType);

			initExpression =
				Convert(
					binds.Count > 0
						? (Expression)MemberInit(newExpression, binds)
						: newExpression,
					toType);

			return initExpression;
		}

		private static NewExpression GetNewExpression(Type originalType)
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

			return New(type);
		}

		private static Expression Convert(Expression expr, Type toType) =>
			expr == null ? null : expr.Type == toType ? expr : Expression.Convert(expr, toType);

		private Expression BuildCollectionMapper(Expression fromExpression, Type toType)
		{
			var fromType = fromExpression.Type;

			if (toType.IsSubClass(typeof(IEnumerable<>)) && fromType.IsSubClass(typeof(IEnumerable<>)))
				return Convert(ConvertCollection(null, fromExpression, toType), toType);

			return null;
		}

		private Expression ConvertCollection(ParameterExpression dic, Expression fromExpression, Type toType)
		{
			var fromType     = fromExpression.Type;
			var fromItemType = fromType.GetItemType();
			var toItemType   = toType.  GetItemType();

			if (toType.IsGenericType && !toType.IsGenericTypeDefinition)
			{
				var toDefinition = toType.GetGenericTypeDefinition();

				if (toDefinition == typeof(List<>) || typeof(List<>).IsSubClass(toDefinition))
					return Convert(
						ToList(this, dic, fromExpression, fromItemType, toItemType), toType);

				if (toDefinition == typeof(HashSet<>) || typeof(HashSet<>).IsSubClass(toDefinition))
					return Convert(
						ToHashSet(this, dic, fromExpression, fromItemType, toItemType), toType);
			}

			if (toType.IsArray)
				return Convert(
					ToArray(this, dic, fromExpression, fromItemType, toItemType), toType);

			throw new NotImplementedException();
		}

		private MemberAssignment BuildAssignment(
			LambdaExpression getter,
			Expression fromExpression,
			Type fromMemberType,
			MemberAccessor toMember)
		{
			var getValue = getter.ReplaceParameters(fromExpression);
			var expr     = _mapperBuilder.MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
			var convert  = expr.ReplaceParameters(getValue);

			return Bind(toMember.MemberInfo, convert);
		}

		#endregion

		#region GetExpression

		public LambdaExpression GetExpression()
		{
			var pFrom = Parameter(_fromType, "from");
			var pTo   = Parameter(_toType,   "to");
			var pDic  = Parameter(typeof(IDictionary<object,object>), "dic" + ++_nameCounter);

			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
			{
				//var type = _mapperBuilder.MappingSchema.IsScalarType(_fromType) ? _fromType : _toType;

				return Lambda(
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

			var l = Lambda(
				_locals.Count > 0 || _expressions.Count > 0 ?
					Block(_locals, _expressions.Concat(expr)) :
					expr,
				pFrom, pTo, pDic);

			return l;
		}

		private class MappingImpl
		{
			public MappingImpl(
				ExpressionBuilder   builder,
				Expression          fromExpression,
				Expression          toExpression,
				ParameterExpression pDic)
			{
				_builder        = builder;
				_fromExpression = fromExpression;
				_toExpression   = toExpression;
				_pDic           = pDic;
				_localObject    = Parameter(_toExpression.Type, "obj" + ++_builder._nameCounter);
				_fromAccessor   = TypeAccessor.GetAccessor(_fromExpression.Type);
				_toAccessor     = TypeAccessor.GetAccessor(_toExpression.  Type);
				_cacheMapper    = _builder._mapperBuilder.ProcessCrossReferences != false;
			}

			private readonly ExpressionBuilder         _builder;
			private readonly Expression                _fromExpression;
			private readonly Expression                _toExpression;
			private readonly ParameterExpression       _pDic;
			private readonly ParameterExpression       _localObject;
			private readonly TypeAccessor              _fromAccessor;
			private readonly TypeAccessor              _toAccessor;
			private readonly List<Expression>          _expressions = new List<Expression>();
			private readonly List<ParameterExpression> _locals      = new List<ParameterExpression>();
			private readonly bool                      _cacheMapper;

			//private Type _actualLocalObjectType;

			public Expression GetExpressionWithDic()
			{
				_locals.Add(_pDic);

				_expressions.Add(Assign(
					_pDic,
					New(InfoOf.Constructor(() => new Dictionary<object,object>()))));

				return GetExpression();
			}

			public Expression GetExpression()
			{
				_locals.Add(_localObject);

				if (!BuildArrayMapper())
				{
					var newLocalObjectExpr = GetNewExpression(_toExpression.Type);

					//_actualLocalObjectType = newLocalObjectExpr.Type;

					_expressions.Add(Assign(
						_localObject,
						Condition(
							Equal(
								_toExpression,
								Constant(
									_builder._mapperBuilder.MappingSchema.GetDefaultValue(_toExpression.Type),
									_toExpression.Type)),
							Convert(newLocalObjectExpr, _toExpression.Type),
							_toExpression)));

					if (_cacheMapper)
					{
						_expressions.Add(
							Call(
								InfoOf.Method(() => Add(null, null, null)),
								_pDic,
								_fromExpression,
								_localObject));
					}

					if (!BuildListMapper())
						GetObjectExpression();
				}

				_expressions.Add(_localObject);

				var expr = Block(_locals, _expressions) as Expression;

				if (_cacheMapper)
					expr = Expression.Convert(
						Coalesce(
							Call(
								InfoOf<IDictionary<object,object>>.Method(_ => GetValue(null, null)),
								_pDic,
								_fromExpression),
							expr),
						_toExpression.Type);

				return expr;
			}

			private void GetObjectExpression()
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

					if (_builder._mapperBuilder.MappingSchema.IsScalarType(fromMember.Type) ||
						_builder._mapperBuilder.MappingSchema.IsScalarType(toMember.Type))
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
						var expr     = IfThenElse(
							// if (from == null)
							Equal(getValue, Constant(_builder._mapperBuilder.MappingSchema.GetDefaultValue(getValue.Type), getValue.Type)),
							//   localObject = null;
							setter.ReplaceParameters(
								_localObject,
								Constant(_builder._mapperBuilder.MappingSchema.GetDefaultValue(toMember.Type), toMember.Type)),
							// else
							toMember.HasGetter ?
								setter.ReplaceParameters(_localObject, BuildClassMapper(getValue, toMember)) :
								setter.ReplaceParameters(_localObject, _builder.GetExpressionExImpl(getValue, toMember.Type)));

						_expressions.Add(expr);
					}
				}
			}

			[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
			private Expression BuildClassMapper(Expression getValue, MemberAccessor toMember)
			{
				var key   = ValueTuple.Create(_fromExpression.Type, toMember.Type);
				var pFrom = Parameter(getValue.Type, "pFrom");
				var pTo   = Parameter(toMember.Type, "pTo");
				var toObj = toMember.GetterExpression.ReplaceParameters(_localObject);

				ParameterExpression nullPrm = null;

				if (_cacheMapper)
				{
					if (_builder._mappers.TryGetValue(key, out nullPrm))
						return Invoke(nullPrm, getValue, toObj, _pDic);

					nullPrm = Parameter(
						Lambda(
							Constant(
								_builder._mapperBuilder.MappingSchema.GetDefaultValue(toMember.Type),
								toMember.Type),
							pFrom,
							pTo,
							_pDic).Type);

					_builder._mappers.Add(key, nullPrm);
					_builder._locals. Add(nullPrm);
				}

				var expr = new MappingImpl(_builder, _cacheMapper ? pFrom : getValue, _cacheMapper ? pTo : toObj, _pDic).GetExpression();

				if (_cacheMapper)
				{
					var lex = Lambda(expr, pFrom, pTo, _pDic);

					_builder._expressions.Add(Assign(nullPrm, lex));

					expr = Invoke(nullPrm, getValue, toObj, _pDic);
				}

				return expr;
			}

			private bool BuildArrayMapper()
			{
				var fromType = _fromExpression.Type;
				var toType   = _localObject.Type;

				if (toType.IsArray && fromType.IsSubClass(typeof(IEnumerable<>)))
				{
					var fromItemType = fromType.GetItemType();
					var toItemType   = toType.  GetItemType();

					var expr = ToArray(_builder, _pDic, _fromExpression, fromItemType, toItemType);

					_expressions.Add(Assign(_localObject, expr));

					return true;
				}

				return false;
			}

			private bool BuildListMapper()
			{
				var fromListType = _fromExpression.Type;
				var toListType   = _localObject.Type;

				if (!toListType.IsSubClass(typeof(IEnumerable<>)) || !fromListType.IsSubClass(typeof(IEnumerable<>)))
					return false;

				var clearMethodInfo = toListType.GetMethod("Clear");

				if (clearMethodInfo != null)
					_expressions.Add(Call(_localObject, clearMethodInfo));

				var fromItemType = fromListType.GetItemType();
				var toItemType   = toListType.  GetItemType();

				var addRangeMethodInfo = toListType.GetMethod("AddRange");

				if (addRangeMethodInfo != null)
				{
					var selectExpr = Select(_builder, _cacheMapper ? _pDic : null, _fromExpression, fromItemType, toItemType);
					_expressions.Add(Call(_localObject, addRangeMethodInfo, selectExpr));
				}
				else if (toListType.IsGenericType && !toListType.IsGenericTypeDefinition)
				{
					if (toListType.IsSubClass(typeof(ICollection<>)))
					{
						var selectExpr = Select(
							_builder,
							_cacheMapper ? _pDic : null,
							_fromExpression, fromItemType,
							toItemType);

						_expressions.Add(
							Call(
								InfoOf.Method(() => ((ICollection<int>)null).AddRange((IEnumerable<int>)null))
									.GetGenericMethodDefinition()
									.MakeGenericMethod(toItemType),
								_localObject,
								selectExpr));
					}
					else
					{
						_expressions.Add(
							Assign(
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

			private Expression BuildAssignment(
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

		private static object GetValue(IDictionary<object,object> dic, object key)
		{
			object result;
			return dic != null && dic.TryGetValue(key, out result) ? result : null;
		}

		private static void Add(IDictionary<object,object> dic, object key, object value)
		{
			if (key != null && dic != null)
				dic[key] = value;
		}

		private static IMapperBuilder GetBuilder<TFrom,TTo>(IMapperBuilder builder) =>
			new MapperBuilder<TFrom,TTo>
			{
				MappingSchema          = builder.MappingSchema,
				MemberMappers          = builder.MemberMappers,
				FromMappingDictionary  = builder.FromMappingDictionary,
				ToMappingDictionary    = builder.ToMappingDictionary,
				MemberFilter           = builder.MemberFilter,
				ProcessCrossReferences = builder.ProcessCrossReferences,
				DeepCopy               = builder.DeepCopy
			};

		private static Expression ToList(
			ExpressionBuilder builder,
			ParameterExpression dic,
			Expression fromExpression,
			Type fromItemType,
			Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => Enumerable.ToList<int>(null)).GetGenericMethodDefinition();
			var expr       = Select(builder, dic, fromExpression, fromItemType, toItemType);

			return Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		Expression ToHashSet(
		private static Expression ToHashSet(
		private Expression ToHashSet(
			ExpressionBuilder builder,
			ParameterExpression dic,
			Expression fromExpression,
			Type fromItemType,
			Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => EnumerableExtensions.ToHashSet<int>(null)).GetGenericMethodDefinition();
			var expr       = Select(builder, dic, fromExpression, fromItemType, toItemType);

			return Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		private static Expression ToArray(
			ExpressionBuilder builder,
			ParameterExpression dic,
			Expression fromExpression,
			Type fromItemType,
			Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => Enumerable.ToArray<int>(null)).GetGenericMethodDefinition();
			var expr       = Select(builder, dic, fromExpression, fromItemType, toItemType);

			return Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		private static Expression Select(
			ExpressionBuilder builder,
			ParameterExpression dic,
			Expression getValue,
			Type fromItemType,
			Type toItemType)
		{
			var getBuilderInfo = InfoOf.Method(() => GetBuilder<int,int>(null)).               GetGenericMethodDefinition();
			var selectInfo     = InfoOf.Method(() => Enumerable.Select<int,int>(null, _ => _)).GetGenericMethodDefinition();
			var itemBuilder    =
				(IMapperBuilder)getBuilderInfo
					.MakeGenericMethod(fromItemType, toItemType)
					.Invoke(null, new object[] { builder._mapperBuilder });

			var expr = getValue;

			if (builder._mapperBuilder.DeepCopy != false || fromItemType != toItemType)
			{
				Expression selector;

				var call = new ExpressionBuilder(itemBuilder, builder._memberMappers)
				{
					_mappers        = builder._mappers,
					_mapperTypes    = builder._mapperTypes,
					_restartCounter = builder._restartCounter
				};

				if (dic == null)
				{
					selector = call.GetExpressionEx();
				}
				else
				{
					var p  = Parameter(fromItemType);
					var ex = call.GetExpression();

					selector = Lambda(
						Invoke(ex, p, Constant(builder._mapperBuilder.MappingSchema.GetDefaultValue(toItemType), toItemType), dic),
						p);
				}

				builder._restartCounter = call._restartCounter;

				expr = Call(selectInfo.MakeGenericMethod(fromItemType, toItemType), getValue, selector);
			}

			return expr;
		}
	}
}
#endif
