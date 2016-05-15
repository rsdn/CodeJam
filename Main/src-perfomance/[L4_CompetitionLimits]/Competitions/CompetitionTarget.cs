using System;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Running;

namespace BenchmarkDotNet.Competitions.Limits
{
	internal class CompetitionTarget : CompetitionLimit
	{
		#region Fields & .ctor
		public CompetitionTarget() : base(0, 0) { }

		public CompetitionTarget(Target target, CompetitionLimit other, bool usesResourceAnnotation) :
			this(target, other.Min, other.Max, usesResourceAnnotation)
		{
			Target = target;
			UsesResourceAnnotation = usesResourceAnnotation;
		}

		public CompetitionTarget(
			Target target, double minRatio, double maxRatio, bool usesResourceAnnotation) :
				base(minRatio, maxRatio)
		{
			Target = target;
			UsesResourceAnnotation = usesResourceAnnotation;
		}
		#endregion

		#region Properties
		public Target Target { get; }
		public bool UsesResourceAnnotation { get; }

		public string CompetitionName => Target.Type.Name;
		public string CandidateName => Target.Method.Name;

		public string MinText => IgnoreMin
			? Min.ToString(EnvironmentInfo.MainCultureInfo)
			: Min.ToString(CompetitionLimitsAnalyserHelpers.RatioFormat, EnvironmentInfo.MainCultureInfo);
		public string MaxText => IgnoreMax
			? Max.ToString(EnvironmentInfo.MainCultureInfo)
			: Max.ToString(CompetitionLimitsAnalyserHelpers.RatioFormat, EnvironmentInfo.MainCultureInfo);
		#endregion

		public CompetitionTarget Clone() => new CompetitionTarget(Target, this, UsesResourceAnnotation);

		public bool UnionWithMin(double newMin)
		{
			if (IgnoreMin || newMin <= 0 || double.IsInfinity(newMin))
				return false;

			if (MinIsEmpty || newMin < Min)
			{
				Min = newMin;
				return true;
			}

			return false;
		}

		public bool UnionWithMax(double newMax)
		{
			if (IgnoreMax || newMax <= 0 || double.IsInfinity(newMax))
				return false;

			if (MaxIsEmpty || newMax > Max)
			{
				Max = newMax;
				return true;
			}

			return false;
		}
	}
}