using System;

using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.SourceAnnotations;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>Helper class to store <see cref="CompetitionAnalyser"/> results.</summary>
	/// <seealso cref="CodeJam.PerfTests.Analysers.Analysis"/>
	internal class CompetitionAnalysis : SummaryAnalysis
	{
		/// <summary>Run state slot for the competition targets.</summary>
		private static readonly RunState<CompetitionTargets> _targetsSlot = new RunState<CompetitionTargets>();

		/// <summary>Global annotation context.</summary>
		private static readonly Lazy<AnnotationContext> _annotationContext = new Lazy<AnnotationContext>(() => new AnnotationContext());

		/// <summary>
		/// Initializes a new instance of the <see cref="CompetitionAnalysis"/> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="summary">The summary.</param>
		public CompetitionAnalysis([NotNull] string id, [NotNull] Summary summary) : base(id, summary)
		{
			Targets = _targetsSlot[summary];
			AnnotationContext = _annotationContext.Value;
		}

		/// <summary>The competition targets.</summary>
		/// <value>The competition targets.</value>
		[NotNull]
		public CompetitionTargets Targets { get; }

		/// <summary>The annotation context.</summary>
		/// <value>The annotation context.</value>
		[NotNull]
		private AnnotationContext AnnotationContext { get; }

		/// <summary>Runs annotation code.</summary>
		/// <typeparam name="TResult">Type of result of annotation.</typeparam>
		/// <param name="annotationCallback">The annotation callback.</param>
		/// <returns>Result of annotation.</returns>
		public TResult RunInAnnotationContext<TResult>([NotNull] Func<AnnotationContext, TResult> annotationCallback) =>
			AnnotationContext.RunInContext(annotationCallback);
	}
}