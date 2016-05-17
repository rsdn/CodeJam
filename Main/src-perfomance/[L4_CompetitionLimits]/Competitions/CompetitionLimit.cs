using System;

using JetBrains.Annotations;

using static BenchmarkDotNet.Competitions.CompetitionLimitConstants;

namespace BenchmarkDotNet.Competitions
{
	[PublicAPI]
	public class CompetitionLimit
	{
		public static readonly CompetitionLimit NoLimit = new CompetitionLimit(IgnoreValue, IgnoreValue);

		public CompetitionLimit(
			double minRatio, double maxRatio)
		{
			if (minRatio < 0)
				minRatio = IgnoreValue;
			if (maxRatio < 0)
				maxRatio = IgnoreValue;

			Min = minRatio;
			Max = maxRatio;
		}

		public double Min { get; protected set; }
		public double Max { get; protected set; }

		// ReSharper disable once CompareOfFloatsByEqualityOperator
		public bool MinIsEmpty => Min == EmptyValue;
		// ReSharper disable once CompareOfFloatsByEqualityOperator
		public bool MaxIsEmpty => Max == EmptyValue;

		public bool IgnoreMin => Min < 0;
		public bool IgnoreMax => Max < 0;

		public bool IsEmpty => MinIsEmpty && MaxIsEmpty;
	}
}