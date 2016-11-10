using System;
using System.Reflection;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

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

		#region Configs core
		private static ManualCompetitionConfig AddLogger(this ManualCompetitionConfig config)
		{
			config.Add(AppConfigHelpers.GetImportantOnlyLogger(typeof(SelfTestHelpers).Assembly));
			return config;
		}

		private static Job CreateHighAccuracyJob(bool outOfProcess = false)
		{
			var job = new Job(
				"HighAccuracyConfig",
				CompetitionHelpers.CreateDefaultJob(),
				EnvMode.RyuJitX64)
			{
				Run =
				{
					LaunchCount = 1,
					WarmupCount = 200,
					TargetCount = 500
				}
			};

			if (outOfProcess)
				throw new NotImplementedException();

			return job;
		}
		#endregion

		#region Ready configs
		public static readonly ICompetitionConfig SelfTestConfig = CreateSelfTestConfig(Platform.X64).AsReadOnly();

		public static readonly ICompetitionConfig HighAccuracyConfig = CreateHighAccuracyConfig().AsReadOnly();

		public static ManualCompetitionConfig CreateSelfTestConfig(Platform? platform)
		{
			var job = new Job(
				$"SelfTestConfig{platform}",
				CompetitionHelpers.CreateDefaultJob(platform))
			{
				Run =
				{
					LaunchCount = 1,
					WarmupCount = 2,
					TargetCount = 2
				},
				Env =
				{
					Affinity = new IntPtr(-1)
				}
			};

			var result = CompetitionHelpers.CreateDefaultConfig(job);
			return result.AddLogger();
		}

		public static ManualCompetitionConfig CreateHighAccuracyConfig(bool outOfProcess = false) => 
			CompetitionHelpers.CreateDefaultConfig(CreateHighAccuracyJob(outOfProcess)).AddLogger();

		public static ManualCompetitionConfig CreateRunConfigAnnotate() =>
			CompetitionHelpers.CreateDefaultConfigAnnotate(CreateHighAccuracyJob()).AddLogger();

		public static ManualCompetitionConfig CreateRunConfigReannotate() =>
			CompetitionHelpers.CreateDefaultConfigReannotate(CreateHighAccuracyJob()).AddLogger();
		#endregion
	}
}