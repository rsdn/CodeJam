using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	using Expressions;
	using Reflection;

	/// <summary>
	/// Maps an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
	/// </summary>
	/// <typeparam name="TFrom">Type to map from.</typeparam>
	/// <typeparam name="TTo">Type to map to.</typeparam>
	[PublicAPI]
	public class ExpressionMapper<TFrom,TTo>
	{
		MappingSchema _mappingSchema = MappingSchema.Default;

		/// <summary>
		/// Mapping schema.
		/// </summary>
		[NotNull]
		public MappingSchema MappingSchema
		{
			get { return _mappingSchema; }
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value), "MappingSchema cannot be null.");

				_mappingSchema = value;
			}
		}

		/// <summary>
		/// Member mappers.
		/// </summary>
		public Tuple<MemberInfo[],LambdaExpression>[] MemberMappers { get; set; }

		/// <summary>
		/// If true, processes object cross references. 
		/// if default (null), the <see cref="GetExpression"/> method does not process cross references,
		/// however the <see cref="GetActionExpression"/> method does.
		/// </summary>
		public bool? ProcessCrossReferences { get; set; }

		/// <summary>
		/// Filters target members to map.
		/// </summary>
		public Func<MemberAccessor,bool> MemberFilter { get; set; } = _ => true;

		#region GetExpression

		/// <summary>
		/// Returns an expression that maps an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure]
		public Expression<Func<TFrom,TTo>> GetExpression()
		{
			if (MappingSchema.IsScalarType(typeof(TFrom)) || MappingSchema.IsScalarType(typeof(TTo)))
				return MappingSchema.GetConvertExpression<TFrom, TTo>();

			var pFrom = Expression.Parameter(typeof(TFrom), "from");

			var expr = ProcessCrossReferences == true ?
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

			foreach (var toMember in toAccessor.Members.Where(MemberFilter))
			{
				if (!toMember.HasSetter)
					continue;

				if (MemberMappers != null)
				{
					var processed = false;

					foreach (var item in MemberMappers)
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

				var fromMember = fromAccessor.Members.FirstOrDefault(mi => mi.Name == toMember.Name);

				if (fromMember == null || !fromMember.HasGetter)
					continue;

				var getter = fromMember.GetterExpression;

				if (MappingSchema.IsScalarType(fromMember.Type) || MappingSchema.IsScalarType(toMember.Type))
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
						Expression.Constant(MappingSchema.GetDefaultValue(toMember.Type), toMember.Type),
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
			var expr     = MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
			var convert  = expr.ReplaceParameters(getValue);

			return Expression.Bind(toMember.MemberInfo, convert);
		}

		#endregion

		#region GetActionExpression

		/// <summary>
		/// Returns an expression that maps an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure]
		public Expression<Func<TFrom,TTo,TTo>> GetActionExpression()
		{
			if (MappingSchema.IsScalarType(typeof(TFrom)))
				throw new ArgumentException($"Type {typeof(TFrom).FullName} cannot be a scalar type. To convert scalar types use ConvertTo<TTo>.From(TFrom value).");

			if (MappingSchema.IsScalarType(typeof(TTo)))
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

			foreach (var toMember in toAccessor.Members.Where(MemberFilter))
			{
				if (!toMember.HasSetter)
					continue;

				var setter = toMember.SetterExpression;

				if (MemberMappers != null)
				{
					var processed = false;

					foreach (var item in MemberMappers)
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

				var fromMember = fromAccessor.Members.FirstOrDefault(mi => mi.Name == toMember.Name);

				if (fromMember == null || !fromMember.HasGetter)
					continue;

				var getter = fromMember.GetterExpression;

				if (MappingSchema.IsScalarType(fromMember.Type) || MappingSchema.IsScalarType(toMember.Type))
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
						Expression.Constant(MappingSchema.GetDefaultValue(toMember.Type), toMember.Type),
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
			var expr     = MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
			var convert  = expr.ReplaceParameters(getValue);

			return setter.ReplaceParameters(toExpression, convert);
		}

		#endregion
	}
}
