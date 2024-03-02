using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace CodeJam;

[TestFixture(Category = "Disposable")]
[SuppressMessage("ReSharper", "HeapView.CanAvoidClosure")]
public static class DisposableExtensionsTests
{
	[Test]
	public static void DisposeAllMustReleaseAllObjects()
	{
		const int expectedDisposeCount = 10;

		int actualDisposeCount = 0;

		var objectsForDispose = Enumerable
			.Range(0, expectedDisposeCount)
			.Select(x => Disposable.Create(() => ++actualDisposeCount));

		objectsForDispose.DisposeAll();

		Assert.AreEqual(expectedDisposeCount, actualDisposeCount);
	}

	[Test]
	public static void DisposeAllMustCollectAllExceptions()
	{
		const int expectedExceptionCount = 7;

		var objectsWithException = Enumerable
			.Range(0, expectedExceptionCount)
			.Select(x => Disposable.Create(() => throw new Exception()));

		const int expectedSuccessCount = 3;

		var objectsWithoutException = Enumerable
			.Range(0, expectedSuccessCount)
			.Select(x => Disposable.Create(() => { }));

		var objectsForDispose = objectsWithException.Concat(objectsWithoutException).ToArray();

		int actualExceptionCount = -1;

		try
		{
			objectsForDispose.DisposeAll();
		}
		catch (AggregateException ex)
		{
			actualExceptionCount = ex.InnerExceptions.Count;
		}

		Assert.AreEqual(expectedExceptionCount, actualExceptionCount);
	}

#if NETSTANDARD21_OR_GREATER || NETCOREAPP30_OR_GREATER
	[Test]
	public static void DisposeAsyncMustNotBlockThread()
	{
		var disposeDuration = new TimeSpan(0, 0, 1);

		var longDisposableObject = Disposable.Create(() => Thread.Sleep(disposeDuration));

		var startTime = DateTime.Now;

		var task = longDisposableObject.DisposeAsync();

		var callDuration = DateTime.Now - startTime;

		Assert.Less(callDuration, disposeDuration);
	}
#endif
}