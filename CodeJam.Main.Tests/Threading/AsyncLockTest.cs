using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

using NUnit.Framework;

#if LESSTHAN_NET45
using Theraot.Core;
#endif

#if NET40
using TaskEx = System.Threading.Tasks.TaskEx;
#else
using TaskEx = System.Threading.Tasks.Task;
#endif

namespace CodeJam.Threading
{
	[TestFixture]
	public class AsyncLockTest
	{
		private static async Task<bool> TryTakeAndHold(
			[NotNull] AsyncLock asyncLock, TimeSpan holdTime, CancellationToken cancellation = default(CancellationToken), Action callback = null)
		{
			try
			{
				using (await asyncLock.AcquireAsync(holdTime, cancellation))
				{
					callback?.Invoke();
					await TaskEx.Delay(holdTime);
				}
				return true;
			}
			catch (OperationCanceledException)
			{
				return false;
			}
			catch (TimeoutException)
			{
				return false;
			}
		}

#if !LESSTHAN_NET40
		[Test]
#endif
		[SuppressMessage("ReSharper", "MethodSupportsCancellation")]
		public async Task LockCancellationTest()
		{
			var asyncLock = new AsyncLock();
			var holdTime = TimeSpan.FromSeconds(2);
			var delayTime = TimeSpan.FromMilliseconds(200);

			var lock1Started = new ManualResetEventSlim(false);

			var lock1 = TryTakeAndHold(asyncLock, holdTime, callback: () => lock1Started.Set());
			lock1Started.Wait();

			var cts2 = new CancellationTokenSource();
			var sw2 = Stopwatch.StartNew();
			var lock2 = TryTakeAndHold(asyncLock, holdTime, cts2.Token);
			await TaskEx.Delay(delayTime);
			cts2.Cancel();
			var lock2Taken = await lock2;
			sw2.Stop();

			var sw3 = Stopwatch.StartNew();
			var lock3 = TryTakeAndHold(asyncLock, delayTime);
			await TaskEx.Delay(delayTime);
			var lock3Taken = await lock3;
			sw3.Stop();

			var lock1Taken = await lock1;

			Assert.IsTrue(lock1Taken);
			Assert.IsFalse(lock2Taken);
			Assert.Less(sw2.Elapsed, holdTime - delayTime);
			Assert.IsFalse(lock3Taken);
			Assert.Less(sw3.Elapsed, holdTime - delayTime);
		}

#if LESSTHAN_NET40
		[Test]
		public void LockCancellationTestDotNetLessThan40()
		{
			LockCancellationTest().Wait();
			Assert.IsTrue(true);
		}
#endif


#if !LESSTHAN_NET40
		[Test]
#endif
		public async Task LockTest()
		{
			var asyncLock = new AsyncLock();

			var opActive = false;
			const int time = 200;
			const int timeInc = 10;
			const int count = 10;

			async Task Op(int num)
			{
				using (await asyncLock.AcquireAsync())
				{
					Assert.IsFalse(opActive);
					opActive = true;
					await TaskEx.Delay(200 + num * timeInc);
					Assert.IsTrue(opActive);
					opActive = false;
				}
			}

			var sw = Stopwatch.StartNew();
			await Enumerable
				.Range(0, 10)
				.Select(i => TaskEx.Run(() => Op(i)))
				.WhenAll();
			sw.Stop();
			Assert.IsFalse(opActive);
			Assert.GreaterOrEqual(sw.ElapsedMilliseconds, time * count + timeInc * count / 2);
		}

#if LESSTHAN_NET40
		[Test]
		public void LockTestDotNetLessThan40()
		{
			LockTest().Wait();
			Assert.IsTrue(true);
		}
#endif
	}
}
