using System;

using BenchmarkDotNet.Competitions;

namespace BenchmarkDotNet.Configs
{
	public interface ICompetitionConfig : IConfig
	{
		// Runner config
		int MaxRunCount { get; }
		bool AllowSlowBenchmarks { get; }
		CompetitionLimit DefaultCompetitionLimit { get; }

		// Validation config
		bool DisableValidation { get; }
		bool RerunIfValidationFailed { get; }

		// Annotation config
		bool AnnotateOnRun { get; }
		bool IgnoreExistingAnnotations { get; }
	}
}