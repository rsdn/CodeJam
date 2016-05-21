using System;
using System.Linq;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Portability;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Configs
{
	[PublicAPI]
	public class ManualCompetitionConfig : ManualConfig, ICompetitionConfig
	{
		public static readonly ICompetitionConfig Default = new ManualCompetitionConfig();

		public static ICompetitionConfig GetFullConfig(
			Type type,
			ICompetitionConfig config)
		{
			config = config ?? Default;
			if (type != null)
			{
				var typeAttributes = type.GetCustomAttributes(true).OfType<IConfigSource>();
				var assemblyAttributes = type.Assembly().GetCustomAttributes<IConfigSource>(false);
				var allAttributes = typeAttributes.Concat(assemblyAttributes);
				foreach (var configSource in allAttributes)
					config = Union(config, configSource.Config);
			}
			return config;
		}


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
			MaxRunsAllowed = config.MaxRunsAllowed;

			// Validation config
			AllowSlowBenchmarks = config.AllowSlowBenchmarks;
			DefaultCompetitionLimit = config.DefaultCompetitionLimit;
			EnableReruns = config.EnableReruns;

			// Annotation config
			AnnotateOnRun = config.AnnotateOnRun;
			IgnoreExistingAnnotations = config.IgnoreExistingAnnotations;
			LogAnnotationResults = config.LogAnnotationResults;
			PreviousLogUri = config.PreviousLogUri;
		}
		#endregion

		// Runner config
		public bool DebugMode { get; set; }
		public bool DisableValidation { get; set; }
		public int MaxRunsAllowed { get; set; } = 10;

		// Validation config
		public bool AllowSlowBenchmarks { get; set; }
		public CompetitionLimit DefaultCompetitionLimit { get; set; }
		public bool EnableReruns { get; set; }

		// Annotation config
		public bool AnnotateOnRun { get; set; }
		public bool IgnoreExistingAnnotations { get; set; }
		public bool LogAnnotationResults { get; set; }
		public string PreviousLogUri { get; set; }
	}
}