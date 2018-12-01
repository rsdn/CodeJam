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
		/// <summary>Fills competition targets with stored metric values.</summary>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Filled targets or empty collection if not filled.</returns>
		CompetitionTarget[] TryGetTargets([NotNull] SummaryAnalysis analysis);

		/// <summary>Saves stored metrics from competition targets.</summary>
		/// <param name="targets">Competition targets with metrics to save.</param>
		/// <param name="annotationContext">The annotation context.</param>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Saved targets, if any.</returns>
		CompetitionTarget[] TrySaveTargets(
			[NotNull] IReadOnlyCollection<CompetitionTarget> targets,
			[NotNull] AnnotationContext annotationContext,
			[NotNull] SummaryAnalysis analysis);
	}
}