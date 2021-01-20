using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace CodeJam.Threading
{
	[TestFixture]
	public class ParallelQueueTest
	{
		private static int _providerCount;
		private static int _consumerCount;

		[ThreadStatic]
		private static bool _providerInit;
		[ThreadStatic]
		private static bool _consumerInit;

		//[Ignore("Test seems to fail on a single core environment")]
		[Test]
		public void MultipleConsumerProviderTest()
		{
			_providerCount = 0;
			_consumerCount = 0;

			Enumerable.Range(1, 100).RunInParallel(
				5,
				i =>
				{
					if (!_providerInit)
					{
						_providerInit = true;
						Interlocked.Increment(ref _providerCount);
						Thread.Sleep(300);
					}

					return i.ToString();
				},
				2,
				s =>
				{
					if (!_consumerInit)
					{
						_consumerInit = true;
						Interlocked.Increment(ref _consumerCount);
						Thread.Sleep(300);
					}
				});

			Assert.That(Volatile.Read(ref _providerCount), Is.EqualTo(5));
			Assert.That(Volatile.Read(ref _consumerCount), Is.EqualTo(2));
		}

		private static int _actionCount;

		[ThreadStatic]
		private static bool _actionInit;

		[Test]
		public void RunInParallelTest()
		{
			_actionCount = 0;

			Enumerable.Range(1, 50)
				.Select(n => (Action)(() =>
				{
					if (!_actionInit)
					{
						_actionInit = true;
						Interlocked.Increment(ref _actionCount);
						Thread.Sleep(100);
					}
				}))
				.RunInParallel(5);

			Assert.That(Volatile.Read(ref _actionCount), Is.EqualTo(5));
		}

		//[Ignore("Test seems to fail on a single core environment")]
		[Test]
		public void RunInParallelActionTest()
		{
			_actionCount = 0;

			new Action[]
			{
				() => Console.WriteLine(1),
				() => Console.WriteLine("2"),
				() => Console.WriteLine("33"),
				() => Console.WriteLine(44),
				() => Console.WriteLine(5),
				() => Console.WriteLine("6"),
				() => Console.WriteLine("7"),
				() => Console.WriteLine("8")
			}
			.RunInParallel(
				5, a =>
				{
					if (!_actionInit)
					{
						_actionInit = true;
						Interlocked.Increment(ref _actionCount);
						Thread.Sleep(100);
					}

					a();
				});

			Assert.That(Volatile.Read(ref _actionCount), Is.EqualTo(5));
		}

		[Test]
		public void RunInParallelFuncTest()
		{
			_actionCount = 0;

			Enumerable.Range(1, 50).RunInParallel(
				5, n =>
				{
					if (!_actionInit)
					{
						_actionInit = true;
						Interlocked.Increment(ref _actionCount);
						Thread.Sleep(100);
					}
				});

			Assert.That(Volatile.Read(ref _actionCount), Is.EqualTo(5));
		}
	}
}
