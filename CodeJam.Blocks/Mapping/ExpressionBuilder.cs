#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using CodeJam.Collections;
#if (NET46_OR_GREATER && !NET472_OR_GREATER) || (NETSTANDARD16_OR_GREATER && !NETSTANDARD21_OR_GREATER)
using CodeJam.Collections.Backported;
#endif
using CodeJam.Targeting;

using JetBrains.Annotations;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

using static System.Linq.Expressions.Expression;

// ReSharper disable TailRecursiveCall

namespace CodeJam.Mapping
{
	using Expressions;

	using Reflection;

	internal class ExpressionBuilder
	{
		private class BuilderData
		{
			public BuilderData(ValueTuple<MemberInfo[],LambdaExpression>[] memberMappers) => MemberMappers = memberMappers;

			public readonly ValueTuple<MemberInfo[],LambdaExpression>[]           MemberMappers;
			[NotNull]
			public readonly Dictionary<ValueTuple<Type,Type>,ParameterExpression> Mappers     = new Dictionary<ValueTuple<Type,Type>, ParameterExpression>();
			[NotNull]
			public readonly HashSet<ValueTuple<Type,Type>>                        MapperTypes = new HashSet<ValueTuple<Type,Type>>();
			[NotNull, ItemNotNull]
			public readonly List<ParameterExpression>                             Locals      = new List<ParameterExpression>();
			[NotNull, ItemNotNull]
			public readonly List<Expression>                                      Expressions = new List<Expression>();

			public ParameterExpression LocalDic;

			public int  NameCounter;
			public int  RestartCounter;

			public bool IsRestart => RestartCounter > 10;
		}

		public ExpressionBuilder([NotNull] IMapperBuilder mapperBuilder, ValueTuple<MemberInfo[],LambdaExpression>[] memberMappers)
			: this(mapperBuilder, new BuilderData(memberMappers)) =>
			_processCrossReferences = mapperBuilder.ProcessCrossReferences == true;

		private ExpressionBuilder([NotNull] IMapperBuilder mapperBuilder, [NotNull] BuilderData data)
		{
			_mapperBuilder          = mapperBuilder;
			_data                   = data;
			_fromType               = mapperBuilder.FromType;
			_toType                 = mapperBuilder.ToType;
			_processCrossReferences = true;
		}

		[NotNull]
		private readonly IMapperBuilder   _mapperBuilder;
		[NotNull]
		private readonly Type             _fromType;
		[NotNull]
		private readonly Type             _toType;
		[NotNull]
		private readonly BuilderData      _data;
		private readonly bool             _processCrossReferences;

		#region GetExpressionEx

		[CanBeNull]
		public LambdaExpression? GetExpressionEx()
		{
			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
				return _mapperBuilder.MappingSchema.GetConvertExpression(_fromType, _toType);

			var pFrom = Parameter(_fromType, "from");

			Expression expr;

			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
			{
				expr = GetExpressionExImpl(pFrom, _toType);
			}
			else if (_processCrossReferences)
			{
				_data.LocalDic = Parameter(typeof(IDictionary<object,object>), "ldic" + ++_data.NameCounter);
				_data.Locals.Add(_data.LocalDic);
				_data.Expressions.Add(Assign(_data.LocalDic, New(InfoOf.Constructor(() => new Dictionary<object,object>()))));

				expr = new MappingImpl(this, pFrom, Constant(_mapperBuilder.MappingSchema.GetDefaultValue(_toType), _toType)).GetExpression();
			}
			else
			{
				expr = GetExpressionExImpl(pFrom, _toType);
			}

			if (_data.IsRestart)
			{
				_mapperBuilder.ProcessCrossReferences = true;
				return new ExpressionBuilder(_mapperBuilder, new BuilderData(_data.MemberMappers)).GetExpressionEx();
			}

			var l = Lambda(
				_data.Locals.Count > 0 || _data.Expressions.Count > 0 ?
					Block(_data.Locals, _data.Expressions.Concat(expr)) :
					expr,
				pFrom);

			return l;
		}

		[CanBeNull]
		private LambdaExpression? GetExpressionExInner()
		{
			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
				return _mapperBuilder.MappingSchema.GetConvertExpression(_fromType, _toType);

			var pFrom = Parameter(_fromType, "from");
			var expr  = GetExpressionExImpl(pFrom, _toType);

			if (_data.IsRestart)
				return null;

			var l = Lambda(expr, pFrom);

			return l;
		}

		[CanBeNull]
		private Expression? GetExpressionExImpl([NotNull] Expression fromExpression, [NotNull] Type toType)
		{
			var fromAccessor = TypeAccessor.GetAccessor(fromExpression.Type);
			var toAccessor   = TypeAccessor.GetAccessor(toType);
			var binds        = new List<MemberAssignment>();
			var key          = new ValueTuple<Type,Type>(fromExpression.Type, toType);

			if (_data.MapperTypes.Contains(key))
			{
				_data.RestartCounter++;

				if (_data.IsRestart)
					return null;
			}
			else
			{
				_data.MapperTypes.Add(key);
			}

			var initExpression = BuildCollectionMapper(fromExpression, toType);

			if (initExpression != null)
				return initExpression;

			foreach (var toMember in toAccessor.Members.Where(_mapperBuilder.MemberFilter))
			{
				if (_data.IsRestart)
					return null;

				if (!toMember.HasSetter)
					continue;

				if (_data.MemberMappers != null)
				{
					var processed = false;

					foreach (var item in _data.MemberMappers)
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

				if (_mapperBuilder.ToMappingDictionary == null ||
					!_mapperBuilder.ToMappingDictionary.TryGetValue(toType, out var mapDic) ||
					!mapDic.TryGetValue(toMember.Name, out var toName))
					toName = toMember.Name;

				var fromMember = fromAccessor.Members.FirstOrDefault(mi =>
				{
					if (_mapperBuilder.FromMappingDictionary == null ||
						!_mapperBuilder.FromMappingDictionary.TryGetValue(fromExpression.Type, out mapDic) ||
						!mapDic.TryGetValue(mi.Name, out var fromName))
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

					if (_data.IsRestart)
						return null;

					var expr     = Condition(
						Equal(getValue, Constant(_mapperBuilder.MappingSchema.GetDefaultValue(getValue.Type), getValue.Type)),
						Constant(_mapperBuilder.MappingSchema.GetDefaultValue(toMember.Type), toMember.Type),
						exExpr);

					binds.Add(Bind(toMember.MemberInfo, expr));
				}
			}

			if (_data.IsRestart)
				return null;

			var newExpression = GetNewExpression(toType);

			initExpression =
				Convert(
					binds.Count > 0
						? (Expression)MemberInit(newExpression, binds)
						: newExpression,
					toType);

			return initExpression;
		}

		[NotNull]
		private static NewExpression GetNewExpression([NotNull] Type originalType)
		{
			var type = originalType;

			if (type.GetIsInterface() && type.GetIsGenericType())
			{
				var definition = type.GetGenericTypeDefinition();

				if (definition == typeof(IList<>) || definition == typeof(IEnumerable<>))
				{
					type = typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);
				}
			}

			return New(type);
		}

		[CanBeNull]
		[ContractAnnotation("expr:null => null; expr:notnull => notnull")]
		private static Expression? Convert([CanBeNull] Expression? expr, [NotNull] Type toType) =>
			expr == null ? null : expr.Type == toType ? expr : Expression.Convert(expr, toType);

		[CanBeNull]
		private Expression? BuildCollectionMapper([NotNull] Expression fromExpression, [NotNull] Type toType)
		{
			var fromType = fromExpression.Type;

			if (toType.IsSubClass(typeof(IEnumerable<>)) && fromType.IsSubClass(typeof(IEnumerable<>)))
				return Convert(ConvertCollection(fromExpression, toType), toType);

			return null;
		}

		[CanBeNull]
		private Expression? ConvertCollection([NotNull] Expression fromExpression, [NotNull] Type toType)
		{
			var fromType     = fromExpression.Type;
			var fromItemType = fromType.GetItemType();
			var toItemType   = toType.  GetItemType();

			if (toType.GetIsGenericType() && !toType.GetIsGenericTypeDefinition())
			{
				var toDefinition = toType.GetGenericTypeDefinition();
				DebugCode.BugIf(toDefinition == null, "toDefinition == null");
				if (toDefinition == typeof(List<>) || typeof(List<>).IsSubClass(toDefinition))
					return Convert(
						ToList(this, fromExpression, fromItemType, toItemType), toType);

				if (toDefinition == typeof(HashSet<>) || typeof(HashSet<>).IsSubClass(toDefinition))
					return Convert(
						ToHashSet(this, fromExpression, fromItemType, toItemType), toType);
			}

			if (toType.IsArray)
				return Convert(ToArray(this, fromExpression, fromItemType, toItemType), toType);

			throw new NotImplementedException();
		}

		private MemberAssignment BuildAssignment(
			[NotNull] LambdaExpression getter,
			[NotNull] Expression fromExpression,
			[NotNull] Type fromMemberType,
			[NotNull] MemberAccessor toMember)
		{
			var getValue = getter.ReplaceParameters(fromExpression);
			var expr     = _mapperBuilder.MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
			var convert  = expr.ReplaceParameters(getValue);

			return Bind(toMember.MemberInfo, convert);
		}

		#endregion

		#region GetExpression

		[NotNull]
		public LambdaExpression GetExpression()
		{
			var pFrom = Parameter(_fromType, "from");
			var pTo   = Parameter(_toType,   "to");
			var pDic  = Parameter(typeof(IDictionary<object,object>), "dic" + ++_data.NameCounter);

			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
			{
				return Lambda(
					_mapperBuilder.MappingSchema.GetConvertExpression(_fromType, _toType).ReplaceParameters(pFrom),
					pFrom,
					pTo,
					pDic);
			}

			_data.LocalDic = Parameter(typeof(IDictionary<object,object>), "ldic" + ++_data.NameCounter);
			_data.Locals.     Add(_data.LocalDic);
			_data.Expressions.Add(Assign(_data.LocalDic, pDic));

			var expr = new MappingImpl(this, pFrom, pTo).GetExpression();

			var l = Lambda(Block(_data.Locals, _data.Expressions.Concat(expr)), pFrom, pTo, pDic);

			return l;
		}

		private LambdaExpression GetExpressionInner()
		{
			var pFrom = Parameter(_fromType, "from");
			var pTo   = Parameter(_toType,   "to");

			if (_mapperBuilder.MappingSchema.IsScalarType(_fromType) || _mapperBuilder.MappingSchema.IsScalarType(_toType))
			{
				return Lambda(
					_mapperBuilder.MappingSchema.GetConvertExpression(_fromType, _toType).ReplaceParameters(pFrom),
					pFrom,
					pTo);
			}

			var expr = new MappingImpl(this, pFrom, pTo).GetExpression();

			var l = Lambda(expr, pFrom, pTo);

			return l;
		}

		private class MappingImpl
		{
			public MappingImpl(
				[NotNull] ExpressionBuilder builder,
				[NotNull] Expression        fromExpression,
				[NotNull] Expression        toExpression)
			{
				_builder        = builder;
				_fromExpression = fromExpression;
				_toExpression   = toExpression;
				_localObject    = Parameter(_toExpression.Type, "obj" + ++_builder._data.NameCounter);
				_fromAccessor   = TypeAccessor.GetAccessor(_fromExpression.Type);
				_toAccessor     = TypeAccessor.GetAccessor(_toExpression.  Type);
				_cacheMapper    = _builder._mapperBuilder.ProcessCrossReferences != false;
			}

			[NotNull] private readonly ExpressionBuilder         _builder;
			[NotNull] private readonly Expression                _fromExpression;
			[NotNull] private readonly Expression                _toExpression;
			[NotNull] private readonly ParameterExpression       _localObject;
			[NotNull] private readonly TypeAccessor              _fromAccessor;
			[NotNull] private readonly TypeAccessor              _toAccessor;
			[NotNull, ItemNotNull] private readonly List<Expression>          _expressions = new List<Expression>();
			[NotNull, ItemNotNull] private readonly List<ParameterExpression> _locals      = new List<ParameterExpression>();
			private readonly bool                      _cacheMapper;

			//private Type _actualLocalObjectType;

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
								_builder._data.LocalDic,
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
								_builder._data.LocalDic,
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

					if (_builder._data.MemberMappers != null)
					{
						var processed = false;

						foreach (var item in _builder._data.MemberMappers)
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

					if (_builder._mapperBuilder.ToMappingDictionary == null ||
						!_builder._mapperBuilder.ToMappingDictionary.TryGetValue(_toExpression.Type, out var mapDic) ||
						!mapDic.TryGetValue(toMember.Name, out var toName))
						toName = toMember.Name;

					var fromMember = _fromAccessor.Members.FirstOrDefault(mi =>
					{
						if (_builder._mapperBuilder.FromMappingDictionary == null ||
							!_builder._mapperBuilder.FromMappingDictionary.TryGetValue(_fromExpression.Type, out mapDic) ||
							!mapDic.TryGetValue(mi.Name, out var fromName))
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

				ParameterExpression? nullPrm = null;

				if (_cacheMapper)
				{
					if (_builder._data.Mappers.TryGetValue(key, out nullPrm))
						return Invoke(nullPrm, getValue, toObj);

					nullPrm = Parameter(
						Lambda(
							Constant(
								_builder._mapperBuilder.MappingSchema.GetDefaultValue(toMember.Type),
								toMember.Type),
							pFrom,
							pTo).Type);

					_builder._data.Mappers.Add(key, nullPrm);
					_builder._data.Locals. Add(nullPrm);
				}

				var expr = new MappingImpl(_builder, _cacheMapper ? pFrom : getValue, _cacheMapper ? pTo : toObj).GetExpression();

				if (_cacheMapper)
				{
					var lex = Lambda(expr, pFrom, pTo);

					_builder._data.Expressions.Add(Assign(nullPrm, lex));

					expr = Invoke(nullPrm, getValue, toObj);
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

					var expr = ToArray(_builder, _fromExpression, fromItemType, toItemType);

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
					var selectExpr = Select(_builder, _fromExpression, fromItemType, toItemType);
					_expressions.Add(Call(_localObject, addRangeMethodInfo, selectExpr));
				}
				else if (toListType.GetIsGenericType() && !toListType.GetIsGenericTypeDefinition())
				{
					if (toListType.IsSubClass(typeof(ICollection<>)))
					{
						var selectExpr = Select(
							_builder,
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
								_builder.ConvertCollection(_fromExpression, toListType)));
					}
				}
				else
				{
					throw new NotImplementedException();
				}

				return true;
			}

			private Expression BuildAssignment(
				[NotNull] LambdaExpression getter,
				[NotNull] LambdaExpression setter,
				[NotNull] Type fromMemberType,
				[NotNull] Expression toExpression,
				MemberAccessor toMember)
			{
				var getValue = getter.ReplaceParameters(_fromExpression);
				var expr     = _builder._mapperBuilder.MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
				var convert  = expr.ReplaceParameters(getValue);

				return setter.ReplaceParameters(toExpression, convert);
			}
		}

		#endregion

		[CanBeNull]
		private static object? GetValue([CanBeNull] IDictionary<object,object>? dic, object key) =>
			dic != null && dic.TryGetValue(key, out var result) ? result : null;

		private static void Add([CanBeNull] IDictionary<object,object>? dic, [CanBeNull] object? key, [CanBeNull] object? value)
		{
			if (key != null && dic != null)
				dic[key] = value;
		}

		[NotNull]
		private static IMapperBuilder GetBuilder<TFrom,TTo>([NotNull] IMapperBuilder builder) =>
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

		[CanBeNull]
		private static Expression? ToList(
			[NotNull] ExpressionBuilder builder,
			Expression fromExpression,
			[NotNull] Type fromItemType,
			[NotNull] Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => Enumerable.ToList<int>(null)).GetGenericMethodDefinition();
			var expr       = Select(builder, fromExpression, fromItemType, toItemType);

			if (builder._data.IsRestart)
				return null;

			return Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		[CanBeNull]
		private static Expression? ToHashSet(
			[NotNull] ExpressionBuilder builder,
			Expression fromExpression,
			[NotNull] Type fromItemType,
			[NotNull] Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => Array<int>.Empty.ToHashSet()).GetGenericMethodDefinition();
			var expr       = Select(builder, fromExpression, fromItemType, toItemType);

			if (builder._data.IsRestart)
				return null;

			return Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		[CanBeNull]
		private static Expression? ToArray(
			[NotNull] ExpressionBuilder builder,
			Expression fromExpression,
			[NotNull] Type fromItemType,
			[NotNull] Type toItemType)
		{
			var toListInfo = InfoOf.Method(() => Enumerable.ToArray<int>(null)).GetGenericMethodDefinition();
			var expr       = Select(builder, fromExpression, fromItemType, toItemType);

			if (builder._data.IsRestart)
				return null;

			return Call(toListInfo.MakeGenericMethod(toItemType), expr);
		}

		[CanBeNull]
		private static Expression? Select(
			[NotNull] ExpressionBuilder builder,
			Expression getValue,
			[NotNull] Type fromItemType,
			[NotNull] Type toItemType)
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

				var selectorBuilder = new ExpressionBuilder(itemBuilder, builder._data);

				if (builder._data.LocalDic == null)
				{
					selector = selectorBuilder.GetExpressionExInner();
				}
				else
				{
					var p  = Parameter(fromItemType);
					var ex = selectorBuilder.GetExpressionInner();

					selector = Lambda(
						Invoke(ex, p, Constant(builder._mapperBuilder.MappingSchema.GetDefaultValue(toItemType), toItemType)),
						p);
				}

				if (builder._data.IsRestart)
					return null;

				expr = Call(selectInfo.MakeGenericMethod(fromItemType, toItemType), getValue, selector);
			}

			return expr;
		}
	}
}
#endif
