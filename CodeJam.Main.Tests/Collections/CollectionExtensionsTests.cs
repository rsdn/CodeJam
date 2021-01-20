using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

using CodeJam.Strings;

using NUnit.Framework;

namespace CodeJam.Collections
{
	public class CollectionExtensionsTests
	{
		[Test]
		public void TestNullOrEmpty()
		{
			var arr = Array<int>.Empty;
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
		public void TestEmptyIfNull()
		{
			var arr = Array<int>.Empty;
			var list = new List<int>();
			var dic = new Dictionary<int, string>();
			var str = "";

			Assert.IsNull(arr.NullIfEmpty());
			NAssert.NotNull(arr.EmptyIfNull());
			Assert.IsNull(dic.NullIfEmpty());
			NAssert.NotNull(dic.EmptyIfNull());
			Assert.IsNull(list.NullIfEmpty());
			NAssert.NotNull(list.EmptyIfNull());
			Assert.IsNull(str.NullIfEmpty());
			NAssert.NotNull(str.EmptyIfNull());

			arr = new int[2];
			list.Add(3);
			dic.Add(1, "A");
			str = "B";

			NAssert.NotNull(arr.NullIfEmpty());
			NAssert.NotNull(arr.EmptyIfNull());
			NAssert.NotNull(dic.NullIfEmpty());
			NAssert.NotNull(dic.EmptyIfNull());
			NAssert.NotNull(list.NullIfEmpty());
			NAssert.NotNull(list.EmptyIfNull());
			NAssert.NotNull(str.NullIfEmpty());
			NAssert.NotNull(str.EmptyIfNull());

			arr = null;
			list = null;
			dic = null;
			str = null;

			Assert.IsNull(arr.NullIfEmpty());
			NAssert.NotNull(arr.EmptyIfNull());
			Assert.IsNull(dic.NullIfEmpty());
			NAssert.NotNull(dic.EmptyIfNull());
			Assert.IsNull(list.NullIfEmpty());
			NAssert.NotNull(list.EmptyIfNull());
			Assert.IsNull(str.NullIfEmpty());
			NAssert.NotNull(str.EmptyIfNull());
		}

		[Test]
		[SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
		[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
		public void TestDefaultIfEmpty()
		{
			var arr = Array<int>.Empty;
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

			Assert.Throws<ArgumentNullException>(() => arr!.DefaultIfEmpty(123).First());
			Assert.Throws<ArgumentNullException>(() => list!.DefaultIfEmpty(123).First());
			Assert.Throws<ArgumentNullException>(() => dic!.DefaultIfEmpty(123, "a").First().Key.ToString(CultureInfo.InvariantCulture));
			Assert.Throws<ArgumentNullException>(() => enumerable!.DefaultIfEmpty(123).First());
		}
	}
}