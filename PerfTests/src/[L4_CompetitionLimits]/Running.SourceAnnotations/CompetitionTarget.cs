using System;

using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Running.Limits;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Stores limits for benchmark target
	/// </summary>
	/// <seealso cref="Target"/>
	internal class CompetitionTarget
	{
		#region Fields & .ctor
		private bool _limitsChanged;

		/// <summary>Initializes a new instance of the <see cref="CompetitionTarget"/> class.</summary>
		/// <param name="target">The target.</param>
		/// <param name="limitsForTarget">Competition limits for the target.</param>
		/// <param name="doesNotCompete">Exclude the benchmark from the competition.</param>
		public CompetitionTarget(
			[NotNull] Target target,
			LimitRange limitsForTarget,
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
			LimitRange limitsForTarget,
			bool doesNotCompete,
			[CanBeNull] CompetitionMetadata competitionMetadata)
		{
			Target = target;
			Limits = limitsForTarget;
			CompetitionMetadata = competitionMetadata;
			DoesNotCompete = doesNotCompete;
		}
		#endregion

		#region Properties
		/// <summary>The benchmark target.</summary>
		/// <value>The benchmark target.</value>
		[NotNull]
		public Target Target { get; }

		/// <summary>The relative-to-baseline timing limits for the target.</summary>
		/// <value>The relative-to-baseline timing limits for the target.</value>
		public LimitRange Limits { get; private set; }

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
		public bool HasRelativeLimits => !Target.Baseline && !DoesNotCompete;

		/// <summary>The limit properties are updated but not saved.</summary>
		/// <value><c>true</c> if this instance has unsaved changes; otherwise, <c>false</c>.</value>
		public bool HasUnsavedChanges => _limitsChanged;
		#endregion

		/// <summary>Adjusts competition limits with specified values.</summary>
		/// <param name="limitsForTarget">Competition limits for the target.</param>
		/// <returns><c>true</c> if any of the limits were updated.</returns>
		public bool UnionWith(LimitRange limitsForTarget)
		{
			if (limitsForTarget.IsEmpty || Limits.Contains(limitsForTarget))
				return false;

			Limits = Limits.UnionWith(limitsForTarget);
			_limitsChanged = true;
			return true;
		}

		/// <summary>Marks limits as saved.</summary>
		public void MarkAsSaved() => _limitsChanged = false;
	}
}