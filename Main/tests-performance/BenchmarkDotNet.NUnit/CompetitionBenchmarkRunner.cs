using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using JetBrains.Annotations;

using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.NUnit
{
	/// <summary>
	/// Runner for competition performance tests
	/// </summary>
	[PublicAPI]
	public static class CompetitionBenchmarkRunner
	{
		#region Public API
		/// <summary>
		/// Runs the competition benchmark from a type of a callee
		/// </summary>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void Run<T>(T thisReference) where T : class =>
			RunCompetition(thisReference.GetType(), null, 0, 0);

		/// <summary>
		/// Runs the competition benchmark from a type of a callee
		/// </summary>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void Run<T>(T thisReference, IConfig config) where T : class =>
			RunCompetition(thisReference.GetType(), config, 0, 0);

		/// <summary>
		/// Runs the competition benchmark
		/// </summary>
		public static void Run<T>(IConfig config) where T : class =>
			RunCompetition(typeof(T), config, 0, 0);
		#endregion

		#region Core logic
		/// <summary>
		/// Runs the competition benchmark
		/// </summary>
		// BASEDON: https://github.com/PerfDotNet/BenchmarkDotNet/blob/master/BenchmarkDotNet.IntegrationTests/PerformanceUnitTest.cs
		public static void RunCompetition(
			Type benchmarkType, IConfig config, double minRatio, double maxRatio)
		{
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				// WORKAROUND: fixing the https://github.com/nunit/nunit3-vs-adapter/issues/96
				Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;

				RunCompetitionUnderSetup(benchmarkType, config, minRatio, maxRatio);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}

		private static void RunCompetitionUnderSetup(
			Type benchmarkType, IConfig baseConfig, double minRatio, double maxRatio)
		{
			ValidateCompetitionSetup(benchmarkType);

			// Capturing the output
			var logger = InitAccumulationLogger();
			// Competition analyzer
			var competitionState = new CompetitionStateAnalyser();
			// Final config
			var competitionConfig = CreateCompetitionConfig(baseConfig, competitionState, logger);

			Summary summary = null;
			try
			{
				summary = RunCore(benchmarkType, competitionConfig, competitionState);
			}
			finally
			{
				DumpOutputSummaryAtTop(summary, logger);
			}

			competitionState.ValidateSummary(summary, minRatio, maxRatio);
		}

		private static void ValidateCompetitionSetup(Type benchmarkType)
		{
			if (!Debugger.IsAttached)
			{
				var assembly = benchmarkType.Assembly;
				if (assembly.IsDebugAssembly())
					throw new InvalidOperationException(
						$"Set the solution configuration into Release mode. Assembly {assembly.GetName().Name} was build as debug.");

				foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
				{
					var refAssembly = Assembly.Load(referencedAssemblyName);
					if (refAssembly.IsDebugAssembly())
						throw new InvalidOperationException(
							$"Set the solution configuration into Release mode. Assembly {refAssembly.GetName().Name} was build as debug.");
				}
			}
		}

		private static AccumulationLogger InitAccumulationLogger()
		{
			var logger = new AccumulationLogger();
			logger.WriteLine();
			logger.WriteLine();
			logger.WriteLine(new string('=', 40));
			logger.WriteLine();
			return logger;
		}

		private static IConfig CreateCompetitionConfig(
			IConfig baseConfig, CompetitionStateAnalyser competitionState, 
			AccumulationLogger logger)
		{
			baseConfig = baseConfig ?? DefaultConfig.Instance;
			var existingParameters = baseConfig.GetAnalysers()
				.OfType<CompetitionParametersAnalyser>()
				.SingleOrDefault();

			// TODO: better setup?
			var result = BenchmarkHelpers.CreateUnitTestConfig(baseConfig);
			result.Add(competitionState);
			if (existingParameters == null)
			{
				result.Add(new CompetitionParametersAnalyser());
			}
			result.Add(logger);
			result.Add(
				StatisticColumn.Min,
				ScaledPercentileColumn.S0Column,
				ScaledPercentileColumn.S50Column,
				ScaledPercentileColumn.S85Column,
				ScaledPercentileColumn.S95Column,
				ScaledPercentileColumn.S100Column,
				StatisticColumn.Max);
			return result;
		}

		private static Summary RunCore(
			Type benchmarkType, IConfig competitionConfig,
			CompetitionStateAnalyser competitionState)
		{
			Summary summary = null;

			const int rerunCount = 10;
			for (var i = 0; i < rerunCount; i++)
			{
				competitionState.LastRun = i == rerunCount - 1;
				competitionState.RerunRequested = false;

				// Running the benchmark
				summary = BenchmarkRunner.Run(benchmarkType, competitionConfig);

				// Rerun if annotated
				if (!competitionState.RerunRequested)
				{
					break;
				}
			}

			return summary;
		}

		private static void DumpOutputSummaryAtTop(Summary summary, AccumulationLogger logger)
		{
			if (summary != null)
			{
				// Dumping the benchmark results to console
				MarkdownExporter.Default.ExportToLog(summary, ConsoleLogger.Default);
			}

			// Dumping all captured output below the benchmark results
			ConsoleLogger.Default.WriteLine(logger.GetLog());
		}
		#endregion
	}
}