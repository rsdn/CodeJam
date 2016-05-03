using System;
using System.Globalization;

using BenchmarkDotNet.Running;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.UnitTesting
{
	/// <summary>
	/// Information for competition target
	/// </summary>
	[PublicAPI]
	public class CompetitionTarget
	{
		#region Fields & .ctor
		private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;
		public CompetitionTarget() { }

		public CompetitionTarget(
			Target target, double min, double max, bool usesResourceAnnotation)
		{
			if (min < 0)
				min = -1;
			if (max < 0)
				max = -1;

			Target = target;
			UsesResourceAnnotation = usesResourceAnnotation;
			Min = min;
			Max = max;
		}
		#endregion

		#region Properties
		public Target Target { get; }
		public bool UsesResourceAnnotation { get; }

		public string CompetitionName => Target.Type.Name;
		public string CandidateName => Target.Method.Name;

		public double Min { get; private set; }
		public double Max { get; private set; }

		public string MinText => IgnoreMin ? Min.ToString(_culture) : Min.ToString("0.00###", _culture);
		public string MaxText => IgnoreMax ? Max.ToString(_culture) : Max.ToString("0.00###", _culture);
		#endregion

		public CompetitionTarget Clone() => new CompetitionTarget(Target, Min, Max, UsesResourceAnnotation);

		// ReSharper disable once CompareOfFloatsByEqualityOperator
		public bool MinIsEmpty => Min == 0;
		// ReSharper disable once CompareOfFloatsByEqualityOperator
		public bool MaxIsEmpty => Max == 0;

		public bool IgnoreMin => Min < 0;
		public bool IgnoreMax => Max < 0;

		public bool IsEmpty => MinIsEmpty && MaxIsEmpty;

		internal bool UnionWithMin(double newMin)
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

		internal bool UnionWithMax(double newMax)
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