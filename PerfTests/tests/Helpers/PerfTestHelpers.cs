using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;

using JetBrains.Annotations;

using NUnit.Framework;

namespace CodeJam.PerfTests
{
	[PublicAPI]
	public static class PerfTestHelpers
	{
		#region Benchmark-related
		public const int PerfTestCount = 10 * 1000;

		public static readonly bool RunUnderNUnit = TestContext.CurrentContext.WorkDirectory != null;

		// Jitting = 1, WarmupCount = 2, TargetCount = 2
		public const int ExpectedSelfTestRunCount = 5;

		public static void Delay(int cycles) => Thread.SpinWait(cycles);

		public static void IgnoreIfDebug()
		{
			var caller = Assembly.GetCallingAssembly();
			if (caller.IsDebugAssembly())
			{
				Assert.Ignore("Please run as a release build");
			}
		}
		#endregion

		#region Configs core
		private static readonly string _assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

		private static readonly ILogger _detailedLogger =
			new FlushableStreamLogger(
				GetLogPath(_assemblyName + ".AllPerfTests.log"),
				false);

		private static readonly ILogger _importantInfoLogger =
			new HostLogger(
				new FlushableStreamLogger(
					GetLogPath(_assemblyName + ".Short.AllPerfTests.log"),
					false),
				HostLogMode.PrefixedOnly);

		private static string GetLogPath(string fileName) =>
			Path.Combine(
				RunUnderNUnit
					? TestContext.CurrentContext.TestDirectory
					: Path.GetTempPath(),
				fileName);

		private static ManualCompetitionConfig CreateRunConfigCore()
		{
			var result = new ManualCompetitionConfig
			{
				RerunIfLimitsFailed = true,
				ReportWarningsAsErrors = true
			};

			result.Add(DefaultConfig.Instance.GetColumns().ToArray());
			//result.Add(TimingsExporter.Instance);
			//result.Add(_detailedLogger, _importantInfoLogger);
			return result;
		}
		#endregion

		#region Ready configs
		public static readonly ICompetitionConfig DefaultRunConfig = CreateRunConfig().AsReadOnly();

		internal static readonly ICompetitionConfig SelfTestConfig = CreateSelfTestConfig(Platform.X64).AsReadOnly();

		internal static ManualCompetitionConfig CreateSelfTestConfig(Platform platform)
		{
			var result = CreateRunConfigCore();
			result.AllowDebugBuilds = true;
			result.Add(
				new Job
				{
					Affinity = -1, // DONTTOUCH: affinity option should be covered by the tests.
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 2,
					TargetCount = 2,
					Platform = platform,
					Toolchain = InProcessToolchain.Instance
				});
			return result;
		}

		public static ManualCompetitionConfig CreateRunConfig(bool outOfProcess = false)
		{
			var result = CreateRunConfigCore();
			result.Add(
				new Job
				{
					LaunchCount = 1,
					Mode = Mode.SingleRun,
					WarmupCount = 50,
					TargetCount = 100,
					Platform = Platform.X64,
					Jit = Jit.RyuJit,
					Toolchain = outOfProcess ? null : InProcessToolchain.DontLogOutput
				});

			return result;
		}

		public static ManualCompetitionConfig CreateRunConfigAnnotate()
		{
			var result = CreateRunConfig();
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