#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using CodeJam.Reflection;

using JetBrains.Annotations;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace CodeJam.Expressions
{
	static partial class ExpressionExtensions
	{
		#region Visit
		private static void VisitInternal<T>([NotNull] IEnumerable<T> source, [NotNull, InstantHandle] Action<T> func)
		{
			foreach (var item in source)
				func(item);
		}

		private static void VisitInternal<T>([NotNull] IEnumerable<T> source, [NotNull, InstantHandle] Action<Expression> func)
			where T : Expression
		{
			foreach (var item in source)
				VisitInternal(item, func);
		}

		/// <summary>
		/// Visits expression tree.
		/// </summary>
		/// <param name="expr"><see cref="Expression"/> to visit.</param>
		/// <param name="func">Visit action.</param>
		public static void Visit([CanBeNull] this Expression expr, [NotNull, InstantHandle] Action<Expression> func)
		{
			Code.NotNull(func, nameof(func));

			VisitInternal(expr, func);
		}

		private static void VisitInternal([CanBeNull] this Expression expr, [NotNull, InstantHandle] Action<Expression> func)
		{
			if (expr == null)
				return;

			switch (expr.NodeType)
			{
				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.ArrayIndex:
				case ExpressionType.Assign:
				case ExpressionType.Coalesce:
				case ExpressionType.Divide:
				case ExpressionType.Equal:
				case ExpressionType.ExclusiveOr:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LeftShift:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.Modulo:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.NotEqual:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.Power:
				case ExpressionType.RightShift:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
				case ExpressionType.AddAssign:
				case ExpressionType.AndAssign:
				case ExpressionType.DivideAssign:
				case ExpressionType.ExclusiveOrAssign:
				case ExpressionType.LeftShiftAssign:
				case ExpressionType.ModuloAssign:
				case ExpressionType.MultiplyAssign:
				case ExpressionType.OrAssign:
				case ExpressionType.PowerAssign:
				case ExpressionType.RightShiftAssign:
				case ExpressionType.SubtractAssign:
				case ExpressionType.AddAssignChecked:
				case ExpressionType.MultiplyAssignChecked:
				case ExpressionType.SubtractAssignChecked:
				{
					var e = (BinaryExpression)expr;

					VisitInternal(e.Conversion, func);
					VisitInternal(e.Left, func);
					VisitInternal(e.Right, func);

					break;
				}

				case ExpressionType.ArrayLength:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
				case ExpressionType.UnaryPlus:
				case ExpressionType.Decrement:
				case ExpressionType.Increment:
				case ExpressionType.IsFalse:
				case ExpressionType.IsTrue:
				case ExpressionType.Throw:
				case ExpressionType.Unbox:
				case ExpressionType.PreIncrementAssign:
				case ExpressionType.PreDecrementAssign:
				case ExpressionType.PostIncrementAssign:
				case ExpressionType.PostDecrementAssign:
				case ExpressionType.OnesComplement:
				{
					VisitInternal(((UnaryExpression)expr).Operand, func);
					break;
				}

				case ExpressionType.Call:
				{
					var e = (MethodCallExpression)expr;

					VisitInternal(e.Object, func);
					VisitInternal(e.Arguments, func);

					break;
				}

				case ExpressionType.Conditional:
				{
					var e = (ConditionalExpression)expr;

					VisitInternal(e.Test, func);
					VisitInternal(e.IfTrue, func);
					VisitInternal(e.IfFalse, func);

					break;
				}

				case ExpressionType.Invoke:
				{
					var e = (InvocationExpression)expr;

					VisitInternal(e.Expression, func);
					VisitInternal(e.Arguments, func);

					break;
				}

				case ExpressionType.Lambda:
				{
					var e = (LambdaExpression)expr;

					VisitInternal(e.Body, func);
					VisitInternal(e.Parameters, func);

					break;
				}

				case ExpressionType.ListInit:
				{
					var e = (ListInitExpression)expr;

					VisitInternal(e.NewExpression, func);
					VisitInternal(e.Initializers, ex => VisitInternal(ex.Arguments, func));

					break;
				}

				case ExpressionType.MemberAccess:
				{
					VisitInternal(((MemberExpression)expr).Expression, func);
					break;
				}

				case ExpressionType.MemberInit:
				{
					void Action(MemberBinding b)
					{
						switch (b.BindingType)
						{
							case MemberBindingType.Assignment:
								VisitInternal(((MemberAssignment)b).Expression, func);
								break;
							case MemberBindingType.ListBinding:
								VisitInternal(((MemberListBinding)b).Initializers, p => VisitInternal(p.Arguments, func));
								break;
							case MemberBindingType.MemberBinding:
								VisitInternal(((MemberMemberBinding)b).Bindings, Action);
								break;
						}
					}

					var e = (MemberInitExpression)expr;

					VisitInternal(e.NewExpression, func);
					VisitInternal(e.Bindings, Action);

					break;
				}

				case ExpressionType.New:
					VisitInternal(((NewExpression)expr).Arguments, func);
					break;
				case ExpressionType.NewArrayBounds:
					VisitInternal(((NewArrayExpression)expr).Expressions, func);
					break;
				case ExpressionType.NewArrayInit:
					VisitInternal(((NewArrayExpression)expr).Expressions, func);
					break;
				case ExpressionType.TypeEqual:
				case ExpressionType.TypeIs:
					VisitInternal(((TypeBinaryExpression)expr).Expression, func);
					break;

				case ExpressionType.Block:
				{
					var e = (BlockExpression)expr;

					VisitInternal(e.Expressions, func);
					VisitInternal(e.Variables, func);

					break;
				}

				case ExpressionType.Dynamic:
				{
					var e = (DynamicExpression)expr;

					VisitInternal(e.Arguments, func);

					break;
				}

				case ExpressionType.Goto:
				{
					var e = (GotoExpression)expr;

					VisitInternal(e.Value, func);

					break;
				}

				case ExpressionType.Index:
				{
					var e = (IndexExpression)expr;

					VisitInternal(e.Object, func);
					VisitInternal(e.Arguments, func);

					break;
				}

				case ExpressionType.Label:
				{
					var e = (LabelExpression)expr;

					VisitInternal(e.DefaultValue, func);

					break;
				}

				case ExpressionType.RuntimeVariables:
				{
					var e = (RuntimeVariablesExpression)expr;

					VisitInternal(e.Variables, func);

					break;
				}

				case ExpressionType.Loop:
				{
					var e = (LoopExpression)expr;

					VisitInternal(e.Body, func);

					break;
				}

				case ExpressionType.Switch:
				{
					var e = (SwitchExpression)expr;

					VisitInternal(e.SwitchValue, func);
					VisitInternal(
						e.Cases, cs =>
						{
							VisitInternal(cs.TestValues, func);
							VisitInternal(cs.Body, func);
						});
					VisitInternal(e.DefaultBody, func);

					break;
				}

				case ExpressionType.Try:
				{
					var e = (TryExpression)expr;

					VisitInternal(e.Body, func);
					VisitInternal(
						e.Handlers, h =>
						{
							VisitInternal(h.Variable, func);
							VisitInternal(h.Filter, func);
							VisitInternal(h.Body, func);
						});
					VisitInternal(e.Finally, func);
					VisitInternal(e.Fault, func);

					break;
				}

				case ExpressionType.Extension:
				{
					if (expr.CanReduce)
						VisitInternal(expr.Reduce(), func);
					break;
				}
			}

			func(expr);
		}

		private static void VisitInternal<T>([NotNull] IEnumerable<T> source, [NotNull, InstantHandle] Func<T, bool> func)
		{
			foreach (var item in source)
				func(item);
		}

		private static void VisitInternal<T>([NotNull] IEnumerable<T> source, [NotNull, InstantHandle] Func<Expression, bool> func)
			where T : Expression
		{
			foreach (var item in source)
				VisitInternal(item, func);
		}

		/// <summary>
		/// Visits expression tree.
		/// </summary>
		/// <param name="expr"><see cref="Expression"/> to visit.</param>
		/// <param name="func">Visit function. Return true to stop.</param>
		public static void Visit([CanBeNull] this Expression expr, [NotNull] Func<Expression, bool> func)
		{
			Code.NotNull(func, nameof(func));

			VisitInternal(expr, func);
		}

		[SuppressMessage("ReSharper", "TailRecursiveCall")]
		private static void VisitInternal([CanBeNull] this Expression expr, [NotNull, InstantHandle] Func<Expression, bool> func)
		{
			if (expr == null || !func(expr))
				return;

			switch (expr.NodeType)
			{
				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.ArrayIndex:
				case ExpressionType.Assign:
				case ExpressionType.Coalesce:
				case ExpressionType.Divide:
				case ExpressionType.Equal:
				case ExpressionType.ExclusiveOr:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LeftShift:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.Modulo:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.NotEqual:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.Power:
				case ExpressionType.RightShift:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
				case ExpressionType.AddAssign:
				case ExpressionType.AndAssign:
				case ExpressionType.DivideAssign:
				case ExpressionType.ExclusiveOrAssign:
				case ExpressionType.LeftShiftAssign:
				case ExpressionType.ModuloAssign:
				case ExpressionType.MultiplyAssign:
				case ExpressionType.OrAssign:
				case ExpressionType.PowerAssign:
				case ExpressionType.RightShiftAssign:
				case ExpressionType.SubtractAssign:
				case ExpressionType.AddAssignChecked:
				case ExpressionType.MultiplyAssignChecked:
				case ExpressionType.SubtractAssignChecked:
				{
					var e = (BinaryExpression)expr;

					VisitInternal(e.Conversion, func);
					VisitInternal(e.Left, func);
					VisitInternal(e.Right, func);

					break;
				}

				case ExpressionType.ArrayLength:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
				case ExpressionType.UnaryPlus:
				case ExpressionType.Decrement:
				case ExpressionType.Increment:
				case ExpressionType.IsFalse:
				case ExpressionType.IsTrue:
				case ExpressionType.Throw:
				case ExpressionType.Unbox:
				case ExpressionType.PreIncrementAssign:
				case ExpressionType.PreDecrementAssign:
				case ExpressionType.PostIncrementAssign:
				case ExpressionType.PostDecrementAssign:
				case ExpressionType.OnesComplement:
				{
					VisitInternal(((UnaryExpression)expr).Operand, func);
					break;
				}

				case ExpressionType.Call:
				{
					var e = (MethodCallExpression)expr;

					VisitInternal(e.Object, func);
					VisitInternal(e.Arguments, func);

					break;
				}

				case ExpressionType.Conditional:
				{
					var e = (ConditionalExpression)expr;

					VisitInternal(e.Test, func);
					VisitInternal(e.IfTrue, func);
					VisitInternal(e.IfFalse, func);

					break;
				}

				case ExpressionType.Invoke:
				{
					var e = (InvocationExpression)expr;

					VisitInternal(e.Expression, func);
					VisitInternal(e.Arguments, func);

					break;
				}

				case ExpressionType.Lambda:
				{
					var e = (LambdaExpression)expr;

					VisitInternal(e.Body, func);
					VisitInternal(e.Parameters, func);

					break;
				}

				case ExpressionType.ListInit:
				{
					var e = (ListInitExpression)expr;

					VisitInternal(e.NewExpression, func);
					VisitInternal(e.Initializers, ex => VisitInternal(ex.Arguments, func));

					break;
				}

				case ExpressionType.MemberAccess:
					VisitInternal(((MemberExpression)expr).Expression, func);
					break;

				case ExpressionType.MemberInit:
				{
					bool Modify(MemberBinding b)
					{
						switch (b.BindingType)
						{
							case MemberBindingType.Assignment:
								VisitInternal(((MemberAssignment)b).Expression, func);
								break;
							case MemberBindingType.ListBinding:
								VisitInternal(((MemberListBinding)b).Initializers, p => VisitInternal(p.Arguments, func));
								break;
							case MemberBindingType.MemberBinding:
								VisitInternal(((MemberMemberBinding)b).Bindings, Modify);
								break;
						}

						return true;
					}

					var e = (MemberInitExpression)expr;

					VisitInternal(e.NewExpression, func);
					VisitInternal(e.Bindings, Modify);

					break;
				}

				case ExpressionType.New:
					VisitInternal(((NewExpression)expr).Arguments, func);
					break;
				case ExpressionType.NewArrayBounds:
					VisitInternal(((NewArrayExpression)expr).Expressions, func);
					break;
				case ExpressionType.NewArrayInit:
					VisitInternal(((NewArrayExpression)expr).Expressions, func);
					break;
				case ExpressionType.TypeEqual:
				case ExpressionType.TypeIs:
					VisitInternal(((TypeBinaryExpression)expr).Expression, func);
					break;

				case ExpressionType.Block:
				{
					var e = (BlockExpression)expr;

					VisitInternal(e.Expressions, func);
					VisitInternal(e.Variables, func);

					break;
				}

				case ExpressionType.Dynamic:
				{
					var e = (DynamicExpression)expr;

					VisitInternal(e.Arguments, func);

					break;
				}

				case ExpressionType.Goto:
				{
					var e = (GotoExpression)expr;

					VisitInternal(e.Value, func);

					break;
				}

				case ExpressionType.Index:
				{
					var e = (IndexExpression)expr;

					VisitInternal(e.Object, func);
					VisitInternal(e.Arguments, func);

					break;
				}

				case ExpressionType.Label:
				{
					var e = (LabelExpression)expr;

					VisitInternal(e.DefaultValue, func);

					break;
				}

				case ExpressionType.RuntimeVariables:
				{
					var e = (RuntimeVariablesExpression)expr;

					VisitInternal(e.Variables, func);

					break;
				}

				case ExpressionType.Loop:
				{
					var e = (LoopExpression)expr;

					VisitInternal(e.Body, func);

					break;
				}

				case ExpressionType.Switch:
				{
					var e = (SwitchExpression)expr;

					VisitInternal(e.SwitchValue, func);
					VisitInternal(
						e.Cases, cs =>
						{
							VisitInternal(cs.TestValues, func);
							VisitInternal(cs.Body, func);
						});
					VisitInternal(e.DefaultBody, func);

					break;
				}

				case ExpressionType.Try:
				{
					var e = (TryExpression)expr;

					VisitInternal(e.Body, func);
					VisitInternal(
						e.Handlers, h =>
						{
							VisitInternal(h.Variable, func);
							VisitInternal(h.Filter, func);
							VisitInternal(h.Body, func);
						});
					VisitInternal(e.Finally, func);
					VisitInternal(e.Fault, func);

					break;
				}

				case ExpressionType.Extension:
				{
					if (expr.CanReduce)
						VisitInternal(expr.Reduce(), func);
					break;
				}
			}
		}
		#endregion

		#region Find
		[CanBeNull]
		private static Expression FindInternal<T>(IEnumerable<T> source, Func<T, Expression> func)
		{
			foreach (var item in source)
			{
				var ex = func(item);
				if (ex != null)
					return ex;
			}

			return null;
		}

		[CanBeNull]
		private static Expression FindInternal<T>(IEnumerable<T> source, Func<Expression, bool> func)
			where T : Expression
		{
			foreach (var item in source)
			{
				var f = FindInternal(item, func);
				if (f != null)
					return f;
			}

			return null;
		}

		/// <summary>
		/// Finds an expression in expression tree.
		/// </summary>
		/// <param name="expr"><see cref="Expression"/> to VisitInternal.</param>
		/// <param name="exprToFind">Expression to find.</param>
		/// <returns>Found expression or null.</returns>
		[Pure]
		public static Expression Find([CanBeNull] this Expression expr, [NotNull] Expression exprToFind)
		{
			Code.NotNull(exprToFind, nameof(exprToFind));

			return expr.FindInternal(e => e == exprToFind);
		}

		/// <summary>
		/// Finds and expression in expression tree.
		/// </summary>
		/// <param name="expr"><see cref="Expression"/> to VisitInternal.</param>
		/// <param name="func">Find function. Return true if expression is found.</param>
		/// <returns>Found expression or null.</returns>
		[Pure]
		public static Expression Find([CanBeNull] this Expression expr, [NotNull] Func<Expression, bool> func)
		{
			Code.NotNull(func, nameof(func));

			return FindInternal(expr, func);
		}

		[SuppressMessage("ReSharper", "TailRecursiveCall")]
		[ContractAnnotation("expr:null => null")]
		[CanBeNull]
		private static Expression FindInternal(this Expression expr, Func<Expression, bool> func)
		{
			if (expr == null || func(expr))
				return expr;

			switch (expr.NodeType)
			{
				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.ArrayIndex:
				case ExpressionType.Assign:
				case ExpressionType.Coalesce:
				case ExpressionType.Divide:
				case ExpressionType.Equal:
				case ExpressionType.ExclusiveOr:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LeftShift:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.Modulo:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.NotEqual:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.Power:
				case ExpressionType.RightShift:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
				case ExpressionType.AddAssign:
				case ExpressionType.AndAssign:
				case ExpressionType.DivideAssign:
				case ExpressionType.ExclusiveOrAssign:
				case ExpressionType.LeftShiftAssign:
				case ExpressionType.ModuloAssign:
				case ExpressionType.MultiplyAssign:
				case ExpressionType.OrAssign:
				case ExpressionType.PowerAssign:
				case ExpressionType.RightShiftAssign:
				case ExpressionType.SubtractAssign:
				case ExpressionType.AddAssignChecked:
				case ExpressionType.MultiplyAssignChecked:
				case ExpressionType.SubtractAssignChecked:
				{
					var e = (BinaryExpression)expr;

					return
						FindInternal(e.Conversion, func) ??
							FindInternal(e.Left, func) ??
								FindInternal(e.Right, func);
				}

				case ExpressionType.ArrayLength:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
				case ExpressionType.UnaryPlus:
				case ExpressionType.Decrement:
				case ExpressionType.Increment:
				case ExpressionType.IsFalse:
				case ExpressionType.IsTrue:
				case ExpressionType.Throw:
				case ExpressionType.Unbox:
				case ExpressionType.PreIncrementAssign:
				case ExpressionType.PreDecrementAssign:
				case ExpressionType.PostIncrementAssign:
				case ExpressionType.PostDecrementAssign:
				case ExpressionType.OnesComplement:
					return FindInternal(((UnaryExpression)expr).Operand, func);

				case ExpressionType.Call:
				{
					var e = (MethodCallExpression)expr;

					return
						FindInternal(e.Object, func) ??
							FindInternal(e.Arguments, func);
				}

				case ExpressionType.Conditional:
				{
					var e = (ConditionalExpression)expr;

					return
						FindInternal(e.Test, func) ??
							FindInternal(e.IfTrue, func) ??
								FindInternal(e.IfFalse, func);
				}

				case ExpressionType.Invoke:
				{
					var e = (InvocationExpression)expr;

					return
						FindInternal(e.Expression, func) ??
							FindInternal(e.Arguments, func);
				}

				case ExpressionType.Lambda:
				{
					var e = (LambdaExpression)expr;

					return
						FindInternal(e.Body, func) ??
							FindInternal(e.Parameters, func);
				}

				case ExpressionType.ListInit:
				{
					var e = (ListInitExpression)expr;

					return
						FindInternal(e.NewExpression, func) ??
							FindInternal(e.Initializers, ex => FindInternal(ex.Arguments, func));
				}

				case ExpressionType.MemberAccess:
					return FindInternal(((MemberExpression)expr).Expression, func);

				case ExpressionType.MemberInit:
				{
					Expression Func(MemberBinding b) =>
						b.BindingType switch
						{
							MemberBindingType.Assignment => FindInternal(((MemberAssignment)b).Expression, func),
							MemberBindingType.ListBinding => FindInternal(
								((MemberListBinding)b).Initializers,
								p => FindInternal(p.Arguments, func)),
							MemberBindingType.MemberBinding => FindInternal(((MemberMemberBinding)b).Bindings, Func),
							_ => null
						};

					var e = (MemberInitExpression)expr;

					return FindInternal(e.NewExpression, func) ?? FindInternal(e.Bindings, Func);
				}

				case ExpressionType.New:
					return FindInternal(((NewExpression)expr).Arguments, func);
				case ExpressionType.NewArrayBounds:
					return FindInternal(((NewArrayExpression)expr).Expressions, func);
				case ExpressionType.NewArrayInit:
					return FindInternal(((NewArrayExpression)expr).Expressions, func);
				case ExpressionType.TypeEqual:
				case ExpressionType.TypeIs:
					return FindInternal(((TypeBinaryExpression)expr).Expression, func);

				case ExpressionType.Block:
				{
					var e = (BlockExpression)expr;

					return
						FindInternal(e.Expressions, func) ??
							FindInternal(e.Variables, func);
				}

				case ExpressionType.Dynamic:
				{
					var e = (DynamicExpression)expr;

					return
						FindInternal(e.Arguments, func);
				}

				case ExpressionType.Goto:
				{
					var e = (GotoExpression)expr;

					return
						FindInternal(e.Value, func);
				}

				case ExpressionType.Index:
				{
					var e = (IndexExpression)expr;

					return
						FindInternal(e.Object, func) ??
							FindInternal(e.Arguments, func);
				}

				case ExpressionType.Label:
				{
					var e = (LabelExpression)expr;

					return
						FindInternal(e.DefaultValue, func);
				}

				case ExpressionType.RuntimeVariables:
				{
					var e = (RuntimeVariablesExpression)expr;

					return
						FindInternal(e.Variables, func);
				}

				case ExpressionType.Loop:
				{
					var e = (LoopExpression)expr;

					return
						FindInternal(e.Body, func);
				}

				case ExpressionType.Switch:
				{
					var e = (SwitchExpression)expr;

					return
						FindInternal(e.SwitchValue, func) ??
							FindInternal(e.Cases, cs => FindInternal(cs.TestValues, func) ?? FindInternal(cs.Body, func)) ??
								FindInternal(e.DefaultBody, func);
				}

				case ExpressionType.Try:
				{
					var e = (TryExpression)expr;

					return
						FindInternal(e.Body, func) ??
							FindInternal(
								e.Handlers, h => FindInternal(h.Variable, func) ?? FindInternal(h.Filter, func) ?? FindInternal(h.Body, func))
								??
								FindInternal(e.Finally, func) ??
									FindInternal(e.Fault, func);
				}

				case ExpressionType.Extension:
					if (expr.CanReduce)
						return FindInternal(expr.Reduce(), func);
					break;
			}

			return null;
		}
		#endregion

		#region Transform
		/// <summary>
		/// Replaces lambda body parameter and returns modified body.
		/// </summary>
		/// <param name="lambda">Original lambda.</param>
		/// <param name="exprToReplaceParameter">An expression to replace lambda parameter.</param>
		/// <returns>Modified body.</returns>
		[Pure][NotNull]
		public static Expression ReplaceParameters(
			[NotNull] this LambdaExpression lambda, [NotNull] Expression exprToReplaceParameter)
		{
			Code.NotNull(lambda, nameof(lambda));
			Code.NotNull(exprToReplaceParameter, nameof(exprToReplaceParameter));

			if (lambda.Parameters.Count == 0)
				throw new ArgumentException("Provided lambda has to have at least one parameter.");

			return TransformInternal(lambda.Body, e => e == lambda.Parameters[0] ? exprToReplaceParameter : e);
		}

		/// <summary>
		/// Replaces lambda body parameters and returns modified body.
		/// </summary>
		/// <param name="lambda">Original lambda.</param>
		/// <param name="exprToReplaceParameter">Expressions to replace lambda parameters.</param>
		/// <returns>Modified body.</returns>
		[Pure]
		[NotNull]
		public static Expression ReplaceParameters(
			[NotNull] this LambdaExpression lambda, [NotNull] params Expression[] exprToReplaceParameter)
		{
			Code.NotNull(lambda, nameof(lambda));
			Code.NotNull(exprToReplaceParameter, nameof(exprToReplaceParameter));

			return TransformInternal(
				lambda.Body, e =>
				{
					for (var i = 0; i < lambda.Parameters.Count && i < exprToReplaceParameter.Length; i++)
					{
						if (e == lambda.Parameters[i])
							return exprToReplaceParameter[i];
					}

					return e;
				});
		}

		[NotNull]
		private static IEnumerable<T> TransformInternal<T>([NotNull] ICollection<T> source, [NotNull, InstantHandle] Func<T, T> func)
			where T : class
		{
			var modified = false;
			var list = new List<T>();

			foreach (var item in source)
			{
				var e = func(item);
				list.Add(e);
				modified = modified || e != item;
			}

			return modified ? list : source;
		}

		[NotNull]
		private static IEnumerable<T> TransformInternal<T>([NotNull] ICollection<T> source, [NotNull, InstantHandle] Func<Expression, Expression> func)
			where T : Expression
		{
			var modified = false;
			var list = new List<T>();

			foreach (var item in source)
			{
				var e = TransformInternal(item, func);
				list.Add((T)e);
				modified = modified || e != item;
			}

			return modified ? list : source;
		}

		/// <summary>
		/// Transforms original expression.
		/// </summary>
		/// <typeparam name="T">Type of expression.</typeparam>
		/// <param name="expr">Expression to transform.</param>
		/// <param name="func">Transform function.</param>
		/// <returns>Modified expression.</returns>
		[Pure, CanBeNull]
		[ContractAnnotation("expr: null => null; expr: notnull => notnull")]
		public static T Transform<T>([CanBeNull] this T expr, [NotNull] Func<Expression, Expression> func)
			where T : LambdaExpression
		{
			Code.NotNull(func, nameof(func));

			return (T)TransformInternal(expr, func);
		}

		/// <summary>
		/// Transforms original expression.
		/// </summary>
		/// <param name="expr">Expression to transform.</param>
		/// <param name="func">Transform function.</param>
		/// <returns>Modified expression.</returns>
		[Pure, CanBeNull]
		[ContractAnnotation("expr: null => null; expr: notnull => notnull")]
		public static Expression Transform([CanBeNull] this Expression expr, [NotNull, InstantHandle] Func<Expression, Expression> func)
		{
			Code.NotNull(func, nameof(func));

			return TransformInternal(expr, func);
		}

		[CanBeNull]
		[ContractAnnotation("expr: null => null; expr: notnull => notnull")]
		private static Expression TransformInternal([CanBeNull] this Expression expr, [NotNull, InstantHandle] Func<Expression, Expression> func)
		{
			if (expr == null)
				return null;

			{
				var ex = func(expr);
				if (ex != expr)
					return ex;
			}

			switch (expr.NodeType)
			{
				case ExpressionType.Add:
				case ExpressionType.AddChecked:
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				case ExpressionType.ArrayIndex:
				case ExpressionType.Assign:
				case ExpressionType.Coalesce:
				case ExpressionType.Divide:
				case ExpressionType.Equal:
				case ExpressionType.ExclusiveOr:
				case ExpressionType.GreaterThan:
				case ExpressionType.GreaterThanOrEqual:
				case ExpressionType.LeftShift:
				case ExpressionType.LessThan:
				case ExpressionType.LessThanOrEqual:
				case ExpressionType.Modulo:
				case ExpressionType.Multiply:
				case ExpressionType.MultiplyChecked:
				case ExpressionType.NotEqual:
				case ExpressionType.Or:
				case ExpressionType.OrElse:
				case ExpressionType.Power:
				case ExpressionType.RightShift:
				case ExpressionType.Subtract:
				case ExpressionType.SubtractChecked:
				case ExpressionType.AddAssign:
				case ExpressionType.AndAssign:
				case ExpressionType.DivideAssign:
				case ExpressionType.ExclusiveOrAssign:
				case ExpressionType.LeftShiftAssign:
				case ExpressionType.ModuloAssign:
				case ExpressionType.MultiplyAssign:
				case ExpressionType.OrAssign:
				case ExpressionType.PowerAssign:
				case ExpressionType.RightShiftAssign:
				case ExpressionType.SubtractAssign:
				case ExpressionType.AddAssignChecked:
				case ExpressionType.MultiplyAssignChecked:
				case ExpressionType.SubtractAssignChecked:
				{
					var e = (BinaryExpression)expr;
					return e.Update(
						TransformInternal(e.Left, func),
						(LambdaExpression)TransformInternal(e.Conversion, func),
						TransformInternal(e.Right, func));
				}

				case ExpressionType.ArrayLength:
				case ExpressionType.Convert:
				case ExpressionType.ConvertChecked:
				case ExpressionType.Negate:
				case ExpressionType.NegateChecked:
				case ExpressionType.Not:
				case ExpressionType.Quote:
				case ExpressionType.TypeAs:
				case ExpressionType.UnaryPlus:
				case ExpressionType.Decrement:
				case ExpressionType.Increment:
				case ExpressionType.IsFalse:
				case ExpressionType.IsTrue:
				case ExpressionType.Throw:
				case ExpressionType.Unbox:
				case ExpressionType.PreIncrementAssign:
				case ExpressionType.PreDecrementAssign:
				case ExpressionType.PostIncrementAssign:
				case ExpressionType.PostDecrementAssign:
				case ExpressionType.OnesComplement:
				{
					var e = (UnaryExpression)expr;
					return e.Update(TransformInternal(e.Operand, func));
				}

				case ExpressionType.Call:
				{
					var e = (MethodCallExpression)expr;
					return e.Update(
						TransformInternal(e.Object, func),
						TransformInternal(e.Arguments, func));
				}

				case ExpressionType.Conditional:
				{
					var e = (ConditionalExpression)expr;
					return e.Update(
						TransformInternal(e.Test, func),
						TransformInternal(e.IfTrue, func),
						TransformInternal(e.IfFalse, func));
				}

				case ExpressionType.Invoke:
				{
					var e = (InvocationExpression)expr;
					return e.Update(
						TransformInternal(e.Expression, func),
						TransformInternal(e.Arguments, func));
				}

				case ExpressionType.Lambda:
				{
					var e = (LambdaExpression)expr;
					var b = TransformInternal(e.Body, func);
					var p = TransformInternal(e.Parameters, func);

					return b != e.Body || !ReferenceEquals(p, e.Parameters) ? Expression.Lambda(expr.Type, b, p.ToArray()) : expr;
				}

				case ExpressionType.ListInit:
				{
					var e = (ListInitExpression)expr;
					return e.Update(
						(NewExpression)TransformInternal(e.NewExpression, func),
						TransformInternal(
							e.Initializers, p =>
							{
								var args = TransformInternal(p.Arguments, func);
								return !ReferenceEquals(args, p.Arguments) ? Expression.ElementInit(p.AddMethod, args) : p;
							}));
				}

				case ExpressionType.MemberAccess:
				{
					var e = (MemberExpression)expr;
					DebugCode.BugIf(e.Expression == null, "e.Expression == null");
					return e.Update(TransformInternal(e.Expression, func));
				}

				case ExpressionType.MemberInit:
				{
					MemberBinding Modify(MemberBinding b)
					{
						switch (b.BindingType)
						{
							case MemberBindingType.Assignment:
							{
								var ma = (MemberAssignment)b;
								return ma.Update(TransformInternal(ma.Expression, func));
							}

							case MemberBindingType.ListBinding:
							{
								var ml = (MemberListBinding)b;
								return ml.Update(TransformInternal(ml.Initializers, p =>
								{
									var args = TransformInternal(p.Arguments, func);
									return !ReferenceEquals(args, p.Arguments) ? Expression.ElementInit(p.AddMethod, args) : p;
								}));
							}

							case MemberBindingType.MemberBinding:
							{
								var mm = (MemberMemberBinding)b;
								return mm.Update(TransformInternal(mm.Bindings, Modify));
							}
						}

						return b;
					}

					var e = (MemberInitExpression)expr;
					return e.Update(
						(NewExpression)TransformInternal(e.NewExpression, func),
						TransformInternal(e.Bindings, Modify));
				}

				case ExpressionType.New:
				{
					var e = (NewExpression)expr;
					return e.Update(TransformInternal(e.Arguments, func));
				}

				case ExpressionType.NewArrayBounds:
				case ExpressionType.NewArrayInit:
				{
					var e = (NewArrayExpression)expr;
					return e.Update(TransformInternal(e.Expressions, func));
				}

				case ExpressionType.TypeEqual:
				case ExpressionType.TypeIs:
				{
					var e = (TypeBinaryExpression)expr;
					return e.Update(TransformInternal(e.Expression, func));
				}

				case ExpressionType.Block:
				{
					var e = (BlockExpression)expr;
					return e.Update(
						TransformInternal(e.Variables, func),
						TransformInternal(e.Expressions, func));
				}

				case ExpressionType.DebugInfo:
				case ExpressionType.Default:
				case ExpressionType.Extension:
				case ExpressionType.Constant:
				case ExpressionType.Parameter:
					return expr;

				case ExpressionType.Dynamic:
				{
					var e = (DynamicExpression)expr;
					return e.Update(TransformInternal(e.Arguments, func));
				}

				case ExpressionType.Goto:
				{
					var e = (GotoExpression)expr;
					return e.Update(e.Target, TransformInternal(e.Value, func));
				}

				case ExpressionType.Index:
				{
					var e = (IndexExpression)expr;
					DebugCode.BugIf(e.Object == null, "e.Object == null");
					return e.Update(
						TransformInternal(e.Object, func),
						TransformInternal(e.Arguments, func));
				}

				case ExpressionType.Label:
				{
					var e = (LabelExpression)expr;
					return e.Update(e.Target, TransformInternal(e.DefaultValue, func));
				}

				case ExpressionType.RuntimeVariables:
				{
					var e = (RuntimeVariablesExpression)expr;
					return e.Update(TransformInternal(e.Variables, func));
				}

				case ExpressionType.Loop:
				{
					var e = (LoopExpression)expr;
					return e.Update(e.BreakLabel, e.ContinueLabel, TransformInternal(e.Body, func));
				}

				case ExpressionType.Switch:
				{
					var e = (SwitchExpression)expr;
					return e.Update(
						TransformInternal(e.SwitchValue, func),
						TransformInternal(
							e.Cases, cs => cs.Update(TransformInternal(cs.TestValues, func), TransformInternal(cs.Body, func))),
						TransformInternal(e.DefaultBody, func));
				}

				case ExpressionType.Try:
				{
					var e = (TryExpression)expr;
					return e.Update(
						TransformInternal(e.Body, func),
						TransformInternal(
							e.Handlers,
							h =>
								h.Update(
									(ParameterExpression)TransformInternal(h.Variable, func), TransformInternal(h.Filter, func),
									TransformInternal(h.Body, func))),
						TransformInternal(e.Finally, func),
						TransformInternal(e.Fault, func));
				}
			}

			throw new InvalidOperationException();
		}
		#endregion

		[CanBeNull]
		private static Func<Expression, string> _getDebugView;

		/// <summary>
		/// Gets the DebugView internal property value of provided expression.
		/// </summary>
		/// <param name="expression">Expression to get DebugView.</param>
		/// <returns>DebugView value.</returns>
		[NotNull]
		public static string GetDebugView([NotNull] this Expression expression)
		{
			Code.NotNull(expression, nameof(expression));

			if (_getDebugView == null)
			{
				var p = Expression.Parameter(typeof(Expression));

				try
				{
					var l = Expression.Lambda<Func<Expression, string>>(
						Expression.PropertyOrField(p, "DebugView"),
						p);

					_getDebugView = l.Compile();
				}
				catch (ArgumentException)
				{
					var l = Expression.Lambda<Func<Expression, string>>(
						Expression.Call(p, InfoOf<Expression>.Method(e => e.ToString())),
						p);

					_getDebugView = l.Compile();
				}
			}

			return _getDebugView(expression);
		}
	}
}

#endif