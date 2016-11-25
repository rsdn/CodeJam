using System;

using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Running.Limits;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Stores limits for benchmark target
	/// </summary>
	/// <seealso cref="CompetitionLimit"/>
	/// <seealso cref="Target"/>
	internal class CompetitionTarget : CompetitionLimit
	{
		#region Fields & .ctor
		private CompetitionLimitProperties _changedProperties;

		/// <summary>Initializes a new instance of the <see cref="CompetitionTarget"/> class.</summary>
		/// <param name="target">The target.</param>
		/// <param name="limitsForTarget">Competition limits for the target.</param>
		/// <param name="doesNotCompete">Exclude the benchmark from the competition.</param>
		public CompetitionTarget(
			[NotNull] Target target,
			[NotNull] CompetitionLimit limitsForTarget,
			bool doesNotCompete) :
				// ReSharper disable once IntroduceOptionalParameters.Global
				this(target, limitsForTarget, doesNotCompete, null) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionTarget"/> class.</summary>
		/// <param name="target">The target.</param>
		/// <param name="limitsForTarget">Competition limits for the target.</param>
		/// <param name="doesNotCompete">Exclude the benchmark from the competition.</param>
		/// <param name="competitionMetadata">Description of embedded resource containing xml document with competition limits.</param>
		public CompetitionTarget(
			[NotNull] Target target,
			[NotNull] CompetitionLimit limitsForTarget,
			bool doesNotCompete,
			[CanBeNull] CompetitionMetadata competitionMetadata) :
				base(limitsForTarget.MinRatio, limitsForTarget.MaxRatio)
		{
			Target = target;
			CompetitionMetadata = competitionMetadata;
			DoesNotCompete = doesNotCompete;
		}
		#endregion

		#region Properties
		/// <summary>The benchmark target.</summary>
		/// <value>The benchmark target.</value>
		[NotNull]
		public Target Target { get; }

		/// <summary>Description of embedded resource containing xml document with competition limits.</summary>
		/// <value>Description of embedded resource containing xml document with competition limits.</value>
		[CanBeNull]
		public CompetitionMetadata CompetitionMetadata { get; }

		/// <summary>Exclude the benchmark from the competition.</summary>
		/// <value>
		/// <c>true</c> if the benchmark does not take part in competition
		/// and should not be validated.
		/// </value>
		public bool DoesNotCompete { get; }

		// DONTTOUCH: renaming the property will break xml annotations.
		/// <summary>The benchmark target is baseline.</summary>
		/// <value><c>true</c> if the benchmark target is baseline.</value>
		public bool Baseline => Target.Baseline;

		/// <summary>The benchmark has limits to check.</summary>
		/// <value><c>true</c> if the benchmark has limits to check.</value>
		public bool CheckLimits => !Target.Baseline && !DoesNotCompete;

		/// <summary>The limit properties are updated but not saved.</summary>
		/// <value><c>true</c> if this instance has unsaved changes; otherwise, <c>false</c>.</value>
		public bool HasUnsavedChanges => _changedProperties != CompetitionLimitProperties.None;
		#endregion

		/// <summary>Determines whether all specified properties are changed.</summary>
		/// <param name="property">The properties to check.</param>
		/// <returns><c>true</c> if all specified properties are changed. </returns>
		public bool IsChanged(CompetitionLimitProperties property) =>
			property != CompetitionLimitProperties.None &&
				_changedProperties.IsFlagSet(property);

		#region Core logic for competition limits
		private void MarkAsChanged(CompetitionLimitProperties property) =>
			_changedProperties = _changedProperties.SetFlag(property);

		private bool UnionWithMinRatio(double newMin)
		{
			if (ShouldBeUpdatedMin(MinRatio, newMin))
			{
				MinRatio = newMin;
				MarkAsChanged(CompetitionLimitProperties.MinRatio);
				return true;
			}

			return false;
		}

		private bool UnionWithMaxRatio(double newMax)
		{
			if (ShouldBeUpdatedMax(MaxRatio, newMax))
			{
				MaxRatio = newMax;
				MarkAsChanged(CompetitionLimitProperties.MaxRatio);
				return true;
			}

			return false;
		}
		#endregion

		/// <summary>Adjusts competition limits with specified values.</summary>
		/// <param name="limitsForTarget">Competition limits for the target.</param>
		/// <returns><c>true</c> if any of the limits were updated.</returns>
		public bool UnionWith([NotNull] CompetitionLimit limitsForTarget)
		{
			Code.NotNull(limitsForTarget, nameof(limitsForTarget));

			var result = false;
			result |= UnionWithMinRatio(limitsForTarget.MinRatioRounded);
			result |= UnionWithMaxRatio(limitsForTarget.MaxRatioRounded);
			return result;
		}

		/// <summary>Marks limits as saved.</summary>
		public void MarkAsSaved() => _changedProperties = CompetitionLimitProperties.None;
	}
}