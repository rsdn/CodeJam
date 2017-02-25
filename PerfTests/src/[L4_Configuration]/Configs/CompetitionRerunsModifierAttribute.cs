using System;

using CodeJam.PerfTests.Configs.Factories;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Sets the <see cref="CompetitionCheckMode.RerunsIfValidationFailed"/> config value.
	/// Sets maximum count of retries performed if the limit checking failed.
	/// </summary>
	/// <seealso cref="CodeJam.PerfTests.CompetitionModifierAttribute"/>
	public sealed class CompetitionRerunsModifierAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			private readonly int _rerunsIfValidationFailed;

			public ModifierImpl(int rerunsIfValidationFailed)
			{
				_rerunsIfValidationFailed = rerunsIfValidationFailed;
			}

			public void Modify(ManualCompetitionConfig competitionConfig) =>
				competitionConfig.ApplyModifier(
					new CompetitionOptions
					{
						Checks = { RerunsIfValidationFailed = _rerunsIfValidationFailed }
					});
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionRerunsModifierAttribute"/> class.</summary>
		/// <param name="rerunsIfValidationFailed">Maximum count of retries performed if the limit checking failed.</param>
		public CompetitionRerunsModifierAttribute(int rerunsIfValidationFailed)
			: base(() => new ModifierImpl(rerunsIfValidationFailed)) { }
	}
}