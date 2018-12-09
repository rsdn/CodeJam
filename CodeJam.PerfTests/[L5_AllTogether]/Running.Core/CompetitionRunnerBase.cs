using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Validators;

using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Configs.Factories;
using CodeJam.PerfTests.Exporters;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Base class for competition benchmark runners</summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	public abstract class CompetitionRunnerBase
	{
		/// <summary>Base class for competition runner's host logger.</summary>
		/// <seealso cref="FilteringLogger"/>
		protected abstract class HostLogger : FilteringLogger
		{
			/// <summary>Initializes a new instance of the <see cref="HostLogger"/> class.</summary>
			/// <param name="wrappedLogger">The logger to redirect the output. Cannot be null.</param>
			/// <param name="logMode">Host logging mode.</param>
			protected HostLogger([NotNull] ILogger wrappedLogger, FilteringLoggerMode logMode)
				: base(wrappedLogger, logMode) { }
		}

		#region Helpers
		private static void SetCurrentDirectoryIfNotNull(string currentDirectory)
		{
			if (currentDirectory.NotNullNorEmpty())
			{
				Environment.CurrentDirectory = currentDirectory;
			}
		}
		#endregion

		#region Public API (expose these via Competiton classes)

		#region With competition features
		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">BenchmarkCase class to run.</typeparam>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public CompetitionState Run<T>([CanBeNull] CompetitionFeatures competitionFeatures = null)
			where T : class =>
				RunCore(typeof(T), null, competitionFeatures);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">BenchmarkCase class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public CompetitionState Run<T>(
			[NotNull] T thisReference,
			[CanBeNull] CompetitionFeatures competitionFeatures = null)
			where T : class =>
				RunCore(thisReference.GetType(), null, competitionFeatures);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">BenchmarkCase class to run.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public CompetitionState Run(
			[NotNull] Type benchmarkType,
			[CanBeNull] CompetitionFeatures competitionFeatures = null) =>
				RunCore(benchmarkType, null, competitionFeatures);
		#endregion

		#region With config
		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">BenchmarkCase class to run.</typeparam>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public CompetitionState Run<T>([NotNull] ICompetitionConfig competitionConfig)
			where T : class =>
				RunCore(typeof(T), competitionConfig, null);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">BenchmarkCase class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public CompetitionState Run<T>(
			[NotNull] T thisReference,
			[NotNull] ICompetitionConfig competitionConfig)
			where T : class =>
				RunCore(thisReference.GetType(), competitionConfig, null);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">BenchmarkCase class to run.</param>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public CompetitionState Run(
			[NotNull] Type benchmarkType,
			[NotNull] ICompetitionConfig competitionConfig) =>
				RunCore(benchmarkType, competitionConfig, null);
		#endregion

		#endregion

		#region Advanced public API (expose these if you wish)

		#region With competition features
		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="assembly">Assembly with benchmarks to run.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public IReadOnlyDictionary<Type, CompetitionState> Run(
			[NotNull] Assembly assembly,
			[CanBeNull] CompetitionFeatures competitionFeatures = null) =>
				Run(BenchmarkHelpers.GetBenchmarkTypes(assembly), competitionFeatures);

		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="benchmarkTypes">BenchmarkCase classes to run.</param>
		/// <param name="competitionFeatures">The competition features.</param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public IReadOnlyDictionary<Type, CompetitionState> Run(
			[NotNull] Type[] benchmarkTypes,
			[CanBeNull] CompetitionFeatures competitionFeatures = null)
		{
			var result = new Dictionary<Type, CompetitionState>();

			foreach (var benchmarkType in benchmarkTypes)
			{
				result[benchmarkType] = RunCore(benchmarkType, null, competitionFeatures);
			}

			return result;
		}
		#endregion

		#region With config
		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="assembly">Assembly with benchmarks to run.</param>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public IReadOnlyDictionary<Type, CompetitionState> Run(
			[NotNull] Assembly assembly,
			[NotNull] ICompetitionConfig competitionConfig) =>
				Run(BenchmarkHelpers.GetBenchmarkTypes(assembly), competitionConfig);

		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="benchmarkTypes">BenchmarkCase classes to run.</param>
		/// <param name="competitionConfig">Custom competition config.</param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public IReadOnlyDictionary<Type, CompetitionState> Run(
			[NotNull] Type[] benchmarkTypes,
			[NotNull] ICompetitionConfig competitionConfig)
		{
			var result = new Dictionary<Type, CompetitionState>();

			foreach (var benchmarkType in benchmarkTypes)
			{
				result[benchmarkType] = RunCore(benchmarkType, competitionConfig, null);
			}

			return result;
		}
		#endregion

		#endregion

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">BenchmarkCase class to run.</param>
		/// <param name="competitionConfig">Custom competition config (optional).</param>
		/// <param name="competitionFeatures">
		/// The competition features. Ignored if <paramref name="competitionConfig"/> is not <c>null</c>.
		/// </param>
		/// <returns>State of the run.</returns>
		[NotNull]
		private CompetitionState RunCore(
			[NotNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig competitionConfig,
			[CanBeNull] CompetitionFeatures competitionFeatures)
		{
			Code.NotNull(benchmarkType, nameof(benchmarkType));

			competitionConfig = CreateBenchmarkConfig(benchmarkType, competitionConfig, competitionFeatures);
			var hostLogger = competitionConfig.GetLoggers().OfType<HostLogger>().Single();

			var previousDirectory = Environment.CurrentDirectory;
			var currentDirectory = GetOutputDirectory(benchmarkType.Assembly);
			if (currentDirectory == previousDirectory)
			{
				currentDirectory = null;
				previousDirectory = null;
			}

			CompetitionState competitionState = null;
			try
			{
				SetCurrentDirectoryIfNotNull(currentDirectory);
				try
				{
					competitionState = CompetitionCore.Run(benchmarkType, competitionConfig);

					ProcessRunComplete(competitionState);
				}
				finally
				{
					ReportHostLogger(hostLogger, competitionState?.LastRunSummary);
				}

				using (LoggerHelpers.BeginImportantLogScope(competitionState.Config))
				{
					ReportMessagesToUser(competitionState);
				}
			}
			finally
			{
				LoggerHelpers.FlushLoggers(competitionConfig);
				SetCurrentDirectoryIfNotNull(previousDirectory);
			}

			return competitionState;
		}

		#region Prepare & run completed logic
		private void ProcessRunComplete([NotNull] CompetitionState competitionState)
		{
			var logger = competitionState.Logger;
			var summary = competitionState.LastRunSummary;

			if (summary == null)
				return;

			logger.WriteVerboseLine($"{competitionState.BenchmarkType.Name} completed.");

			if (competitionState.Options.RunOptions.DetailedLogging)
			{
				var messages = competitionState.GetMessages();

				if (messages.Any())
				{
					logger.WriteSeparatorLine("All messages");
					foreach (var message in messages)
					{
						logger.LogMessage(message);
					}
				}
				else
				{
					logger.WriteSeparatorLine();
					logger.WriteVerboseLine("No messages in run.");
				}

				logger.WriteLine();
			}
			else
			{
				using (LoggerHelpers.BeginImportantLogScope(summary.Config))
				{
					var summaryLogger = DumpSummaryToHostLogger
						? logger
						: new CompositeLogger(
							summary.Config
								.GetLoggers()
								.Where(l => !(l is HostLogger))
								.ToArray());

					// Dumping the benchmark summary
					summaryLogger.WriteSeparatorLine("Summary");
					MarkdownExporter.Console.ExportToLog(summary, summaryLogger);

					logger.WriteLine();
				}
			}
		}
		#endregion

		#region Override test running behavior
		/// <summary>Returns output directory that should be used for running the test.</summary>
		/// <param name="targetAssembly">The descriptor assembly tests will be run for.</param>
		/// <returns>
		/// Output directory that should be used for running the test or <c>null</c> if the current directory should be used.
		/// </returns>
		protected virtual string GetOutputDirectory(Assembly targetAssembly) => null;

		/// <summary>Gets a value indicating whether the last run summary should be dumped into host logger.</summary>
		/// <value>
		/// <c>true</c> if the last run summary should be dumped into host logger; otherwise, <c>false</c>.
		/// </value>
		protected virtual bool DumpSummaryToHostLogger => true;
		#endregion

		#region Host-related logic
		/// <summary>Creates a host logger.</summary>
		/// <param name="hostLogMode">The host log mode.</param>
		/// <returns>An instance of <see cref="HostLogger"/></returns>
		[NotNull]
		protected abstract HostLogger CreateHostLogger(FilteringLoggerMode hostLogMode);

		/// <summary>Reports content of the host logger to user.</summary>
		/// <param name="logger">The host logger.</param>
		/// <param name="summary">The summary to report.</param>
		protected abstract void ReportHostLogger([NotNull] HostLogger logger, [CanBeNull] Summary summary);

		/// <summary>Reports the execution errors to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected abstract void ReportExecutionErrors([NotNull] string messages, [NotNull] CompetitionState competitionState);

		/// <summary>Reports failed assertions to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected abstract void ReportAssertionsFailed([NotNull] string messages, [NotNull] CompetitionState competitionState);

		/// <summary>Reports warnings to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="competitionState">State of the run.</param>
		protected abstract void ReportWarnings([NotNull] string messages, [NotNull] CompetitionState competitionState);
		#endregion

		#region Create benchark config
		private ICompetitionConfig CreateBenchmarkConfig(
			[NotNull] Type benchmarkType,
			ICompetitionConfig competitionConfig,
			CompetitionFeatures competitionFeatures)
		{
			// ReSharper disable once UseObjectOrCollectionInitializer
			var result = new ManualCompetitionConfig(
				competitionConfig ??
					CompetitionConfigFactory.FindFactoryAndCreate(benchmarkType, competitionFeatures));

			InitCompetitionConfigOverride(result);

			FixCompetitionConfig(result);

			return result.AsReadOnly();
		}

		private void FixCompetitionConfig(ManualCompetitionConfig competitionConfig)
		{
			// DONTTOUCH: call order is important.
			FixConfigJobs(competitionConfig);
			FixConfigLoggers(competitionConfig);
			FixConfigValidators(competitionConfig);
			FixConfigExporters(competitionConfig);
			FixConfigAnalysers(competitionConfig);
			FixConfigMetrics(competitionConfig);
			FixConfigDuplicates(competitionConfig);
		}

		private static void FixConfigJobs(ManualCompetitionConfig competitionConfig)
		{
			var jobs = competitionConfig.Jobs;
			for (var i = 0; i < jobs.Count; i++)
			{
				var job = jobs[i];
				if (job.Infrastructure.Toolchain == null)
				{
					var id = job.HasValue(CharacteristicObject.IdCharacteristic) ? job.Id : null;
					jobs[i] = job
						.With(InProcessToolchain.Instance)
						.WithId(id);
				}
			}

			if (competitionConfig.Jobs.Count == 0)
			{
				competitionConfig.Add(CompetitionConfigFactory.DefaultJob);
			}
		}

		private void FixConfigLoggers(ManualCompetitionConfig competitionConfig)
		{
			var runOptions = competitionConfig.Options.RunOptions;
			var hostLogMode = runOptions.DetailedLogging ? FilteringLoggerMode.AllMessages : FilteringLoggerMode.PrefixedOrErrors;
			competitionConfig.Loggers.Insert(0, CreateHostLogger(hostLogMode));
		}

		private static void FixConfigValidators(ManualCompetitionConfig competitionConfig)
		{
			var competitionOptions = competitionConfig.Options;
			var debugMode = competitionOptions.RunOptions.AllowDebugBuilds;

			var validators = competitionConfig.Validators;
			var customToolchain = competitionConfig.Jobs.Any(j => !(j.Infrastructure.Toolchain is InProcessToolchain));
			if (!customToolchain &&
				!validators.Any(v => v is InProcessValidator))
			{
				validators.Insert(0, debugMode ? InProcessValidator.DontFailOnError : InProcessValidator.FailOnError);
			}

			if (debugMode)
			{
				validators.RemoveAll(v => v is JitOptimizationsValidator);
			}
			else if (competitionOptions.Adjustments.AdjustMetrics &&
				!validators.OfType<JitOptimizationsValidator>().Any())
			{
				validators.Insert(0, JitOptimizationsValidator.FailOnError);
			}

			Code.BugIf(
				validators.OfType<RunStateSlots>().Any(),
				"The config validators should not contain instances of RunStateSlots.");

			// DONTTOUCH: the RunStateSlots should be first in the chain.
			validators.Insert(0, new RunStateSlots());
		}

		private void FixConfigExporters(ManualCompetitionConfig competitionConfig)
		{
			// HACK: shuts up the ConfigValidator
			if (competitionConfig.Exporters.Count == 0)
			{
				competitionConfig.Exporters.Add(new StubExporter());
			}
		}

		private static void FixConfigAnalysers(ManualCompetitionConfig competitionConfig)
		{
			Code.BugIf(
				competitionConfig.Analysers.OfType<CompetitionAnalyser>().Any(),
				"The config analysers should not contain instances of CompetitionAnalyser.");

			// DONTTOUCH: these should be first analysers in the chain.
			competitionConfig.Analysers.Insert(0, ValidatorMessagesAnalyser.Instance);
			competitionConfig.Analysers.Insert(0, CompetitionPreconditionsAnalyser.Instance);

			// DONTTOUCH: the CompetitionAnnotateAnalyser should be last analyser in the chain.
			competitionConfig.Analysers.Add(CompetitionAnalyser.Instance);
		}

		private static void FixConfigMetrics(ManualCompetitionConfig competitionConfig)
		{
			var metrics = competitionConfig.Metrics;

			competitionConfig.Add(
				metrics.Select(m => m.GetColumnProvider()).Where(c => c != null).ToArray());

			competitionConfig.Add(
				metrics.Select(m => m.GetDiagnosers()).SelectMany(d => d).ToArray());
		}

		private static void FixConfigDuplicates(ManualCompetitionConfig competitionConfig)
		{
			RemoveDuplicates(competitionConfig.Analysers);
			RemoveDuplicates(competitionConfig.ColumnProviders);
			RemoveDuplicates(competitionConfig.Diagnosers);
			RemoveDuplicates(competitionConfig.Exporters);
			RemoveDuplicates(competitionConfig.Jobs);
			RemoveDuplicates(competitionConfig.Loggers);
			RemoveDuplicates(competitionConfig.Metrics, m => m.AttributeType);
			RemoveDuplicates(competitionConfig.Validators);
		}

		private static void RemoveDuplicates<T>(List<T> valuesList)
		{
			var visitedValues = new HashSet<T>();
			valuesList.RemoveAll(v => !visitedValues.Add(v));
		}

		private static void RemoveDuplicates<T, TKey>([NotNull] List<T> valuesList, [NotNull] Func<T, TKey> keySelector)
		{
			var visitedValues = new HashSet<TKey>();
			valuesList.RemoveAll(v => !visitedValues.Add(keySelector(v)));
		}

		/// <summary>Customize competition config.</summary>
		/// <param name="competitionConfig">The competition configuration.</param>
		protected virtual void InitCompetitionConfigOverride(ManualCompetitionConfig competitionConfig) { }
		#endregion

		#region Messages
		private void ReportMessagesToUser([NotNull] CompetitionState competitionState)
		{
			var criticalErrorMessages = GetMessageLines(
				competitionState, m => m.MessageSeverity > MessageSeverity.TestError, true);
			var hasCriticalMessages = criticalErrorMessages.Any();

			var testFailedMessages = GetMessageLines(
				competitionState, m => m.MessageSeverity == MessageSeverity.TestError, hasCriticalMessages);
			var hasTestFailedMessages = testFailedMessages.Any();

			var warningMessages = GetMessageLines(
				competitionState, m => m.MessageSeverity == MessageSeverity.Warning, true);
			var hasWarnings = warningMessages.Any();

			var infoMessages = GetMessageLines(competitionState, m => m.MessageSeverity < MessageSeverity.Warning, true);
			var hasInfo = infoMessages.Any();

			if (!(hasCriticalMessages || hasTestFailedMessages || hasWarnings))
				return;

			var allMessages = new List<string>();

			// TODO: simplify
			if (hasCriticalMessages)
			{
				allMessages.Add("Test completed with errors, details below.");
			}
			else if (hasTestFailedMessages)
			{
				allMessages.Add("Test failed, details below.");
			}
			else
			{
				allMessages.Add("Test completed with warnings, details below.");
			}

			if (hasCriticalMessages)
			{
				allMessages.Add("Errors:");
				allMessages.AddRange(criticalErrorMessages);
			}
			if (hasTestFailedMessages)
			{
				allMessages.Add("Failed assertions:");
				allMessages.AddRange(testFailedMessages);
			}
			if (hasWarnings)
			{
				allMessages.Add("Warnings:");
				allMessages.AddRange(warningMessages);
			}
			if (hasInfo)
			{
				allMessages.Add("Diagnostic messages:");
				allMessages.AddRange(infoMessages);
			}

			var messageText = allMessages.Join(Environment.NewLine);
			var runOptions = competitionState.Options.RunOptions;
			if (hasCriticalMessages)
			{
				ReportExecutionErrors(messageText, competitionState);
			}
			else if (hasTestFailedMessages || runOptions.ReportWarningsAsErrors)
			{
				ReportAssertionsFailed(messageText, competitionState);
			}
			else
			{
				ReportWarnings(messageText, competitionState);
			}
		}

		[NotNull, ItemNotNull]
		private string[] GetMessageLines(
			[NotNull] CompetitionState competitionState,
			[NotNull] Func<IMessage, bool> severityFilter,
			bool fromAllRuns)
		{
			var result =
				from message in competitionState.GetMessages()
				where severityFilter(message) && ShouldReport(message, competitionState.RunNumber, fromAllRuns)
				orderby message.RunNumber, message.RunMessageNumber
				select $"    * Run #{message.RunNumber}: {message.MessageText}";

			return result.ToArray();
		}

		private bool ShouldReport([NotNull] IMessage message, int runNumber, bool fromAllRuns)
		{
			if (fromAllRuns || message.RunNumber == runNumber)
				return true;

			// all of these on last run only.
			switch (message.MessageSource)
			{
				case MessageSource.Validator:
				case MessageSource.Analyser:
				case MessageSource.Diagnoser:
					return false;
				default:
					return true;
			}
		}
		#endregion
	}
}