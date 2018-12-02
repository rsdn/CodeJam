using System;

using BenchmarkDotNet.Helpers;

using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Competition config attribute.</summary>
	/// <seealso cref="ICompetitionConfigSource"/>
	// ReSharper disable RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = true, AllowMultiple = false)]
	// ReSharper restore RedundantAttributeUsageProperty
	[PublicAPI, MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
	public class CompetitionConfigAttribute : Attribute, ICompetitionConfigSource
	{
		private readonly AttributeValue<ICompetitionConfig> _value;

		/// <summary>Initializes a new instance of the <see cref="CompetitionConfigAttribute"/> class.</summary>
		/// <param name="valueType">Type of the competition config. Should have a public parameterless constructor.</param>
		public CompetitionConfigAttribute([NotNull] Type valueType) =>
			_value = new AttributeValue<ICompetitionConfig>(valueType, nameof(valueType));

		/// <summary>Initializes a new instance of the <see cref="CompetitionConfigAttribute"/> class.</summary>
		/// <param name="valueFactory">The value factory.</param>
		protected CompetitionConfigAttribute([NotNull] Func<ICompetitionConfig> valueFactory) =>
			_value = new AttributeValue<ICompetitionConfig>(valueFactory);

		/// <summary>The competition config.</summary>
		/// <value>The competition config.</value>
		public ICompetitionConfig Config => _value.Value;
	}
}