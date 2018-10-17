#if LESSTHAN_NET35
using System;
using System.Threading;
using JetBrains.Annotations;

namespace CodeJam.NET20
{
	internal static class MonitorEx
	{
		[PublicAPI]
		// ReSharper disable once RedundantAssignment
		internal static void Enter(object obj, ref bool taken)
		{
			Monitor.Enter(obj);
			taken = true;
		}

		[PublicAPI]
		internal static bool TryEnter(object obj) => Monitor.TryEnter(obj);

		[PublicAPI]
		// ReSharper disable once RedundantAssignment
		internal static void TryEnter(object obj, ref bool taken)
		{
			taken = Monitor.TryEnter(obj);
		}

		[PublicAPI]
		internal static bool TryEnter(object obj, int millisecondsTimeout) => Monitor.TryEnter(obj, millisecondsTimeout);

		[PublicAPI]
		internal static bool TryEnter(object obj, TimeSpan timeout) => Monitor.TryEnter(obj, timeout);

		[PublicAPI]
		// ReSharper disable once RedundantAssignment
		internal static void TryEnter(object obj, int millisecondsTimeout, ref bool taken)
		{
			taken = Monitor.TryEnter(obj, millisecondsTimeout);
		}
	}
}
#endif