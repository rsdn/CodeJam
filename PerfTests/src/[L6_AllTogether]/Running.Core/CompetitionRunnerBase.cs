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
using CodeJam.PerfTests.Running.CompetitionLimitProviders;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Base class for competition benchmark runners</summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "VirtualMemberNeverOverriden.Global")]
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

			/// <summary>The logger to redirect the output.</summary>
			/// <value>The logger to redirect the output.</value>
			public new ILogger WrappedLogger => base.WrappedLogger;
		}

		#region Default values
		// DONTTOUCH: update xml docs for ICompetitionConfig.MaxRunsAllowed after changing the constant.
		private const int DefaultMaxRunsAllowed = 10;

		private static readonly TimeSpan _tooFastRunLimit = new TimeSpan(15); // 1500 ns

		// DONTTOUCH: update xml docs for ICompetitionConfig.AllowLongRunningBenchmarks after changing the value.
		private static readonly TimeSpan _longRunLimit = TimeSpan.FromMilliseconds(500); // 500 ms
		private static readonly TimeSpan _allowLongRunLimit = TimeSpan.FromDays(1);
		#endregion

		#region Public API
		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>State of the run.</returns>
		[NotNull]
		public CompetitionState Run<T>([NotNull] T thisReference, [CanBeNull] ICompetitionConfig competitionConfig)
			where T : class =>
				RunCompetition(thisReference.GetType(), competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>State of the run.</returns>
		[NotNull]
		public CompetitionState Run<T>([CanBeNull] ICompetitionConfig competitionConfig)
			where T : class =>
				RunCompetition(typeof(T), competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>State of the run.</returns>
		[NotNull]
		public virtual CompetitionState RunCompetition(
			[NotNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig competitionConfig)
		{
			Code.NotNull(benchmarkType, nameof(benchmarkType));

			competitionConfig = PrepareCompetitionConfig(competitionConfig);
			CheckCompetitionSetup(benchmarkType, competitionConfig);

			var benchmarkConfig = CreateBenchmarkConfig(competitionConfig);
			var hostLogger = benchmarkConfig.GetLoggers().OfType<HostLogger>().Single();

			CompetitionState competitionState = null;
			try
			{
				competitionState = CompetitionCore.Run(benchmarkType, benchmarkConfig, competitionConfig.MaxRunsAllowed);

				ProcessRunComplete(competitionConfig, competitionState);
			}
			finally
			{
				ReportHostLogger(hostLogger, competitionState?.LastRunSummary);
			}

			ReportMessagesToUser(hostLogger, competitionConfig, competitionState);

			return competitionState;
		}
		#endregion

		#region Prepare & run completed logic
		[NotNull]
		private ICompetitionConfig PrepareCompetitionConfig([CanBeNull] ICompetitionConfig competitionConfig)
		{
			var result = new ManualCompetitionConfig(competitionConfig);

			if (result.CompetitionLimitProvider == null)
			{
				result.CompetitionLimitProvider = PercentileLimitProvider.P90;
			}
			if (result.MaxRunsAllowed <= 0)
			{
				result.MaxRunsAllowed = DefaultMaxRunsAllowed;
			}

			return result.AsReadOnly();
		}

		private void CheckCompetitionSetup(
			[NotNull] Type benchmarkType,
			[NotNull] ICompetitionConfig competitionConfig)
		{
			if (!competitionConfig.AllowDebugBuilds && !EnvironmentInfo.GetCurrent().HasAttachedDebugger)
			{
				var assembly = benchmarkType.Assembly;
				if (assembly.IsDebugAssembly())
					throw CodeExceptions.InvalidOperation(
						$"Set the solution configuration into Release mode. Assembly {assembly.GetName().Name} was build as debug.");

				foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
				{
					var refAssembly = Assembly.Load(referencedAssemblyName);
					if (refAssembly.IsDebugAssembly())
						throw CodeExceptions.InvalidOperation(
							$"Set the solution configuration into Release mode. Assembly {refAssembly.GetName().Name} was build as debug.");
				}
			}
		}

		private void ProcessRunComplete(
			[NotNull] ICompetitionConfig competitionConfig,
			[NotNull] CompetitionState competitionState)
		{
			if (competitionConfig.DetailedLogging && competitionState.Logger != null)
			{
				var logger = competitionState.Logger;
				var messages = competitionState.GetMessages();

				if (messages.Any())
				{
					logger.WriteSeparatorLine();
					logger.WriteLineInfo("// All messages:");
					foreach (var message in messages)
					{
						logger.LogMessage(message);
					}
				}
				else
				{
					logger.WriteSeparatorLine();
					logger.WriteLineInfo("// No messages in run.");
				}
			}
		}
		#endregion

		#region Messages
		private void ReportMessagesToUser(
			HostLogger hostLogger,
			[NotNull] ICompetitionConfig competitionConfig,
			[NotNull] CompetitionState competitionState)
		{
			var criticalErrorMessages = GetMessageLines(
				competitionState,
				m => m.MessageSeverity > MessageSeverity.TestError, true);
			bool hasCriticalMessages = criticalErrorMessages.Length > 0;

			var testFailedMessages = GetMessageLines(
				competitionState,
				m => m.MessageSeverity == MessageSeverity.TestError, hasCriticalMessages);
			bool hasTestFailedMessages = testFailedMessages.Length > 0;

			var warningMessages = GetMessageLines(
				competitionState,
				m => m.MessageSeverity == MessageSeverity.Warning, true);
			bool hasWarnings = warningMessages.Length > 0;

			var infoMessages = GetMessageLines(
				competitionState,
				m => m.MessageSeverity < MessageSeverity.Warning, true);
			bool hasInfo = infoMessages.Length > 0;

			if (!(hasCriticalMessages || hasTestFailedMessages || hasWarnings))
				return;

			var allMessages = new List<string>();

			// TODO: simplify
			if (hasCriticalMessages)
			{
				allMessages.Add("Execution failed, details below.");
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
				ReportExecutionErrors(messageText, hostLogger);
			}
			else if (hasTestFailedMessages || competitionConfig.ReportWarningsAsErrors)
			{
				ReportAssertionsFailed(messageText, hostLogger);
			}
			else
			{
				ReportWarnings(messageText, hostLogger);
			}
		}

		private string[] GetMessageLines(CompetitionState competitionState, Func<IMessage, bool> filter, bool fromAllRuns)
		{
			var result =
				from message in competitionState.GetMessages()
				where fromAllRuns || ShouldReport(message, competitionState)
				where filter(message)
				orderby
					message.MessageSource,
					(int)message.MessageSeverity descending,
					message.RunNumber,
					message.RunMessageNumber
				select $"    * Run #{message.RunNumber}: {message.MessageText}";

			return result.ToArray();
		}

		private bool ShouldReport(IMessage message, CompetitionState competitionState) =>
			message.RunNumber == competitionState.RunNumber ||
				(message.MessageSource != MessageSource.Analyser && message.MessageSource != MessageSource.Diagnoser);
		#endregion

		#region Host-related abstract methods
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
		/// <param name="hostLogger">The host logger.</param>
		protected abstract void ReportExecutionErrors([NotNull] string messages, HostLogger hostLogger);

		/// <summary>Reports failed assertions to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="hostLogger">The host logger.</param>
		protected abstract void ReportAssertionsFailed([NotNull] string messages, HostLogger hostLogger);

		/// <summary>Reports warnings to user.</summary>
		/// <param name="messages">The messages to report.</param>
		/// <param name="hostLogger">The host logger.</param>
		protected abstract void ReportWarnings([NotNull] string messages, HostLogger hostLogger);
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
					BaselineDiffColumn.Scaled50,
					BaselineDiffColumn.Scaled85,
					BaselineDiffColumn.Scaled95,
					StatisticColumn.Max
				});

			return result;
		}

		private List<IValidator> GetValidators(ICompetitionConfig competitionConfig)
		{
			var result = OverrideValidators(competitionConfig);

			bool debugMode = competitionConfig.AllowDebugBuilds || EnvironmentInfo.GetCurrent().HasAttachedDebugger;

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

			if (competitionConfig.GetJobs().Any(j => j.Toolchain is InProcessToolchain))
			{
				result.Insert(
					0,
					debugMode ? InProcessValidator.DontFailOnError : InProcessValidator.FailOnError);
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

		private static CompetitionAnalyser GetCompetitionAnalyser(ICompetitionConfig competitionConfig)
		{
			var competitionAnalyser = competitionConfig.UpdateSourceAnnotations
				? new CompetitionAnnotateAnalyser
				{
					PreviousRunLogUri = competitionConfig.PreviousRunLogUri,
					AdditionalRerunsIfAnnotationsUpdated = competitionConfig.RerunIfLimitsFailed ? 2 : 0
				}
				: new CompetitionAnalyser();

			competitionAnalyser.TooFastBenchmarkLimit = _tooFastRunLimit;
			competitionAnalyser.LongRunningBenchmarkLimit = competitionConfig.AllowLongRunningBenchmarks
				? _allowLongRunLimit
				: _longRunLimit;

			competitionAnalyser.IgnoreExistingAnnotations = competitionConfig.IgnoreExistingAnnotations;
			competitionAnalyser.LogCompetitionLimits = competitionConfig.LogCompetitionLimits;
			competitionAnalyser.CompetitionLimitProvider = competitionConfig.CompetitionLimitProvider;

			competitionAnalyser.MaxRerunsIfValidationFailed = competitionConfig.RerunIfLimitsFailed
				? 3
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
	}
}