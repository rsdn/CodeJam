using System;

using BenchmarkDotNet.Running;

namespace CodeJam.PerfTests.Running.SourceAnnotations
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

		public CompetitionTarget(
			Target target, double minRatio, double maxRatio, bool usesResourceAnnotation) :
				base(minRatio, maxRatio)
		{
			Target = target;
			UsesResourceAnnotation = usesResourceAnnotation;
		}

		public CompetitionTarget(
			Target target, CompetitionLimit other, bool usesResourceAnnotation) :
				this(target, other.MinRatio, other.MaxRatio, usesResourceAnnotation) { }
		#endregion

		#region Properties
		public Target Target { get; }
		public bool UsesResourceAnnotation { get; }
		public bool HasUnsavedChanges => _changedProperties != CompetitionTargetProperties.None;

		public string CompetitionFullName => Target.Type.FullName + ", " + Target.Type.Assembly.GetName().Name;
		public string CompetitionName => Target.Type.Name;
		public string CandidateName => Target.Method.Name;
		#endregion

		public bool IsChanged(CompetitionTargetProperties property) =>
			(_changedProperties & property) == property;

		private void MarkAsChanged(CompetitionTargetProperties property) =>
			_changedProperties |= property;

		private bool UnionWithMinRatio(double newMin)
		{
			if (ShouldBeUpdatedMin(MinRatio, newMin))
			{
				MinRatio = newMin;
				MarkAsChanged(CompetitionTargetProperties.MinRatio);
				return true;
			}

			return false;
		}

		private bool UnionWithMaxRatio(double newMax)
		{
			if (ShouldBeUpdatedMax(MaxRatio, newMax))
			{
				MaxRatio = newMax;
				MarkAsChanged(CompetitionTargetProperties.MaxRatio);
				return true;
			}

			return false;
		}

		public bool UnionWith(CompetitionLimit newProperties)
		{
			var result = false;
			result |= UnionWithMinRatio(newProperties.MinRatio);
			result |= UnionWithMaxRatio(newProperties.MaxRatio);
			return result;
		}

		// TODO:  propertiesToLoose
		public void LooseLimitsAndMarkAsSaved(int percent)
		{
			if (percent < 0 || percent >= 100)
			{
				throw new ArgumentOutOfRangeException(
					nameof(percent), percent,
					$"The {nameof(percent)} should be in range (0..100).");
			}

			if (IsChanged(CompetitionTargetProperties.MinRatio))
			{
				var newValue = Math.Floor(MinRatio * (100 - percent)) / 100;
				UnionWithMinRatio(newValue);
			}
			if (IsChanged(CompetitionTargetProperties.MaxRatio))
			{
				var newValue = Math.Ceiling(MaxRatio * (100 + percent)) / 100;
				UnionWithMaxRatio(newValue);
			}
			_changedProperties = CompetitionTargetProperties.None;
		}
	}
}