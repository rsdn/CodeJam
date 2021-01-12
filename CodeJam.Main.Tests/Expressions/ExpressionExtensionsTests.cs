#if NET40_OR_GREATER || TARGETS_NETCOREAPP // TODO: update after fixes in Theraot.Core
using System;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

namespace CodeJam.Expressions
{
	[TestFixture(Category = "Expressions")]
	public class ExpressionExtensionsTests
	{
		private static Expression Transform(Expression e)
		{
			if (e.NodeType == ExpressionType.Constant)
			{
				var ce = (ConstantExpression)e;
				if (ce.Value is int)
					return Expression.Constant((int)ce.Value * 2);
			}

			if (e.NodeType == ExpressionType.Multiply)
			{
				var be = (BinaryExpression)e;
				return Expression.Add(be.Left.Transform(Transform), be.Right.Transform(Transform));
			}

			return e;
		}

		private static Func<int, int> Transform(Expression<Func<int, int>> expr) =>
			expr.Transform(Transform).Compile();

		[Test]
		public void LambdaTest()
		{
			Assert.That(Transform(p => p * 3)(4),                          Is.EqualTo(10));
			Assert.That(Transform(p => 2 + Math.Max(p, 3) * 5)(4),         Is.EqualTo(20));
			Assert.That(Transform(p => (int)(2L + Math.Max(p, 3) * 5))(4), Is.EqualTo(18));
		}

		private class MultiSelectItem
		{
			public byte    ItemType;
			public int     ItemValue;
			public string? ItemText;
		}

		private static Func<T,MultiSelectItem> ToMultiSelectItem<T>(
			Expression<Func<T,byte>>   getItemType,
			Expression<Func<T,int>>    getItemValue,
			Expression<Func<T,string>> getItemText)
		{
			Expression<Func<T,MultiSelectItem>> expr = t => new MultiSelectItem
			{
				ItemType  = 1,
				ItemValue = 2,
				ItemText  = "$3$"
			};

			var ex1 = getItemType. ReplaceParameters(expr.Parameters[0]);
			var ex2 = getItemValue.ReplaceParameters(expr.Parameters[0]);
			var ex3 = getItemText. ReplaceParameters(expr.Parameters[0]);

			expr = expr.Transform(ex =>
			{
				var ce = ex as ConstantExpression;

				if (ce != null)
				{
					if (ce.Value is byte) return ex1;
					if (ce.Value is int)  return ex2;
					if (ce.Value!.ToString() == "$3$") return ex3;
				}

				return ex;
			});

			return expr.Compile();
		}

		[Test]
		public void PseudoQuasiQuotation()
		{
			var selector = ToMultiSelectItem<DateTime>(d => 4, d => d.Year, d => d.DayOfWeek.ToString());
			var item     = new[] { new DateTime(2016, 1, 1) }.Select(selector).Single();

			Assert.That(item.ItemType,  Is.EqualTo(4));
			Assert.That(item.ItemValue, Is.EqualTo(2016));
			Assert.That(item.ItemText,  Is.EqualTo("Friday"));
		}
	}
}
#endif