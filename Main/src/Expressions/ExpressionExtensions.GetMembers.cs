using System;
using System.Linq.Expressions;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Expressions
{
	public static partial class ExpressionExtensions
	{
		/// <summary>
		/// Gets the <see cref="MemberInfo"/>.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static MemberInfo GetMemberInfo([NotNull] this LambdaExpression expression)
		{
			Code.NotNull(expression, nameof(expression));

			var body = expression.Body;
			var unary = body as UnaryExpression;
			if (unary != null)
				body = unary.Operand;

			switch (body.NodeType)
			{
				case ExpressionType.MemberAccess:
					return ((MemberExpression)body).Member;
				case ExpressionType.Call:
					return ((MethodCallExpression)body).Method;
				default:
					return ((NewExpression)body).Constructor;
			}
		}

		/// <summary>
		/// Returns the property.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="PropertyInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static PropertyInfo GetProperty([NotNull] this LambdaExpression expression) =>
			(PropertyInfo)GetMemberExpression(expression).Member;

		/// <summary>
		/// Returns the field.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="FieldInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static FieldInfo GetField([NotNull] this LambdaExpression expression) =>
			(FieldInfo)GetMemberExpression(expression).Member;

		/// <summary>
		/// Returns the constructor.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="ConstructorInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static ConstructorInfo GetConstructor([NotNull] this LambdaExpression expression) =>
			(ConstructorInfo)GetMemberInfo(expression);

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static MethodInfo GetMethod([NotNull] this LambdaExpression expression)
		{
			var info = GetMemberInfo(expression);
			return
				info is PropertyInfo
					? ((PropertyInfo)info).GetGetMethod(true)
					: (MethodInfo)info;
		}

		/// <summary>
		/// Returns a name of the property.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// A name of the property.
		/// </returns>
		[NotNull, Pure]
		public static string GetPropertyName([NotNull] this LambdaExpression expression) =>
			GetMemberExpression(expression).Member.Name;

		/// <summary>
		/// Returns a composited name of the property.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// A composited name of the property.
		/// </returns>
		[NotNull, Pure]
		public static string GetFullPropertyName([NotNull] this LambdaExpression expression) =>
			GetFullPropertyNameImpl(GetMemberExpression(expression));

		/// <summary>
		/// Returns a name of the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// A name of the method.
		/// </returns>
		[NotNull, Pure]
		public static string GetMethodName([NotNull] this LambdaExpression expression) =>
			GetMethod(expression).Name;

		private static string GetFullPropertyNameImpl(MemberExpression expression)
		{
			var name = expression.Member.Name;
			while ((expression = expression.Expression as MemberExpression) != null)
				name = expression.Member.Name + "." + name;

			return name;
		}

		private static MemberExpression GetMemberExpression(LambdaExpression expression)
		{
			var body = expression.Body;
			var unary = body as UnaryExpression;
			return unary != null
				? (MemberExpression)unary.Operand
				: (MemberExpression)body;
		}
	}
}