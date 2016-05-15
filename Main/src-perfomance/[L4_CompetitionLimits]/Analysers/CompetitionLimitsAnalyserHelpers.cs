using System;

namespace BenchmarkDotNet.Analysers
{
	internal static class CompetitionLimitsAnalyserHelpers
	{
		public const int EmptyValue = 0;
		public const int IgnoreValue = -1;
		public const string RatioFormat = "0.00###";

		#region XML metadata constants
		public const string CompetitionBenchmarksRootNode = "CompetitionBenchmarks";
		public const string CompetitionNode = "Competition";
		public const string CandidateNode = "Candidate";
		public const string TargetAttribute = "Target";
		public const string MinRatioAttribute = "MinRatio";
		public const string MaxRatioAttribute = "MaxRatio";
		#endregion
	}
}