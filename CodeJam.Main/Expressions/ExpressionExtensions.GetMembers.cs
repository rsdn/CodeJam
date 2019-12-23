using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Expressions
{
	using Reflection;

	/// <summary>
	/// <see cref="Expression"/> Extensions.
	/// </summary>
	[PublicAPI]
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
		public static MemberExpression GetMemberExpression([NotNull] this LambdaExpression expression)
		{
			var body = expression.Body;
			return body is UnaryExpression unary
				? (MemberExpression)unary.Operand
				: (MemberExpression)body;
		}

		/// <summary>
		/// Gets the <see cref="MemberInfo"/>.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static MemberExpression GetMemberExpression(this Expression expression)
		{
			var body = expression is LambdaExpression lambda
				? lambda.Body
				: expression;

			return body is UnaryExpression unary
				? (MemberExpression)unary.Operand
				: (MemberExpression)body;
		}

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
			if (body is UnaryExpression unary)
				body = unary.Operand;

			return
				body.NodeType switch
				{
					ExpressionType.MemberAccess => ((MemberExpression)body).Member,
					ExpressionType.Call => ((MethodCallExpression)body).Method,
					_ => ((NewExpression)body).Constructor
				};
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
			var propertyInfo = info as PropertyInfo;
			return
				propertyInfo != null
					? propertyInfo.GetGetMethod(true)
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
		/// Returns a composed name of the property.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// A composed name of the property.
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

		[NotNull]
		private static string GetFullPropertyNameImpl([NotNull] MemberExpression expression)
		{
			var name = expression.Member.Name;
			while ((expression = expression.Expression as MemberExpression) != null)
				name = expression.Member.Name + "." + name;

			return name;
		}

		/// <summary>
		/// Gets the <see cref="MemberInfo"/>.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static MemberInfo[] GetMembersInfo([NotNull] this LambdaExpression expression)
		{
			Code.NotNull(expression, nameof(expression));

			var body = expression.Body;
			if (body is UnaryExpression unary)
				body = unary.Operand;

			return GetMembers(body).Reverse().ToArray();
		}

		[NotNull, ItemNotNull]
		private static IEnumerable<MemberInfo> GetMembers([NotNull] Expression expression, bool passIndexer = true)
		{
			MemberInfo lastMember = null;

			for (; ; )
			{
				switch (expression.NodeType)
				{
					case ExpressionType.Parameter:
						if (lastMember == null)
							goto default;
						yield break;

					case ExpressionType.Call:
						{
							if (lastMember == null)
								goto default;

							var cExpr = (MethodCallExpression)expression;
							var expr = cExpr.Object;

							if (expr == null)
							{
								if (cExpr.Arguments.Count == 0)
									goto default;

								expr = cExpr.Arguments[0];
							}

							if (expr.NodeType != ExpressionType.MemberAccess)
								goto default;

#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
							var member = ((MemberExpression)expr).Member;
							var memberType = member.GetMemberType();

							if (lastMember.ReflectedType != memberType.GetItemType())
								goto default;
#endif

							expression = expr;

							break;
						}

					case ExpressionType.MemberAccess:
						{
							var mExpr = (MemberExpression)expression;
							var member = lastMember = mExpr.Member;

							yield return member;

							expression = mExpr.Expression;

							break;
						}

					case ExpressionType.ArrayIndex:
						{
							if (passIndexer)
							{
								expression = ((BinaryExpression)expression).Left;
								break;
							}

							goto default;
						}

					default:
						throw new InvalidOperationException($"Expression '{expression}' is not an association.");
				}
			}
		}
	}
}