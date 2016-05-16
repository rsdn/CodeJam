using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Validators;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Running.Competitions.Core
{
	/// <summary>
	/// Runner for competition performance tests
	/// </summary>
	[PublicAPI]
	public static class CompetitionRunnerCore
	{
		#region Reusable runner
		public static void ValidateCompetitionSetup(Type benchmarkType)
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

		public static AccumulationLogger InitAccumulationLogger()
		{
			var logger = new AccumulationLogger();
			logger.WriteLine();
			logger.WriteLine();
			logger.WriteLine(new string('=', 40));
			logger.WriteLine();
			return logger;
		}

		public static ManualConfig CreateCompetitionConfig(
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
		#endregion
	}
}