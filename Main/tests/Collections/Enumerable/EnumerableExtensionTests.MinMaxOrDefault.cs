using System;
using System.Linq;

using NUnit.Framework;

using static NUnit.Framework.Assert;

namespace CodeJam.Collections
{
	[TestFixture]
	public partial class EnumerableExtensionTests
	{
		#region MinOrDefault
		[Test]
		public void MinOrDefaultEmpty()
		{
			AreEqual(new int[0].MinOrDefault(), 0);
			AreEqual(new int[0].MinOrDefault(42), 42);
			AreEqual(new int?[0].MinOrDefault(), null);
			AreEqual(new int?[0].MinOrDefault(42), 42);
			AreEqual(new string[0].MinOrDefault(), null);
			AreEqual(new string[0].MinOrDefault("Hello!"), "Hello!");
		}

		[TestCase(new[] { 1.0 }, 1.0)]
		[TestCase(new[] { double.NaN, double.NaN, 1, double.NaN, 2, 3, 4, 5, 6, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { double.NaN, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { 1.0, 2, 3, 4, 5, 6 }, 1)]
		[TestCase(new[] { -1.0, -1, -1 }, -1)]
		[TestCase(new[] { -1.0, double.NaN, -1 }, double.NaN)]
		public void MinOrDefault(double[] source, double expected)
		{
			var rnd = TestTools.GetTestRandom();
			AreEqual(source.MinOrDefault(123), expected);
			AreEqual(source.MinOrDefault(), expected);
			AreEqual(source.Min(), expected);
			AreEqual(source.Shuffle(rnd).MinOrDefault(123), expected);
			AreEqual(source.Shuffle(rnd).MinOrDefault(), expected);
			AreEqual(source.Shuffle(rnd).Min(), expected);
		}

		[TestCase(new[] { 1.0 }, 1.0)]
		[TestCase(new[] { double.NaN, double.NaN, 1, double.NaN, 2, 3, 4, 5, 6, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { double.NaN, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { 1.0, 2, 3, 4, 5, 6 }, 1)]
		[TestCase(new[] { -1.0, -1, -1 }, -1)]
		[TestCase(new[] { -1, double.NaN, -1 }, double.NaN)]
		public void MinOrDefaultNullable(double[] source, double expected)
		{
			var expectedItem = expected.Equals(-1) ? (double?)null : expected;
			var rnd = TestTools.GetTestRandom();
			var items = source
				.Select(i => i.Equals(-1.0) ? (double?)null : i)
				.Prepend(null, null)
				.Concat(null, null)
				.ToArray();

			AreEqual(items.MinOrDefault(123), expectedItem);
			AreEqual(items.MinOrDefault(), expectedItem);
			AreEqual(items.Min(), expectedItem);
			AreEqual(items.Shuffle(rnd).MinOrDefault(123), expectedItem);
			AreEqual(items.Shuffle(rnd).MinOrDefault(), expectedItem);
			AreEqual(items.Shuffle(rnd).Min(), expectedItem);
		}

		[TestCase(new[] { "A" }, "A")]
		[TestCase(new[] { "A", "B", "C", "D", "E" }, "A")]
		[TestCase(new string[] { null, null, null }, null)]
		[TestCase(new [] { null, null, "A", null }, "A")]
		public void MinOrDefaultString(string[] source, string expected)
		{
			var rnd = TestTools.GetTestRandom();

			AreEqual(source.MinOrDefault("!"), expected);
			AreEqual(source.MinOrDefault(), expected);
			AreEqual(source.Min(), expected);
			AreEqual(source.Shuffle(rnd).MinOrDefault("!"), expected);
			AreEqual(source.Shuffle(rnd).MinOrDefault(), expected);
			AreEqual(source.Shuffle(rnd).Min(), expected);
		}
		#endregion

		#region MaxOrDefault
		[Test]
		public void MaxOrDefaultEmpty()
		{
			AreEqual(new int[0].MaxOrDefault(), 0);
			AreEqual(new int[0].MaxOrDefault(42), 42);
			AreEqual(new int?[0].MaxOrDefault(), null);
			AreEqual(new int?[0].MaxOrDefault(42), 42);
			AreEqual(new string[0].MaxOrDefault(), null);
			AreEqual(new string[0].MaxOrDefault("Hello!"), "Hello!");
		}

		[TestCase(new[] { 1.0 }, 1.0)]
		[TestCase(new[] { double.NaN, double.NaN, 1, double.NaN, 2, 3, 4, 5, 6, double.NaN, double.NaN }, 6)]
		[TestCase(new[] { double.NaN, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { 1.0, 2, 3, 4, 5, 6 }, 6)]
		[TestCase(new[] { -1.0, -1, -1 }, -1)]
		[TestCase(new[] { -1.0, double.NaN, -1 }, -1)]
		public void MaxOrDefault(double[] source, double expected)
		{
			var rnd = TestTools.GetTestRandom();
			AreEqual(source.MaxOrDefault(123), expected);
			AreEqual(source.MaxOrDefault(), expected);
			AreEqual(source.Max(), expected);
			AreEqual(source.Shuffle(rnd).MaxOrDefault(123), expected);
			AreEqual(source.Shuffle(rnd).MaxOrDefault(), expected);
			AreEqual(source.Shuffle(rnd).Max(), expected);
		}

		[TestCase(new[] { 1.0 }, 1.0)]
		[TestCase(new[] { double.NaN, double.NaN, 1, double.NaN, 2, 3, 4, 5, 6, double.NaN, double.NaN }, 6)]
		[TestCase(new[] { double.NaN, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { 1.0, 2, 3, 4, 5, 6 }, 6)]
		[TestCase(new[] { -1.0, -1, -1 }, -1)]
		[TestCase(new[] { -1, double.NaN, -1 }, double.NaN)]
		public void MaxOrDefaultNullable(double[] source, double expected)
		{
			var expectedItem = expected.Equals(-1) ? (double?)null : expected;
			var rnd = TestTools.GetTestRandom();
			var items = source
				.Select(i => i.Equals(-1.0) ? (double?)null : i)
				.Prepend(null, null)
				.Concat(null, null)
				.ToArray();

			AreEqual(items.MaxOrDefault(123), expectedItem);
			AreEqual(items.MaxOrDefault(), expectedItem);
			AreEqual(items.Max(), expectedItem);
			AreEqual(items.Shuffle(rnd).MaxOrDefault(123), expectedItem);
			AreEqual(items.Shuffle(rnd).MaxOrDefault(), expectedItem);
			AreEqual(items.Shuffle(rnd).Max(), expectedItem);
		}

		[TestCase(new[] { "A" }, "A")]
		[TestCase(new[] { "A", "B", "C", "D", "E" }, "E")]
		[TestCase(new string[] { null, null, null }, null)]
		[TestCase(new[] { null, null, "A", null }, "A")]
		public void MaxOrDefaultString(string[] source, string expected)
		{
			var rnd = TestTools.GetTestRandom();

			AreEqual(source.MaxOrDefault("!"), expected);
			AreEqual(source.MaxOrDefault(), expected);
			AreEqual(source.Max(), expected);
			AreEqual(source.Shuffle(rnd).MaxOrDefault("!"), expected);
			AreEqual(source.Shuffle(rnd).MaxOrDefault(), expected);
			AreEqual(source.Shuffle(rnd).Max(), expected);
		}
		#endregion

	}
}
