using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Attribute for <see cref="AssemblyCompetitionConfig"/>.
	/// </summary>
	public sealed class AssemblyCompetitionConfigAttribute : CompetitionConfigAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="AssemblyCompetitionConfigAttribute"/> class.</summary>
		/// <param name="anyTypeFromTargetAssembly">Any type from assembly the attribute is applied to.</param>
		public AssemblyCompetitionConfigAttribute([NotNull] Type anyTypeFromTargetAssembly) :
			base(() =>
				AssemblyCompetitionConfig.GetConfigForAssembly(
					anyTypeFromTargetAssembly.Assembly)) { }
	}
}