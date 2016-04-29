using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

namespace CodeJam.Expressions
{
	[TestFixture(Category = "Reflection")]
	public class ExprTests
	{
		[Test]
		public void TestExpressionsFunc()
		{
			var ex1 = Expr.Func((int a, int b) => a + b);
			Expression<Func<int, int, int>> ex2 = (a, b) => a + b;
			Expression<Func<int, int, int>> ex3 = (a, b) => a - b;

			Assert.AreEqual(ex1.ToString(), ex2.ToString());
			Assert.AreNotEqual(ex1.ToString(), ex3.ToString());
		}

		[Test]
		public void TestExpressionsComplexFunc()
		{
			var ex1 = Expr.Func((int a) => Enumerable.Repeat(a, a));
			Expression<Func<int, IEnumerable<int>>> ex2 = a => Enumerable.Repeat(a, a);
			Expression<Func<int, IEnumerable<int>>> ex3 = a => Enumerable.Empty<int>();

			Assert.AreEqual(ex1.ToString(), ex2.ToString());
			Assert.AreNotEqual(ex1.ToString(), ex3.ToString());
		}
	}
}
