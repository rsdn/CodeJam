using System;

using BenchmarkDotNet.Helpers;

using CodeJam.PerfTests.Configs.Factories;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Competition config modifier attribute.</summary>
	/// <seealso cref="ICompetitionModifierSource"/>
	// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = true, AllowMultiple = true)]
	[PublicAPI, MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
	public class CompetitionModifierAttribute : Attribute, ICompetitionModifierSource
	{
		private readonly AttributeValue<ICompetitionModifier> _value;

		/// <summary>Initializes a new instance of the <see cref="CompetitionModifierAttribute"/> class.</summary>
		/// <param name="competitionModifierType">
		/// Type of the competition modifier.
		/// Should implement the <see cref="ICompetitionModifier"/> interface and have a public parameterless constructor.
		/// </param>
		public CompetitionModifierAttribute([NotNull] Type competitionModifierType) => _value = new AttributeValue<ICompetitionModifier>(competitionModifierType, nameof(competitionModifierType));

		/// <summary>Initializes a new instance of the <see cref="CompetitionModifierAttribute"/> class.</summary>
		/// <param name="valueFactory">The competition modifier factory.</param>
		protected CompetitionModifierAttribute([NotNull] Func<ICompetitionModifier> valueFactory) => _value = new AttributeValue<ICompetitionModifier>(valueFactory);

		/// <summary>The competition modifier.</summary>
		/// <value>The competition modifier.</value>
		public ICompetitionModifier Modifier => _value.Value;
	}
}