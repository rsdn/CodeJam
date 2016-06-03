using System;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>Default competition config.</summary>
	public class DefaultCompetitionConfig : ReadOnlyCompetitionConfig
	{
		/// <summary>Instance of the <see cref="DefaultCompetitionConfig"/> class.</summary>
		public static readonly ICompetitionConfig Instance = new DefaultCompetitionConfig();

		/// <summary>Initializes a new instance of the <see cref="DefaultCompetitionConfig"/> class.</summary>
		public DefaultCompetitionConfig() : base(new ManualCompetitionConfig())
		{ }
	}
}