using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodeJam.Mapping
{
	using Expressions;
	using Reflection;

	class ExpressionMapper<TFrom,TTo>
	{
		public ExpressionMapper(Mapper<TFrom,TTo> mapper, Tuple<MemberInfo[],LambdaExpression>[] memberMappers)
		{
			_mapper        = mapper;
			_memberMappers = memberMappers;
		}

		readonly Mapper<TFrom,TTo>                      _mapper;
		readonly Tuple<MemberInfo[],LambdaExpression>[] _memberMappers;

		#region GetExpression

		public Expression<Func<TFrom,TTo>> GetExpression()
		{
			if (_mapper.MappingSchema.IsScalarType(typeof(TFrom)) || _mapper.MappingSchema.IsScalarType(typeof(TTo)))
				return _mapper.MappingSchema.GetConvertExpression<TFrom, TTo>();

			var pFrom = Expression.Parameter(typeof(TFrom), "from");

			var expr = _mapper.ProcessCrossReferences == true ?
				GetActionExpressionImpl(pFrom, Expression.Constant(null, typeof(TTo))) :
				GetExpressionImpl      (pFrom, typeof(TTo));

			var l = Expression.Lambda<Func<TFrom,TTo>>(expr, pFrom);

			return l;
		}

		Expression GetExpressionImpl(Expression fromExpression, Type toType)
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
						GetExpressionImpl(getValue, toMember.Type));

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

		#region GetActionExpression

		public Expression<Func<TFrom,TTo,TTo>> GetActionExpression()
		{
			if (_mapper.MappingSchema.IsScalarType(typeof(TFrom)))
				throw new ArgumentException($"Type {typeof(TFrom).FullName} cannot be a scalar type. To convert scalar types use ConvertTo<TTo>.From(TFrom value).");

			if (_mapper.MappingSchema.IsScalarType(typeof(TTo)))
				throw new ArgumentException($"Type {typeof(TTo).FullName} cannot be a scalar type. To convert scalar types use ConvertTo<TTo>.From(TFrom value).");

			var pFrom = Expression.Parameter(typeof(TFrom), "from");
			var pTo   = Expression.Parameter(typeof(TTo),   "to");
			var expr  = GetActionExpressionImpl(pFrom, pTo);

			var l = Expression.Lambda<Func<TFrom,TTo,TTo>>(expr, pFrom, pTo);

			return l;
		}

		Expression GetActionExpressionImpl(Expression fromExpression, Expression toExpression)
		{
			var fromAccessor = TypeAccessor.GetAccessor(fromExpression.Type);
			var toAccessor   = TypeAccessor.GetAccessor(toExpression.  Type);

			var expressions = new List<Expression>();
			var locals      = new List<ParameterExpression>();
			var localObject = Expression.Parameter(toExpression.Type, "obj");

			locals.Add(localObject);

			expressions.Add(Expression.Assign(
				localObject,
				Expression.Condition(
					Expression.Equal(toExpression, Expression.Constant(null, toExpression.Type)),
					Expression.New(toExpression.Type.GetDefaultConstructor()),
					toExpression)));

			foreach (var toMember in toAccessor.Members.Where(_mapper.MemberFilter))
			{
				if (!toMember.HasSetter)
					continue;

				var setter = toMember.SetterExpression;

				if (_memberMappers != null)
				{
					var processed = false;

					foreach (var item in _memberMappers)
					{
						if (item.Item1.Length == 1 && item.Item1[0] == toMember.MemberInfo)
						{
							expressions.Add(BuildAssignment(item.Item2, setter, fromExpression, item.Item2.Type, localObject, toMember));
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
					!_mapper.ToMappingDictionary.TryGetValue(toExpression.Type, out mapDic) ||
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
					expressions.Add(BuildAssignment(getter, setter, fromExpression, fromMember.Type, localObject, toMember));
				}
				else if (fromMember.Type == toMember.Type)
				{
					expressions.Add(setter.ReplaceParameters(localObject, getter.ReplaceParameters(fromExpression)));
				}
				else
				{
					var getValue = getter.ReplaceParameters(fromExpression);
					var expr     = Expression.Condition(
						Expression.Equal(getValue, Expression.Constant(null, getValue.Type)),
						Expression.Constant(_mapper.MappingSchema.GetDefaultValue(toMember.Type), toMember.Type),
						toMember.HasGetter ?
							GetActionExpressionImpl(getValue, toMember.GetterExpression.ReplaceParameters(localObject)) :
							GetExpressionImpl      (getValue, toMember.Type));

					expressions.Add(setter.ReplaceParameters(localObject, expr));
				}
			}

			expressions.Add(localObject);

			return Expression.Block(locals, expressions);
		}

		Expression BuildAssignment(
			LambdaExpression getter,
			LambdaExpression setter,
			Expression fromExpression, Type fromMemberType,
			Expression toExpression,   MemberAccessor toMember)
		{
			var getValue = getter.ReplaceParameters(fromExpression);
			var expr     = _mapper.MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
			var convert  = expr.ReplaceParameters(getValue);

			return setter.ReplaceParameters(toExpression, convert);
		}

		#endregion
	}
}
