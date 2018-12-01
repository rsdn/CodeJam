using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture]
	public class PartitionPointTest
	{
		[Test]
		public void Test01NegativeFrom()
		{
			var list = new List<int> { 0 };
			const int from = -1;
			Assert.That(() => list.PartitionPoint(from, list.Count, _ => _ > 0)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test02NegativeTo()
		{
			var list = new List<int> { 0 };
			const int to = -1;
			Assert.That(() => list.PartitionPoint(0, to, _ => _ > 0)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}


		[Test]
		public void Test03ToExceedsCount()
		{
			var list = new List<int> { 0 };
			var to = list.Count + 1;
			Assert.That(() => list.PartitionPoint(0, to, _ => _ > 0)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test04BadFromToOrder()
		{
			var list = new List<int> { 0, 1, 2 };
			const int from = 2;
			const int to = 0;
			Assert.That(() => list.PartitionPoint(from, to, _ => _ > 0), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		[TestCase(new int[0], 1, 0, 0, ExpectedResult = 0)]
		[TestCase(new[] { 1, 5, 12, 35, 123 }, 10, 0, 0, ExpectedResult = 0)]
		[TestCase(new[] { 1, 5, 12, 35, 123 }, 11, 2, 2, ExpectedResult = 2)]
		[TestCase(new[] { 1, 5, 12, 35, 123 }, 30, 5, 5, ExpectedResult = 5)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 15, 0, 3, ExpectedResult = 3)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 5, 1, 4, ExpectedResult = 1)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 30000, 0, 4, ExpectedResult = 4)]
		public int Test05WithAllParams(int[] data, int partitionValue, int from, int to)
		{
			var list = (IList<int>)data;
			return list.PartitionPoint(from, to, _ => _ < partitionValue);
		}

		[Test]
		[TestCase(new int[0], 4, 0, ExpectedResult = 0)]
		[TestCase(new[] { 1, 5, 12, 35, 123 }, 35, 5, ExpectedResult = 5)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 15, 0, ExpectedResult = 4)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 5, 0, ExpectedResult = 1)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, -1, 4, ExpectedResult = 4)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 42, 6, ExpectedResult = 6)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 1002, 3, ExpectedResult = 7)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 12, 3, ExpectedResult = 3)]
		public int Test06WithFrom(int[] data, int partitionValue, int from)
		{
			var list = (IList<int>)data;
			return list.PartitionPoint(from, _ => _ < partitionValue);
		}

		[Test]
		[TestCase(new int[0], 14, ExpectedResult = 0)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 50, ExpectedResult = 4)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 5, ExpectedResult = 1)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 30000, ExpectedResult = 8)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, -1, ExpectedResult = 0)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 42, ExpectedResult = 4)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 1002, ExpectedResult = 7)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 12, ExpectedResult = 2)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 3, ExpectedResult = 1)]
		[TestCase(new[] { 1, 5, 12, 12, 123, 512, 512, 14534 }, 14534, ExpectedResult = 7)]
		public int Test07WithoutParams(int[] data, int partitionValue)
		{
			var list = (IList<int>)data;
			return list.PartitionPoint(_ => _ < partitionValue);
		}
	}
}
