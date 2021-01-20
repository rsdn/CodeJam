using System;
using System.Linq;
using System.Linq.Expressions;

using CodeJam.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	public partial class QueryableExtensions
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property) =>
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IOrderedQueryable<T> OrderByDescending<T>(
			this IQueryable<T> source, string property) =>
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property) =>
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IOrderedQueryable<T> ThenByDescending<T>(
			this IOrderedQueryable<T> source,
			string property) =>
				ApplyOrder(source, property, nameof(ThenByDescending));

		[Pure, System.Diagnostics.Contracts.Pure]
		private static IOrderedQueryable<T> ApplyOrder<T>(this IQueryable<T> source, string property, string method)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNullNorEmpty(property, nameof(property));

			var parameter = Expression.Parameter(typeof(T), "p");
			var member = !property.Contains('.', StringComparison.Ordinal)
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