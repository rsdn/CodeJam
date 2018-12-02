#if LESSTHAN_NET35
extern alias nunitlinq;
#endif

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace CodeJam
{
	// ReSharper disable ImplicitlyCapturedClosure
	[TestFixture]
	public class UpperBoundTest
	{
		/// <summary>Shows how to use UpperBound to search for a value greater than the given value</summary>
		[Test]
		public void Test01UpperBoundSearchSample()
		{
			// The list should be sorted!
			var sortedData = new List<string> { "B", "C", "C", "D", "F" };

			var indexOfUpperBound = sortedData.UpperBound("A");
			Assert.That(indexOfUpperBound, Is.EqualTo(sortedData.IndexOf("B")));

			indexOfUpperBound = sortedData.UpperBound("B");
			Assert.That(indexOfUpperBound, Is.EqualTo(sortedData.IndexOf("C")));

			indexOfUpperBound = sortedData.UpperBound("C");
			Assert.That(indexOfUpperBound, Is.EqualTo(sortedData.IndexOf("D")));

			indexOfUpperBound = sortedData.UpperBound("E");
			Assert.That(indexOfUpperBound, Is.EqualTo(sortedData.IndexOf("F")));

			indexOfUpperBound = sortedData.UpperBound("G");
			// No greater value, so the position after the last element should be returned
			Assert.That(indexOfUpperBound, Is.EqualTo(sortedData.Count));
		}

		/// <summary>Shows how to iterate over a range of values in the sorted list</summary>
		[Test]
		public void Test02RangeIterationsSample()
		{
			// Histogram of a value over a discrete range
			var histogram = new List<KeyValuePair<int, int>>
			{
				new KeyValuePair<int, int>(7, 24),
				new KeyValuePair<int, int>(1, 10),
				new KeyValuePair<int, int>(6, 34),
				new KeyValuePair<int, int>(4, 25),
				new KeyValuePair<int, int>(5, 18),
				new KeyValuePair<int, int>(3, 8),
				new KeyValuePair<int, int>(8, 7),
				new KeyValuePair<int, int>(2, 12),
				new KeyValuePair<int, int>(9, 4)
			};

			// Calculate the sum of all values in the range [5, 8]
			var summationRange = ValueTuple.Create(5, 8);
			var expected = histogram.Where(_ => _.Key >= summationRange.Item1 && _.Key <= summationRange.Item2).Sum(_ => _.Value);

			Func<KeyValuePair<int, int>, KeyValuePair<int, int>, int> comparer = (x, y) => Comparer<int>.Default.Compare(x.Key, y.Key);
			Func<KeyValuePair<int, int>, int, int> keyComparer = (x, y) => Comparer<int>.Default.Compare(x.Key, y);

			// First, sort it!
			histogram.Sort((x,y) => comparer(x, y));

			// Find the index range
			var indexFrom = histogram.LowerBound(summationRange.Item1, 0, histogram.Count, keyComparer);
			var indexTo = histogram.UpperBound(summationRange.Item2, indexFrom, histogram.Count, keyComparer);

			var sum = 0;
			for (var index = indexFrom; index < indexTo; ++index)
			{
				sum += histogram[index].Value;
			}
			Assert.That(sum, Is.EqualTo(expected));
		}

		[Test]
		public void Test03NegativeFrom()
		{
			var list = new List<double> { 0 };
			const int from = -1;
			// comparer version
			Assert.That(() => list.UpperBound(10.0, from, list.Count, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.UpperBound(10.0, from, list.Count)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.UpperBound(10.0, from, list.Count)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test04NegativeTo()
		{
			var list = new List<double> { 0 };
			const int to = -1;
			// comparer version
			Assert.That(() => list.UpperBound(10.0, 0, to, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.UpperBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.UpperBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test05ToExceedsCount()
		{
			var list = new List<double> { 0 };
			var to = list.Count + 1;
			// comparer version
			Assert.That(() => list.UpperBound(10.0, 0, to, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.UpperBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.UpperBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test06BadFromToOrder()
		{
			var list = new List<double> { 0, 1, 2 };
			const int from = 2;
			const int to = 1;
			// comparer version
			Assert.That(
				() => list.UpperBound(10.0, from, to, Comparer<double>.Default.Compare),
				Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.UpperBound(10.0, from, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.UpperBound(10.0, from, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		[TestCase(new double[0], 1.0, 0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 5.0, 0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 21.0, 3, 3, 3)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 101.0, 5, 5, 5)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 15.0, 0, 3, 3)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 1, 4, 2)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 30000.0, 0, 4, 4)]
		public void Test07WithAllParams(double[] data, double value, int from, int to, int expected)
		{
			// comparer version
			var list = (IList<double>)data;
			Assert.That(list.UpperBound(value, from, to, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.UpperBound(value, from, to), Is.EqualTo(expected));
			// specific version
			Assert.That(list.UpperBound(value, from, to), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 11.0, 0)]
		public void Test08WithComparer(double[] data, double value, int expected)
		{
			// comparer version
			var list = (IList<double>)data;
			Assert.That(list.UpperBound(value, Comparer<double>.Default.Compare), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 4.0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 15.0, 5, 5)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 15.0, 0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 0, 2)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, -1.0, 4, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1.0, 3, 3)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 42.0, 6, 6)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1002.0, 3, 7)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 12.0, 1, 4)]
		public void Test09WithFrom(double[] data, double value, int from, int expected)
		{
			// comparer version
			var list = (IList<double>)data;
			Assert.That(list.UpperBound(value, from, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.UpperBound(value, from), Is.EqualTo(expected));
			// specific version
			Assert.That(list.UpperBound(value, from), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 14.0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 50.0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 2)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 30000.0, 8)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, -1.0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1.0, 1)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 42.0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1002.0, 7)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 12.0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 3.0, 1)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 14534.0, 8)]
		public void Test10WithoutParams(double[] data, double value, int expected)
		{
			var list = (IList<double>)data;
			Assert.That(list.UpperBound(value, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.UpperBound(value), Is.EqualTo(expected));
			// specific version
			Assert.That(list.UpperBound(value), Is.EqualTo(expected));
		}
	}
}
