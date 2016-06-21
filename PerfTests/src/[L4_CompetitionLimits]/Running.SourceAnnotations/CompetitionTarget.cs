using System;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Stores limits for benchmark target
	/// </summary>
	/// <seealso cref="CompetitionLimit"/>
	/// <seealso cref="Target"/>
	[SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global")]
	internal class CompetitionTarget : CompetitionLimit
	{
		#region Fields & .ctor
		private CompetitionLimitProperties _changedProperties;

		/// <summary>Initializes a new instance of the <see cref="CompetitionTarget"/> class.</summary>
		/// <param name="target">The target.</param>
		/// <param name="limitsForTarget">Competition limits for the target.</param>
		public CompetitionTarget(
			[CanBeNull] Target target,
			[NotNull] CompetitionLimit limitsForTarget) :
				this(target, limitsForTarget, false, null) { }

		/// <summary>Initializes a new instance of the <see cref="CompetitionTarget"/> class.</summary>
		/// <param name="target">The target.</param>
		/// <param name="limitsForTarget">Competition limits for the target.</param>
		/// <param name="fromMetadataResource"><c>true</c> if the limits are obtained from xml resource.</param>
		/// <param name="metadataResourcePath">The relative path to the resource containing xml document with competition limits.</param>
		public CompetitionTarget(
			[CanBeNull] Target target,
			[NotNull] CompetitionLimit limitsForTarget,
			bool fromMetadataResource, string metadataResourcePath) :
				base(limitsForTarget.MinRatio, limitsForTarget.MaxRatio)
		{
			Target = target;
			FromMetadataResource = fromMetadataResource;
			MetadataResourcePath = metadataResourcePath;
		}
		#endregion

		#region Properties
		/// <summary>The benchmark target.</summary>
		/// <value>The benchmark target.</value>
		public Target Target { get; }

		/// <summary>Gets a value indicating whether benchmark limits are obtained from xml resource.</summary>
		/// <value>
		/// <c>true</c> if benchmark limits are obtained from xml resource; otherwise, <c>false</c>.
		/// </value>
		public bool FromMetadataResource { get; }

		/// <summary>The relative path to the resource containing xml document with competition limits.</summary>
		/// <value>The relative path to the resource containing xml document with competition limits.</value>
		public string MetadataResourcePath { get; }

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
			result |= UnionWithMinRatio(limitsForTarget.MinRatio);
			result |= UnionWithMaxRatio(limitsForTarget.MaxRatio);
			return result;
		}

		/// <summary>Marks limits as saved.</summary>
		public void MarkAsSaved() => _changedProperties = CompetitionLimitProperties.None;
	}
}