using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>Helper class to trace competition analysis.</summary>
	[PublicAPI]
	public class MessageLogger : IMessageLogger
	{
		/// <summary>Initializes a new instance of the <see cref="MessageLogger" /> class.</summary>
		/// <param name="config">The config.</param>
		public MessageLogger([NotNull] IConfig config) : this(config, MessageSource.Analyser) { }

		/// <summary>Initializes a new instance of the <see cref="MessageLogger"/> class.</summary>
		/// <param name="config">The config.</param>
		/// <param name="messageSource">Source for the messages.</param>
		public MessageLogger([NotNull] IConfig config, MessageSource messageSource)
		{
			Code.NotNull(config, nameof(config));
			DebugEnumCode.Defined(messageSource, nameof(messageSource));

			RunState = CompetitionCore.RunState[config];
			MessageSource = messageSource;
		}

		#region Properties
		/// <summary>Source for the messages.</summary>
		/// <value>Source for the messages.</value>
		protected MessageSource MessageSource { get; }

		/// <summary>The state of the competition.</summary>
		/// <value>The state of the competition.</value>
		[NotNull]
		protected CompetitionState RunState { get; }
		#endregion

		#region IMessageLogger implementation
		/// <summary>Gets logger (can be used for direct log output).</summary>
		/// <value>The logger.</value>
		public ILogger Logger => RunState.Logger;

		/// <summary>Adds a message for the competition.</summary>
		/// <param name="messageSeverity">Severity of the message.</param>
		/// <param name="message">Text of the message.</param>
		/// <param name="hint">Hints for the message.</param>
		public void WriteMessage(MessageSeverity messageSeverity, string message, string hint = null) =>
			RunState.WriteMessageCore(MessageSource, messageSeverity, message, hint);
		#endregion
	}
}