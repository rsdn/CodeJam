using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using NUnit.Framework;

using static NUnit.Framework.Assert;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

namespace CodeJam.Collections
{
	[TestFixture]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public partial class EnumerableExtensionTests
	{
		#region MinOrDefault
		[Test]
		public void MinOrDefaultEmpty()
		{
			var comparer = Comparer<int>.Default;
			var comparerNullable = Comparer<int?>.Default;
			var comparerString = Comparer<string>.Default;

			AreEqual(new int[0].MinOrDefault(), 0);
			AreEqual(new int[0].MinOrDefault(42), 42);
			AreEqual(new int[0].MinOrDefault(comparer, 42), 42);
			AreEqual(new int?[0].MinOrDefault(), null);
			AreEqual(new int?[0].MinOrDefault(42), 42);
			AreEqual(new int?[0].MinOrDefault(comparerNullable, 42), 42);
			AreEqual(new string[0].MinOrDefault(), null);
			AreEqual(new string[0].MinOrDefault("Hello!"), "Hello!");
			AreEqual(new string[0].MinOrDefault(comparerString, "Hello!"), "Hello!");

			AreEqual(new int[0].Wrap().MinOrDefault(i => i.Value), 0);
			AreEqual(new int[0].Wrap().MinOrDefault(i => i.Value, 42), 42);
			AreEqual(new int[0].Wrap().MinOrDefault(i => i.Value, comparer, 42), 42);
			AreEqual(new int?[0].Wrap().MinOrDefault(i => i.Value), null);
			AreEqual(new int?[0].Wrap().MinOrDefault(i => i.Value, 42), 42);
			AreEqual(new int?[0].Wrap().MinOrDefault(i => i.Value, comparerNullable, 42), 42);
			AreEqual(new string[0].Wrap().MinOrDefault(i => i.Value), null);
			AreEqual(new string[0].Wrap().MinOrDefault(i => i.Value, "Hello!"), "Hello!");
			AreEqual(new string[0].Wrap().MinOrDefault(i => i.Value, comparerString, "Hello!"), "Hello!");
		}

		[TestCase(new[] { 1.0 }, 1.0)]
		[TestCase(new[] { double.NaN, double.NaN, 1, double.NaN, 2, 3, 4, 5, 6, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { double.NaN, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { 1.0, 2, 3, 4, 5, 6 }, 1)]
		[TestCase(new[] { -1.0, -1, -1 }, -1)]
		[TestCase(new[] { -1.0, double.NaN, -1 }, double.NaN)]
		public void MinOrDefault([NotNull] double[] source, double expected)
		{
			var rnd = TestTools.GetTestRandom();
			var comparer = Comparer<double>.Default;

			AreEqual(source.MinOrDefault(comparer), expected);
			AreEqual(source.MinOrDefault(123), expected);
			AreEqual(source.MinOrDefault(), expected);
			AreEqual(source.Min(), expected);
			AreEqual(source.Shuffle(rnd).MinOrDefault(comparer), expected);
			AreEqual(source.Shuffle(rnd).MinOrDefault(123), expected);
			AreEqual(source.Shuffle(rnd).MinOrDefault(), expected);
			AreEqual(source.Shuffle(rnd).Min(), expected);

			AreEqual(source.Wrap().MinOrDefault(i => i.Value, comparer), expected);
			AreEqual(source.Wrap().MinOrDefault(i => i.Value, 123), expected);
			AreEqual(source.Wrap().MinOrDefault(i => i.Value), expected);
			AreEqual(source.Wrap().Min(i => i.Value), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MinOrDefault(i => i.Value, comparer), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MinOrDefault(i => i.Value, 123), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MinOrDefault(i => i.Value), expected);
			AreEqual(source.Wrap().Shuffle(rnd).Min(i => i.Value), expected);
		}

		[TestCase(new[] { 1.0 }, 1.0)]
		[TestCase(new[] { double.NaN, double.NaN, 1, double.NaN, 2, 3, 4, 5, 6, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { double.NaN, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { 1.0, 2, 3, 4, 5, 6 }, 1)]
		[TestCase(new[] { -1.0, -1, -1 }, -1)]
		[TestCase(new[] { -1, double.NaN, -1 }, double.NaN)]
		public void MinOrDefaultNullable([NotNull] double[] source, double expected)
		{
			var rnd = TestTools.GetTestRandom();
			var comparer = Comparer<double?>.Default;
			var expectedItem = expected.Equals(-1) ? (double?)null : expected;
			var items = source
				.Select(i => i.Equals(-1.0) ? (double?)null : i)
				.Prepend(null, null)
				.Concat(null, null)
				.ToArray();

			AreEqual(items.MinOrDefault(comparer), expectedItem);
			AreEqual(items.MinOrDefault(123), expectedItem);
			AreEqual(items.MinOrDefault(), expectedItem);
			AreEqual(items.Min(), expectedItem);
			AreEqual(items.Shuffle(rnd).MinOrDefault(comparer), expectedItem);
			AreEqual(items.Shuffle(rnd).MinOrDefault(123), expectedItem);
			AreEqual(items.Shuffle(rnd).MinOrDefault(), expectedItem);
			AreEqual(items.Shuffle(rnd).Min(), expectedItem);

			AreEqual(items.Wrap().MinOrDefault(i => i.Value, comparer), expectedItem);
			AreEqual(items.Wrap().MinOrDefault(i => i.Value, 123), expectedItem);
			AreEqual(items.Wrap().MinOrDefault(i => i.Value), expectedItem);
			AreEqual(items.Wrap().Min(i => i.Value), expectedItem);
			AreEqual(items.Wrap().Shuffle(rnd).MinOrDefault(i => i.Value, comparer), expectedItem);
			AreEqual(items.Wrap().Shuffle(rnd).MinOrDefault(i => i.Value, 123), expectedItem);
			AreEqual(items.Wrap().Shuffle(rnd).MinOrDefault(i => i.Value), expectedItem);
			AreEqual(items.Wrap().Shuffle(rnd).Min(i => i.Value), expectedItem);
		}

		[TestCase(new[] { "A" }, "A")]
		[TestCase(new[] { "A", "B", "C", "D", "E" }, "A")]
		[TestCase(new string?[] { null, null, null }, null)]
		[TestCase(new[] { null, null, "A", null }, "A")]
		public void MinOrDefaultString([NotNull] string[] source, [NotNull] string expected)
		{
			var rnd = TestTools.GetTestRandom();
			var comparer = Comparer<string>.Default;

			AreEqual(source.MinOrDefault(comparer), expected);
			AreEqual(source.MinOrDefault("!"), expected);
			AreEqual(source.MinOrDefault(), expected);
			AreEqual(source.Min(), expected);
			AreEqual(source.Shuffle(rnd).MinOrDefault(comparer), expected);
			AreEqual(source.Shuffle(rnd).MinOrDefault("!"), expected);
			AreEqual(source.Shuffle(rnd).MinOrDefault(), expected);
			AreEqual(source.Shuffle(rnd).Min(), expected);

			AreEqual(source.Wrap().MinOrDefault(i => i.Value, comparer), expected);
			AreEqual(source.Wrap().MinOrDefault(i => i.Value, "!"), expected);
			AreEqual(source.Wrap().MinOrDefault(i => i.Value), expected);
			AreEqual(source.Wrap().Min(i => i.Value), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MinOrDefault(i => i.Value, comparer), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MinOrDefault(i => i.Value, "!"), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MinOrDefault(i => i.Value), expected);
			AreEqual(source.Wrap().Shuffle(rnd).Min(i => i.Value), expected);
		}
		#endregion

		#region MaxOrDefault
		[Test]
		public void MaxOrDefaultEmpty()
		{
			var comparer = Comparer<int>.Default;
			var comparerNullable = Comparer<int?>.Default;
			var comparerString = Comparer<string>.Default;

			AreEqual(new int[0].MaxOrDefault(), 0);
			AreEqual(new int[0].MaxOrDefault(42), 42);
			AreEqual(new int[0].MaxOrDefault(comparer, 42), 42);
			AreEqual(new int?[0].MaxOrDefault(), null);
			AreEqual(new int?[0].MaxOrDefault(42), 42);
			AreEqual(new int?[0].MaxOrDefault(comparerNullable, 42), 42);
			AreEqual(new string[0].MaxOrDefault(), null);
			AreEqual(new string[0].MaxOrDefault("Hello!"), "Hello!");
			AreEqual(new string[0].MaxOrDefault(comparerString, "Hello!"), "Hello!");

			AreEqual(new int[0].Wrap().MaxOrDefault(i => i.Value), 0);
			AreEqual(new int[0].Wrap().MaxOrDefault(i => i.Value, 42), 42);
			AreEqual(new int[0].Wrap().MaxOrDefault(i => i.Value, comparer, 42), 42);
			AreEqual(new int?[0].Wrap().MaxOrDefault(i => i.Value), null);
			AreEqual(new int?[0].Wrap().MaxOrDefault(i => i.Value, 42), 42);
			AreEqual(new int?[0].Wrap().MaxOrDefault(i => i.Value, comparerNullable, 42), 42);
			AreEqual(new string[0].Wrap().MaxOrDefault(i => i.Value), null);
			AreEqual(new string[0].Wrap().MaxOrDefault(i => i.Value, "Hello!"), "Hello!");
			AreEqual(new string[0].Wrap().MaxOrDefault(i => i.Value, comparerString, "Hello!"), "Hello!");
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
			var comparer = Comparer<double>.Default;

			AreEqual(source.MaxOrDefault(comparer), expected);
			AreEqual(source.MaxOrDefault(123), expected);
			AreEqual(source.MaxOrDefault(), expected);
			AreEqual(source.Max(), expected);
			AreEqual(source.Shuffle(rnd).MaxOrDefault(comparer), expected);
			AreEqual(source.Shuffle(rnd).MaxOrDefault(123), expected);
			AreEqual(source.Shuffle(rnd).MaxOrDefault(), expected);
			AreEqual(source.Shuffle(rnd).Max(), expected);

			AreEqual(source.Wrap().MaxOrDefault(i => i.Value, comparer), expected);
			AreEqual(source.Wrap().MaxOrDefault(i => i.Value, 123), expected);
			AreEqual(source.Wrap().MaxOrDefault(i => i.Value), expected);
			AreEqual(source.Wrap().Max(i => i.Value), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MaxOrDefault(i => i.Value, comparer), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MaxOrDefault(i => i.Value, 123), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MaxOrDefault(i => i.Value), expected);
			AreEqual(source.Wrap().Shuffle(rnd).Max(i => i.Value), expected);
		}

		[TestCase(new[] { 1.0 }, 1.0)]
		[TestCase(new[] { double.NaN, double.NaN, 1, double.NaN, 2, 3, 4, 5, 6, double.NaN, double.NaN }, 6)]
		[TestCase(new[] { double.NaN, double.NaN, double.NaN }, double.NaN)]
		[TestCase(new[] { 1.0, 2, 3, 4, 5, 6 }, 6)]
		[TestCase(new[] { -1.0, -1, -1 }, -1)]
		[TestCase(new[] { -1, double.NaN, -1 }, double.NaN)]
		public void MaxOrDefaultNullable(double[] source, double expected)
		{
			var rnd = TestTools.GetTestRandom();
			var comparer = Comparer<double?>.Default;
			var expectedItem = expected.Equals(-1) ? (double?)null : expected;
			var items = source
				.Select(i => i.Equals(-1.0) ? (double?)null : i)
				.Prepend(null, null)
				.Concat(null, null)
				.ToArray();

			AreEqual(items.MaxOrDefault(comparer), expectedItem);
			AreEqual(items.MaxOrDefault(123), expectedItem);
			AreEqual(items.MaxOrDefault(), expectedItem);
			AreEqual(items.Max(), expectedItem);
			AreEqual(items.Shuffle(rnd).MaxOrDefault(comparer), expectedItem);
			AreEqual(items.Shuffle(rnd).MaxOrDefault(123), expectedItem);
			AreEqual(items.Shuffle(rnd).MaxOrDefault(), expectedItem);
			AreEqual(items.Shuffle(rnd).Max(), expectedItem);

			AreEqual(items.Wrap().MaxOrDefault(i => i.Value, comparer), expectedItem);
			AreEqual(items.Wrap().MaxOrDefault(i => i.Value, 123), expectedItem);
			AreEqual(items.Wrap().MaxOrDefault(i => i.Value), expectedItem);
			AreEqual(items.Wrap().Max(i => i.Value), expectedItem);
			AreEqual(items.Wrap().Shuffle(rnd).MaxOrDefault(i => i.Value, comparer), expectedItem);
			AreEqual(items.Wrap().Shuffle(rnd).MaxOrDefault(i => i.Value, 123), expectedItem);
			AreEqual(items.Wrap().Shuffle(rnd).MaxOrDefault(i => i.Value), expectedItem);
			AreEqual(items.Wrap().Shuffle(rnd).Max(i => i.Value), expectedItem);
		}

		[TestCase(new[] { "A" }, "A")]
		[TestCase(new[] { "A", "B", "C", "D", "E" }, "E")]
		[TestCase(new string?[] { null, null, null }, null)]
		[TestCase(new[] { null, null, "A", null }, "A")]
		public void MaxOrDefaultString(string[] source, string expected)
		{
			var rnd = TestTools.GetTestRandom();
			var comparer = Comparer<string>.Default;

			AreEqual(source.MaxOrDefault(comparer), expected);
			AreEqual(source.MaxOrDefault("!"), expected);
			AreEqual(source.MaxOrDefault(), expected);
			AreEqual(source.Max(), expected);
			AreEqual(source.Shuffle(rnd).MaxOrDefault(comparer), expected);
			AreEqual(source.Shuffle(rnd).MaxOrDefault("!"), expected);
			AreEqual(source.Shuffle(rnd).MaxOrDefault(), expected);
			AreEqual(source.Shuffle(rnd).Max(), expected);

			AreEqual(source.Wrap().MaxOrDefault(i => i.Value, comparer), expected);
			AreEqual(source.Wrap().MaxOrDefault(i => i.Value, "!"), expected);
			AreEqual(source.Wrap().MaxOrDefault(i => i.Value), expected);
			AreEqual(source.Wrap().Max(i => i.Value), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MaxOrDefault(i => i.Value, comparer), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MaxOrDefault(i => i.Value, "!"), expected);
			AreEqual(source.Wrap().Shuffle(rnd).MaxOrDefault(i => i.Value), expected);
			AreEqual(source.Wrap().Shuffle(rnd).Max(i => i.Value), expected);
		}
		#endregion
	}
}