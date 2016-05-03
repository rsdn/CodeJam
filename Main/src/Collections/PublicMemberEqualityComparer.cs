using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	using Reflection;

	/// <summary>
	/// Provides an implementations of the <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> generic interface
	/// based on object public members equality.
	/// </summary>
	/// <typeparam name="T">The type of objects to compare.</typeparam>
	public class PublicMemberEqualityComparer<T> : EqualityComparer<T>
	{
		#region Implementation of IEqualityComparer<in T>

		/// <summary>Determines whether the specified objects are equal.</summary>
		/// <returns>true if the specified objects are equal; otherwise, false.</returns>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object  to compare.</param>
		[Pure]
		public override bool Equals(T x, T y)
			=> x != null ? y != null && _equals(x, y) : y == null;

		/// <summary>Returns a hash code for the specified object.</summary>
		/// <returns>A hash code for the specified object.</returns>
		/// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.
		/// </exception>
		[Pure]
		public override int GetHashCode(T obj)
			=> obj == null ? 0 : _getHashCode(obj);

		#endregion

		static readonly Func<T,T,bool> _equals = GetEquals();

		static Func<T,T,bool> GetEquals()
		{
			var ta  = TypeAccessor.GetAccessor<T>();
			var x   = Expression.Parameter(typeof(T), "x");
			var y   = Expression.Parameter(typeof(T), "y");
			var exs = ta.Members.Select(ma =>
			{
				var eq = typeof(EqualityComparer<>).MakeGenericType(ma.Type);
				var pi = eq.GetProperty("Default");
				var mi = eq.GetMethods().Single(m => m.IsPublic && m.Name == "Equals" && m.GetParameters().Length == 2);

				return (Expression)Expression.Call(
					Expression.Property(null, pi),
					mi,
					Expression.PropertyOrField(x, ma.Name),
					Expression.PropertyOrField(y, ma.Name));
			});

			var ex = exs.AggregateOrDefault(Expression.AndAlso, () => Expression.Constant(true));
			var l  = Expression.Lambda<Func<T,T,bool>>(ex, x, y);

			return l.Compile();
		}

		static readonly Func<T,int> _getHashCode = GetGetHashCode();

		static Func<T,int> GetGetHashCode()
		{
			var ta  = TypeAccessor.GetAccessor<T>();
			var p   = Expression.Parameter(typeof(T),   "p");
			var num = Expression.Parameter(typeof(int), "num");
			var ex  = Expression.Block(
				new[] { num },
				new Expression[] { Expression.Assign(num, Expression.Constant(Objects.Random.Next())) }
					.Concat(ta.Members.Select(ma =>
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
									Expression.PropertyOrField(p, ma.Name))
								));
					}))
					.Concat(num));

			var l = Expression.Lambda<Func<T,int>>(ex, p);

			return l.Compile();
		}
	}
}
