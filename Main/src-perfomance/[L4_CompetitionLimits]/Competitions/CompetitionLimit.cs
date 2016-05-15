using System;

using BenchmarkDotNet.Analysers;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Competitions.Limits
{
	[PublicAPI]
	public class CompetitionLimit
	{
		public static readonly CompetitionLimit NoLimit = new CompetitionLimit(
			CompetitionLimitsAnalyserHelpers.IgnoreValue, CompetitionLimitsAnalyserHelpers.IgnoreValue);

		public CompetitionLimit(
			double minRatio, double maxRatio)
		{
			if (minRatio < 0)
				minRatio = CompetitionLimitsAnalyserHelpers.IgnoreValue;
			if (maxRatio < 0)
				maxRatio = CompetitionLimitsAnalyserHelpers.IgnoreValue;

			Min = minRatio;
			Max = maxRatio;
		}

		public double Min { get; protected set; }
		public double Max { get; protected set; }

		// ReSharper disable once CompareOfFloatsByEqualityOperator
		public bool MinIsEmpty => Min == CompetitionLimitsAnalyserHelpers.EmptyValue;
		// ReSharper disable once CompareOfFloatsByEqualityOperator
		public bool MaxIsEmpty => Max == CompetitionLimitsAnalyserHelpers.EmptyValue;

		public bool IgnoreMin => Min < 0;
		public bool IgnoreMax => Max < 0;

		public bool IsEmpty => MinIsEmpty && MaxIsEmpty;
	}
}