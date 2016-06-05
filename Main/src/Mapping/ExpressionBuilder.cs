using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodeJam.Mapping
{
	using Expressions;
	using Reflection;

	class ExpressionBuilder<TFrom,TTo>
	{
		public ExpressionBuilder(Mapper<TFrom,TTo> mapper, Tuple<MemberInfo[],LambdaExpression>[] memberMappers)
		{
			_mapper        = mapper;
			_memberMappers = memberMappers;
		}

		readonly Mapper<TFrom,TTo>                      _mapper;
		readonly Tuple<MemberInfo[],LambdaExpression>[] _memberMappers;

		#region GetExpressionEx

		public Expression<Func<TFrom,TTo>> GetExpressionEx()
		{
			if (_mapper.MappingSchema.IsScalarType(typeof(TFrom)) || _mapper.MappingSchema.IsScalarType(typeof(TTo)))
				return _mapper.MappingSchema.GetConvertExpression<TFrom, TTo>();

			var pFrom = Expression.Parameter(typeof(TFrom), "from");

			var expr = _mapper.ProcessCrossReferences == true ?
				new MappingImpl(this, pFrom, Expression.Constant(null, typeof(TTo))).GetExpression() :
				GetExpressionExImpl(pFrom, typeof(TTo));

			var l = Expression.Lambda<Func<TFrom,TTo>>(expr, pFrom);

			return l;
		}

		Expression GetExpressionExImpl(Expression fromExpression, Type toType)
		{
			var fromAccessor = TypeAccessor.GetAccessor(fromExpression.Type);
			var toAccessor   = TypeAccessor.GetAccessor(toType);
			var binds        = new List<MemberAssignment>();

			foreach (var toMember in toAccessor.Members.Where(_mapper.MemberFilter))
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

				if (_mapper.ToMappingDictionary == null ||
					!_mapper.ToMappingDictionary.TryGetValue(toType, out mapDic) ||
					!mapDic.TryGetValue(toMember.Name, out toName))
					toName = toMember.Name;

				var fromMember = fromAccessor.Members.FirstOrDefault(mi =>
				{
					string fromName;
					if (_mapper.FromMappingDictionary == null ||
						!_mapper.FromMappingDictionary.TryGetValue(fromExpression.Type, out mapDic) ||
						!mapDic.TryGetValue(mi.Name, out fromName))
						fromName = mi.Name;
					return fromName == toName;
				});

				if (fromMember == null || !fromMember.HasGetter)
					continue;

				var getter = fromMember.GetterExpression;

				if (_mapper.MappingSchema.IsScalarType(fromMember.Type) || _mapper.MappingSchema.IsScalarType(toMember.Type))
				{
					binds.Add(BuildAssignment(getter, fromExpression, fromMember.Type, toMember));
				}
				else if (fromMember.Type == toMember.Type)
				{
					binds.Add(Expression.Bind(toMember.MemberInfo, getter.ReplaceParameters(fromExpression)));
				}
				else
				{
					var getValue = getter.ReplaceParameters(fromExpression);
					var expr     = Expression.Condition(
						Expression.Equal(getValue, Expression.Constant(null, getValue.Type)),
						Expression.Constant(_mapper.MappingSchema.GetDefaultValue(toMember.Type), toMember.Type),
						GetExpressionExImpl(getValue, toMember.Type));

					binds.Add(Expression.Bind(toMember.MemberInfo, expr));
				}
			}

			var newExpression  = Expression.New(toType.GetDefaultConstructor());
			var initExpression = binds.Count > 0 ? (Expression)Expression.MemberInit(newExpression, binds) : newExpression;

			return initExpression;
		}

		MemberAssignment BuildAssignment(LambdaExpression getter, Expression fromExpression, Type fromMemberType, MemberAccessor toMember)
		{
			var getValue = getter.ReplaceParameters(fromExpression);
			var expr     = _mapper.MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
			var convert  = expr.ReplaceParameters(getValue);

			return Expression.Bind(toMember.MemberInfo, convert);
		}

		#endregion

		#region GetExpression

		public Expression<Func<TFrom,TTo,TTo>> GetExpression()
		{
			if (_mapper.MappingSchema.IsScalarType(typeof(TFrom)))
				throw new ArgumentException($"Type {typeof(TFrom).FullName} cannot be a scalar type. To convert scalar types use ConvertTo<TTo>.From(TFrom value).");

			if (_mapper.MappingSchema.IsScalarType(typeof(TTo)))
				throw new ArgumentException($"Type {typeof(TTo).FullName} cannot be a scalar type. To convert scalar types use ConvertTo<TTo>.From(TFrom value).");

			var pFrom = Expression.Parameter(typeof(TFrom), "from");
			var pTo   = Expression.Parameter(typeof(TTo),   "to");
			var expr  = new MappingImpl(this, pFrom, pTo).GetExpression();

			var l = Expression.Lambda<Func<TFrom,TTo,TTo>>(expr, pFrom, pTo);

			return l;
		}

		class MappingImpl
		{
			public MappingImpl(ExpressionBuilder<TFrom,TTo> builder, Expression fromExpression, Expression toExpression)
			{
				_builder        = builder;
				_fromExpression = fromExpression;
				_toExpression   = toExpression;
				_localObject    = Expression.Parameter(_toExpression.Type, "obj");
				_fromAccessor   = TypeAccessor.GetAccessor(_fromExpression.Type);
				_toAccessor     = TypeAccessor.GetAccessor(_toExpression.  Type);
			}

			readonly ExpressionBuilder<TFrom,TTo> _builder;
			readonly Expression                   _fromExpression;
			readonly Expression                   _toExpression;
			readonly ParameterExpression          _localObject;
			readonly TypeAccessor                 _fromAccessor;
			readonly TypeAccessor                 _toAccessor;
			readonly List<Expression>             _expressions = new List<Expression>();
			readonly List<ParameterExpression>    _locals      = new List<ParameterExpression>();

			public Expression GetExpression()
			{
				_locals.Add(_localObject);

				_expressions.Add(Expression.Assign(
					_localObject,
					Expression.Condition(
						Expression.Equal(_toExpression, Expression.Constant(null, _toExpression.Type)),
						Expression.New(_toExpression.Type.GetDefaultConstructor()),
						_toExpression)));

				foreach (var toMember in _toAccessor.Members.Where(_builder._mapper.MemberFilter))
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

					if (_builder._mapper.ToMappingDictionary == null ||
						!_builder._mapper.ToMappingDictionary.TryGetValue(_toExpression.Type, out mapDic) ||
						!mapDic.TryGetValue(toMember.Name, out toName))
						toName = toMember.Name;

					var fromMember = _fromAccessor.Members.FirstOrDefault(mi =>
					{
						string fromName;
						if (_builder._mapper.FromMappingDictionary == null ||
							!_builder._mapper.FromMappingDictionary.TryGetValue(_fromExpression.Type, out mapDic) ||
							!mapDic.TryGetValue(mi.Name, out fromName))
							fromName = mi.Name;
						return fromName == toName;
					});

					if (fromMember == null || !fromMember.HasGetter)
						continue;

					var getter = fromMember.GetterExpression;

					if (_builder._mapper.MappingSchema.IsScalarType(fromMember.Type) || _builder._mapper.MappingSchema.IsScalarType(toMember.Type))
					{
						_expressions.Add(BuildAssignment(getter, setter, fromMember.Type, _localObject, toMember));
					}
					else if (fromMember.Type == toMember.Type)
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
								Expression.Constant(_builder._mapper.MappingSchema.GetDefaultValue(toMember.Type), toMember.Type)),
							// else
							setter.ReplaceParameters(_localObject, 
								toMember.HasGetter ? BuildClassMapper(getValue, toMember) : _builder.GetExpressionExImpl(getValue, toMember.Type)));

						_expressions.Add(expr);
					}
				}

				_expressions.Add(_localObject);

				return Expression.Block(_locals, _expressions);
			}

			Expression BuildClassMapper(Expression getValue, MemberAccessor toMember)
			{
				var expr = new MappingImpl(_builder, getValue, toMember.GetterExpression.ReplaceParameters(_localObject)).GetExpression();

				if (_builder._mapper.ProcessCrossReferences != false)
				{
					
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
				var expr     = _builder._mapper.MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
				var convert  = expr.ReplaceParameters(getValue);

				return setter.ReplaceParameters(toExpression, convert);
			}
		}

		#endregion
	}
}
