using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using NUnit.Framework;
// ReSharper disable AccessToModifiedClosure

namespace CodeJam
{
	[TestFixture(Category = "Disposable")]
	[SuppressMessage("ReSharper", "HeapView.CanAvoidClosure")]
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
			using var disposable = Disposable.Create(
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
						throw new InvalidOperationException("Test message");
				});

			var disposable = Disposable.Merge(disposable1, disposable2, disposable3);
			Assert.That(value1, Is.EqualTo(0));
			Assert.That(value2, Is.EqualTo(0));
			Assert.That(value3, Is.EqualTo(0));

			var ex = Assert.Throws<AggregateException>(() => disposable.Dispose());
			Assert.That(ex.InnerExceptions.Count, Is.EqualTo(1));
			Assert.That(
				ex.InnerExceptions.Cast<InvalidOperationException>().Single().Message,
				Is.EqualTo("Test message"));
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

		[Test]
		public static void TestParameterizedAnonymousDisposable()
		{
			var state = "";
			var disposed = false;

			using (Disposable.Create(s => { disposed = true; state = s; }, "state"))
			{
			}

			Assert.IsTrue(disposed);
			Assert.AreEqual("state", state);
		}

		[Test]
		public static void InitDisposeTest1()
		{
			var i = 0;

			using (InitDispose.Create(
				() => Assert.That(++i, Is.EqualTo(1)),
				() => Assert.That(++i, Is.EqualTo(3))))
			{
				Assert.That(++i, Is.EqualTo(2));
			}

			Assert.That(++i, Is.EqualTo(4));
		}

		[Test]
		public static void InitDisposeTest2()
		{
			var i = 0;

			using (InitDispose.Create(
				() => Assert.That(++i, Is.EqualTo(1).Or.EqualTo(3))))
			{
				Assert.That(++i, Is.EqualTo(2));
			}

			Assert.That(++i, Is.EqualTo(4));
		}

		[Test]
		public static void InitDisposeTest3()
		{
			var i = 0;

			using (InitDispose.Create(
				isInit => Assert.That(i += isInit ? 1 : 2, Is.EqualTo(1).Or.EqualTo(4))))
			{
				Assert.That(++i, Is.EqualTo(2));
			}

			Assert.That(++i, Is.EqualTo(5));
		}

		[Test]
		public static void InitDisposeTest4()
		{
			var i = 0;

			using (InitDispose.Create(
				() =>
				{
					Assert.That(++i, Is.EqualTo(1));
					return "123";
				},
				s =>
				{
					Assert.That(++i, Is.EqualTo(3));
					Assert.That(s, Is.EqualTo("123"));
				}))
			{
				Assert.That(++i, Is.EqualTo(2));
			}

			Assert.That(++i, Is.EqualTo(4));
		}

		[Test]
		public static void CustomDisposeMustProcessSingleEntity()
		{
			const int expectedInitializeCount = 1;
			const int expectedUseCount = 1;
			const int expectedDestroyCount = 1;

			int actualInitializeCount = 0;
			int actualUseCount = 0;
			int actualDisposeCount = 0;

			var rawTestObject = new SomeTestableEntity(
				() => ++actualInitializeCount,
				() => ++actualUseCount,
				() => ++actualDisposeCount);

			var wrappedTestObject = Disposable.Create(
				() => { rawTestObject.Initialize(); return rawTestObject; },
				innerTestableEntity => innerTestableEntity.Destroy());

			Assert.DoesNotThrow(wrappedTestObject.Entity.Use);
			Assert.DoesNotThrow(wrappedTestObject.Dispose);
			Assert.Throws<ObjectDisposedException>(() => wrappedTestObject.Entity.Use());

			Assert.AreEqual(expectedInitializeCount, actualInitializeCount);
			Assert.AreEqual(expectedUseCount, actualUseCount);
			Assert.AreEqual(expectedDestroyCount, actualDisposeCount);
		}

		[Test]
		public static void CustomDisposeMustProcessMultipleEntities()
		{
			const int expectedEntitiesCount = 10;
			const int expectedInitializeCount = 10;
			const int expectedDestroyCount = 10;

			int actualEntitiesCount = 0;
			int actualInitializeCount = 0;
			int actualDisposeCount = 0;

			var create = () => new SomeTestableEntity(
				() => ++actualInitializeCount,
				() => { },
				() => ++actualDisposeCount);

			var wrappedTestObject = Disposable.Create(
				() => Enumerable.Range(0, expectedEntitiesCount),
				index => { var rawTestObject = create(); rawTestObject.Initialize(); return rawTestObject; },
				innerTestableEntity => innerTestableEntity.Destroy());

			Assert.DoesNotThrow(() => actualEntitiesCount = wrappedTestObject.Entities.Count());
			Assert.DoesNotThrow(wrappedTestObject.Dispose);
			Assert.Throws<ObjectDisposedException>(() => actualEntitiesCount = wrappedTestObject.Entities.Count());

			Assert.AreEqual(expectedEntitiesCount, actualEntitiesCount);
			Assert.AreEqual(expectedInitializeCount, actualInitializeCount);
			Assert.AreEqual(expectedDestroyCount, actualDisposeCount);
		}

		private class SomeTestableEntity
		{
			private readonly Action _initialize;
			private readonly Action _use;
			private readonly Action _destroy;

			public SomeTestableEntity(Action initialize, Action use, Action destroy)
			{
				_initialize = initialize;
				_use = use;
				_destroy = destroy;
			}

			public void Initialize() => _initialize();
			public void Use() => _use();
			public void Destroy() => _destroy();
		}
	}
}