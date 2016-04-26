using System;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace
// ReSharper disable once RedundantAttributeUsageProperty

namespace BenchmarkDotNet.NUnit
{
	/// <summary>
	/// Specifies XML resource to be used as benchmark limit data.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	[PublicAPI, MeansImplicitUse]
	public class CompetitionMetadataAttribute : Attribute
	{
		/// <summary>
		/// Specifies XML resource to be used as benchmark limit data.
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