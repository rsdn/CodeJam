using System;
using System.Threading;

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
		private readonly Lazy<ICompetitionConfig> _configLazy;
		/// <summary>Initializes a new instance of the <see cref="CompetitionConfigAttribute"/> class.</summary>
		/// <param name="configType">The type of the competition config. Should have a public constructor without parameters.</param>
		public CompetitionConfigAttribute([NotNull] Type configType)
		{
			Code.NotNull(configType, nameof(configType));

			_configLazy = new Lazy<ICompetitionConfig>(
				()=>(ICompetitionConfig)Activator.CreateInstance(configType),
				LazyThreadSafetyMode.ExecutionAndPublication);
		}

		/// <summary>Initializes a new instance of the <see cref="CompetitionConfigAttribute"/> class.</summary>
		/// <param name="configFactory">Instance of the competition config.</param>
		protected CompetitionConfigAttribute(Func<ICompetitionConfig> configFactory)
		{
			_configLazy = new Lazy<ICompetitionConfig>(
				configFactory,
				LazyThreadSafetyMode.ExecutionAndPublication);
		}

		/// <summary>The competition config.</summary>
		/// <value>The competition config.</value>
		public ICompetitionConfig Config => _configLazy.Value;
	}
}