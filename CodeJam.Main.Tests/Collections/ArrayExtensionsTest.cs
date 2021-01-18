
using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public class ArrayExtensionsTest
	{
		[TestCase(new[] { 1,2,3,4 }, TestName = "Not Empty", ExpectedResult = new[] { 0,0,0,0 })]
		[TestCase(new int[0],        TestName = "Empty",     ExpectedResult = new int[0])]
		public int[] Clear(int[] input)
		{
			input.Clear();
			return input;
		}

		[Test]
		public void EqualsTo()
		{
			Assert.IsTrue(new      [] { 1, 2, 3, 4 }.            EqualsTo(new      [] { 1, 2, 3, 4 }));
			Assert.IsTrue(new      [] { 1, 2, 3, 4, 5, 6, 7, 8 }.EqualsTo(new      [] { 1, 2, 3, 4, 5, 6, 7, 8 }));
			Assert.IsTrue(new int? [] { 1, null, 3, 4 }.         EqualsTo(new int? [] { 1, null, 3, 4 }));
			Assert.IsTrue(new byte?[] { 1, null, 3, 4 }.         EqualsTo(new byte?[] { 1, null, 3, 4 }));
			Assert.IsTrue(new      [] { "1", "2", "3", "4" }.    EqualsTo(new      [] { "1", "2", "3", "4" }));
		}
	}
}