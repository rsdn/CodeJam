using System;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	using Expressions;

	/// <summary>
	/// Provides data convertion functionality.
	/// </summary>
	/// <typeparam name="TFrom">Type to convert from.</typeparam>
	/// <typeparam name="TTo">Type to convert to.</typeparam>
	[PublicAPI]
	public static class Convert<TFrom,TTo>
	{
		static Convert()
		{
			Init();
		}

		private static void Init()
		{
			var expr = ConvertBuilder.GetConverter(null, typeof(TFrom), typeof(TTo));

			_expression = (Expression<Func<TFrom,TTo>>)expr.Item1;

			var rexpr = (Expression<Func<TFrom,TTo>>)expr.Item1.Transform(e => e is DefaultValueExpression ? e.Reduce() : e);

			_lambda = rexpr.Compile();
		}

		private static Expression<Func<TFrom,TTo>> _expression;

		/// <summary>
		/// Represents an expression that converts a value of <i>TFrom</i> type to <i>TTo</i> type.
		/// </summary>
		public static Expression<Func<TFrom,TTo>> Expression
		{
			get { return _expression; }
			set
			{
				var setDefault = _expression != null;

				if (value == null)
				{
					Init();
				}
				else
				{
					_expression = value;
					_lambda = _expression.Compile();
				}

				if (setDefault)
					ConvertInfo.Default.Set(
						typeof(TFrom),
						typeof(TTo),
						new ConvertInfo.LambdaInfo(_expression, null, _lambda, false));
			}
		}

		private static Func<TFrom,TTo> _lambda;

		/// <summary>
		/// Represents a function that converts a value of <i>TFrom</i> type to <i>TTo</i> type.
		/// </summary>
		public static Func<TFrom,TTo> Lambda
		{
			get { return _lambda; }
			set
			{
				var setDefault = _expression != null;

				if (value == null)
				{
					Init();
				}
				else
				{
					var p = System.Linq.Expressions.Expression.Parameter(typeof(TFrom), "p");

					_lambda     = value;
					_expression =
						System.Linq.Expressions.Expression.Lambda<Func<TFrom,TTo>>(
							System.Linq.Expressions.Expression.Invoke(
								System.Linq.Expressions.Expression.Constant(value),
								p),
							p);
				}

				if (setDefault)
					ConvertInfo.Default.Set(
						typeof(TFrom),
						typeof(TTo),
						new ConvertInfo.LambdaInfo(_expression, null, _lambda, false));
			}
		}

		/// <summary>
		/// Returns a function that converts a value of <i>TFrom</i> type to <i>TTo</i> type.
		/// </summary>
		public static Func<TFrom,TTo> From => _lambda;
	}
}
