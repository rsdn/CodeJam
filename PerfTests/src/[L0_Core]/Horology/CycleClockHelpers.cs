// BASEDON:https://github.com/Azure/DotNetty/blob/dev/test/DotNetty.Microbench/Utilities/CycleTime.cs

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using JetBrains.Annotations;

using Microsoft.Win32.SafeHandles;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Horology
{
	/// <summary>Helper method for CPU cycle timers.</summary>
	internal static class CycleClockHelpers
	{
		#region Interop
		[DllImport("Kernel32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool QueryProcessCycleTime(SafeProcessHandle processHandle, out ulong cycleTime);
		#endregion

		[DllImport("Kernel32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool QueryThreadCycleTime(IntPtr threadHandle, out ulong cycleTime);

		private static bool IsWindows()
		{
#if !CORE
			return new[] { PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows, PlatformID.WinCE }
				.Contains(Environment.OSVersion.Platform);
#else
			return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
		}

		/// <summary>The current process handle</summary>
		public static readonly SafeProcessHandle CurrentProcessHandle = Process.GetCurrentProcess().SafeHandle;

		/// <summary>The current thread handle</summary>
		public static readonly IntPtr CurrentThreadHandle = new IntPtr(-2);

		/// <summary>Gets process cycle time.</summary>
		/// <param name="processHandle">The process handle.</param>
		/// <returns>Process cycle time timestamp.</returns>
		public static long GetProcessTimestamp(SafeProcessHandle processHandle)
		{
			ulong result;
			if (!QueryProcessCycleTime(processHandle, out result))
				throw new Win32Exception();

			return checked((long)result);
		}

		/// <summary>Gets thread the .</summary>
		/// <param name="threadHandle">The thread handle.</param>
		/// <returns>Thread cycle time timestamp.</returns>
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

		/// <summary>
		/// Estimates frequency coefficient for process cycle time.
		/// WARNING: result is inaccurate (up to +/- 30% to actual time)
		/// see https://blogs.msdn.microsoft.com/oldnewthing/20160429-00/?p=93385 for more.
		/// </summary>
		/// <param name="clock">The clock.</param>
		/// <param name="iterationsCount">Iterations performed to estimate the time.</param>
		/// <param name="frequency">The frequency.</param>
		/// <returns>Frequency coefficient for process cycle time</returns>
		public static bool EstimateProcessCycleTimeFrequency(IClock clock, int iterationsCount, out long frequency)
		{
			var freq = default(long);
			var result = RunHighestPriority(
				() =>
				{
					var local = CurrentProcessHandle;

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

		/// <summary>
		/// Estimates frequency coefficient for thread cycle time.
		/// WARNING: results are inaccurate (up to +/- 30% to actual time),
		/// see https://blogs.msdn.microsoft.com/oldnewthing/20160429-00/?p=93385 for more.
		/// </summary>
		/// <param name="clock">The clock.</param>
		/// <param name="iterationsCount">Iterations performed to estimate the time.</param>
		/// <param name="frequency">The frequency.</param>
		/// <returns>Frequency coefficient for thread cycle time</returns>
		public static bool EstimateThreadCycleTimeFrequency(IClock clock, int iterationsCount, out long frequency)
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
}