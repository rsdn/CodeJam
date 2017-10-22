using System;

using BenchmarkDotNet.Helpers;

using CodeJam.PerfTests.Configs.Factories;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Competition config factory attribute.</summary>
	/// <seealso cref="ICompetitionConfigFactorySource"/>
	// ReSharper disable RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = true, AllowMultiple = false)]
	// ReSharper restore RedundantAttributeUsageProperty
	[PublicAPI]
	public class CompetitionConfigFactoryAttribute : Attribute, ICompetitionConfigFactorySource
	{
		private readonly AttributeValue<ICompetitionConfigFactory> _value;

		/// <summary>Initializes a new instance of the <see cref="CompetitionConfigFactoryAttribute"/> class.</summary>
		/// <param name="valueType">Type of the competition config factory. Should have a public parameterless constructor.</param>
		public CompetitionConfigFactoryAttribute([NotNull] Type valueType) =>
			_value = new AttributeValue<ICompetitionConfigFactory>(valueType, nameof(valueType));

		/// <summary>Initializes a new instance of the <see cref="CompetitionConfigFactoryAttribute"/> class.</summary>
		/// <param name="valueFactory">The value factory.</param>
		protected CompetitionConfigFactoryAttribute([NotNull] Func<ICompetitionConfigFactory> valueFactory) =>
			_value = new AttributeValue<ICompetitionConfigFactory>(valueFactory);

		/// <summary>The competition config factory.</summary>
		/// <value>The competition config factory.</value>
		public ICompetitionConfigFactory ConfigFactory => _value.Value;
	}
}