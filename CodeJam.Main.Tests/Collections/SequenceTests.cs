using System;
using System.Linq;

using NUnit.Framework;

namespace CodeJam.Collections
{
	[TestFixture]
	public class SequenceTests
	{
		[Test]
		public void CreateNullable()
		{
			var res = Sequence.Create((int?)1, x => x == null ? 0 : x + 1).Take(2);
			CollectionAssert.AreEquivalent(new[]{ 1, 2 }, res);

			var res2 = Sequence.Create((object?)1, x => x == null ? 0 : (int)x + 1).Take(2);
			CollectionAssert.AreEquivalent(new[] { 1, 2 }, res2);
		}

		[Test]
		public void CreateNonNullable()
		{
			var res = Sequence.Create("a", x =>  x + "1").Take(2);
			CollectionAssert.AreEquivalent(new[] { "a", "a1" }, res);

			var res2 = Sequence.Create((object)"a", x => x + "1").Take(2);
			CollectionAssert.AreEquivalent(new[] { "a", "a1" }, res2);
		}

		[Test]
		public void CreateNullableResultSelector()
		{
			var res = Sequence.Create(
				(int?)1,
				x => x == null ? 0 : x + 1,
				x => x!.Value
				).Take(2);
			CollectionAssert.AreEquivalent(new[] { 1, 2 }, res);

			var res2 = Sequence.Create(
				(object?)1,
				x => x == null ? 0 : (int)x + 1,
				x => x!
				).Take(2);
			CollectionAssert.AreEquivalent(new[] { 1, 2 }, res2);
		}

		[Test]
		public void CreateNonNullableResultSelector()
		{
			var res = Sequence.Create(
				"a",
				x => x + "1",
				x => x.ToString()
				).Take(2);
			CollectionAssert.AreEquivalent(new[] { "a", "a1" }, res);

			var res2 = Sequence.Create(
				(object)"a",
				x => x + "1",
				x => x.ToString()
				).Take(2);
			CollectionAssert.AreEquivalent(new[] { "a", "a1" }, res2);
		}

		[Test]
		public void CreateSingle()
		{
			CollectionAssert.AreEquivalent(
				new object?[] { null },
				Sequence.CreateSingle((object?)null));

			CollectionAssert.AreEquivalent(
				new[] { 1 },
				Sequence.CreateSingle(1));

			CollectionAssert.AreEquivalent(
				new object?[] { null },
				Sequence.CreateSingle(() => (object?)null));

			CollectionAssert.AreEquivalent(
				new[] { 1 },
				Sequence.CreateSingle(() => 1));
		}

		[Test]
		public void CreateWhileNotNull()
		{
			var typeofObject = new[] { typeof(string), typeof(object) };
			CollectionAssert.AreEquivalent(typeofObject, Sequence.CreateWhileNotNull(typeof(string), t => t.BaseType));
			CollectionAssert.AreEquivalent(typeofObject, Sequence.CreateWhileNotNull((Type?)typeof(string), t => t?.BaseType));

			// ReSharper disable once UseNameOfInsteadOfTypeOf
			var objectName = new[] { typeof(string).Name, typeof(object).Name };
			CollectionAssert.AreEquivalent(objectName, Sequence.CreateWhileNotNull(typeof(string), t => t.BaseType, t => t.Name  ));
			CollectionAssert.AreEquivalent(objectName, Sequence.CreateWhileNotNull((Type?)typeof(string), t => t?.BaseType, t => t.Name));

		}
	}
}
