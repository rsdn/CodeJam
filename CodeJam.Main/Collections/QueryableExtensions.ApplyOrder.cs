using System;
using System.Linq;
using System.Linq.Expressions;

using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class QueryableExtensions
	{
		/// <summary>
		/// Sorts the elements of a sequence in ascending order according to a key.
		/// </summary>
		/// <typeparam name="T">Type of elements in source.</typeparam>
		/// <param name="source">A sequence of values to order.</param>
		/// <param name="property">The property name.</param>
		/// <returns>
		/// An <see cref="IOrderedQueryable{TElement}"/> whose elements are sorted according to a key.
		/// </returns>
		[NotNull, Pure]
		public static IOrderedQueryable<T> OrderBy<T>([NotNull] this IQueryable<T> source, [NotNull] string property) =>
			ApplyOrder(source, property, nameof(OrderBy));

		/// <summary>
		/// Sorts the elements of a sequence in descending order according to a key.
		/// </summary>
		/// <typeparam name="T">Type of elements in source.</typeparam>
		/// <param name="source">A sequence of values to order.</param>
		/// <param name="property">The property name.</param>
		/// <returns>
		/// An <see cref="IOrderedQueryable{TElement}"/> whose elements are sorted according to a key.
		/// </returns>
		[NotNull, Pure]
		public static IOrderedQueryable<T> OrderByDescending<T>(
			[NotNull] this IQueryable<T> source, [NotNull] string property) =>
				ApplyOrder(source, property, nameof(OrderByDescending));

		/// <summary>
		/// Performs a subsequent ordering of the elements in a sequence in ascending order according to a key.
		/// </summary>
		/// <typeparam name="T">Type of elements in source.</typeparam>
		/// <param name="source">An <see cref="IOrderedEnumerable{TElement}"/> that contains elements to sort.</param>
		/// <param name="property">The property name.</param>
		/// <returns>
		/// An <see cref="IOrderedQueryable{TElement}"/> whose elements are sorted according to a key.
		/// </returns>
		[NotNull, Pure]
		public static IOrderedQueryable<T> ThenBy<T>([NotNull] this IOrderedQueryable<T> source, [NotNull] string property) =>
			ApplyOrder(source, property, nameof(ThenBy));

		/// <summary>
		/// Performs a subsequent ordering of the elements in a sequence in descending order according to a key.
		/// </summary>
		/// <typeparam name="T">Type of elements in source.</typeparam>
		/// <param name="source">An <see cref="IOrderedEnumerable{TElement}"/> that contains elements to sort.</param>
		/// <param name="property">The property name.</param>
		/// <returns>
		/// An <see cref="IOrderedQueryable{TElement}"/> whose elements are sorted according to a key.
		/// </returns>
		[NotNull, Pure]
		public static IOrderedQueryable<T> ThenByDescending<T>(
			[NotNull] this IOrderedQueryable<T> source,
			[NotNull] string property)
			=> ApplyOrder(source, property, nameof(ThenByDescending));

		[NotNull, Pure]
		private static IOrderedQueryable<T> ApplyOrder<T>([NotNull] this IQueryable<T> source, [NotNull] string property, [NotNull] string method)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNullNorEmpty(property, nameof(property));

			var parameter = Expression.Parameter(typeof(T), "p");
			var member = property.IndexOf('.') == -1
				? Expression.PropertyOrField(parameter, property)
				: property.Split('.').Aggregate((Expression)parameter, Expression.PropertyOrField);
			var expression = Expression.Lambda(member, parameter);

			return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(
				Expression.Call(
					typeof(Queryable),
					method,
					new[] { typeof(T), ((MemberExpression)member).Member.GetMemberType() },
					source.Expression,
					Expression.Quote(expression)));
		}
	}
}