using System;

using BenchmarkDotNet.Configs;

namespace CodeJam.PerfTests.Configs
{

	/// <summary>Competition config.</summary>
	public interface ICompetitionConfig : IConfig
	{
		// Runner config
		bool DebugMode { get; }
		bool DisableValidation { get; }
		bool ReportWarningsAsErrors { get; }
		int MaxRunsAllowed { get; }

		// Validation config
		bool AllowSlowBenchmarks { get; }
		bool EnableReruns { get; }
		bool LogAnnotationResults { get; }

		// Annotation config
		bool UpdateSourceAnnotations { get; }
		bool IgnoreExistingAnnotations { get; }
		string PreviousLogUri { get; }
	}
}