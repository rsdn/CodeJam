#if LESSTHAN_NET35
using System;
using System.Threading;

namespace CodeJam.Targeting.NET20
{
	internal class MonitorEx
	{
		internal static void Enter(object obj, ref bool taken)
		{
			Monitor.Enter(obj);
			taken = true;
		}

		internal static bool TryEnter(object obj)
		{
			return Monitor.TryEnter(obj);
		}

		internal static void TryEnter(object obj, ref bool taken)
		{
			taken = Monitor.TryEnter(obj);
		}

		internal static bool TryEnter(object obj, int millisecondsTimeout)
		{
			return Monitor.TryEnter(obj, millisecondsTimeout);
		}

		internal static bool TryEnter(object obj, TimeSpan timeout)
		{
			return Monitor.TryEnter(obj, timeout);
		}

		internal static void TryEnter(object obj, int millisecondsTimeout, ref bool taken)
		{
			taken = Monitor.TryEnter(obj, millisecondsTimeout);
		}
	}
}
#endif