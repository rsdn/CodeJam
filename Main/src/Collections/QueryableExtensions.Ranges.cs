#if !LESSTHAN_NET40
using System;
using System.Linq;
using System.Linq.Expressions;

using CodeJam.Expressions;
using CodeJam.Ranges;

using JetBrains.Annotations;

using static System.Linq.Expressions.Expression;

namespace CodeJam.Collections
{
	/// <summary>
	/// Marks boundaries that should be checked for inifinte (<c>null</c> values)
	/// </summary>
	[Flags]
	public enum QueryRangeBoundaries
	{
		/// <summary>Both boundaries are finite.</summary>
		FiniteBoth = 0x0,

		/// <summary>The boundary from is infinite.</summary>
		InfinityFrom = 0x2,

		/// <summary>The boundary to is infinite.</summary>
		InfinityTo = 0x1,

		/// <summary>Both boundaries are infinite</summary>
		InfiniteBoth = InfinityFrom | InfinityTo
	}

	/// <summary>
	/// Extension methods for <see cref="IQueryable"/>
	/// </summary>
	public static partial class QueryableExtensions
	{
		private static BinaryExpression FalseExpression() => NotEqual(Constant(1), Constant(1));

		private static Expression EnsureType<TValue>(Expression expression)
		{
			var result = expression;

			if (result.Type != typeof(TValue))
				result = Convert(result, typeof(TValue));

			return result;
		}

		// TODO: generic overloads W/O boxing ?

		/// <summary>Intersects source by specified values range.</summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="fromValueSelector">From value selector.</param>
		/// <param name="toValueSelector">To value selector.</param>
		/// <param name="range">The range.</param>
		/// <returns>Intersection</returns>
		public static IQueryable<T> Intersect<T, TValue>(
			[NotNull] this IQueryable<T> source,
			[NotNull] Expression<Func<T, object>> fromValueSelector,
			[NotNull] Expression<Func<T, object>> toValueSelector,
			Range<TValue> range) =>
				Intersect(source, fromValueSelector, toValueSelector, range, QueryRangeBoundaries.InfiniteBoth);

		/// <summary>Intersects source by specified values range.</summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="fromValueSelector">From value selector.</param>
		/// <param name="toValueSelector">To value selector.</param>
		/// <param name="range">The range.</param>
		/// <param name="rangeBoundaries">The range boundaries.</param>
		/// <returns>Intersection</returns>
		public static IQueryable<T> Intersect<T, TValue>(
			[NotNull] this IQueryable<T> source,
			[NotNull] Expression<Func<T, object>> fromValueSelector,
			[NotNull] Expression<Func<T, object>> toValueSelector,
			Range<TValue> range,
			QueryRangeBoundaries rangeBoundaries)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(fromValueSelector, nameof(fromValueSelector));
			Code.NotNull(toValueSelector, nameof(toValueSelector));

			if (range.IsInfinite)
				return source;

			if (range.IsEmpty)
				return source.Where(Lambda<Func<T, bool>>(FalseExpression(), Parameter(typeof(T))));

			Expression<Func<T, bool>> fromIsInfinite = null;
			Expression<Func<T, bool>> toIsInfinite = null;
			if (rangeBoundaries != QueryRangeBoundaries.FiniteBoth)
			{
				var eParam = fromValueSelector.Parameters.FirstOrDefault()
					?? toValueSelector.Parameters.FirstOrDefault()
						?? Parameter(typeof(T));

				if (rangeBoundaries.IsFlagSet(QueryRangeBoundaries.InfinityFrom))
				{
					var fromMember = fromValueSelector.ReplaceParameters(eParam).GetMemberExpression();
					fromIsInfinite = Lambda<Func<T, bool>>(Equal(fromMember, Constant(null)), eParam);
				}

				if (rangeBoundaries.IsFlagSet(QueryRangeBoundaries.InfinityTo))
				{
					var toMember = toValueSelector.ReplaceParameters(eParam).GetMemberExpression();
					toIsInfinite = Lambda<Func<T, bool>>(Equal(toMember, Constant(null)), eParam);
				}
			}

			return Intersect(source, fromValueSelector, toValueSelector, fromIsInfinite, toIsInfinite, range);
		}

		/// <summary>Intersects source by specified values range.</summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="source">The source.</param>
		/// <param name="fromValueSelector">From value selector.</param>
		/// <param name="toValueSelector">To value selector.</param>
		/// <param name="fromInfinityPredicate">From infinity predicate.</param>
		/// <param name="toInfinityPredicate">To infinity predicate.</param>
		/// <param name="range">The range.</param>
		/// <returns>Intersection</returns>
		public static IQueryable<T> Intersect<T, TValue>(
			[NotNull] this IQueryable<T> source,
			[NotNull] Expression<Func<T, object>> fromValueSelector,
			[NotNull] Expression<Func<T, object>> toValueSelector,
			[CanBeNull] Expression<Func<T, bool>> fromInfinityPredicate,
			[CanBeNull] Expression<Func<T, bool>> toInfinityPredicate,
			Range<TValue> range)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(fromValueSelector, nameof(fromValueSelector));
			Code.NotNull(toValueSelector, nameof(toValueSelector));

			if (range.IsInfinite)
				return source;

			if (range.IsEmpty)
				return source.Where(Lambda<Func<T, bool>>(FalseExpression(), Parameter(typeof(T))));

			var eParam = fromValueSelector.Parameters.FirstOrDefault()
				?? toValueSelector.Parameters.FirstOrDefault()
					?? Parameter(typeof(T));

			var fromMember = fromValueSelector.ReplaceParameters(eParam).GetMemberExpression();
			var toMember = toValueSelector.ReplaceParameters(eParam).GetMemberExpression();

			var fromInfinityPredicateExpression = fromInfinityPredicate?.ReplaceParameters(eParam);
			var toInfinityPredicateExpression = toInfinityPredicate?.ReplaceParameters(eParam);

			var left =
				range.From.IsNegativeInfinity
					? null
					: range.From.IsInclusiveBoundary
						? GreaterThanOrEqual(
							EnsureType<TValue>(toMember),
							Constant(range.FromValue, typeof(TValue)))
						: GreaterThan(
							EnsureType<TValue>(toMember),
							Constant(range.FromValue, typeof(TValue)));
			if (left != null && toInfinityPredicateExpression != null)
			{
				left = OrElse(toInfinityPredicateExpression, left);
			}

			var right =
				range.To.IsPositiveInfinity
					? null
					: range.To.IsInclusiveBoundary
						? LessThanOrEqual(
							EnsureType<TValue>(fromMember),
							Constant(range.ToValue, typeof(TValue)))
						: LessThan(
							EnsureType<TValue>(fromMember),
							Constant(range.ToValue, typeof(TValue)));
			if (right != null && fromInfinityPredicateExpression != null)
			{
				right = OrElse(fromInfinityPredicateExpression, right);
			}

			var predicate = left != null && right != null
				? And(left, right)
				: left ?? right ?? FalseExpression();

			// DbEnd >= @from AND DbStart <= @to
			return source.Where(Lambda<Func<T, bool>>(predicate, eParam));
		}
	}
}
#endif