using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if TARGETS_NET || NETSTANDARD20_OR_GREATER || NETCOREAPP20_OR_GREATER
using CodeJam.Reflection;
#endif
using JetBrains.Annotations;

namespace CodeJam.Expressions
{
	/// <summary>
	/// <see cref="Expression"/> Extensions.
	/// </summary>
	[PublicAPI]
	public static
#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
		partial
#endif
		class ExpressionExtensions
	{
		/// <summary>
		/// Gets the <see cref="MemberInfo"/>.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MemberInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MemberExpression GetMemberExpression(this LambdaExpression expression)
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
		[Pure, System.Diagnostics.Contracts.Pure]
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
		/// The <see cref="MemberInfo"/> instance. For value types, the method returns null for the default constructor.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MemberInfo? GetMemberInfo(this LambdaExpression expression)
		{
			Code.NotNull(expression, nameof(expression));

			var body = expression.Body;
			if (body is UnaryExpression unary)
				body = unary.Operand;

			return
				body switch
				{
					MemberExpression {Member:{} member} => member,
					MethodCallExpression {Method:{} method} => method,
					NewExpression ne => ne.Constructor,
					_ => throw CodeExceptions.Argument(
						nameof(expression),
						$"Unsupported expression type '{expression.NodeType}' or expression is invalid.")
					};
		}

		/// <summary>
		/// Returns the property.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="PropertyInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static PropertyInfo GetProperty(this LambdaExpression expression) =>
			(PropertyInfo)GetMemberExpression(expression).Member;

		/// <summary>
		/// Returns the field.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="FieldInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static FieldInfo GetField(this LambdaExpression expression) =>
			(FieldInfo)GetMemberExpression(expression).Member;

		/// <summary>
		/// Returns the constructor.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="ConstructorInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static ConstructorInfo? GetConstructor(this LambdaExpression expression)
		{
			var memberInfo = GetMemberInfo(expression);
			if (memberInfo == null)
				return null;
			return memberInfo is ConstructorInfo ctor
				? ctor
				: throw CodeExceptions.Argument(nameof(expression), "Expression is not constructor call.");
		}

		/// <summary>
		/// Returns the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// The <see cref="MethodInfo"/> instance.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MethodInfo GetMethod(this LambdaExpression expression) =>
			GetMemberInfo(expression) switch
			{
				PropertyInfo pi => pi.GetGetMethod(true)
					?? throw CodeExceptions.Argument(nameof(expression), "Property has no get accessor."),
				MethodInfo mi => mi,
				_ => throw CodeExceptions.Argument(nameof(expression), "Expression is invalid.")
				};

		/// <summary>
		/// Returns a name of the property.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// A name of the property.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string GetPropertyName(this LambdaExpression expression) =>
			GetMemberExpression(expression).Member.Name;

		/// <summary>
		/// Returns a composed name of the property.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// A composed name of the property.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string GetFullPropertyName(this LambdaExpression expression) =>
			GetFullPropertyNameImpl(GetMemberExpression(expression));

		/// <summary>
		/// Returns a name of the method.
		/// </summary>
		/// <param name="expression">The expression to analyze.</param>
		/// <returns>
		/// A name of the method.
		/// </returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static string GetMethodName(this LambdaExpression expression) =>
			GetMethod(expression).Name;

		private static string GetFullPropertyNameImpl(MemberExpression expression)
		{
			var name = expression.Member.Name;

#pragma warning disable IDE0019 // Use pattern matching
			var curExpr = expression;
#pragma warning restore IDE0019 // Use pattern matching

			while ((curExpr = curExpr.Expression as MemberExpression) != null)
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
		[Pure, System.Diagnostics.Contracts.Pure]
		public static MemberInfo[] GetMembersInfo(this LambdaExpression expression)
		{
			Code.NotNull(expression, nameof(expression));

			var body = expression.Body;
			if (body is UnaryExpression unary)
				body = unary.Operand;

			return GetMembers(body).Reverse().ToArray();
		}

		private static IEnumerable<MemberInfo> GetMembers(Expression expression, bool passIndexer = true)
		{
			Code.NotNull(expression, nameof(expression));

			MemberInfo? lastMember = null;

			for (;;)
			{
				switch (expression)
				{
					case {NodeType: ExpressionType.Parameter }:
						if (lastMember == null)
							goto default;
						yield break;

					case MethodCallExpression mce:
					{
						if (lastMember == null)
							goto default;

						var expr = mce.Object;

						if (expr == null)
						{
							if (mce.Arguments.Count == 0)
								goto default;

							expr = mce.Arguments[0];
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

					case MemberExpression me:
					{
						var member = lastMember = me.Member;

						yield return member;

						expression = me.Expression
							?? throw CodeExceptions.Argument(
								nameof(expression),
								"Invalid expression, MemberExpression.Member is null.");

						break;
					}

					case BinaryExpression{NodeType: ExpressionType.ArrayIndex } be:
					{
						if (passIndexer)
						{
							expression = be.Left;
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