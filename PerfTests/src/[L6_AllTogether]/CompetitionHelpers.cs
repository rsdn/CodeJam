using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.InProcess;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Reusable API for performance tests.
	/// </summary>
	[PublicAPI]
	public static class CompetitionHelpers
	{
		#region Constants
		/// <summary>Default category prefix for performance tests.</summary>
		public const string PerfTestCategory = "Performance";

		/// <summary>Default explanation for bad performance tests.</summary>
		public const string TemporarilyExcludedReason =
			"Temporary disabled as the results are unstable. Please, run the test manually from the Test Explorer window.";
		#endregion

		#region Benchmark-related
		/// <summary>Default count for performance test loops.</summary>
		public const int DefaultCount = 10 * 1000;

		/// <summary>Default delay implementation. Performs delay for specified number of cycles.</summary>
		/// <param name="cycles">The number of cycles to delay.</param>
		public static void Delay(int cycles) => Thread.SpinWait(cycles);
		#endregion

		#region Config instances
		/// <summary>Default configuration that should be used for most performance tests.</summary>
		public static readonly ICompetitionConfig DefaultConfig = CreateDefaultConfig().AsReadOnly();

		/// <summary>
		/// Configuration that should be used for new performance tests.
		/// Automatically annotates the source with new timing limits.
		/// </summary>
		public static readonly ICompetitionConfig DefaultConfigAnnotate = CreateDefaultConfigAnnotate().AsReadOnly();

		/// <summary>Configuration that should be used in case the existing limits should be ignored.</summary>
		public static readonly ICompetitionConfig DefaultConfigReannotate = CreateDefaultConfigReannotate().AsReadOnly();
		#endregion

		#region Configs core
		/// <summary>Helper for custom configs: creates detailed logger for current assembly.</summary>
		/// <param name="targetAssembly">Assembly with performance tests. Calling assembly will be used if <c>null</c>.</param>
		/// <returns>Detailed logger for current assembly.</returns>
		public static ILogger CreateDetailedLoggerForAssembly([CanBeNull] Assembly targetAssembly = null) =>
			GetAssemblyLevelLogger(targetAssembly ?? Assembly.GetCallingAssembly(), ".AllPerfTests.log");

		/// <summary>Helper for custom configs: creates important info logger for current assembly.</summary>
		/// <param name="targetAssembly">Assembly with performance tests. Calling assembly will be used if <c>null</c>.</param>
		/// <returns>Important info logger for current assembly.</returns>
		public static ILogger CreateImportantInfoLoggerForAssembly([CanBeNull] Assembly targetAssembly = null) =>
			new HostLogger(
				GetAssemblyLevelLogger(targetAssembly ?? Assembly.GetCallingAssembly(), ".Short.AllPerfTests.log"),
				HostLogMode.PrefixedOnly);

		private static LazyStreamLogger GetAssemblyLevelLogger(Assembly assembly, string suffix)
		{
			var fileName = assembly.GetName().Name + suffix;
			return new LazyStreamLogger(
				() => new StreamWriter(
					new FileStream(
						fileName,
						FileMode.Create, FileAccess.Write, FileShare.Read)));
		}
		#endregion

		#region Configs
		/// <summary>Helper for custom configs: creates default job for the config.</summary>
		/// <param name="platform">Target platform for the job.</param>
		/// <returns>Default job for the config.</returns>
		public static IJob CreateDefaultJob(Platform platform = Platform.Host) =>
			new Job
			{
				LaunchCount = 1,
				Mode = Mode.SingleRun,
				WarmupCount = 100,
				TargetCount = 300,
				Platform = platform,
				Jit = platform == Platform.X64 ? Jit.RyuJit : Jit.Host,
				Toolchain = InProcessToolchain.DontLogOutput
			};

		/// <summary>Helper for custom configs: configuration that should be used for new performance tests.</summary>
		/// <param name="job">The job for the config. Default one will be used if <c>null</c>.</param>
		/// <returns>Configuration that should be used for new performance tests.</returns>
		public static ManualCompetitionConfig CreateDefaultConfig(IJob job = null)
		{
			var defaultConfig = BenchmarkDotNet.Configs.DefaultConfig.Instance;

			var result = new ManualCompetitionConfig
			{
				RerunIfLimitsFailed = true,
				KeepBenchmarkFiles = defaultConfig.KeepBenchmarkFiles
			};

			result.Add(defaultConfig.GetColumns().ToArray());
			result.Add(defaultConfig.GetValidators().ToArray());
			result.Add(defaultConfig.GetAnalysers().ToArray());
			result.Add(defaultConfig.GetDiagnosers().ToArray());
			result.Set(defaultConfig.GetOrderProvider());

			result.Add(job ?? CreateDefaultJob());

			return result;
		}

		/// <summary>
		/// Helper for custom configs: creates configuration that should be used for new performance tests.
		/// Automatically annotates the source with new timing limits.
		/// </summary>
		/// <param name="job">The job for the config. Default one will be used if <c>null</c>.</param>
		/// <returns>Configuration that should be used for new performance tests.</returns>
		public static ManualCompetitionConfig CreateDefaultConfigAnnotate(IJob job = null)
		{
			var result = CreateDefaultConfig(job);
			result.LogCompetitionLimits = true;
			result.RerunIfLimitsFailed = true;
			result.UpdateSourceAnnotations = true;
			result.Add(Exporters.TimingsExporter.Instance);
			return result;
		}

		/// <summary>
		/// Helper for custom configs: creates configuration that should be used in case the existing limits should be ignored.
		/// </summary>
		/// <param name="job">The job for the config. Default one will be used if <c>null</c>.</param>
		/// <returns>Configuration that should be used in case the existing limits should be ignored.</returns>
		public static ManualCompetitionConfig CreateDefaultConfigReannotate(IJob job = null)
		{
			var result = CreateDefaultConfigAnnotate(job);
			result.IgnoreExistingAnnotations = true;
			return result;
		}
		#endregion
	}
}