using System;
using System.Diagnostics;
using System.Linq;
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
		private static ManualCompetitionConfig CreateRunConfigCore()
		{
			var result = new ManualCompetitionConfig
			{
				RerunIfLimitsFailed = true,
				ReportWarningsAsErrors = true
			};

			result.Add(BenchmarkDotNet.Configs.DefaultConfig.Instance.GetColumnProviders().ToArray());
			result.Add(AppConfigHelpers.GetImportantOnlyLogger(typeof(SelfTestHelpers).Assembly));
			return result;
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

			var result = CreateRunConfigCore();
			result.AllowDebugBuilds = true;
			result.Add(job);
			return result;
		}

		public static ManualCompetitionConfig CreateHighAccuracyConfig(bool outOfProcess = false)
		{
			var job = new Job("HighAccuracyConfig",
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

			if( outOfProcess)
				throw new NotImplementedException();

			var result = CreateRunConfigCore();
			result.Add(job);

			return result;
		}

		public static ManualCompetitionConfig CreateRunConfigAnnotate()
		{
			var result = CreateHighAccuracyConfig();
			result.LogCompetitionLimits = true;
			result.RerunIfLimitsFailed = true;
			result.UpdateSourceAnnotations = true;
			result.MaxRunsAllowed = 6;
			return result;
		}

		public static ManualCompetitionConfig CreateRunConfigReAnnotate()
		{
			var result = CreateRunConfigAnnotate();
			result.IgnoreExistingAnnotations = true;
			return result;
		}
		#endregion
	}
}