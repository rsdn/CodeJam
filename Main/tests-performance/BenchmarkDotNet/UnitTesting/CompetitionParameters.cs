using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.UnitTesting
{
	/// <summary>
	/// Competition parameters
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	public class CompetitionParameters : IValidator
	{
		#region Public API
		public bool RerunIfModified { get; set; }
		public bool AnnotateOnRun { get; set; }
		public bool IgnoreExistingAnnotations { get; set; }
		public bool AllowSlowBenchmarks { get; set; }
		#endregion

		#region Implementation of IValidator
		IEnumerable<IValidationError> IValidator.Validate(IList<Benchmark> benchmarks) => 
			Enumerable.Empty<IValidationError>();

		bool IValidator.TreatsWarningsAsErrors => false;
		#endregion
	}
}