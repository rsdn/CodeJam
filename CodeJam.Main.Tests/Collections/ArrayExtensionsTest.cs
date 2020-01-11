using System;
using System.Collections.Generic;

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

		[Test]
		public void DeconstructArray()
		{
			var array = new[] { 1, 2, 3, 4, 5 };
			Assert.IsTrue(array is (1, _, _, _, _));
			Assert.IsFalse(array is (2, _, _, _, _));
			Assert.IsTrue(array is (_, _, 3, _, _));
			Assert.IsFalse(array is (_, _, 5, _, _));
			Assert.IsTrue(array is (1, _, 3, _, _));
			Assert.IsFalse(array is (1, _, 5, _, _));
			Assert.IsTrue(array is (1, 2, 3, 4, 5));
			Assert.IsFalse(array is (1, 2, 3, 6, 5));
		}

		[Test]
		public void DeconstructList()
		{
			IList<int> array = new[] { 1, 2, 3, 4, 5 };
			Assert.IsTrue(array is (1, _, _, _, _));
			Assert.IsFalse(array is (2, _, _, _, _));
			Assert.IsTrue(array is (_, _, 3, _, _));
			Assert.IsFalse(array is (_, _, 5, _, _));
			Assert.IsTrue(array is (1, _, 3, _, _));
			Assert.IsFalse(array is (1, _, 5, _, _));
			Assert.IsTrue(array is (1, 2, 3, 4, 5));
			Assert.IsFalse(array is (1, 2, 3, 6, 5));
		}

		[Test]
		public void DeconstructEnumerable()
		{
			IEnumerable<int> array = new[] { 1, 2, 3, 4, 5 };
			Assert.IsTrue(array is (1, _, _, _, _));
			Assert.IsFalse(array is (2, _, _, _, _));
			Assert.IsTrue(array is (_, _, 3, _, _));
			Assert.IsFalse(array is (_, _, 5, _, _));
			Assert.IsTrue(array is (1, _, 3, _, _));
			Assert.IsFalse(array is (1, _, 5, _, _));
			Assert.IsTrue(array is (1, 2, 3, 4, 5));
			Assert.IsFalse(array is (1, 2, 3, 6, 5));

			Assert.Throws<ArgumentException>(
				() =>
				{
					var b = array is (1, _, _, _, _, _);
				});
		}
	}
}