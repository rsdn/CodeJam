using System;
using System.IO;
using System.Linq.Expressions;

using NUnit.Framework;

namespace CodeJam.Reflection
{
	[TestFixture(Category = "Reflection")]
	public class ExpressionHelperTests
	{
		[Test]
		public void TestExpressionsFunc()
		{
			var ex1 = ExpressionHelper.Func((int a, int b) => a + b);
			Expression<Func<int, int, int>> ex2 = (a, b) => a + b;
			Expression<Func<int, int, int>> ex3 = (a, b) => a - b;

			Assert.AreEqual(ex1.ToString(), ex2.ToString());
			Assert.AreNotEqual(ex1.ToString(), ex3.ToString());
		}
	}
}
