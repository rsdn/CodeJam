using System;
using System.Linq;

using CodeJam.Collections;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture]
	public partial class MinMaxByTests
	{
		#region Min
		[TestCase(new[] {3, 1, 0, 4, 6}, ExpectedResult = "0")]
		[TestCase(new[] {1}, ExpectedResult = "1")]
		public string MinByString(int[] source) =>
			source.Select(v => new Item<string>(v.ToString())).MinBy(i => i.Value)?.Value;


		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] {null, null})]
		public void MinByStringNoElems(string[] source) =>
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			Assert.Throws<InvalidOperationException>(() => source.MinBy(s => s));

		[TestCase(new[] { 3, 1, 0, 4, 6 }, ExpectedResult = "0")]
		[TestCase(new[] { 1 }, ExpectedResult = "1")]
		[TestCase(new int[0], ExpectedResult = null)]
		public string MinByOrDefaultString(int[] source) =>
			source.Select(v => new Item<string>(v.ToString())).MinByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Max
		[TestCase(new[] { 3, 1, 8, 0, 6 }, ExpectedResult = "8")]
		[TestCase(new[] { 1 }, ExpectedResult = "1")]
		public string MaxByString(int[] source) =>
			source.Select(v => new Item<string>(v.ToString())).MaxBy(i => i.Value)?.Value;

		[TestCase(arg: new string[0])]
		[TestCase(arg: new string[] { null, null })]
		public void MaxByStringNoElems(string[] source) =>
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			Assert.Throws<InvalidOperationException>(() => source.MaxBy(s => s));

		[TestCase(new[] { 3, 1, 0, 4, 6 }, ExpectedResult = "6")]
		[TestCase(new[] { 1 }, ExpectedResult = "1")]
		[TestCase(new int[0], ExpectedResult = null)]
		public string MaxByOrDefaultString(int[] source) =>
			source.Select(v => new Item<string>(v.ToString())).MaxByOrDefault(i => i.Value)?.Value;
		#endregion

		#region Item class
		private class Item<T>
		{
			public Item(T value)
			{
				Value = value;
			}

			public T Value { get; }
		}
		#endregion
	}
}