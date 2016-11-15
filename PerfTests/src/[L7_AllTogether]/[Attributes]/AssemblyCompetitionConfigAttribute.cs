using System;

using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Attribute for <see cref="CompetitionConfig.GetConfigForAssembly"/>.
	/// </summary>
	public sealed class AssemblyCompetitionConfigAttribute : CompetitionConfigAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="AssemblyCompetitionConfigAttribute"/> class.</summary>
		/// <param name="anyTypeFromTargetAssembly">Any type from assembly the attribute is applied to.</param>
		public AssemblyCompetitionConfigAttribute([NotNull] Type anyTypeFromTargetAssembly) :
			base(() => CompetitionConfig.GetConfigForAssembly(anyTypeFromTargetAssembly.Assembly))
		{ }
	}
}