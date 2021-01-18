using System;
using System.Linq.Expressions;
using System.Reflection;

using CodeJam.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	///     Provides a helper class to get the property, field, ctor or method from an expression.
	/// </summary>
	[PublicAPI]
	public static class InfoOf
	{
		#region Member
		/// <summary>
		/// Returns the <see cref="MemberInfo"/>.
		/// </summary>
		/// <typeparam name="TValue">Type of value.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MemberInfo? Member<TValue>(Expression<Func<TValue>> expression) =>
			expression.GetMemberInfo();

		/// <summary>
		/// Returns the <see cref="MemberInfo"/>.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MemberInfo? Member<T>(Expression<Func<T, object>> expression) =>
			expression.GetMemberInfo();

		/// <summary>
		/// Returns the <see cref="MemberInfo"/>.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <typeparam name="TValue">Type of value.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MemberInfo? Member<T, TValue>(Expression<Func<T, TValue>> expression) =>
			expression.GetMemberInfo();
		#endregion

		#region Property
		/// <summary>
		/// Returns the property.
		/// </summary>
		/// <typeparam name="TValue">Type of value.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="PropertyInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static PropertyInfo Property<TValue>(Expression<Func<TValue>> expression) =>
			expression.GetProperty();

		/// <summary>
		/// Returns the property.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="PropertyInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static PropertyInfo Property<T>(Expression<Func<T, object>> expression) =>
			expression.GetProperty();

		/// <summary>
		/// Returns the property.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <typeparam name="TValue">Type of value.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="PropertyInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static PropertyInfo Property<T, TValue>(Expression<Func<T, TValue>> expression) =>
			expression.GetProperty();
		#endregion

		#region Field
		/// <summary>
		/// Returns the field.
		/// </summary>
		/// <typeparam name="TValue">Type of value.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="FieldInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static FieldInfo Field<TValue>(Expression<Func<TValue>> expression) =>
			expression.GetField();

		/// <summary>
		/// Returns the field.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="FieldInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static FieldInfo Field<T>(Expression<Func<T, object>> expression) =>
			expression.GetField();

		/// <summary>
		/// Returns the field.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <typeparam name="TValue">Type of value.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="FieldInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static FieldInfo Field<T, TValue>(Expression<Func<T, TValue>> expression) =>
			expression.GetField();
		#endregion

		#region Constructor
		/// <summary>
		/// Returns the constructor.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <returns>
		/// The <see cref="ConstructorInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ConstructorInfo? Constructor<T>() where T : new() =>
			Expression.New(typeof(T)).Constructor;

		/// <summary>
		/// Returns the constructor.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="ConstructorInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ConstructorInfo? Constructor<T>(Expression<Func<T>> expression) =>
			expression.GetConstructor();
		#endregion

		#region Method
		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MethodInfo Method<T>(Expression<Func<T>> expression) =>
			expression.GetMethod();

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MethodInfo Method<T>(Expression<Func<T, object>> expression) =>
			expression.GetMethod();

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <typeparam name="TResult">Type of result.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MethodInfo Method<T, TResult>(Expression<Func<T, TResult>> expression) =>
			expression.GetMethod();

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MethodInfo Method(Expression<Action> expression) =>
			expression.GetMethod();

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MethodInfo Method<T>(Expression<Action<T>> expression) =>
			expression.GetMethod();
		#endregion
	}
}