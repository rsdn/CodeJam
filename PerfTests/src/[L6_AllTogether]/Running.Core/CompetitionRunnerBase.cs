using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Validators;

using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Columns;
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;
using CodeJam.PerfTests.Running.Limits;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Base class for competition benchmark runners</summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "VirtualMemberNeverOverriden.Global")]
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	public abstract class CompetitionRunnerBase
	{
		/// <summary>Base class for competition runner's host logger.</summary>
		/// <seealso cref="Loggers.HostLogger"/>
		protected abstract class HostLogger : Loggers.HostLogger
		{
			/// <summary>Initializes a new instance of the <see cref="HostLogger"/> class.</summary>
			/// <param name="wrappedLogger">The logger to redirect the output. Cannot be null.</param>
			/// <param name="logMode">Host logging mode.</param>
			protected HostLogger([NotNull] ILogger wrappedLogger, HostLogMode logMode)
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

		private static ManualCompetitionConfig GetFirstConfig(
			[CanBeNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig customConfig)
		{
			if (customConfig != null)
				return new ManualCompetitionConfig(customConfig);

			var configSource = benchmarkType?.TryGetMetadataAttribute<ICompetitionConfigSource>();

			return new ManualCompetitionConfig(
				configSource?.Config ??
					CompetitionHelpers.DefaultConfig);
		}
		#endregion

		#region Public API (expose these via Competiton classes)
		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="competitionConfig">
		/// The competition config.
		/// If<c>null</c> config from <see cref="CompetitionConfigAttribute"/> will be used.
		/// </param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public CompetitionState Run<T>([CanBeNull] ICompetitionConfig competitionConfig = null)
			where T : class =>
				RunCore(typeof(T), competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionConfig">
		/// The competition config.
		/// If<c>null</c> config from <see cref="CompetitionConfigAttribute"/> will be used.
		/// </param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public CompetitionState Run<T>(
			[NotNull] T thisReference,
			[CanBeNull] ICompetitionConfig competitionConfig = null)
			where T : class =>
				RunCore(thisReference.GetType(), competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">
		/// The competition config.
		/// If<c>null</c> config from <see cref="CompetitionConfigAttribute"/> will be used.
		/// </param>
		/// <returns>The state of the competition.</returns>
		[NotNull]
		public CompetitionState Run(
			[NotNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig competitionConfig = null) =>
				RunCore(benchmarkType, competitionConfig);
		#endregion

		#region Advanced public API (expose these if you wish)
		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="assembly">Assembly with benchmarks to run.</param>
		/// <param name="competitionConfig">
		/// The competition config.
		/// If<c>null</c> config from <see cref="CompetitionConfigAttribute"/> will be used.
		/// </param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public IReadOnlyDictionary<Type, CompetitionState> Run(
			[NotNull] Assembly assembly,
			[CanBeNull] ICompetitionConfig competitionConfig = null) =>
				Run(BenchmarkHelpers.GetBenchmarkTypes(assembly), competitionConfig);

		/// <summary>Runs all benchmarks defined in the assembly.</summary>
		/// <param name="benchmarkTypes">Benchmark classes to run.</param>
		/// <param name="competitionConfig">
		/// The competition config.
		/// If<c>null</c> config from <see cref="CompetitionConfigAttribute"/> will be used.
		/// </param>
		/// <returns>The state of the competition for each benchmark that was run.</returns>
		[NotNull]
		public IReadOnlyDictionary<Type, CompetitionState> Run(
			[NotNull] Type[] benchmarkTypes,
			[CanBeNull] ICompetitionConfig competitionConfig = null)
		{
			var result = new Dictionary<Type, CompetitionState>();

			foreach (var benchmarkType in benchmarkTypes)
			{
				result[benchmarkType] = RunCore(benchmarkType, competitionConfig);
			}

			return result;
		}
		#endregion

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">
		/// The competition config.
		/// If<c>null</c> config from <see cref="CompetitionConfigAttribute"/> will be used.
		/// </param>
		/// <returns>State of the run.</returns>
		[NotNull]
		private CompetitionState RunCore(
			[NotNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig competitionConfig)
		{
			Code.NotNull(benchmarkType, nameof(benchmarkType));

			competitionConfig = PrepareCompetitionConfig(benchmarkType, competitionConfig);
			var benchmarkConfig = CreateBenchmarkConfig(competitionConfig);
			var hostLogger = benchmarkConfig.GetLoggers().OfType<HostLogger>().Single();

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
					competitionState = CompetitionCore.Run(
						benchmarkType, benchmarkConfig,
						competitionConfig.MaxRunsAllowed,
						competitionConfig.AllowDebugBuilds,
						competitionConfig.ConcurrentRunBehavior);

					ProcessRunComplete(competitionConfig, competitionState);
				}
				finally
				{
					ReportHostLogger(hostLogger, competitionState?.LastRunSummary);
				}

				using (Loggers.HostLogger.BeginLogImportant(benchmarkConfig))
				{
					ReportMessagesToUser(competitionConfig, competitionState);
				}
			}
			finally
			{
				Loggers.HostLogger.FlushLoggers(benchmarkConfig);
				SetCurrentDirectoryIfNotNull(previousDirectory);
			}

			return competitionState;
		}

		#region Prepare & run completed logic
		[NotNull]
		private ICompetitionConfig PrepareCompetitionConfig(
			[NotNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig competitionConfig)
		{
			var result = GetFirstConfig(benchmarkType, competitionConfig);

			if (result.CompetitionLimitProvider == null)
			{
				result.CompetitionLimitProvider = LogNormalLimitProvider.Instance;
			}

			if (result.MaxRunsAllowed <= 0)
			{
				result.MaxRunsAllowed = DefaultMaxRunsAllowed;
			}

			if (HostEnvironmentInfo.GetCurrent().HasAttachedDebugger)
			{
				result.AllowDebugBuilds = true;
			}

			return result.AsReadOnly();
		}

		private void ProcessRunComplete(
			[NotNull] ICompetitionConfig competitionConfig,
			[NotNull] CompetitionState competitionState)
		{
			var logger = competitionState.Logger;
			var summary = competitionState.LastRunSummary;

			if (logger == null)
				return;

			if (competitionConfig.DetailedLogging)
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
					logger.WriteLineInfo("{LogVerbosePrefix} No messages in run.");
				}
			}
			else if (summary != null)
			{
				using (Loggers.HostLogger.BeginLogImportant(summary.Config))
				{
					var summarylogger = DumpSummaryToHostLogger
						? logger
						: new CompositeLogger(
							summary.Config
								.GetLoggers()
								.Where(l => !(l is HostLogger))
								.ToArray());

					// Dumping the benchmark results to console
					summarylogger.WriteSeparatorLine("Summary");
					MarkdownExporter.Console.ExportToLog(summary, summarylogger);
				}
			}
		}
		#endregion

		#region Override test running behavior
		/// <summary>Returns output directory that should be used for running the test.</summary>
		/// <param name="targetAssembly">The target assembly tests will be run for.</param>
		/// <returns>
		/// Output directory that should be used for running the test or <c>null</c> if the current directory should be used.
		/// </returns>
		protected virtual string GetOutputDirectory(Assembly targetAssembly) => null;

		/// <summary>Gets a value indicating whether the last run summary should be dumped into host logger.</summary>
		/// <value>
		/// <c>true</c> if the last run summary should be dumped into host logger; otherwise, <c>false</c>.
		/// </value>
		protected virtual bool DumpSummaryToHostLogger => true;

		/// <summary>Default timing limit to detect too fast benchmarks.</summary>
		/// <value>The default timing limit to detect too fast benchmarks.</value>
		// DONTTOUCH: update xml docs for ICompetitionConfig.MaxRunsAllowed after changing the constant.
		protected virtual int DefaultMaxRunsAllowed => 10;

		/// <summary>Timing limit for too fast runs.</summary>
		/// <value>The timing limit for too fast runs</value>
		protected virtual TimeSpan DefaultTooFastBenchmarkLimit => new TimeSpan(15); // 1500 ns

		private static readonly TimeSpan _defaultLongRunsAllowedLimit = TimeSpan.FromDays(1);

		/// <summary>Default timing limit to detect long-running benchmarks.</summary>
		/// <value>The default timing limit to detect long-running benchmarks.</value>
		// DONTTOUCH: update xml docs for ICompetitionConfig.AllowLongRunningBenchmarks after changing the value.
		protected virtual TimeSpan DefaultLongRunningBenchmarkLimit => TimeSpan.FromMilliseconds(500); // 500 ms

		/// <summary>Maximum count of retries performed if the limit checking failed.</summary>
		/// <value>The maximum count of retries performed if the validation failed.</value>
		protected virtual int DefaultMaxRerunsIfValidationFailed => 3;

		/// <summary>Count of runs skipped before source annotations will be applied.</summary>
		/// <value>The count of runs performed before updating the limits annotations.</value>
		protected virtual int DefaultSkipRunsBeforeApplyingAnnotations => 0;

		/// <summary>Count of additional runs performed after updating source annotations.</summary>
		/// <value>The count of additional runs performed after updating the limits annotations.</value>
		protected virtual int DefaultAdditionalRerunsIfAnnotationsUpdated => 2;
		#endregion

		#region Host-related logic
		/// <summary>Creates a host logger.</summary>
		/// <param name="hostLogMode">The host log mode.</param>
		/// <returns>An instance of <see cref="HostLogger"/></returns>
		[NotNull]
		protected abstract HostLogger CreateHostLogger(HostLogMode hostLogMode);

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
		private IConfig CreateBenchmarkConfig(ICompetitionConfig competitionConfig)
		{
			// ReSharper disable once UseObjectOrCollectionInitializer
			var result = new ManualConfig();

			// TODO: to overridable methods?
			result.KeepBenchmarkFiles = competitionConfig.KeepBenchmarkFiles;
			result.UnionRule = competitionConfig.UnionRule;
			result.Set(competitionConfig.GetOrderProvider());

			result.Add(OverrideJobs(competitionConfig).ToArray());
			result.Add(OverrideExporters(competitionConfig).ToArray());
			result.Add(OverrideDiagnosers(competitionConfig).ToArray());

			result.Add(GetLoggers(competitionConfig).ToArray());
			result.Add(GetColumns(competitionConfig).ToArray());
			result.Add(GetValidators(competitionConfig).ToArray());
			result.Add(GetAnalysers(competitionConfig).ToArray());

			return result.AsReadOnly();
		}

		private List<ILogger> GetLoggers(ICompetitionConfig competitionConfig)
		{
			var result = OverrideLoggers(competitionConfig);

			var hostLogMode = competitionConfig.DetailedLogging ? HostLogMode.AllMessages : HostLogMode.PrefixedOnly;
			result.Insert(0, CreateHostLogger(hostLogMode));

			return result;
		}

		private List<IColumn> GetColumns(ICompetitionConfig competitionConfig)
		{
			// TODO: better columns.
			var result = OverrideColumns(competitionConfig);
			result.AddRange(
				new[]
				{
					StatisticColumn.Min,
					new CompetitionLimitColumn(competitionConfig.CompetitionLimitProvider, false),
					new CompetitionLimitColumn(competitionConfig.CompetitionLimitProvider, true),
					StatisticColumn.Max
				});

			return result;
		}

		private List<IValidator> GetValidators(ICompetitionConfig competitionConfig)
		{
			var result = OverrideValidators(competitionConfig);

			bool debugMode = competitionConfig.AllowDebugBuilds;

			if (result.Any(v => v is JitOptimizationsValidator))
			{
				if (debugMode)
				{
					result.RemoveAll(v => v is JitOptimizationsValidator && v.TreatsWarningsAsErrors);
				}
			}
			else if (competitionConfig.UpdateSourceAnnotations && !debugMode)
			{
				result.Insert(0, JitOptimizationsValidator.FailOnError);
			}

			if (competitionConfig.GetJobs().Any(j => j.Toolchain is InProcessToolchain) &&
				!result.Any(v => v is InProcessValidator))
			{
				result.Insert(0, debugMode ? InProcessValidator.DontFailOnError : InProcessValidator.FailOnError);
			}

			// DONTTOUCH: the RunStateSlots should be first in the chain.
			result.Insert(0, new RunStateSlots());

			return result;
		}

		private List<IAnalyser> GetAnalysers(ICompetitionConfig competitionConfig)
		{
			var result = OverrideAnalysers(competitionConfig);

			// DONTTOUCH: the CompetitionAnnotateAnalyser should be last analyser in the chain.
			result.Add(GetCompetitionAnalyser(competitionConfig));

			return result;
		}

		private CompetitionAnalyser GetCompetitionAnalyser(ICompetitionConfig competitionConfig)
		{
			var competitionAnalyser = competitionConfig.UpdateSourceAnnotations
				? new CompetitionAnnotateAnalyser
				{
					PreviousRunLogUri = competitionConfig.PreviousRunLogUri,
					AdditionalRerunsIfAnnotationsUpdated = competitionConfig.RerunIfLimitsFailed
						? DefaultAdditionalRerunsIfAnnotationsUpdated
						: 0,
					SkipRunsBeforeApplyingAnnotations = DefaultSkipRunsBeforeApplyingAnnotations
				}
				: new CompetitionAnalyser();

			competitionAnalyser.TooFastBenchmarkLimit = DefaultTooFastBenchmarkLimit;
			competitionAnalyser.LongRunningBenchmarkLimit = competitionConfig.AllowLongRunningBenchmarks
				? _defaultLongRunsAllowedLimit
				: DefaultLongRunningBenchmarkLimit;

			competitionAnalyser.IgnoreExistingAnnotations = competitionConfig.IgnoreExistingAnnotations;
			competitionAnalyser.LogCompetitionLimits = competitionConfig.LogCompetitionLimits;
			competitionAnalyser.CompetitionLimitProvider = competitionConfig.CompetitionLimitProvider;

			competitionAnalyser.MaxRerunsIfValidationFailed = competitionConfig.RerunIfLimitsFailed
				? DefaultMaxRerunsIfValidationFailed
				: 0;

			return competitionAnalyser;
		}

		#region Override config parameters
		/// <summary>Override competition jobs.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The jobs for the competition</returns>
		protected virtual List<IJob> OverrideJobs([NotNull] ICompetitionConfig competitionConfig) =>
			competitionConfig.GetJobs().ToList();

		/// <summary>Override competition exporters.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The jobs for the competition</returns>
		protected virtual List<IExporter> OverrideExporters([NotNull] ICompetitionConfig competitionConfig) =>
			competitionConfig.GetExporters().ToList();

		/// <summary>Override competition diagnosers.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The diagnosers for the competition</returns>
		protected virtual List<IDiagnoser> OverrideDiagnosers([NotNull] ICompetitionConfig competitionConfig) =>
			competitionConfig.GetDiagnosers().ToList();

		/// <summary>Override competition loggers.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The loggers for the competition</returns>
		protected virtual List<ILogger> OverrideLoggers([NotNull] ICompetitionConfig competitionConfig) =>
			competitionConfig.GetLoggers().ToList();

		/// <summary>Override competition columns.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The columns for the competition</returns>
		protected virtual List<IColumn> OverrideColumns([NotNull] ICompetitionConfig competitionConfig) =>
			competitionConfig.GetColumns().ToList();

		/// <summary>Override competition validators.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The validators for the competition</returns>
		protected virtual List<IValidator> OverrideValidators([NotNull] ICompetitionConfig competitionConfig) =>
			competitionConfig.GetValidators().ToList();

		/// <summary>Override competition analysers.</summary>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>The analysers for the competition</returns>
		protected virtual List<IAnalyser> OverrideAnalysers([NotNull] ICompetitionConfig competitionConfig) =>
			competitionConfig.GetAnalysers().ToList();
		#endregion

		#endregion

		#region Messages
		private void ReportMessagesToUser(
			[NotNull] ICompetitionConfig competitionConfig,
			[NotNull] CompetitionState competitionState)
		{
			var criticalErrorMessages = GetMessageLines(
				competitionState, m => m.MessageSeverity > MessageSeverity.TestError, true);
			bool hasCriticalMessages = criticalErrorMessages.Any();

			var testFailedMessages = GetMessageLines(
				competitionState, m => m.MessageSeverity == MessageSeverity.TestError, hasCriticalMessages);
			bool hasTestFailedMessages = testFailedMessages.Any();

			var warningMessages = GetMessageLines(
				competitionState, m => m.MessageSeverity == MessageSeverity.Warning, true);
			bool hasWarnings = warningMessages.Any();

			var infoMessages = GetMessageLines(competitionState, m => m.MessageSeverity < MessageSeverity.Warning, true);
			bool hasInfo = infoMessages.Any();

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

			var messageText = string.Join(Environment.NewLine, allMessages);
			if (hasCriticalMessages)
			{
				ReportExecutionErrors(messageText, competitionState);
			}
			else if (hasTestFailedMessages || competitionConfig.ReportWarningsAsErrors)
			{
				ReportAssertionsFailed(messageText, competitionState);
			}
			else
			{
				ReportWarnings(messageText, competitionState);
			}
		}

		private string[] GetMessageLines(
			CompetitionState competitionState,
			Func<IMessage, bool> severityFilter,
			bool fromAllRuns)
		{
			var result = from message in competitionState.GetMessages()
				where severityFilter(message) && ShouldReport(message, competitionState.RunNumber, fromAllRuns)
				orderby message.RunNumber, message.RunMessageNumber
				select $"    * Run #{message.RunNumber}: {message.MessageText}";

			return result.ToArray();
		}

		private bool ShouldReport(IMessage message, int runNumber, bool fromAllRuns)
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