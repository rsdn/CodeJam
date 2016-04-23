using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace CodeJam
{
	// ReSharper disable ImplicitlyCapturedClosure
	[TestFixture]
	public class LowerBoundTest
	{
		/// <summary>Shows how to use LowerBound to search for a value equal or greater than the given value</summary>
		[Test]
		public void Test01LowerBoundSearchSample()
		{
			// The list should be sorted!
			var sortedData = new List<string> { "B", "C", "C", "D", "F" };

			var indexOfLowerBound = sortedData.LowerBound("A");
			// No value, so the position of the first greater value should be returned
			Assert.That(indexOfLowerBound, Is.EqualTo(sortedData.IndexOf("B")));

			indexOfLowerBound = sortedData.LowerBound("B");
			// Exact match
			Assert.That(indexOfLowerBound, Is.EqualTo(sortedData.IndexOf("B")));

			indexOfLowerBound = sortedData.LowerBound("C");
			// The position of the first match should be returned
			Assert.That(indexOfLowerBound, Is.EqualTo(sortedData.IndexOf("C")));

			indexOfLowerBound = sortedData.LowerBound("E");
			// No value, so the position of the first greater value should be returned
			Assert.That(indexOfLowerBound, Is.EqualTo(sortedData.IndexOf("F")));

			indexOfLowerBound = sortedData.LowerBound("G");
			// No value and no greater value, so the position after the last element should be returned
			Assert.That(indexOfLowerBound, Is.EqualTo(sortedData.Count));
		}

		/// <summary>Shows how to use LowerBound to insert a new value to the sorted list while preserving the order</summary>
		[Test]
		public void Test02LowerBoundInsertionSample()
		{
			// The list should be sorted!
			var sortedData = new List<int> { 1, 2, 3, 5, 6 };
			const int valueToInsert = 4;
			var insertionIndex = sortedData.LowerBound(valueToInsert);
			sortedData.Insert(insertionIndex, valueToInsert);
			Assert.That(sortedData.IndexOf(valueToInsert), Is.EqualTo(3));
		}

		[Test]
		public void Test03NegativeFrom()
		{
			var list = new List<double> { 0 };
			const int from = -1;
			// comparer version
			Assert.That(() => list.LowerBound(10.0, from, list.Count, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.LowerBound(10.0, from, list.Count)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.LowerBound(10.0, from, list.Count)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test04NegativeTo()
		{
			var list = new List<double> { 0 };
			const int to = -1;
			// comparer version
			Assert.That(() => list.LowerBound(10.0, 0, to, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.LowerBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.LowerBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}


		[Test]
		public void Test05ToExceedsCount()
		{
			var list = new List<double> { 0 };
			var to = list.Count + 1;
			// comparer version
			Assert.That(() => list.LowerBound(10.0, 0, to, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.LowerBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.LowerBound(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test06BadFromToOrder()
		{
			var list = new List<double> { 0, 1, 2 };
			const int from = 2;
			const int to = 0;
			// comparer version
			Assert.That(
				() => list.LowerBound(10.0, from, to, Comparer<double>.Default.Compare),
				Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.LowerBound(10.0, from, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.LowerBound(10.0, from, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		[TestCase(new double[0], 1.0, 0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 10.0, 0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 11.0, 2, 2, 2)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 30.0, 5, 5, 5)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 15.0, 0, 3, 3)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 1, 4, 1)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 30000.0, 0, 4, 4)]
		public void Test07WithAllParams(double[] data, double value, int from, int to, int expected)
		{
			// comparer version
			var list = (IList<double>)data;
			Assert.That(list.LowerBound(value, from, to, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.LowerBound(value, from, to), Is.EqualTo(expected));
			// specific version
			Assert.That(list.LowerBound(value, from, to), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 11.0, 0)]
		public void Test08WithComparer(double[] data, double value, int expected)
		{
			// comparer version
			var list = (IList<double>)data;
			Assert.That(list.LowerBound(value, Comparer<double>.Default.Compare), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 4.0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 35.0, 5, 5)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 15.0, 0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 0, 1)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, -1.0, 4, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 42.0, 6, 6)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1002.0, 3, 7)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 12.0, 3, 3)]
		public void Test09WithFrom(double[] data, double value, int from, int expected)
		{
			// comparer version
			var list = (IList<double>)data;
			Assert.That(list.LowerBound(value, from, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.LowerBound(value, from), Is.EqualTo(expected));
			// specific version
			Assert.That(list.LowerBound(value, from), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 14, 0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 50.0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 1)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 30000.0, 8)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, -1.0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 42.0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1002.0, 7)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 12.0, 2)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 3.0, 1)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 14534.0, 7)]
		public void Test10WithoutParams(double[] data, double value, int expected)
		{
			// comparer version
			var list = (IList<double>)data;
			Assert.That(list.LowerBound(value, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.LowerBound(value), Is.EqualTo(expected));
			// specific version
			Assert.That(list.LowerBound(value), Is.EqualTo(expected));
		}
	}
}
