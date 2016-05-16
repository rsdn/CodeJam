using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running.Messages;
using BenchmarkDotNet.Toolchains.InProcess;
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


		public static void ReportMessagesToUser(
			CompetitionState competitionState,
			Action<string> reportErrorCallback,
			Action<string> reportWarningCallback)
		{
			if (competitionState == null)
				return;

			var messages = competitionState.GetMessages();

			var errorMessages = messages
				.Where(m => m.MessageSeverity > MessageSeverity.Warning)
				.OrderBy(m => (int)m.MessageSource)
				.ThenByDescending(m => m.MessageSeverity)
				.Select(m => "\t* " + m.MessageText)
				.ToArray();
			var validationMessages = messages
				.Where(m => m.MessageSeverity == MessageSeverity.Warning)
				.OrderBy(m => (int)m.MessageSource)
				.ThenByDescending(m => m.MessageSeverity)
				.Select(m => "\t* " + m.MessageText)
				.ToArray();

			if (errorMessages.Length > 0)
			{
				var tmp = new List<string>(errorMessages.Length + validationMessages.Length + 2);
				tmp.Add("Errors:");
				tmp.AddRange(errorMessages);
				if (validationMessages.Length > 0)
				{
					tmp.Add("Warnings:");
					tmp.AddRange(validationMessages);
				}

				reportErrorCallback?.Invoke(
					string.Join(Environment.NewLine, tmp));
			}
			else if (validationMessages.Length > 0)
			{
				var tmp = new List<string>(validationMessages.Length + 1);
				tmp.Add("Warnings:");
				tmp.AddRange(validationMessages);

				reportWarningCallback?.Invoke(
					string.Join(Environment.NewLine, tmp));
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
			if (template.GetJobs().Any(j => j.Toolchain is InProcessToolchain))
			{
				result.Add(new InProcessValidator());
			}

			return result;
		}

		private static void InitCompetitionConfig(ManualConfig config, CompetitionParameters competitionParameters)
		{
			if (competitionParameters.DisableValidation)
			{
				return;
			}
			var annotator = new Analysers.CompetitionLimitsAnnotateAnalyser()
			{
				AllowSlowBenchmarks = competitionParameters.AllowSlowBenchmarks,
				AnnotateOnRun = competitionParameters.AnnotateOnRun,
				IgnoreExistingAnnotations = competitionParameters.IgnoreExistingAnnotations,
				DefaultCompetitionLimit = competitionParameters.DefaultCompetitionLimit,
				MaxRuns = 3,
				AdditionalRunsOnAnnotate = 2
			};
			config.Add(annotator);
		}
		#endregion
	}
}