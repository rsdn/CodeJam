using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Environments;

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
			var runMode = config.GetJobs().Single().Run;
			var unrollFactor = runMode.UnrollFactor;
			return GetExpectedCountCore(config, methodsCount, unrollFactor);
		}

		public static int GetExpectedCountIgnoreUnroll(ICompetitionConfig config, int methodsCount)
		{
			return GetExpectedCountCore(config, methodsCount, 1);
		}

		private static int GetExpectedCountCore(ICompetitionConfig config, int methodsCount, int unrollFactor)
		{
			const int JittingCount = 1;
			var runMode = config.GetJobs().Single().Run;

			var singleLaunchCount = JittingCount * unrollFactor +
				(runMode.WarmupCount + runMode.TargetCount) * runMode.InvocationCount;

			return singleLaunchCount * runMode.LaunchCount * methodsCount;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void IgnoreIfDebug()
		{
			var caller = Assembly.GetCallingAssembly();
			if ((caller.GetCustomAttribute<DebuggableAttribute>()?.IsJITOptimizerDisabled ?? false) &&
				!HostEnvironmentInfo.GetCurrent().HasAttachedDebugger)
			{
				Assert.Ignore("Please run as a release build");
			}
		}
		#endregion
	}
}