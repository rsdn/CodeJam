using System;

using CodeJam.PerfTests.Configs.Factories;

namespace CodeJam.PerfTests.Configs
{
	/// <summary>
	/// Sets the <see cref="CompetitionCheckMode.RerunsIfValidationFailed"/> config value.
	/// Sets maximum count of retries performed if metric limits check failed.
	/// </summary>
	/// <seealso cref="CompetitionModifierAttribute"/>
	public sealed class CompetitionMaximumRerunsAttribute : CompetitionModifierAttribute
	{
		private class ModifierImpl : ICompetitionModifier
		{
			private readonly int _rerunsIfValidationFailed;

			public ModifierImpl(int rerunsIfValidationFailed) => _rerunsIfValidationFailed = rerunsIfValidationFailed;

			public void Modify(ManualCompetitionConfig competitionConfig) =>
				competitionConfig.ApplyModifier(
					new CompetitionOptions
					{
						Checks = { RerunsIfValidationFailed = _rerunsIfValidationFailed }
					});
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionMaximumRerunsAttribute"/> class.</summary>
		/// <param name="rerunsIfValidationFailed">Amount of reruns performed if validation failed.</param>
		public CompetitionMaximumRerunsAttribute(int rerunsIfValidationFailed)
			: base(() => new ModifierImpl(rerunsIfValidationFailed)) { }
	}
}