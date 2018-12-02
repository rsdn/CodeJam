using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	public class CollectionExtensionsTests
	{
		[Test]
		public static void TestNullOrEmpty()
		{
			var arr = new int[0];
			var list = new List<int>();
			var dic = new Dictionary<int, string>();
			var str = "";

			Assert.IsTrue(arr.IsNullOrEmpty());
			Assert.IsFalse(arr.NotNullNorEmpty());
			Assert.IsTrue(dic.IsNullOrEmpty());
			Assert.IsFalse(dic.NotNullNorEmpty());
			Assert.IsTrue(list.IsNullOrEmpty());
			Assert.IsFalse(list.NotNullNorEmpty());
			Assert.IsTrue(str.IsNullOrEmpty());
			Assert.IsFalse(str.NotNullNorEmpty());

			arr = new int[2];
			list.Add(3);
			dic.Add(1, "A");
			str = "B";

			Assert.IsFalse(arr.IsNullOrEmpty());
			Assert.IsTrue(arr.NotNullNorEmpty());
			Assert.IsFalse(dic.IsNullOrEmpty());
			Assert.IsTrue(dic.NotNullNorEmpty());
			Assert.IsFalse(list.IsNullOrEmpty());
			Assert.IsTrue(list.NotNullNorEmpty());
			Assert.IsFalse(str.IsNullOrEmpty());
			Assert.IsTrue(str.NotNullNorEmpty());

			arr = null;
			list = null;
			dic = null;
			str = null;

			Assert.IsTrue(arr.IsNullOrEmpty());
			Assert.IsFalse(arr.NotNullNorEmpty());
			Assert.IsTrue(dic.IsNullOrEmpty());
			Assert.IsFalse(dic.NotNullNorEmpty());
			Assert.IsTrue(list.IsNullOrEmpty());
			Assert.IsFalse(list.NotNullNorEmpty());
			Assert.IsTrue(str.IsNullOrEmpty());
			Assert.IsFalse(str.NotNullNorEmpty());
		}

		[Test]
		public static void TestEmptyIfNull()
		{
			var arr = new int[0];
			var list = new List<int>();
			var dic = new Dictionary<int, string>();
			var str = "";

			Assert.IsNull(arr.NullIfEmpty());
			Assert.NotNull(arr.EmptyIfNull());
			Assert.IsNull(dic.NullIfEmpty());
			Assert.NotNull(dic.EmptyIfNull());
			Assert.IsNull(list.NullIfEmpty());
			Assert.NotNull(list.EmptyIfNull());
			Assert.IsNull(str.NullIfEmpty());
			Assert.NotNull(str.EmptyIfNull());

			arr = new int[2];
			list.Add(3);
			dic.Add(1, "A");
			str = "B";

			Assert.NotNull(arr.NullIfEmpty());
			Assert.NotNull(arr.EmptyIfNull());
			Assert.NotNull(dic.NullIfEmpty());
			Assert.NotNull(dic.EmptyIfNull());
			Assert.NotNull(list.NullIfEmpty());
			Assert.NotNull(list.EmptyIfNull());
			Assert.NotNull(str.NullIfEmpty());
			Assert.NotNull(str.EmptyIfNull());

			arr = null;
			list = null;
			dic = null;
			str = null;

			Assert.IsNull(arr.NullIfEmpty());
			Assert.NotNull(arr.EmptyIfNull());
			Assert.IsNull(dic.NullIfEmpty());
			Assert.NotNull(dic.EmptyIfNull());
			Assert.IsNull(list.NullIfEmpty());
			Assert.NotNull(list.EmptyIfNull());
			Assert.IsNull(str.NullIfEmpty());
			Assert.NotNull(str.EmptyIfNull());
		}

		[Test]
		[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
		[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
		public static void TestDefaultIfEmpty()
		{
			var arr = new int[0];
			var list = new List<int>();
			var dic = new Dictionary<int, string>();
			var enumerable = list.AsEnumerable();

			Assert.AreEqual(123, arr.DefaultIfEmpty(123).First());
			Assert.AreEqual(123, list.DefaultIfEmpty(123).First());
			Assert.AreEqual(123, dic.DefaultIfEmpty(123, "a").First().Key);
			Assert.AreEqual(123, enumerable.DefaultIfEmpty(123).First());

			arr = new int[2];
			list.Add(3);
			dic.Add(1, "A");
			enumerable = list.AsEnumerable();

			Assert.AreEqual(0, arr.DefaultIfEmpty(123).First());
			Assert.AreEqual(3, list.DefaultIfEmpty(123).First());
			Assert.AreEqual(1, dic.DefaultIfEmpty(123, "a").First().Key);
			Assert.AreEqual(3, enumerable.DefaultIfEmpty(123).First());

			arr = null;
			list = null;
			dic = null;
			enumerable = null;

			Assert.Throws<ArgumentNullException>(() => arr.DefaultIfEmpty(123).First());
			Assert.Throws<ArgumentNullException>(() => list.DefaultIfEmpty(123).First());
			Assert.Throws<ArgumentNullException>(() => dic.DefaultIfEmpty(123, "a").First().Key.ToString());
			Assert.Throws<ArgumentNullException>(() => enumerable.DefaultIfEmpty(123).First());
		}
	}
}