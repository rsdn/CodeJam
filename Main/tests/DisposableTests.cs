using System;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture]
	public static class DisposableTests
	{
		[Test]
		public static void TestDisposable()
		{
			var value = 0;
			var disposable = Disposable.Create(() => value++);

			Assert.That(value, Is.EqualTo(0));
			disposable.Dispose();
			Assert.That(value, Is.EqualTo(1));
			disposable.Dispose();
			Assert.That(value, Is.EqualTo(1));
		}

		[Test]
		public static void TestDisposableThrow()
		{
			var value = 0;
			var disposable = Disposable.Create(
				() =>
				{
					value++;
					if (value != 3)
						throw new InvalidOperationException();
				});

			Assert.That(value, Is.EqualTo(0));
			Assert.Throws<InvalidOperationException>(() => disposable.Dispose());
			Assert.That(value, Is.EqualTo(1));
			Assert.Throws<InvalidOperationException>(() => disposable.Dispose());
			Assert.That(value, Is.EqualTo(2));
			Assert.DoesNotThrow(() => disposable.Dispose());
			Assert.That(value, Is.EqualTo(3));
			Assert.DoesNotThrow(() => disposable.Dispose());
			Assert.That(value, Is.EqualTo(3));
		}
	}
}