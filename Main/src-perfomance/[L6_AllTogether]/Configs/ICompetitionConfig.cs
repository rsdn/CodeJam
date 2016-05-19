using System;

using BenchmarkDotNet.Competitions;

namespace BenchmarkDotNet.Configs
{
	public interface ICompetitionConfig : IConfig
	{
		// Runner config
		bool DetailedLogging { get; }
		bool DisableValidation { get; }
		int MaxRunsAllowed { get; }

		// Validation config
		bool AllowSlowBenchmarks { get; }
		CompetitionLimit DefaultCompetitionLimit { get; }
		bool EnableReruns { get; }

		// Annotation config
		bool AnnotateOnRun { get; }
		bool IgnoreExistingAnnotations { get; }
		bool LogAnnotationResults { get; }
	}
}