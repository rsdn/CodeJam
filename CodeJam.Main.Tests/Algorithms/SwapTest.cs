
using NUnit.Framework;

namespace CodeJam
{
	[TestFixture]
	public class SwapTest
	{
		[Test]
		public void TestValueTypeSwap()
		{
			var value1 = 10;
			var value2 = 20;
			Algorithms.Swap(ref value1, ref value2);
			Assert.That(value1, Is.EqualTo(20));
			Assert.That(value2, Is.EqualTo(10));
		}

		[Test]
		public void TestRefTypeSwap1()
		{
			var value1 = new object();
			var value2 = new object();
			var value1Expected = value2;
			var value2Expected = value1;
			Algorithms.Swap(ref value1, ref value2);
			Assert.That(value1, Is.EqualTo(value1Expected));
			Assert.That(value2, Is.EqualTo(value2Expected));
		}

		[Test]
		public void TestRefTypeSwap2()
		{
			// test self swap
			var value1 = new object();
			var value2 = value1;
			var valueExpected = value1;
			Algorithms.Swap(ref value1, ref value2);
			Assert.That(value1, Is.EqualTo(valueExpected));
			Assert.That(value2, Is.EqualTo(valueExpected));
		}
	}
}