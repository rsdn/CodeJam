using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;

using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[PublicAPI]
	internal static class SelfTestHelpers
	{
		#region Benchmark tests-related
		// Jitting = 1, WarmupCount = 2, TargetCount = 2
		public const int ExpectedSelfTestRunCount = 5;

		public static int GetExpectedCount(ICompetitionConfig config, int methodsCount)
		{
			const int JittingCount = 1;
			var runMode = config.GetJobs().Single().Run;

			var sigleLaunchCount = JittingCount * runMode.UnrollFactor +
				(runMode.WarmupCount + runMode.TargetCount) * runMode.InvocationCount;

			return sigleLaunchCount * runMode.LaunchCount * methodsCount;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void IgnoreIfDebug()
		{
			var caller = Assembly.GetCallingAssembly();
			if (caller.IsDebugAssembly() && !HostEnvironmentInfo.GetCurrent().HasAttachedDebugger)
			{
				Assert.Ignore("Please run as a release build");
			}
		}
		#endregion
	}
}