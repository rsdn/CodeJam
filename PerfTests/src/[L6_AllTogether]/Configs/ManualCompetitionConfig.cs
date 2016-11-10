using System;

using BenchmarkDotNet.Configs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Class for competition config config creation</summary>
	[PublicAPI]
	public sealed class ManualCompetitionConfig : ManualConfig, ICompetitionConfig
	{
		private CompetitionMode _competitionMode;

		#region Ctor & Add()
		/// <summary>Initializes a new instance of the <see cref="ManualCompetitionConfig"/> class.</summary>
		public ManualCompetitionConfig() { }

		/// <summary>Initializes a new instance of the <see cref="ManualCompetitionConfig"/> class.</summary>
		/// <param name="config">The config to init from.</param>
		public ManualCompetitionConfig([CanBeNull] IConfig config)
		{
			Add(config);
		}

		// TODO: as override
		/// <summary>Fills properties from the specified config.</summary>
		/// <param name="config">The config to init from.</param>
		public new void Add([CanBeNull] IConfig config)
		{
			if (config == null)
				return;

			base.Add(config);

			var competitionConfig = config as ICompetitionConfig;
			if (competitionConfig != null)
			{
				Add(competitionConfig.CompetitionMode);
			}
		}
		#endregion

		/// <summary>Applies the specified competition mode.</summary>
		/// <param name="competitionMode">The competition mode.</param>
		public void Add(CompetitionMode competitionMode) => 
			CompetitionMode = competitionMode == null
				? null 
				: new CompetitionMode(CompetitionMode, competitionMode);

		/// <summary>Competition parameters.</summary>
		/// <value>Competition parameters.</value>
		public CompetitionMode CompetitionMode
		{
			get
			{
				return _competitionMode ?? CompetitionMode.Default;
			}
			set
			{
				_competitionMode = value?.Freeze();
			}
		}

		/// <summary>Returns read-only wrapper for the config.</summary>
		/// <returns>Read-only wrapper for the config</returns>
		public ICompetitionConfig AsReadOnly() => new ReadOnlyCompetitionConfig(this);
	}
}