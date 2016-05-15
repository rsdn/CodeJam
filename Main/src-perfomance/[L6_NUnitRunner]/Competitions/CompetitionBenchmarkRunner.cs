using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Competitions.RunState;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Validators;

using JetBrains.Annotations;

using NUnit.Framework;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.Competitions
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
			RunCompetition(thisReference.GetType(), null);

		/// <summary>
		/// Runs the competition benchmark from a type of a callee
		/// </summary>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static void Run<T>(T thisReference, IConfig config) where T : class =>
			RunCompetition(thisReference.GetType(), config);

		/// <summary>
		/// Runs the competition benchmark
		/// </summary>
		public static void Run<T>(IConfig config) where T : class =>
			RunCompetition(typeof(T), config);
		#endregion

		#region Core logic
		/// <summary>
		/// Runs the competition benchmark
		/// </summary>
		public static void RunCompetition(
			Type benchmarkType, IConfig config)
		{
			var currentDirectory = Environment.CurrentDirectory;
			try
			{
				// WORKAROUND: fixing the https://github.com/nunit/nunit3-vs-adapter/issues/96
				if (TestContext.CurrentContext.WorkDirectory != null)
				{
					Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
				}
				RunCompetitionUnderSetup(benchmarkType, config);
			}
			finally
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}

		private static void RunCompetitionUnderSetup(
			Type benchmarkType, IConfig baseConfig)
		{
			ValidateCompetitionSetup(benchmarkType);

			// Capturing the output
			var logger = InitAccumulationLogger();
			// Final config
			var competitionConfig = CreateCompetitionConfig(baseConfig, logger);

			CompetitionState competitionState = null;
			try
			{
				competitionState = CompetitionRunnerBase.RunCore(benchmarkType, competitionConfig);
			}
			finally
			{
				DumpOutputSummaryAtTop(competitionState?.LastRunSummary, logger);
			}
			ReportNUnit(competitionState);
		}

		private static void ReportNUnit(CompetitionState competitionState)
		{
			if (competitionState == null)
				return;
			var messages = competitionState.GetMessages();

			var errorMessages = messages
				.Where(m => m.MessageSeverity > MessageSeverity.Warning)
				.OrderBy(m => (int)m.MessageSource)
				.ThenByDescending(m => m.MessageSeverity)
				.Select(m => m.MessageText)
				.ToArray();
			if (errorMessages.Length > 0)
			{
				throw new AssertionException(
					string.Join(Environment.NewLine, errorMessages));
			}

			var validationMessages = messages
				.Where(m => m.MessageSeverity == MessageSeverity.Warning)
				.OrderBy(m => (int)m.MessageSource)
				.ThenByDescending(m => m.MessageSeverity)
				.Select(m => m.MessageText)
				.ToArray();
			if (validationMessages.Length > 0)
			{
				throw new IgnoreException(
					string.Join(Environment.NewLine, validationMessages));
			}
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

		/// <summary>
		/// Removes the default console logger, removes all exporters but default markdown exporter.
		/// </summary>
		// TODO: do not filter the exporters?
		private static ManualConfig CreateCompetitionTestConfig(IConfig template)
		{
			var result = new ManualConfig();

			result.Add(template.GetColumns().ToArray());
			//result.Add(template.GetExporters().ToArray());
			result.Add(template.GetExporters().Where(l => l == MarkdownExporter.Default).ToArray());
			result.Add(template.GetLoggers().Where(l => l != ConsoleLogger.Default).ToArray());
			result.Add(template.GetDiagnosers().ToArray());
			result.Add(template.GetAnalysers().ToArray());
			result.Add(template.GetJobs().ToArray());

			bool noOpt = EnvironmentInfo.GetCurrent().HasAttachedDebugger;

			if (noOpt)
			{
				var validators = template.GetValidators()
					.Where(v => !(v is JitOptimizationsValidator))
					.ToArray();
				result.Add(validators);
			}
			else
			{
				var validators = template.GetValidators()
					.Where(v => !(v is JitOptimizationsValidator))
					.ToArray();
				result.Add(validators);
				if (validators.All(v => !(v is JitOptimizationsValidator)))
				{
					result.Add(JitOptimizationsValidator.FailOnError);
				}
			}
			if (template.GetJobs().Any(j => j.Toolchain is Toolchains.InProcessToolchain))
			{
				result.Add(new InProcessValidator());
			}

			return result;
		}

		private static ManualConfig CreateCompetitionConfig(
			IConfig baseConfig, AccumulationLogger logger)
		{
			baseConfig = baseConfig ?? DefaultConfig.Instance;
			var existingParameters = baseConfig.GetValidators()
				.OfType<CompetitionParameters>()
				.SingleOrDefault();

			// TODO: better setup?
			var result = CreateCompetitionTestConfig(baseConfig);
			if (existingParameters == null)
			{
				existingParameters = new CompetitionParameters();
				result.Add(existingParameters);
			}
			result.Add(logger);

			InitCompetitionConfig(result, existingParameters);
			result.Add(
				StatisticColumn.Min,
				BaselineDiffColumn.Scaled50,
				BaselineDiffColumn.Scaled85,
				BaselineDiffColumn.Scaled95,
				StatisticColumn.Max);
			return result;
		}

		private static void InitCompetitionConfig(ManualConfig config, CompetitionParameters existingParameters)
		{
			if (existingParameters.DisableValidation)
			{
				return;
			}
			var annotator = new Analysers.CompetitionLimitsAnnotateAnalyser()
			{
				AllowSlowBenchmarks = existingParameters.AllowSlowBenchmarks,
				AnnotateOnRun = existingParameters.AnnotateOnRun,
				IgnoreExistingAnnotations = existingParameters.IgnoreExistingAnnotations,
				DefaultCompetitionLimit = existingParameters.DefaultCompetitionLimit,
				RerunIfValidationFailed = existingParameters.RerunIfValidationFailed
			};
			config.Add(annotator);
		}

		private static void DumpOutputSummaryAtTop(Summary summary, AccumulationLogger logger)
		{
			if (summary != null)
			{
				// Dumping the benchmark results to console
				MarkdownExporter.Console.ExportToLog(summary, ConsoleLogger.Default);
			}

			// Dumping all captured output below the benchmark results
			ConsoleLogger.Default.WriteLine(logger.GetLog());
		}
		#endregion
	}
}