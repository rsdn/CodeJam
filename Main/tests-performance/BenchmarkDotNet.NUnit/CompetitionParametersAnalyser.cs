using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Reports;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.NUnit
{
	/// <summary>
	/// Options for competition run
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public class CompetitionParametersAnalyser : IAnalyser
	{
		#region Public API
		public bool RerunIfModified { get; set; }
		public bool AnnotateOnRun { get; set; }
		public bool IgnoreExistingAnnotations { get; set; }

		public IEnumerable<IWarning> Analyse(Summary summary)
		{
			var warnings = new List<IWarning>();
			if (AnnotateOnRun)
			{
				AnnotateSourceHelper.AnnotateBenchmarkFiles(summary, warnings);
			}

			return warnings;
		}
		#endregion
	}
}