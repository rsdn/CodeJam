using NUnit.Framework;

namespace CodeJam.Structures
{
	[TestFixture]
	public class OptionTests
	{
		[Test]
		public void SomeTest()
		{
			var some = Option.Some(1);
			Assert.IsTrue(some.HasValue);
			Assert.AreEqual(some.Value, 1);
			Assert.AreEqual(some.GetValueOrDefault(2), 1);
			Assert.AreEqual(some.GetValueOrDefault(value => 10, () => 20), 10);
		}

		[Test]
		public void NoneTest()
		{
			var none = Option.None<int>();
			Assert.IsFalse(none.HasValue);
			Assert.AreEqual(none.GetValueOrDefault(2), 2);
			Assert.AreEqual(none.GetValueOrDefault(value => 10, () => 20), 20);
		}
	}
}
