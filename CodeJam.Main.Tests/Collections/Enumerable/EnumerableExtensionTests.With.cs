﻿using System;
using System.Linq;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public partial class EnumerableExtensionTests
	{
		[Test]
		public void IndexTest()
		{
			for (var n = 1; n < 10; n++)
			{
				var list = Enumerable.Range(0, n).WithIndex().ToArray();
				foreach (var (index, item) in list)
					Assert.AreEqual(item, index, "#Index");

				Assert.IsTrue(list[0].IsFirst, "#IsFirst");
				Assert.IsTrue(list.Last().IsLast, "#IsLast");
			}
		}

		[Test]
		public void WithPreviousNextTest()
		{
			for (var n = 0; n < 10; n++)
			{
				var list = Enumerable.Range(0, n).ToArray();

				Assert.AreEqual(
					list.CombineWithPrevious((prev, next) => prev + next),
					list.Zip(list.Skip(1), (prev, next) => prev + next));

				Assert.AreEqual(
					list.CombineWithPrevious(-1, (prev, next) => prev + next),
					list.Zip(list.Prepend(-1), (prev, next) => prev + next));


				Assert.AreEqual(
					list.CombineWithNext(n, (prev, next) => prev + next),
					list.Zip(list.Skip(1).Concat(n), (prev, next) => prev + next));
			}
		}
	}
}
