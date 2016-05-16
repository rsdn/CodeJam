using System;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Competitions
{
	/// <summary>
	/// Specifies XML resource to be used as benchmark annotation.
	/// </summary>
	// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	[PublicAPI, MeansImplicitUse]
	public class CompetitionMetadataAttribute : Attribute
	{
		/// <summary>
		/// Specifies XML resource to be used as benchmark annotation.
		/// </summary>
		public CompetitionMetadataAttribute(string metadataResourceName)
		{
			MetadataResourceName = metadataResourceName;
		}

		/// <summary>
		/// Path to the XML resource to be used as benchmark limit data.
		/// </summary>
		public string MetadataResourceName { get; }
	}
}