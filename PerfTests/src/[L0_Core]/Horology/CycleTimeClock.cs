// BASEDON:https://github.com/Azure/DotNetty/blob/dev/test/DotNetty.Microbench/Utilities/CycleTime.cs
using System;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

using JetBrains.Annotations;

using Microsoft.Win32.SafeHandles;

// ReSharper disable once CheckNamespace
namespace BenchmarkDotNet.Horology
{
	internal static class CycleClockHelpers
	{
		#region Interop
		[DllImport("Kernel32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool QueryThreadCycleTime(IntPtr threadHandle, out ulong cycleTime);

		[DllImport("Kernel32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool QueryProcessCycleTime(SafeProcessHandle processHandle, out ulong cycleTime);
		#endregion

		private static bool IsWindows()
		{
#if !CORE
			return new[] { PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows, PlatformID.WinCE }
				.Contains(Environment.OSVersion.Platform);
#else
			return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
		}

		public static readonly SafeProcessHandle ProcessHandle = Process.GetCurrentProcess().SafeHandle;
		public static readonly IntPtr CurrentThreadHandle = new IntPtr(-2);

		public static long GetProcessTimestamp(SafeProcessHandle processHandle)
		{
			ulong result;
			if (!QueryProcessCycleTime(processHandle, out result))
				throw new Win32Exception();

			return checked((long)result);
		}

		public static long GetThreadTimestamp(IntPtr threadHandle)
		{
			ulong result;
			if (!QueryThreadCycleTime(threadHandle, out result))
				throw new Win32Exception();

			return checked((long)result);
		}

		private static bool RunHighestPriority([InstantHandle] Action callback)
		{ 
			if (!IsWindows())
			{
				return false;
			}

			var process = Process.GetCurrentProcess();
			var thread = Thread.CurrentThread;

			var oldProcessPriority = process.PriorityClass;
			var oldThreadPriority = thread.Priority;
			try
			{
				thread.Priority = ThreadPriority.Highest;
				process.PriorityClass = ProcessPriorityClass.RealTime;

				callback();
				return true;
			}
			catch (Win32Exception)
			{
				return false;
			}
			finally
			{
				thread.Priority = oldThreadPriority;
				process.PriorityClass = oldProcessPriority;
			}
		}

		// WARNING: rough estimate only.
		// see https://blogs.msdn.microsoft.com/oldnewthing/20160429-00/?p=93385 for more.
		public static bool GetProcessFrequency(IClock clock, int iterationsCount, out long frequency)
		{
			var freq = default(long);
			var result = RunHighestPriority(
				() =>
				{
					var local = ProcessHandle;

					var t = clock.Start();
					var ts1 = GetProcessTimestamp(local);
					Thread.SpinWait(iterationsCount);
					var t2 = t.Stop();
					var ts2 = GetProcessTimestamp(local);

					freq = (long)((ts2 - ts1) / t2.GetSeconds());
				});

			frequency = freq;
			return result;
		}

		// WARNING: rough estimate only.
		// see https://blogs.msdn.microsoft.com/oldnewthing/20160429-00/?p=93385 for more.
		public static bool GetThreadFrequency(IClock clock, int iterationsCount, out long frequency)
		{
			var freq = default(long);
			var result = RunHighestPriority(
				() =>
				{
					var local = CurrentThreadHandle;

					var t = clock.Start();
					var ts1 = GetThreadTimestamp(local);
					Thread.SpinWait(iterationsCount);
					var t2 = t.Stop();
					var ts2 = GetThreadTimestamp(local);

					freq = (long)((ts2 - ts1) / t2.GetSeconds());
				});

			frequency = freq;
			return result;
		}
	}

	public class ProcessCycleTimeClock : IClock
	{
		#region Static members
		private static readonly bool _isAvailable;
		private static readonly long _frequency;

		static ProcessCycleTimeClock()
		{
			_isAvailable = CycleClockHelpers.GetProcessFrequency(
				Chronometer.BestClock,
				1000*1000, out _frequency);
		}
		#endregion

		public bool IsAvailable => _isAvailable;
		public Frequency Frequency => new Frequency(_frequency);

		public long GetTimestamp()
		{
			return CycleClockHelpers.GetProcessTimestamp(CycleClockHelpers.ProcessHandle);
		}
	}

	public class ThreadCycleTimeClock : IClock
	{
		#region Static members
		private static readonly bool _isAvailable;
		private static readonly long _frequency;

		static ThreadCycleTimeClock()
		{
			_isAvailable = CycleClockHelpers.GetThreadFrequency(
				Chronometer.BestClock,
				1000 * 1000, out _frequency);
		}
		#endregion

		public bool IsAvailable => _isAvailable;
		public Frequency Frequency => new Frequency(_frequency);

		public long GetTimestamp()
		{
			return CycleClockHelpers.GetThreadTimestamp(CycleClockHelpers.CurrentThreadHandle);
		}
	}
}
