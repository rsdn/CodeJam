using System;

using BenchmarkDotNet.Helpers;

using CodeJam.PerfTests.Configs.Factories;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Competition config modifier attribute.</summary>
	/// <seealso cref="ICompetitionModifierSource"/>
	// ReSharper disable RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = true, AllowMultiple = true)]
	// ReSharper restore RedundantAttributeUsageProperty
	[PublicAPI, MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
	public class CompetitionModifierAttribute : Attribute, ICompetitionModifierSource
	{
		private readonly AttributeValue<ICompetitionModifier> _value;

		/// <summary>Initializes a new instance of the <see cref="CompetitionModifierAttribute"/> class.</summary>
		/// <param name="valueType">Type of the competition modifier. Should have a public parameterless constructor.</param>
		public CompetitionModifierAttribute([NotNull] Type valueType)
		{
			_value = new AttributeValue<ICompetitionModifier>(valueType, nameof(valueType));
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionModifierAttribute"/> class.</summary>
		/// <param name="valueFactory">The value factory.</param>
		protected CompetitionModifierAttribute([NotNull] Func<ICompetitionModifier> valueFactory)
		{
			_value = new AttributeValue<ICompetitionModifier>(valueFactory, nameof(valueFactory));
		}

		/// <summary>The competition modifier.</summary>
		/// <value>The competition modifier.</value>
		public ICompetitionModifier Modifier => _value.Value;
	}
}