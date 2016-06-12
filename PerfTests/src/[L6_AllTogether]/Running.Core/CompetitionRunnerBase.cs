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
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Loggers;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Base class for competition benchmark runners</summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "VirtualMemberNeverOverriden.Global")]
	[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
	public abstract class CompetitionRunnerBase
	{
		/// <summary>Basic host logger implementation</summary>
		/// <seealso cref="Loggers.HostLogger"/>
		protected abstract class HostLogger : Loggers.HostLogger
		{
			/// <summary>Initializes a new instance of the <see cref="HostLogger"/> class.</summary>
			/// <param name="wrappedLogger">The logger to redirect the output. Cannot be null.</param>
			/// <param name="logMode">Host logging mode.</param>
			protected HostLogger([NotNull] ILogger wrappedLogger, HostLogMode logMode)
				: base(wrappedLogger, logMode) { }
		}

		#region Default values
		// DONTTOUCH: update xml docs for ICompetitionConfig.MaxRunsAllowed after changing the constant.
		private const int DefaultMaxRunsAllowed = 10;

		private static readonly TimeSpan _tooFastRunLimit = new TimeSpan(5); // 500 ns

		// DONTTOUCH: update xml docs for ICompetitionConfig.AllowLongRunningBenchmarks after changing the value.
		private static readonly TimeSpan _longRunLimit = TimeSpan.FromMilliseconds(500); // 500 ms
		private static readonly TimeSpan _allowLongRunLimit = TimeSpan.FromDays(1);
		#endregion

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="thisReference">Reference used to infer type of the benchmark.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>State of the run.</returns>
		public CompetitionState Run<T>([NotNull] T thisReference, [CanBeNull] ICompetitionConfig competitionConfig)
			where T : class =>
				RunCompetition(thisReference.GetType(), competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <typeparam name="T">Benchmark class to run.</typeparam>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>State of the run.</returns>
		public CompetitionState Run<T>([CanBeNull] ICompetitionConfig competitionConfig)
			where T : class =>
				RunCompetition(typeof(T), competitionConfig);

		/// <summary>Runs the benchmark.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">The competition config.</param>
		/// <returns>State of the run.</returns>
		public virtual CompetitionState RunCompetition(
			[NotNull] Type benchmarkType,
			[CanBeNull] ICompetitionConfig competitionConfig)
		{
			Code.NotNull(benchmarkType, nameof(benchmarkType));

			competitionConfig = competitionConfig ?? DefaultCompetitionConfig.Instance;

			ValidateCompetitionSetup(benchmarkType, competitionConfig);
			OnBeforeRun(benchmarkType, competitionConfig);

			var benchmarkConfig = CreateBenchmarkConfig(competitionConfig);
			var maxRunsAllowed = competitionConfig.MaxRunsAllowed;
			if (maxRunsAllowed <= 0)
			{
				maxRunsAllowed = DefaultMaxRunsAllowed;
			}

			bool runSucceed = false;
			CompetitionState competitionState = null;
			try
			{
				competitionState = CompetitionCore.Run(benchmarkType, benchmarkConfig, maxRunsAllowed);
				runSucceed = true;

				Code.AssertState(
					competitionState.Completed,
					"Bug: compettion state not marked as completed.");

				ProcessRunComplete(competitionConfig, competitionState);
			}
			finally
			{
				Code.AssertState(
					runSucceed == (competitionState != null),
					"Bug: compettion state does not match runSucceed flag.");

				OnAfterRun(runSucceed, benchmarkConfig, competitionState);

				var hostLogger = benchmarkConfig.GetLoggers().OfType<HostLogger>().Single();
				ReportHostLogger(hostLogger, competitionState?.LastRunSummary);
			}

			ReportMessagesToUser(competitionConfig, competitionState);

			return competitionState;
		}

		private void ValidateCompetitionSetup(
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
					logger.WriteLine();
					logger.WriteLineInfo("// All messages:");
					foreach (var message in messages)
					{
						logger.LogMessage(message);
					}
				}
				else
				{
					logger.WriteLine();
					logger.WriteLineInfo("// No messages in run.");
				}
			}
		}

		private void ReportMessagesToUser(
			[NotNull] ICompetitionConfig competitionConfig,
			[NotNull] CompetitionState competitionState)
		{
			var criticalErrorMessages = GetMessageLines(
				competitionState,
				m => m.MessageSeverity > MessageSeverity.TestError, false);
			bool hasCriticalMessages = criticalErrorMessages.Length > 0;

			var testFailedMessages = GetMessageLines(
				competitionState,
				m => m.MessageSeverity == MessageSeverity.TestError, hasCriticalMessages);
			bool hasTestFailedMessages = testFailedMessages.Length > 0;

			var warningMessages = GetMessageLines(
				competitionState,
				m => m.MessageSeverity == MessageSeverity.Warning, hasCriticalMessages);
			bool hasWarnings = warningMessages.Length > 0;

			var infoMessages = GetMessageLines(
				competitionState,
				m => m.MessageSeverity < MessageSeverity.Warning, hasCriticalMessages);
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
				ReportExecutionErrors(messageText);
			}
			else if (hasTestFailedMessages || competitionConfig.ReportWarningsAsErrors)
			{
				ReportAssertionsFailed(messageText);
			}
			else
			{
				ReportWarnings(messageText);
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

		#region Host-related logic
		/// <summary>Called before competition run.</summary>
		/// <param name="benchmarkType">Benchmark class to run.</param>
		/// <param name="competitionConfig">The competition config.</param>
		protected virtual void OnBeforeRun(
			[NotNull] Type benchmarkType,
			[NotNull] ICompetitionConfig competitionConfig) { }

		/// <summary>Called after competiton run.</summary>
		/// <param name="runSucceed">If set to <c>true</c> the run was succeed.</param>
		/// <param name="benchmarkConfig">The benchmark configuration.</param>
		/// <param name="competitionState">State of the run.</param>
		protected virtual void OnAfterRun(
			bool runSucceed,
			[NotNull] IConfig benchmarkConfig,
			[CanBeNull] CompetitionState competitionState) { }

		/// <summary>Creates a host logger.</summary>
		/// <param name="hostLogMode">The host log mode.</param>
		/// <returns>An instance of <seealso cref="HostLogger"/></returns>
		[NotNull]
		protected abstract HostLogger CreateHostLogger(HostLogMode hostLogMode);

		/// <summary>Reports content of the host logger to user.</summary>
		/// <param name="logger">The host logger.</param>
		/// <param name="summary">The summary to report.</param>
		protected abstract void ReportHostLogger([NotNull] HostLogger logger, [CanBeNull] Summary summary);

		/// <summary>Reports the execution errors to user.</summary>
		/// <param name="messages">The messages to report.</param>
		protected abstract void ReportExecutionErrors([NotNull] string messages);

		/// <summary>Reports failed assertions to user.</summary>
		/// <param name="messages">The messages to report.</param>
		protected abstract void ReportAssertionsFailed([NotNull] string messages);

		/// <summary>Reports warnings to user.</summary>
		/// <param name="messages">The messages to report.</param>
		protected abstract void ReportWarnings([NotNull] string messages);
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
			var hostLogger = CreateHostLogger(
				competitionConfig.DetailedLogging ? HostLogMode.AllMessages : HostLogMode.PrefixedOnly);
			result.Insert(0, hostLogger);
			return result;
		}

		private List<IColumn> GetColumns(ICompetitionConfig competitionConfig)
		{
			var result = OverrideColumns(competitionConfig);
			result.AddRange(
				new[]
				{
					StatisticColumn.Min,
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
			if (competitionConfig.GetJobs().Any(j => j.Toolchain is InProcessToolchain))
			{
				// ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
				if (competitionConfig.AllowDebugBuilds || !competitionConfig.UpdateSourceAnnotations)
				{
					result.Insert(0, InProcessValidator.DontFailOnError);
				}
				else
				{
					result.Insert(0, InProcessValidator.FailOnError);
				}
			}

			if (competitionConfig.AllowDebugBuilds || EnvironmentInfo.GetCurrent().HasAttachedDebugger)
			{
				result.RemoveAll(v => v is JitOptimizationsValidator);
			}
			else
			{
				if (result.All(v => !(v is JitOptimizationsValidator)))
				{
					result.Insert(0, JitOptimizationsValidator.FailOnError);
				}
			}
			result.Insert(0, new RunStateSlots());

			return result;
		}

		private List<IAnalyser> GetAnalysers(ICompetitionConfig competitionConfig)
		{
			var result = OverrideAnalysers(competitionConfig);

			// DONTTOUCH: the CompetitionAnnotateAnalyser should be last analyser in the chain.
			if (!competitionConfig.DontCheckAnnotations)
			{
				var annotator = new CompetitionAnnotateAnalyser
				{
					TooFastBenchmarkLimit = _tooFastRunLimit,
					LongRunningBenchmarkLimit = competitionConfig.AllowLongRunningBenchmarks ? _allowLongRunLimit : _longRunLimit,
					IgnoreExistingAnnotations = competitionConfig.IgnoreExistingAnnotations,
					LogCompetitionLimits = competitionConfig.LogCompetitionLimits,
					UpdateSourceAnnotations = competitionConfig.UpdateSourceAnnotations,
					PreviousRunLogUri = competitionConfig.PreviousRunLogUri
				};

				if (competitionConfig.RerunIfLimitsFailed)
				{
					annotator.MaxRerunsIfValidationFailed = 3;
					annotator.AdditionalRerunsOnAnnotate = 2;
				}

				result.Add(annotator);
			}

			return result;
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