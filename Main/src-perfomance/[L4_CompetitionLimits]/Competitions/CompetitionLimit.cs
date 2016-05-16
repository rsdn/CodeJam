using System;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Competitions
{
	[PublicAPI]
	public class CompetitionLimit
	{
		public static readonly CompetitionLimit NoLimit = new CompetitionLimit(
			CompetitionLimitConstants.IgnoreValue, CompetitionLimitConstants.IgnoreValue);

		public CompetitionLimit(
			double minRatio, double maxRatio)
		{
			if (minRatio < 0)
				minRatio = CompetitionLimitConstants.IgnoreValue;
			if (maxRatio < 0)
				maxRatio = CompetitionLimitConstants.IgnoreValue;

			Min = minRatio;
			Max = maxRatio;
		}

		public double Min { get; protected set; }
		public double Max { get; protected set; }

		// ReSharper disable once CompareOfFloatsByEqualityOperator
		public bool MinIsEmpty => Min == CompetitionLimitConstants.EmptyValue;
		// ReSharper disable once CompareOfFloatsByEqualityOperator
		public bool MaxIsEmpty => Max == CompetitionLimitConstants.EmptyValue;

		public bool IgnoreMin => Min < 0;
		public bool IgnoreMax => Max < 0;

		public bool IsEmpty => MinIsEmpty && MaxIsEmpty;
	}
}