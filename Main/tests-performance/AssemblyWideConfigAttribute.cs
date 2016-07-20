using System;

using CodeJam.PerfTests;

using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Attribute for <see cref="AssemblyWideConfig"/>.
	/// </summary>
	public sealed class AssemblyWideConfigAttribute : CompetitionConfigAttribute
	{
		/// <summary>Initializes a new instance of the <see cref="AssemblyWideConfig"/> class.</summary>
		/// <param name="anyTypeFromTargetAssembly">Any type from assembly the attribute is applied to.</param>
		public AssemblyWideConfigAttribute([NotNull] Type anyTypeFromTargetAssembly) :
			base(() =>
				AssemblyWideConfig.GetConfigForAssembly(
					anyTypeFromTargetAssembly.Assembly)) { }
	}
}