using System;
using System.Linq.Expressions;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	///     Provides a helper class to get the property, field, ctor or method from an expression.
	/// </summary>
	[PublicAPI]
	public static class InfoOf {
		#region Member
		/// <summary>
		/// Returns the <see cref="MemberInfo" />.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static MemberInfo Member<TValue>([NotNull] Expression<Func<TValue>> expression) =>
			ExpressionHelper.GetMemberInfo(expression);

		/// <summary>
		/// Returns the <see cref="MemberInfo" />.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static MemberInfo Member<T>([NotNull] Expression<Func<T, object>> expression) =>
			ExpressionHelper.GetMemberInfo(expression);

		/// <summary>
		/// Returns the <see cref="MemberInfo" />.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static MemberInfo Member<T, TValue>([NotNull] Expression<Func<T, TValue>> expression) =>
			ExpressionHelper.GetMemberInfo(expression);
		#endregion

		#region Property
		/// <summary>
		/// Returns the property.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="PropertyInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static PropertyInfo Property<TValue>([NotNull] Expression<Func<TValue>> expression) =>
			ExpressionHelper.GetProperty(expression);

		/// <summary>
		/// Returns the property.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="PropertyInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static PropertyInfo Property<T>([NotNull] Expression<Func<T, object>> expression) =>
			ExpressionHelper.GetProperty(expression);

		/// <summary>
		/// Returns the property.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="PropertyInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static PropertyInfo Property<T, TValue>([NotNull] Expression<Func<T, TValue>> expression) =>
			ExpressionHelper.GetProperty(expression);
		#endregion

		#region Field
		/// <summary>
		/// Returns the field.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="FieldInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static FieldInfo Field<TValue>([NotNull] Expression<Func<TValue>> expression) =>
			ExpressionHelper.GetField(expression);

		/// <summary>
		/// Returns the field.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="FieldInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static FieldInfo Field<T>([NotNull] Expression<Func<T, object>> expression) =>
			ExpressionHelper.GetField(expression);

		/// <summary>
		/// Returns the field.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="FieldInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static FieldInfo Field<T, TValue>([NotNull] Expression<Func<T, TValue>> expression) =>
			ExpressionHelper.GetField(expression);
		#endregion

		#region Constructor
		/// <summary>
		/// Returns the constructor.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="ConstructorInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static ConstructorInfo Constructor<T>([NotNull] Expression<Func<T>> expression) =>
			ExpressionHelper.GetConstructor(expression);
		#endregion

		#region Method
		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static MethodInfo Method<T>([NotNull] Expression<Func<T>> expression) =>
			ExpressionHelper.GetMethod(expression);

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static MethodInfo Method<T>([NotNull] Expression<Func<T, object>> expression) =>
			ExpressionHelper.GetMethod(expression);

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static MethodInfo Method<T, TResult>([NotNull] Expression<Func<T, TResult>> expression) =>
			ExpressionHelper.GetMethod(expression);

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static MethodInfo Method([NotNull] Expression<Action> expression) =>
			ExpressionHelper.GetMethod(expression);

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo" /> instance.
		/// </returns>
		[NotNull, Pure]
		public static MethodInfo Method<T>([NotNull] Expression<Action<T>> expression) =>
			ExpressionHelper.GetMethod(expression);
		#endregion
	}
}
