using System;

using CodeJam.PerfTests.Running.Core;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Attribute for Competition config that can be configured via app.config
	/// Check the <see cref="AppConfigOptions"/> for settings avaliable.
	/// </summary>
	public sealed class AssemblyCompetitionConfigAttribute : CompetitionConfigAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="AssemblyCompetitionConfigAttribute"/> class.</summary>
		/// <param name="anyTypeFromTargetAssembly">Any type from target assembly.</param>
		public AssemblyCompetitionConfigAttribute([NotNull] Type anyTypeFromTargetAssembly) :
			base(new AssemblyCompetitionConfig(anyTypeFromTargetAssembly.Assembly)) { }
	}
}