using System;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture(Category="Disposable")]
	public static class DisposableTests
	{
		[Test]
		public static void TestDisposable()
		{
			var value = 0;
			var disposable = Disposable.Create(() => value++);

			// Fails if  Disposable.Create returns struct
			var disposable2 = disposable;

			Assert.That(value, Is.EqualTo(0));
			disposable.Dispose();
			Assert.That(value, Is.EqualTo(1));
			disposable.Dispose();
			Assert.That(value, Is.EqualTo(1));

			disposable2.Dispose();
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

		[Test]
		public static void TestDisposableMerge()
		{
			var value1 = 0;
			var disposable1 = Disposable.Create(() => value1++);
			var value2 = 0;
			var disposable2 = Disposable.Create(
				() =>
				{
					value1++;
					value2++;
				});

			var value3 = 0;
			var disposable3 = Disposable.Create(
				() =>
				{
					value3++;
					if (value3 != 3)
						throw new InvalidOperationException();
				});

			var disposable = Disposable.Merge(disposable1, disposable2, disposable3);

			Assert.That(value1, Is.EqualTo(0));
			Assert.That(value2, Is.EqualTo(0));
			Assert.That(value3, Is.EqualTo(0));
			Assert.Throws<AggregateException>(() => disposable.Dispose());
			Assert.That(value1, Is.EqualTo(2));
			Assert.That(value2, Is.EqualTo(1));
			Assert.That(value3, Is.EqualTo(1));
			Assert.Throws<AggregateException>(() => disposable.Dispose());
			Assert.That(value1, Is.EqualTo(2));
			Assert.That(value2, Is.EqualTo(1));
			Assert.That(value3, Is.EqualTo(2));
			Assert.DoesNotThrow(() => disposable.Dispose());
			Assert.That(value1, Is.EqualTo(2));
			Assert.That(value2, Is.EqualTo(1));
			Assert.That(value3, Is.EqualTo(3));
			Assert.DoesNotThrow(() => disposable.Dispose());
			Assert.That(value1, Is.EqualTo(2));
			Assert.That(value2, Is.EqualTo(1));
			Assert.That(value3, Is.EqualTo(3));
		}
	}
}