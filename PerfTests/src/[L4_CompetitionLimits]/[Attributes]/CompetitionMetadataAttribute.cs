using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>>Attribute for benchmark classes that stores competition limits as embedded XML resource.</summary>
	// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	[PublicAPI, MeansImplicitUse]
	public class CompetitionMetadataAttribute : Attribute
	{
		/// <summary>Constructor for competition metadata attribute.</summary>
		/// <param name="metadataResourceName">The name of the resource containing xml document with competition limits.</param>
		public CompetitionMetadataAttribute(string metadataResourceName)
		{
			MetadataResourceName = metadataResourceName;
		}

		/// <summary>The name of the resource containing xml document with competition limits.</summary>
		/// <value>The name of the resource containing xml document with competition limits.</value>
		public string MetadataResourceName { get; }
	}
}