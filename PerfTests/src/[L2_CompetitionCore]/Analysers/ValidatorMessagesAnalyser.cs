using System;
using System.Collections.Generic;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Reports;

using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	/// <summary>
	/// Analyser that dumps validation messages.
	/// </summary>
	/// <seealso cref="BenchmarkDotNet.Analysers.IAnalyser" />
	internal sealed class ValidatorMessagesAnalyser : IAnalyser
	{
		/// <summary>The instance of <see cref="ValidatorMessagesAnalyser"/>.</summary>
		public static readonly ValidatorMessagesAnalyser Instance = new ValidatorMessagesAnalyser();

		/// <summary>Prevents a default instance of the <see cref="ValidatorMessagesAnalyser"/> class from being created.</summary>
		private ValidatorMessagesAnalyser() { }

		/// <value>The identifier of the analyser.</value>
		public string Id => GetType().Name;

		/// <summary>Performs limit checking for competition benchmarks.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns>Enumerable with warnings for the benchmarks.</returns>
		public IEnumerable<Conclusion> Analyse([NotNull] Summary summary)
		{
			var analysis = new Analysis(Id, summary, MessageSource.Validator);

			foreach (var validationError in summary.ValidationErrors)
			{
				var message = validationError.Benchmark == null
					? validationError.Message
					: $"Benchmark {validationError.Benchmark.DisplayInfo}:{Environment.NewLine}\t{validationError.Message}";

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