using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Reports;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.UnitTesting
{
	// TODO: As validator once
	// https://github.com/PerfDotNet/BenchmarkDotNet/issues/153
	// will be fixed
	/// <summary>
	/// Competition parameters
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public class CompetitionParameters : IAnalyser
	{
		#region Public API
		public bool RerunIfModified { get; set; }
		public bool AnnotateOnRun { get; set; }
		public bool IgnoreExistingAnnotations { get; set; }

		public IEnumerable<IWarning> Analyse(Summary summary) =>
			Enumerable.Empty<IWarning>();
		#endregion
	}
}