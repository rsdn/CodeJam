using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CodeJam.PerfTests.Configs.Factories;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Sets the <see cref="CompetitionLimitsMode.RerunsIfValidationFailed"/> config value.
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute" />
	public sealed class CompetitionRerunsModifierAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			private int _rerunCount;

			public ModifierImpl(int rerunCount)
			{
				_rerunCount = rerunCount;
			}
			public void Modify(ManualCompetitionConfig competitionConfig) =>
				competitionConfig.ApplyModifier(new CompetitionOptions()
				{
					Limits = { RerunsIfValidationFailed = _rerunCount }
				});
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionRerunsModifierAttribute" /> class.</summary>
		/// <param name="rerunCount">The rerun count.</param>
		public CompetitionRerunsModifierAttribute(int rerunCount) : base(() => new ModifierImpl(rerunCount)) { }
	}
}
