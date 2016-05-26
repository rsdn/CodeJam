using System;

using BenchmarkDotNet.Helpers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	[PublicAPI]
	public class CompetitionLimit
	{
		public const int EmptyValue = 0;
		public const int IgnoreValue = -1;

		public const string RatioFormat = "0.00";
		public const string ActualRatioFormat = "0.00#";

		public static readonly CompetitionLimit NoLimit = new CompetitionLimit(IgnoreValue, IgnoreValue);
		public static readonly CompetitionLimit Empty = new CompetitionLimit(EmptyValue, EmptyValue);

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

		public string MinText => IgnoreMin
			? Min.ToString(EnvironmentInfo.MainCultureInfo)
			: Min.ToString(RatioFormat, EnvironmentInfo.MainCultureInfo);
		public string MaxText => IgnoreMax
			? Max.ToString(EnvironmentInfo.MainCultureInfo)
			: Max.ToString(RatioFormat, EnvironmentInfo.MainCultureInfo);

		public bool IsEmpty => MinIsEmpty && MaxIsEmpty;
	}
}