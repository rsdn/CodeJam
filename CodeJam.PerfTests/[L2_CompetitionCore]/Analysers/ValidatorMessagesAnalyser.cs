using System;
using System.Collections.Generic;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>
	/// Analyser that dumps validation messages.
	/// </summary>
	/// <seealso cref="IAnalyser" />
	internal sealed class ValidatorMessagesAnalyser : IAnalyser
	{
		/// <summary>The instance of <see cref="ValidatorMessagesAnalyser"/>.</summary>
		public static readonly ValidatorMessagesAnalyser Instance = new ValidatorMessagesAnalyser();

		/// <summary>Prevents a default instance of the <see cref="ValidatorMessagesAnalyser"/> class from being created.</summary>
		private ValidatorMessagesAnalyser() { }

		/// <value>The identifier of the analyser.</value>
		[NotNull]
		public string Id => GetType().Name;

		/// <summary>Dumps validation messages.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Enumerable with warnings for the benchmarks.</returns>
		[NotNull]
		public IEnumerable<Conclusion> Analyse([NotNull] Summary summary)
		{
			var analysis = new SummaryAnalysis(Id, summary, MessageSource.Validator);

			foreach (var validationError in summary.ValidationErrors)
			{
				var message = validationError.BenchmarkCase == null
					? validationError.Message
					: $"BenchmarkCase {validationError.BenchmarkCase.DisplayInfo}:{Environment.NewLine}\t{validationError.Message}";

				if (validationError.IsCritical)
				{
					analysis.WriteSetupErrorMessage(message);
				}
				else
				{
					analysis.WriteWarningMessage(message);
				}
			}

			return analysis.Conclusions;
		}
	}
}