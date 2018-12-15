// BASEDON: https://github.com/Azure/DotNetty/blob/dev/test/DotNetty.Microbench/Utilities/CycleTime.cs

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Horology
{
	/// <summary>Helper method for CPU cycle timers.</summary>
	internal static class CycleClockHelpers
	{
		#region Interop
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll")]
		private static extern IntPtr GetCurrentThread();

		[DllImport("Kernel32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool QueryProcessCycleTime(IntPtr processHandle, out ulong cycleTime);

		[DllImport("Kernel32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool QueryThreadCycleTime(IntPtr threadHandle, out ulong cycleTime);
		#endregion

		/// <summary>Current process pseudo-handle</summary>
		private static readonly IntPtr _currentProcessHandle = GetCurrentProcess();

		/// <summary>Current thread pseudo-handle</summary>
		private static readonly IntPtr _currentThreadHandle = GetCurrentThread();

		/// <summary>Gets current process timestamp.</summary>
		/// <returns>Process cycle time timestamp.</returns>
		public static long GetCurrentProcessTimestamp()
		{
			if (!QueryProcessCycleTime(_currentProcessHandle, out var result))
				throw new Win32Exception();

			return checked((long)result);
		}

		/// <summary>Gets current thread timestamp.</summary>
		/// <returns>Thread cycle time timestamp.</returns>
		public static long GetCurrentThreadTimestamp()
		{
			if (!QueryThreadCycleTime(_currentThreadHandle, out var result))
				throw new Win32Exception();

			return checked((long)result);
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
		public static bool TryEstimateProcessCycleTimeFrequency(IClock clock, int iterationsCount, out long frequency)
		{
			var freq = default(long);
			var result = RunHighestPriorityIfWindows(
				() =>
				{
					var t = clock.Start();
					var ts1 = GetCurrentProcessTimestamp();
					Thread.SpinWait(iterationsCount);
					var t2 = t.GetElapsed();
					var ts2 = GetCurrentProcessTimestamp();

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
		public static bool TryEstimateThreadCycleTimeFrequency(IClock clock, int iterationsCount, out long frequency)
		{
			var freq = default(long);
			var result = RunHighestPriorityIfWindows(
				() =>
				{
					var t = clock.Start();
					var ts1 = GetCurrentThreadTimestamp();
					Thread.SpinWait(iterationsCount);
					var t2 = t.GetElapsed();
					var ts2 = GetCurrentThreadTimestamp();

					freq = (long)((ts2 - ts1) / t2.GetSeconds());
				});

			frequency = freq;
			return result;
		}

		private static bool RunHighestPriorityIfWindows([InstantHandle] Action callback)
		{
			if (!IsWindows())
			{
				return false;
			}

			using (BenchmarkHelpers.SetupHighestPriorityScope(null, ConsoleLogger.Default))
			{
				try
				{
					callback();
					return true;
				}
				catch (Win32Exception)
				{
					return false;
				}
			}
		}

		private static bool IsWindows()
		{
#if !CORE || TARGETS_NET
			return new[] { PlatformID.Win32NT, PlatformID.Win32S, PlatformID.Win32Windows, PlatformID.WinCE }
				.Contains(Environment.OSVersion.Platform);
#else
			return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
		}
	}
}