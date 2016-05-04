using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	using Expressions;
	using Reflection;

	/// <summary>
	/// Builds comparer fanctions and comparers.
	/// </summary>
	/// <typeparam name="T">The type of objects to compare.</typeparam>
	[PublicAPI]
	public static class ComparerBuilder<T>
	{
		/// <summary>
		/// Returns GetEqualsFunc function for type T to compare.
		/// </summary>
		/// <returns>GetEqualsFunc function.</returns>
		[Pure]
		public static Func<T,T,bool> GetEqualsFunc()
			=> GetEqualsFunc(TypeAccessor.GetAccessor<T>().Members);

		/// <summary>
		/// Returns GetEqualsFunc function for provided members for type T to compare.
		/// </summary>
		/// <param name="members">Members to compare.</param>
		/// <returns>GetEqualsFunc function.</returns>
		[Pure]
		public static Func<T,T,bool> GetEqualsFunc(IEnumerable<MemberAccessor> members)
		{
			var x   = Expression.Parameter(typeof(T), "x");
			var y   = Expression.Parameter(typeof(T), "y");
			var exs = members.Select(ma =>
			{
				var eq = typeof(EqualityComparer<>).MakeGenericType(ma.Type);
				var pi = eq.GetProperty("Default");
				var mi = eq.GetMethods().Single(m => m.IsPublic && m.Name == "Equals" && m.GetParameters().Length == 2);

				return (Expression)Expression.Call(
					Expression.Property(null, pi),
					mi,
					ma.GetterExpression.ReplaceParameters(x),
					ma.GetterExpression.ReplaceParameters(y));
			});

			var ex = exs.AggregateOrDefault(Expression.AndAlso, () => Expression.Constant(true));
			var l  = Expression.Lambda<Func<T,T,bool>>(ex, x, y);

			return l.Compile();
		}

		/// <summary>
		/// Returns GetHashCode function for type T to compare.
		/// </summary>
		/// <returns>GetHashCode function.</returns>
		[Pure]
		public static Func<T,int> GetGetHashCodeFunc()
			=> GetGetHashCodeFunc(TypeAccessor.GetAccessor<T>().Members);

		// ReSharper disable once StaticMemberInGenericType
		static readonly int _randomSeed = Objects.Random.Next();

		/// <summary>
		/// Returns GetHashCode function for provided members for type T to compare.
		/// </summary>
		/// <param name="members">Members to compare.</param>
		/// <returns>GetHashCode function.</returns>
		[Pure]
		public static Func<T,int> GetGetHashCodeFunc(IEnumerable<MemberAccessor> members)
		{
			var p   = Expression.Parameter(typeof(T),   "p");
			var num = Expression.Parameter(typeof(int), "num");
			var ex  = Expression.Block(
				new[] { num },
				new Expression[] { Expression.Assign(num, Expression.Constant(_randomSeed)) }
					.Concat(members.Select(ma =>
					{
						var eq = typeof(EqualityComparer<>).MakeGenericType(ma.Type);
						var pi = eq.GetProperty("Default");
						var mi = eq.GetMethods().Single(m => m.IsPublic && m.Name == "GetHashCode" && m.GetParameters().Length == 1);

						return Expression.Assign(
							num,
							Expression.Add(
								Expression.Multiply(num, Expression.Constant(-1521134295 )),
								Expression.Call(
									Expression.Property(null, pi),
									mi,
									ma.GetterExpression.ReplaceParameters(p))
								));
					}))
					.Concat(num));

			var l = Expression.Lambda<Func<T,int>>(ex, p);

			return l.Compile();
		}

		class Comparer : EqualityComparer<T>
		{
			public Comparer(Func<T,T,bool> equals, Func<T,int> getHashCode)
			{
				_equals      = equals;
				_getHashCode = getHashCode;
			}

			private readonly Func<T,T,bool> _equals;
			private readonly Func<T,int>    _getHashCode;

			public override bool Equals(T x, T y)
				=> x != null ? y != null && _equals(x, y) : y == null;

			public override int GetHashCode(T obj)
				=> obj == null ? 0 : _getHashCode(obj);
		}

		static Comparer _equalityComparer;

		/// <summary>
		/// Returns implementations of the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface
		/// based on object public members equality.
		/// </summary>
		/// <returns>Instance of <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</returns>
		[Pure]
		public static IEqualityComparer<T> GetEqualityComparer()
			=> _equalityComparer ?? (_equalityComparer = new Comparer(GetEqualsFunc(), GetGetHashCodeFunc()));

		/// <summary>
		/// Returns implementations of the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface
		/// based on provided object public members equality.
		/// </summary>
		/// <param name="membersToCompare">members to compare.</param>
		/// <returns>Instance of <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</returns>
		[Pure]
		public static IEqualityComparer<T> GetEqualityComparer([NotNull] params Expression<Func<T,object>>[] membersToCompare)
		{
			if (membersToCompare == null) throw new ArgumentNullException(nameof(membersToCompare));

			var hashSet = new HashSet<MemberInfo>();

			foreach (var func in membersToCompare)
			{
				var mi = InfoOf<T>.Member(func);

				if (!hashSet.Contains(mi))
					hashSet.Add(mi);
			}

			var members = TypeAccessor<T>.GetAccessor().Members.Where(m => hashSet.Contains(m.MemberInfo)).ToList();

			return new Comparer(GetEqualsFunc(members), GetGetHashCodeFunc(members));
		}
	}
}
