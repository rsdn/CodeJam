using System;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Helpers;

namespace BenchmarkDotNet.Running.Competitions.SourceAnnotations
{
	[Flags]
	internal enum CompetitionTargetProperties
	{
		None = 0x0,
		MinRatio = 0x1,
		MaxRatio = 0x2
	}

	internal class CompetitionTarget : CompetitionLimit
	{
		#region Fields & .ctor
		private CompetitionTargetProperties _changedProperties;

		public CompetitionTarget() : base(0, 0) { }

		public CompetitionTarget(
			Target target, double minRatio, double maxRatio, bool usesResourceAnnotation) :
				base(minRatio, maxRatio)
		{
			Target = target;
			UsesResourceAnnotation = usesResourceAnnotation;
		}

		public CompetitionTarget(
			Target target, CompetitionLimit other, bool usesResourceAnnotation) :
				this(target, other.Min, other.Max, usesResourceAnnotation)
		{
		}
		#endregion

		#region Properties
		public Target Target { get; }
		public bool UsesResourceAnnotation { get; }
		public bool WasUpdated => _changedProperties != CompetitionTargetProperties.None;

		public string CompetitionFullName => Target.Type.FullName + ", " + Target.Type.Assembly.GetName().Name;
		public string CompetitionName => Target.Type.Name;
		public string CandidateName => Target.Method.Name;
		#endregion

		public bool IsChanged(CompetitionTargetProperties property) =>
			(_changedProperties & property) == property;
		private void MarkAsChanged(CompetitionTargetProperties property) =>
			_changedProperties |= property;

		public bool UnionWithMin(double newMin)
		{
			if (IgnoreMin || newMin <= 0 || double.IsInfinity(newMin))
				return false;

			if (MinIsEmpty || newMin < Min)
			{
				Min = newMin;
				MarkAsChanged(CompetitionTargetProperties.MinRatio);
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
				MarkAsChanged(CompetitionTargetProperties.MaxRatio);
				return true;
			}

			return false;
		}
	}
}