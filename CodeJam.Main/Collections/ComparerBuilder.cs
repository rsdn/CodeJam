#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
// ReSharper disable once RedundantUsingDirective
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	using Expressions;

	using Reflection;

	/// <summary>
	/// Builds comparer functions and comparers.
	/// </summary>
	/// <typeparam name="T">The type of objects to compare.</typeparam>
	[PublicAPI]
	public static class ComparerBuilder<T>
		where T : notnull
	{
		/// <summary>
		/// Returns GetEqualsFunc function for type T to compare.
		/// </summary>
		/// <returns>GetEqualsFunc function.</returns>
		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T?, T?, bool> GetEqualsFunc() =>
			GetEqualsFunc(TypeAccessor.GetAccessor<T>().Members);

		/// <summary>
		/// Returns GetEqualsFunc function for provided members for type T to compare.
		/// </summary>
		/// <param name="members">Members to compare.</param>
		/// <returns>GetEqualsFunc function.</returns>
		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T?, T?, bool> GetEqualsFunc([NotNull, InstantHandle] IEnumerable<MemberAccessor> members) =>
			CreateEqualsFunc(members.Select(m => m.GetterExpression));

		/// <summary>
		/// Returns GetEqualsFunc function for provided members for type T to compare.
		/// </summary>
		/// <param name="members">Members to compare.</param>
		/// <returns>GetEqualsFunc function.</returns>
		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T?, T?, bool> GetEqualsFunc([NotNull] params Expression<Func<T, object>>[] members) =>
			CreateEqualsFunc(members);

		/// <summary>
		/// Returns GetHashCode function for type T to compare.
		/// </summary>
		/// <returns>GetHashCode function.</returns>
		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T?, int> GetGetHashCodeFunc() =>
			GetGetHashCodeFunc(TypeAccessor.GetAccessor<T>().Members);

		// ReSharper disable once StaticMemberInGenericType
		private static readonly int _randomSeed = Objects.Random.Next();

		/// <summary>
		/// Returns GetHashCode function for provided members for type T to compare.
		/// </summary>
		/// <param name="members">Members to compare.</param>
		/// <returns>GetHashCode function.</returns>
		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T?, int> GetGetHashCodeFunc([NotNull, InstantHandle] IEnumerable<MemberAccessor> members) =>
			CreateGetHashCodeFunc(members.Select(m => m.GetterExpression));

		/// <summary>
		/// Returns GetHashCode function for provided members for type T to compare.
		/// </summary>
		/// <param name="members">Members to compare.</param>
		/// <returns>GetHashCode function.</returns>
		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		public static Func<T, int> GetGetHashCodeFunc([NotNull] params Expression<Func<T, object>>[] members) =>
			CreateGetHashCodeFunc(members);

		private class Comparer : EqualityComparer<T?>
		{
			public Comparer([NotNull] Func<T?, T?, bool> equals, [NotNull] Func<T?, int> getHashCode)
			{
				_equals = equals;
				_getHashCode = getHashCode;
			}

			[NotNull] private readonly Func<T?, T?, bool> _equals;
			[NotNull] private readonly Func<T?, int> _getHashCode;

			public override bool Equals(T? x, T? y) =>
				x != null ? y != null && _equals(x, y) : y == null;

			public override int GetHashCode(T? obj) =>
				obj == null ? 0 : _getHashCode(obj);
		}

		private static Comparer? _equalityComparer;

		/// <summary>
		/// Returns implementations of the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface
		/// based on object public members equality.
		/// </summary>
		/// <returns>Instance of <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</returns>
		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		public static IEqualityComparer<T?> GetEqualityComparer() =>
			_equalityComparer ??= new Comparer(GetEqualsFunc(), GetGetHashCodeFunc());

		/// <summary>
		/// Returns implementations of the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface
		/// based on provided object public members equality.
		/// </summary>
		/// <param name="membersToCompare">Members to compare.</param>
		/// <returns>Instance of <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</returns>
		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		public static IEqualityComparer<T> GetEqualityComparer([NotNull] params Expression<Func<T?, object>>[] membersToCompare)
		{
			Code.NotNull(membersToCompare, nameof(membersToCompare));
			return new Comparer(CreateEqualsFunc(membersToCompare), CreateGetHashCodeFunc(membersToCompare));
		}

		/// <summary>
		/// Returns implementations of the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface
		/// based on provided object public members equality.
		/// </summary>
		/// <param name="membersToCompare">A function that returns members to compare.</param>
		/// <returns>Instance of <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</returns>
		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		public static IEqualityComparer<T?> GetEqualityComparer(
			[NotNull, InstantHandle] Func<TypeAccessor<T>,
			IEnumerable<MemberAccessor>> membersToCompare)
		{
			var members = membersToCompare(TypeAccessor<T>.GetAccessor()).ToList();
			return new Comparer(GetEqualsFunc(members), GetGetHashCodeFunc(members));
		}

		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		private static Func<T?, T?, bool> CreateEqualsFunc([NotNull] IEnumerable<LambdaExpression> membersToCompare)
		{
			Code.NotNull(membersToCompare, nameof(membersToCompare));

			var x = Expression.Parameter(typeof(T), "x");
			var y = Expression.Parameter(typeof(T), "y");

			var expressions = membersToCompare.Select(me =>
			{
				var arg0 = RemoveCastToObject(me.ReplaceParameters(x));
				var arg1 = RemoveCastToObject(me.ReplaceParameters(y));

				var eq = typeof(EqualityComparer<>).MakeGenericType(arg1.Type);
				var pi = eq.GetProperty("Default");
				var mi = eq.GetMethods().Single(m => m.IsPublic && m.Name == "Equals" && m.GetParameters().Length == 2);

				DebugCode.BugIf(pi is null, "pi != null");
				// ReSharper disable once AssignNullToNotNullAttribute
				return (Expression)Expression.Call(Expression.Property(null, pi), mi, arg0, arg1);
			});

			var expression = expressions
				.DefaultIfEmpty(Expression.Constant(true))
				.Aggregate(Expression.AndAlso);

			return Expression
				.Lambda<Func<T?, T?, bool>>(expression, x, y)
				.Compile();
		}

		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		private static Func<T?, int> CreateGetHashCodeFunc([NotNull] IEnumerable<LambdaExpression> membersToEval)
		{
			Code.NotNull(membersToEval, nameof(membersToEval));

			var parameter = Expression.Parameter(typeof(T), "parameter");
			var expression = membersToEval.Aggregate(
				(Expression)Expression.Constant(_randomSeed),
				(e, me) =>
				{
					var ma = RemoveCastToObject(me.ReplaceParameters(parameter));
					var eq = typeof(EqualityComparer<>).MakeGenericType(ma.Type);
					var pi = eq.GetProperty("Default");
					var mi = eq.GetMethods().Single(m => m.IsPublic && m.Name == "GetHashCode" && m.GetParameters().Length == 1);

					DebugCode.BugIf(pi is null, "pi != null");
					return Expression.Add(
						Expression.Multiply(e, Expression.Constant(-1521134295)),
						// ReSharper disable once AssignNullToNotNullAttribute
						Expression.Call(Expression.Property(null, pi), mi, ma));
				});

			return Expression
				.Lambda<Func<T?, int>>(expression, parameter)
				.Compile();
		}

		[NotNull, Pure, System.Diagnostics.Contracts.Pure]
		private static Expression RemoveCastToObject([NotNull] Expression expression)
		{
			if (expression.Type != typeof(object) || expression.NodeType != ExpressionType.Convert)
				return expression;

			return ((UnaryExpression)expression).Operand;
		}
	}
}
#endif