using System;

using BenchmarkDotNet.Competitions;

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
		public bool HasUnsavedChanges => _changedProperties != CompetitionTargetProperties.None;

		public string CompetitionFullName => Target.Type.FullName + ", " + Target.Type.Assembly.GetName().Name;
		public string CompetitionName => Target.Type.Name;
		public string CandidateName => Target.Method.Name;
		#endregion

		public bool IsChanged(CompetitionTargetProperties property) =>
			(_changedProperties & property) == property;
		private void MarkAsChanged(CompetitionTargetProperties property) =>
			_changedProperties |= property;

		private bool IsLessThanCore(double current, double newValue)
		{
			if (current < 0 || newValue <= 0 || double.IsInfinity(newValue))
				return false;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return current == 0 || current < newValue;
		}
		private bool IsGreaterThanCore(double current, double newValue)
		{
			if (current < 0 || newValue <= 0 || double.IsInfinity(newValue))
				return false;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return current == 0 || current > newValue;
		}

		private bool UnionWithMin(double newMin)
		{
			if (IsGreaterThanCore(Min, newMin))
			{
				Min = newMin;
				MarkAsChanged(CompetitionTargetProperties.MinRatio);
				return true;
			}

			return false;
		}
		private bool UnionWithMax(double newMax)
		{
			if (IsLessThanCore(Max, newMax))
			{
				Max = newMax;
				MarkAsChanged(CompetitionTargetProperties.MaxRatio);
				return true;
			}

			return false;
		}

		public bool UnionWith(CompetitionLimit newProperties)
		{
			bool result = false;
			result |= UnionWithMin(newProperties.Min);
			result |= UnionWithMax(newProperties.Max);
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
				var newValue = Math.Floor(Min * (100 - percent)) / 100;
				UnionWithMin(newValue);
			}
			if (IsChanged(CompetitionTargetProperties.MaxRatio))
			{
				var newValue = Math.Ceiling(Max * (100 + percent)) / 100;
				UnionWithMax(newValue);
			}
			_changedProperties = CompetitionTargetProperties.None;
		}
	}
}