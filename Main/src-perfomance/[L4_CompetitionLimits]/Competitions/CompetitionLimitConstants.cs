using System;

using static BenchmarkDotNet.Loggers.HostLogger;

namespace BenchmarkDotNet.Competitions
{
	internal static class CompetitionLimitConstants
	{
		public const int EmptyValue = 0;
		public const int IgnoreValue = -1;

		public const string RatioFormat = "0.00";
		public const string ActualRatioFormat = "0.00#";

		#region XML metadata constants
		public const string LogAnnotationStart = LogImportantAreaStart + "------xml_annotation_begin------";
		public const string LogAnnotationEnd = LogImportantAreaEnd + "------xml_annotation_end------";
		public const string CompetitionBenchmarksRootNode = "CompetitionBenchmarks";
		public const string CompetitionNode = "Competition";
		public const string CandidateNode = "Candidate";
		public const string TargetAttribute = "Target";
		public const string MinRatioAttribute = "MinRatio";
		public const string MaxRatioAttribute = "MaxRatio";
		#endregion
	}
}