using System;
using System.Collections.Generic;

using CodeJam.PerfTests.Analysers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Common interface for competition annotation storage.
	/// </summary>
	internal interface IAnnotationStorage
	{
		/// <summary>Fills competition descriptors with stored metric values.</summary>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Filled descriptors or empty collection if not filled.</returns>
		CompetitionTarget[] TryGetTargets([NotNull] SummaryAnalysis analysis);

		/// <summary>Saves stored metrics from competition descriptors.</summary>
		/// <param name="descriptors">Competition descriptors with metrics to save.</param>
		/// <param name="annotationContext">The annotation context.</param>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Saved descriptors, if any.</returns>
		CompetitionTarget[] TrySaveTargets(
			[NotNull] IReadOnlyCollection<CompetitionTarget> descriptors,
			[NotNull] AnnotationContext annotationContext,
			[NotNull] SummaryAnalysis analysis);
	}
}