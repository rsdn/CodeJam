using System;

using CodeJam.PerfTests.Configs;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Default competition config attribute.</summary>
	/// <seealso cref="ICompetitionConfigSource"/>
	// ReSharper disable RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = true, AllowMultiple = false)]
	// ReSharper restore RedundantAttributeUsageProperty
	[PublicAPI, MeansImplicitUse]
	public class CompetitionConfigAttribute : Attribute, ICompetitionConfigSource
	{
		/// <summary>Initializes a new instance of the <see cref="CompetitionConfigAttribute"/> class.</summary>
		/// <param name="type">The type of the competition config. Should have a public constructor without parameters.</param>
		public CompetitionConfigAttribute([NotNull] Type type)
		{
			Code.NotNull(type, nameof(type));
			Config = (ICompetitionConfig)Activator.CreateInstance(type);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionConfigAttribute"/> class.</summary>
		/// <param name="config">Instance of the competition config.</param>
		protected CompetitionConfigAttribute(ICompetitionConfig config)
		{
			Code.NotNull(config, nameof(config));
			Config = config;
		}

		/// <summary>The competition config.</summary>
		/// <value>The competition config.</value>
		public ICompetitionConfig Config { get; }
	}
}