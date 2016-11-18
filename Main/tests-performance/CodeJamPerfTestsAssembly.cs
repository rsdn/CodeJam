using System;

using CodeJam.PerfTests;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// CodeJam perftests config attribute.
	/// </summary>
	public sealed class CodeJamPerfTestsAssembly : CompetitionConfigFactoryAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="CodeJamPerfTestsAssembly"/> class.</summary>
		public CodeJamPerfTestsAssembly() : base(typeof(CodeJamCompetitionFactory)) { }
	}
}