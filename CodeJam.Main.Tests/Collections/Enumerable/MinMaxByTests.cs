using System;
using System.Globalization;
using System.Linq;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture(Category = "MinMaxBy")]
	public partial class MinMaxByTests
	{
		#region NaN support
		[TestCase(new[] { 1, double.NaN, 2, 3, 4, 5, 6 }, ExpectedResult = 1.0)]
		[TestCase(new[] { double.NaN, 1, 2, 3, 4, 5, 6 }, ExpectedResult = 1.0)]
		public double MinByNaN(double[] source) =>
			source.Select(v => new Item<double>(v)).MinBy(i => i.Value).Value;

		[TestCase(new[] { 1, double.NaN, 2, 3, 4, 5, 6 }, ExpectedResult = 1.0)]
		[TestCase(new[] { double.NaN, 1, 2, 3, 4, 5, 6 }, ExpectedResult = 1.0)]
		public double MinByOrDefaultNaN(double[] source) =>
			source.Select(v => new Item<double>(v)).MinByOrDefault(i => i.Value)!.Value;

		[TestCase(new[] { 1, double.NaN, 2, 3, 4, 5, 6 }, ExpectedResult = 1.0)]
		[TestCase(new[] { double.NaN, 1, 2, 3, 4, 5, 6 }, ExpectedResult = 1.0)]
		public double? MinByNullableNaN(double[] source) =>
			source.Select(v => new Item<double?>(v)).MinBy(i => i.Value).Value;

		[TestCase(new[] { 1, double.NaN, 6, 5, 4, 3, 2, 1 }, ExpectedResult = 6.0)]
		[TestCase(new[] { double.NaN, 6, 5, 4, 3, 2, 1 }, ExpectedResult = 6.0)]
		public double MaxByNaN(double[] source) =>
			source.Select(v => new Item<double>(v)).MaxBy(i => i.Value).Value;
		#endregion

		#region Min
		[TestCase(new[] { 3, 1, 0, 4, 6 }, ExpectedResult = "0")]
		[TestCase(new[] { 1 }, ExpectedResult = "1")]
		public string MinByString(int[] source) =>
			source.Select(v => new Item<string>(v.ToString(CultureInfo.InvariantCulture))).MinBy(i => i.Value)?.Value!;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string?[] { null, null })]
		public void MinByStringNoElements(string[] source) =>
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			Assert.Throws<InvalidOperationException>(() => source.MinBy(s => s));

		[TestCase(new[] { 3, 1, 0, 4, 6 }, ExpectedResult = "0")]
		[TestCase(new[] { 1 }, ExpectedResult = "1")]
		[TestCase(new int[0], ExpectedResult = null)]
		public string? MinByOrDefaultString(int[] source) =>
			source.Select(v => new Item<string>(v.ToString(CultureInfo.InvariantCulture))).MinByOrDefault(i => i.Value)?.Value;

		#endregion

		#region Max
		[TestCase(new[] { 3, 1, 8, 0, 6 }, ExpectedResult = "8")]
		[TestCase(new[] { 1 }, ExpectedResult = "1")]
		public string MaxByString(int[] source) =>
			source.Select(v => new Item<string>(v.ToString(CultureInfo.InvariantCulture))).MaxBy(i => i.Value)!.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string?[] { null, null })]
		public void MaxByStringNoElements(string[] source) =>
			Assert.Throws<InvalidOperationException>(() => _ = source.MaxBy(s => s));

		[TestCase(new[] { 3, 1, 0, 4, 6 }, ExpectedResult = "6")]
		[TestCase(new[] { 1 }, ExpectedResult = "1")]
		[TestCase(new int[0], ExpectedResult = null)]
		public string? MaxByOrDefaultString(int[] source) =>
			source.Select(v => new Item<string>(v.ToString(CultureInfo.InvariantCulture))).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Item class
		private class Item<T>
		{
			public Item(T value) => Value = value;

			public T Value { get; }
		}
		#endregion
	}
}