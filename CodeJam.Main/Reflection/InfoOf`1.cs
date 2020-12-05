using System;
using System.Linq.Expressions;
using System.Reflection;

using CodeJam.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	/// Provides a helper class to get the property, field, ctor or method from an expression.
	/// </summary>
	/// <typeparam name="T">Type of object.</typeparam>
	[PublicAPI]
	public static class InfoOf<T>
	{
		/// <summary>
		/// Returns the property or field.
		/// </summary>
		/// <typeparam name="TValue">Member value.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo"/> instance.
		/// </returns>
		[Pure]
		public static MemberInfo? Member<TValue>([NotNull] Expression<Func<T, TValue>> expression) =>
			expression.GetMemberInfo();

		/// <summary>
		/// Returns the property.
		/// </summary>
		/// <typeparam name="TValue">Member value.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="PropertyInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static PropertyInfo Property<TValue>([NotNull] Expression<Func<T, TValue>> expression) =>
			expression.GetProperty();

		/// <summary>
		/// Returns the field.
		/// </summary>
		/// <typeparam name="TValue">Member value.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="FieldInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static FieldInfo Field<TValue>([NotNull] Expression<Func<T, TValue>> expression) =>
			expression.GetField();

		/// <summary>
		/// Returns the constructor.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="ConstructorInfo"/> instance.
		/// </returns>
		[Pure]
		public static ConstructorInfo? Constructor([NotNull] Expression<Func<T>> expression) =>
			expression.GetConstructor();

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <typeparam name="TResult">Type of result.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static MethodInfo Method<TResult>([NotNull] Expression<Func<T, TResult>> expression) =>
			expression.GetMethod();

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static MethodInfo Method([NotNull] Expression<Action<T>> expression) =>
			expression.GetMethod();
	}
}