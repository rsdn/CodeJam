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
		/// Filters target members to map.
		/// </summary>
		public Func<MemberAccessor,bool> MemberFilter { get; set; } = _ => true;

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
			var expr  = GetExpression(pFrom, null, typeof(TTo));

			var l = Expression.Lambda<Func<TFrom,TTo>>(expr, pFrom);

			return l;
		}

		/// <summary>
		/// Returns an expression that maps an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure]
		public Expression<Action<TFrom,TTo>> GetActionExpression()
		{
			if (MappingSchema.IsScalarType(typeof(TFrom)))
				throw new ArgumentException($"Type {typeof(TFrom).FullName} cannot be a scalar type. To convert scalar types use ConvertTo<TTo>.From(TFrom value).");

			if (MappingSchema.IsScalarType(typeof(TTo)))
				throw new ArgumentException($"Type {typeof(TTo).FullName} cannot be a scalar type. To convert scalar types use ConvertTo<TTo>.From(TFrom value).");

			var pFrom = Expression.Parameter(typeof(TFrom), "from");
			var pTo   = Expression.Parameter(typeof(TTo),   "to");
			var expr  = GetExpression(pFrom, pTo, pTo.Type);

			var l = Expression.Lambda<Action<TFrom,TTo>>(expr, pFrom, pTo);

			return l;
		}

		Expression GetExpression(Expression fromExpression, Expression toExpression, Type toType)
		{
			var fromAccessor = TypeAccessor.GetAccessor(fromExpression.Type);
			var toAccessor   = TypeAccessor.GetAccessor(toType);

			var expressions = new List<Expression>();
			var binds       = new List<MemberAssignment>();

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
							BuildAssign(expressions, binds, item.Item2, setter, fromExpression, item.Item2.Type, toExpression, toMember);
							processed = true;
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
					BuildAssign(expressions, binds, getter, setter, fromExpression, fromMember.Type, toExpression, toMember);
				}
			}

			if (toExpression == null)
			{
				var newExpression = Expression.New(toType.GetDefaultConstructor());
				toExpression = binds.Count > 0 ? (Expression)Expression.MemberInit(newExpression, binds) : newExpression;
			}

			expressions.Add(toExpression);

			return expressions.Count > 1 ? Expression.Block(expressions) : expressions[0];
		}

		void BuildAssign(
			List<Expression>       expressions,
			List<MemberAssignment> binds,
			LambdaExpression       getter,
			LambdaExpression       setter,
			Expression fromExpression, Type fromMemberType,
			Expression toExpression,   MemberAccessor toMember)
		{
			var getValue = getter.ReplaceParameters(fromExpression);
			var expr     = MappingSchema.GetConvertExpression(fromMemberType, toMember.Type);
			var convert  = expr.ReplaceParameters(getValue);

			if (toExpression != null)
			{
				var setValue = setter.ReplaceParameters(toExpression, convert);
				expressions.Add(setValue);
			}
			else
			{
				var setValue = Expression.Bind(toMember.MemberInfo, convert);
				binds.Add(setValue);
			}
		}
	}
}
