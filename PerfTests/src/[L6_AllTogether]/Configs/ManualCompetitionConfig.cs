using System;

using BenchmarkDotNet.Configs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	[PublicAPI]
	public class ManualCompetitionConfig : ManualConfig, ICompetitionConfig
	{
		public static readonly ICompetitionConfig Default = new ManualCompetitionConfig();

		public static ManualCompetitionConfig Union(
			ICompetitionConfig globalConfig, IConfig localConfig)
		{
			var competitionConfig = new ManualCompetitionConfig();
			switch (localConfig.UnionRule)
			{
				case ConfigUnionRule.AlwaysUseLocal:
					competitionConfig.Add(localConfig);
					break;
				case ConfigUnionRule.AlwaysUseGlobal:
					competitionConfig.Add(globalConfig);
					break;
				case ConfigUnionRule.Union:
					competitionConfig.Add(globalConfig);
					competitionConfig.Add(localConfig);
					break;
			}
			return competitionConfig;
		}

		#region Ctor & Add()
		public ManualCompetitionConfig() { }

		public ManualCompetitionConfig(IConfig config)
		{
			if (config != null)
			{
				Add(config);
			}
		}

		// TODO: as override
		public new void Add(IConfig config)
		{
			var competitionConfig = config as ICompetitionConfig;
			if (competitionConfig != null)
			{
				base.Add(config);
				AddCompetitionProperties(competitionConfig);
			}
			else
			{
				base.Add(config);
			}
		}

		private void AddCompetitionProperties(ICompetitionConfig config)
		{
			// Runner config
			DebugMode = config.DebugMode;
			DisableValidation = config.DisableValidation;
			ReportWarningsAsErrors = config.ReportWarningsAsErrors;
			MaxRunsAllowed = config.MaxRunsAllowed;

			// Validation config
			AllowSlowBenchmarks = config.AllowSlowBenchmarks;
			EnableReruns = config.EnableReruns;
			LogAnnotationResults = config.LogAnnotationResults;

			// Annotation config
			UpdateSourceAnnotations = config.UpdateSourceAnnotations;
			IgnoreExistingAnnotations = config.IgnoreExistingAnnotations;
			PreviousLogUri = config.PreviousLogUri;
		}
		#endregion

		// Runner config
		public bool DebugMode { get; set; }
		public bool DisableValidation { get; set; }
		public bool ReportWarningsAsErrors { get; set; }
		public int MaxRunsAllowed { get; set; } = 10;

		// Validation config
		public bool AllowSlowBenchmarks { get; set; }
		public bool EnableReruns { get; set; }
		public bool LogAnnotationResults { get; set; }

		// Annotation config
		public bool UpdateSourceAnnotations { get; set; }
		public bool IgnoreExistingAnnotations { get; set; }
		public string PreviousLogUri { get; set; }
	}
}