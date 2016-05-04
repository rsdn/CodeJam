using System;
using System.Linq;

using CodeJam.Strings;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.Collections
{
	partial class QueryableExtensionsTests
	{
		[Test]
		public void OrderByTest()
		{
			var actual = GetQuery1().OrderBy("Char").Join(", ");
			Assert.AreEqual("(A, 0), (B, 0), (C, 0)", actual);
		}

		[Test]
		public void OrderByDescendingTest()
		{
			var actual = GetQuery1().OrderByDescending("Char").Join(", ");
			Assert.AreEqual("(C, 0), (B, 0), (A, 0)", actual);
		}

		[Test]
		public void OrderByThenByTest()
		{
			var actual = GetQuery2().OrderBy("Char").ThenBy("Value").Join(", ");
			Assert.AreEqual("(A, 0), (A, 1), (B, 0), (C, 0), (C, 1)", actual);
		}

		[Test]
		public void OrderByDescendingThenByTest()
		{
			var actual = GetQuery2().OrderByDescending("Char").ThenBy("Value").Join(", ");
			Assert.AreEqual("(C, 0), (C, 1), (B, 0), (A, 0), (A, 1)", actual);
		}

		[Test]
		public void OrderByThenByDescendingTest()
		{
			var actual = GetQuery2().OrderBy("Char").ThenByDescending("Value").Join(", ");
			Assert.AreEqual("(A, 1), (A, 0), (B, 0), (C, 1), (C, 0)", actual);
		}

		[Test]
		public void OrderByDescendingThenByDescendingTest()
		{
			var actual = GetQuery2().OrderByDescending("Char").ThenByDescending("Value").Join(", ");
			Assert.AreEqual("(C, 1), (C, 0), (B, 0), (A, 1), (A, 0)", actual);
		}

		private static IQueryable<Data> GetQuery1() =>
			new[] {new Data('C', 0), new Data('B', 0), new Data('A', 0)}.AsQueryable();

		private static IQueryable<Data> GetQuery2() =>
			new[] {new Data('C', 0), new Data('B', 0), new Data('A', 1), new Data('A', 0), new Data('C', 1)}.AsQueryable();

		#region Inner types

		[UsedImplicitly(ImplicitUseTargetFlags.Members)]
		private class Data
		{
			public char Char { get; }
			public int Value { get; }

			public Data(char ch, int value)
			{
				Char = ch;
				Value = value;
			}

			public override string ToString() => $"({Char}, {Value})";
		}

		#endregion
	}
}
