using System;

using CodeJam.PerfTests;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// CodeJam perftests config attribute.
	/// </summary>
	public sealed class CodeJamPerfTestsAssembly : CompetitionConfigAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="CodeJamPerfTestsAssembly"/> class.</summary>
		/// <param name="anyTypeFromTargetAssembly">Any type from assembly the attribute is applied to.</param>
		public CodeJamPerfTestsAssembly([NotNull] Type anyTypeFromTargetAssembly) :
			base(() => CodeJamCompetitionConfig.GetConfigForAssembly(anyTypeFromTargetAssembly.Assembly)) { }
	}
}