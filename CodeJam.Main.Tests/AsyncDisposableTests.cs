#if NETCOREAPP30_OR_GREATER
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace CodeJam
{
	[TestFixture(Category = "AsyncDisposable")]
	[SuppressMessage("ReSharper", "HeapView.CanAvoidClosure")]
	public static class AsyncDisposableTests
	{
		[Test]
		public static void TestAsyncDisposable()
		{
			var value = 0;
			var disposable = AsyncDisposable.Create(
				() =>
				{
					value++;
					return default;
				});
			// Fails if  AsyncDisposable.Create returns struct
			var disposable2 = disposable;

			Assert.That(value, Is.EqualTo(0));

			Assert.DoesNotThrowAsync(async () => await disposable.DisposeAsync());
			Assert.That(value, Is.EqualTo(1));

			Assert.DoesNotThrowAsync(async () => await disposable.DisposeAsync());
			Assert.That(value, Is.EqualTo(1));

			Assert.DoesNotThrowAsync(async () => await disposable2.DisposeAsync());
			Assert.That(value, Is.EqualTo(1));
		}

		[Test]
		public static void TestAsyncDisposableThrow()
		{
			var value = 0;
			var disposable = AsyncDisposable.Create(
				() =>
				{
					value++;
					if (value != 3)
						throw new InvalidOperationException();
					return default;
				});
			Assert.That(value, Is.EqualTo(0));

			Assert.ThrowsAsync<InvalidOperationException>(async () => await disposable.DisposeAsync());
			Assert.That(value, Is.EqualTo(1));

			Assert.ThrowsAsync<InvalidOperationException>(async () => await disposable.DisposeAsync());
			Assert.That(value, Is.EqualTo(2));

			Assert.DoesNotThrowAsync(async () => await disposable.DisposeAsync());
			Assert.That(value, Is.EqualTo(3));

			Assert.DoesNotThrowAsync(async () => await disposable.DisposeAsync());
			Assert.That(value, Is.EqualTo(3));
		}

		[Test]
		public static void TestAsyncDisposableMerge()
		{
			var value1 = 0;
			var disposable1 = AsyncDisposable.Create(
				() =>
				{
					value1++;
					return default;
				});
			var value2 = 0;
			var disposable2 = AsyncDisposable.Create(
				() =>
				{
					value1++;
					value2++;
					return default;
				});

			var value3 = 0;
			var disposable3 = AsyncDisposable.Create(
				() =>
				{
					value3++;
					if (value3 != 3)
						throw new InvalidOperationException("Test message");
					return default;
				});

			var disposable = AsyncDisposable.Merge(disposable1, disposable2, disposable3);
			Assert.That(value1, Is.EqualTo(0));
			Assert.That(value2, Is.EqualTo(0));
			Assert.That(value3, Is.EqualTo(0));

			var ex = Assert.ThrowsAsync<AggregateException>(async () => await disposable.DisposeAsync());
			Assert.That(ex.InnerExceptions.Count, Is.EqualTo(1));
			Assert.That(
				ex.InnerExceptions.Cast<InvalidOperationException>().Single().Message,
				Is.EqualTo("Test message"));
			Assert.That(value1, Is.EqualTo(2));
			Assert.That(value2, Is.EqualTo(1));
			Assert.That(value3, Is.EqualTo(1));

			Assert.ThrowsAsync<AggregateException>(async () => await disposable.DisposeAsync());
			Assert.That(value1, Is.EqualTo(2));
			Assert.That(value2, Is.EqualTo(1));
			Assert.That(value3, Is.EqualTo(2));

			Assert.DoesNotThrowAsync(async () => await disposable.DisposeAsync());
			Assert.That(value1, Is.EqualTo(2));
			Assert.That(value2, Is.EqualTo(1));
			Assert.That(value3, Is.EqualTo(3));

			Assert.DoesNotThrowAsync(async () => await disposable.DisposeAsync());
			Assert.That(value1, Is.EqualTo(2));
			Assert.That(value2, Is.EqualTo(1));
			Assert.That(value3, Is.EqualTo(3));
		}

		[Test]
		public static async Task TestParameterizedAnonymousAsyncDisposable()
		{
			var state = "";
			var disposed = false;

			await using (AsyncDisposable.Create(
				s =>
				{
					disposed = true;
					state = s;
					return default;
				}, "state")) { }

			Assert.IsTrue(disposed);
			Assert.AreEqual("state", state);
		}

		[Test]
		public static async Task AsyncInitDisposeTest1()
		{
			var i = 0;

			await using (AsyncInitDispose.Create(
				() =>
				{
					Assert.That(++i, Is.EqualTo(1));
					return default;
				},
				() =>
				{
					Assert.That(++i, Is.EqualTo(3));
					return default;
				}))
			{
				Assert.That(++i, Is.EqualTo(2));
			}

			Assert.That(++i, Is.EqualTo(4));
		}

		[Test]
		public static async Task AsyncInitDisposeTest2()
		{
			var i = 0;

			await using (AsyncInitDispose.Create(
				() =>
				{
					Assert.That(++i, Is.EqualTo(1).Or.EqualTo(3));
					return default;
				}))
			{
				Assert.That(++i, Is.EqualTo(2));
			}

			Assert.That(++i, Is.EqualTo(4));
		}

		[Test]
		public static async Task AsyncInitDisposeTest3()
		{
			var i = 0;

			await using (AsyncInitDispose.Create(
				isInit =>
				{
					Assert.That(i += isInit ? 1 : 2, Is.EqualTo(1).Or.EqualTo(4));
					return default;
				}))
			{
				Assert.That(++i, Is.EqualTo(2));
			}

			Assert.That(++i, Is.EqualTo(5));
		}

		[Test]
		public static async Task AsyncInitDisposeTest4()
		{
			var i = 0;

			await using (AsyncInitDispose.Create(
				() =>
				{
					Assert.That(++i, Is.EqualTo(1));
					return "123";
				},
				s =>
				{
					Assert.That(++i, Is.EqualTo(3));
					Assert.That(s, Is.EqualTo("123"));
					return default;
				}))
			{
				Assert.That(++i, Is.EqualTo(2));
			}

			Assert.That(++i, Is.EqualTo(4));
		}
	}
}
#endif