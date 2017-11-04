using System;

using BenchmarkDotNet.Configs;

using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Helper class to trace competition analysis.</summary>
	[PublicAPI]
	public class Analysis : MessageLogger
	{
		/// <summary>Initializes a new instance of the <see cref="Analysis" /> class.</summary>
		/// <param name="config">The config.</param>
		public Analysis([NotNull] IConfig config)
			: this(config, MessageSource.Analyser) { }

		/// <summary>Initializes a new instance of the <see cref="Analysis"/> class.</summary>
		/// <param name="config">The config.</param>
		/// <param name="messageSource">Source for the messages.</param>
		public Analysis([NotNull] IConfig config, MessageSource messageSource) : base(config, messageSource)
		{
		}

		#region Properties
		/// <summary>The state of the competition.</summary>
		/// <value>The state of the competition.</value>
		[NotNull]
		public new CompetitionState RunState => base.RunState;

		/// <summary>Config for the competition.</summary>
		/// <value>The config.</value>
		[NotNull]
		public ICompetitionConfig Config => RunState.Config;

		/// <summary>Competition options.</summary>
		/// <value>Competition options.</value>
		[NotNull]
		public CompetitionOptions Options => RunState.Config.Options;

		/// <summary>Analysis has no execution or setup errors so far and can be safely performed.</summary>
		/// <value><c>true</c> if analysis has no errors; otherwise, <c>false</c>.</value>
		public bool SafeToContinue => !RunState.HasCriticalErrorsInRun;
		#endregion
	}
}