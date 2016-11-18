using System;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Ranges;

using NUnit.Framework;

namespace CodeJam
{
	// ReSharper disable ImplicitlyCapturedClosure
	[TestFixture]
	public class EqualRangeTest
	{
		[Test]
		public void Test01NegativeFrom()
		{
			var list = new List<double> { 0 };
			const int from = -1;
			// comparer version
			Assert.That(() => list.EqualRange(10.0, from, list.Count, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.EqualRange(10.0, from, list.Count)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.EqualRange(10.0, from, list.Count)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test02NegativeTo()
		{
			var list = new List<double> { 0 };
			const int to = -1;
			// comparer version
			Assert.That(() => list.EqualRange(10.0, 0, to, Comparer<double>.Default.Compare)
				, Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.EqualRange(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.EqualRange(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test03ToExceedsCount()
		{
			var list = new List<double> { 0 };
			var to = list.Count + 1;
			// comparer version
			Assert.That(
				() => list.EqualRange(10.0, 0, to, Comparer<double>.Default.Compare),
				Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.EqualRange(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.EqualRange(10.0, 0, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		public void Test04BadFromToOrder()
		{
			var list = new List<double> { 0, 1, 2 };
			const int from = 2;
			const int to = 1;
			// comparer version
			Assert.That(
				() => list.EqualRange(10.0, from, to, Comparer<double>.Default.Compare),
				Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(() => list2.EqualRange(10.0, from, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
			// specific version
			Assert.That(() => list.EqualRange(10.0, from, to), Throws.InstanceOf(typeof(ArgumentOutOfRangeException)));
		}

		[Test]
		[TestCase(new double[0], 1.0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 5.0, 0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 21.0, 3, 3)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 101.0, 5, 5)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 15.0, 0, 3)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 1, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 30000.0, 0, 4)]
		public void Test05WithAllParams(double[] data, double value, int from, int to)
		{
			var list = (IList<double>)data;
			var expected = Range.Create(list.LowerBound(value, from, to), list.UpperBound(value, from, to));
			// comparer version
			Assert.That(list.EqualRange(value, from, to, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.EqualRange(value, from, to), Is.EqualTo(expected));
			// specific version
			Assert.That(list.EqualRange(value, from, to), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 11.0)]
		public void Test06WithComparer(double[] data, double value)
		{
			// comparer version
			var list = (IList<double>)data;
			var expected = Range.Create(list.LowerBound(value), list.UpperBound(value));
			Assert.That(list.EqualRange(value, Comparer<double>.Default.Compare), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 4.0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 35, 123 }, 15.0, 5)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 15.0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0, 0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, -1.0, 4)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1.0, 3)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 42.0, 6)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1002.0, 3)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 12.0, 1)]
		public void Test07WithFrom(double[] data, double value, int from)
		{
			var list = (IList<double>)data;
			var expected = Range.Create(list.LowerBound(value, from), list.UpperBound(value, from));
			// comparer version
			Assert.That(list.EqualRange(value, from, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.EqualRange(value, from), Is.EqualTo(expected));
			// specific version
			Assert.That(list.EqualRange(value, from), Is.EqualTo(expected));
		}

		[Test]
		[TestCase(new double[0], 14.0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 50.0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 5.0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 30000.0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, -1.0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1.0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 42.0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 1002.0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 12.0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 3.0)]
		[TestCase(new[] { 1.0, 5, 12, 12, 123, 512, 512, 14534 }, 14534.0)]
		public void Test08WithoutParams(double[] data, double value)
		{
			var list = (IList<double>)data;
			var expected = Range.Create(list.LowerBound(value), list.UpperBound(value));
			// comparer version
			Assert.That(list.EqualRange(value, Comparer<double>.Default.Compare), Is.EqualTo(expected));
			// IComparable version
			var list2 = list.Cast<IComparable<double>>().ToList();
			Assert.That(list2.EqualRange(value), Is.EqualTo(expected));
			// specific version
			Assert.That(list.EqualRange(value), Is.EqualTo(expected));
		}

		[Test]
		public void Test09Randomized()
		{
			var list = new List<double>();
			var rnd = new Random();
			const int maxValue = 10000;
			for (var i = 0; i < maxValue; ++i)
			{
				var repeats = rnd.Next(1, 11);
				list.AddRange(Enumerable.Repeat(i, repeats).Select(_ => _ * 0.01));
			}
			var list2 = list.Cast<IComparable<double>>().ToList();
			for (var j = -10.0; j < maxValue / 100.0 + 10; ++j)
			{
				var expected = Range.Create(list.LowerBound(j), list.UpperBound(j));
				// comparer version
				Assert.That(list.EqualRange(j, Comparer<double>.Default.Compare), Is.EqualTo(expected));
				// IComparable version
				Assert.That(list2.EqualRange(j), Is.EqualTo(expected));
				// specific version
				Assert.That(list.EqualRange(j), Is.EqualTo(expected));
			}
		}
	}
}
