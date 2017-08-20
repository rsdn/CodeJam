using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Helper class to trace competition analysis.</summary>
	[PublicAPI]
	public class Analysis : IMessageLogger
	{
		/// <summary>Initializes a new instance of the <see cref="Analysis" /> class.</summary>
		/// <param name="config">The config.</param>
		public Analysis([NotNull] IConfig config)
			: this(config, MessageSource.Analyser) { }

		/// <summary>Initializes a new instance of the <see cref="Analysis"/> class.</summary>
		/// <param name="config">The config.</param>
		/// <param name="messageSource">Source for the messages.</param>
		public Analysis([NotNull] IConfig config, MessageSource messageSource)
		{
			Code.NotNull(config, nameof(config));
			DebugEnumCode.Defined(messageSource, nameof(messageSource));

			RunState = CompetitionCore.RunState[config];
			MessageSource = messageSource;
		}

		#region Properties
		/// <summary>Source for the messages.</summary>
		/// <value>Source for the messages.</value>
		public MessageSource MessageSource { get; }

		/// <summary>The state of the competition.</summary>
		/// <value>The state of the competition.</value>
		[NotNull]
		public CompetitionState RunState { get; }

		/// <summary>Config for the competition.</summary>
		/// <value>The config.</value>
		public ICompetitionConfig Config => RunState.Config;

		/// <summary>Competition options.</summary>
		/// <value>Competition options.</value>
		public CompetitionOptions Options => RunState.Config.Options;

		/// <summary>Analysis has no execution or setup errors so far and can be safely performed.</summary>
		/// <value><c>true</c> if analysis has no errors; otherwise, <c>false</c>.</value>
		public bool SafeToContinue => !RunState.HasCriticalErrorsInRun;
		#endregion

		#region Warnings
		/// <summary>Reports test error conclusion.</summary>
		/// <param name="message">Message text.</param>
		/// <param name="report">The report the message belongs to.</param>
		public virtual void AddTestErrorConclusion(
			[NotNull] string message,
			BenchmarkReport report = null)
		{
			this.WriteTestErrorMessage(message);
		}

		/// <summary>Reports test error conclusion.</summary>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">Message text.</param>
		/// <param name="report">The report the message belongs to.</param>
		public virtual void AddTestErrorConclusion(
			[NotNull] Target target,
			[NotNull] string message,
			BenchmarkReport report = null)
		{
			this.WriteTestErrorMessage(target, message);
		}

		/// <summary>Reports analyser warning conclusion.</summary>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hint how to fix the warning.</param>
		/// <param name="report">The report the message belongs to.</param>
		public virtual void AddWarningConclusion(
			[NotNull] string message,
			[NotNull] string hint,
			BenchmarkReport report = null)
		{
			this.WriteWarningMessage(message, hint);
		}

		/// <summary>Reports analyser warning conclusion.</summary>
		/// <param name="target">Target the message applies for.</param>
		/// <param name="message">Message text.</param>
		/// <param name="hint">Hint how to fix the warning.</param>
		/// <param name="report">The report the message belongs to.</param>
		public virtual void AddWarningConclusion(
			[NotNull] Target target,
			[NotNull] string message,
			[NotNull] string hint,
			BenchmarkReport report = null)
		{
			this.WriteWarningMessage(target, message, hint);
		}
		#endregion

		#region IMessageLogger implementation
		/// <summary>Gets logger (can be used for direct log output).</summary>
		/// <value>The logger.</value>
		public ILogger Logger => RunState.Logger;

		/// <summary>Adds a message for the competition.</summary>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="message">Text of the message.</param>
		/// <param name="hint">Hints for the message.</param>
		public void WriteMessage(MessageSeverity messageSeverity, string message, string hint = null) => RunState.WriteMessage(MessageSource, messageSeverity, message, hint);
		#endregion
	}
}