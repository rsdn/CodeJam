using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Attribute for benchmark classes that stores competition limits as embedded XML resource.</summary>
	/// <remarks>
	/// Let's say here's a benchmark class named MyNamespace.Benchmark123
	/// and it's located at
	/// <code>%project_root%\Some Dir\Benchmarks.cs</code>.
	/// The default namespace for the project is MyAmazingNamespace.
	/// So, the resource with competition limits should be located at
	/// <code>%project_root%\Some Dir\Benchmarks.xml</code>.
	/// And the attribute should be declared as
	/// <code>
	/// [CompetitionMetadata("MyAmazingNamespace.Some_Dir.Benchmarks.xml")]
	/// </code>
	/// Note that all non-alphanumeric symbols are replaced with '_'.
	/// </remarks>
	// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	[PublicAPI, MeansImplicitUse]
	public class CompetitionMetadataAttribute : Attribute
	{
		/// <summary>Constructor for competition metadata attribute.</summary>
		/// <param name="metadataResourceName">
		/// The name of the resource containing xml document with competition limits.
		/// The resource location and file name should match the ones for the source file with the benchmark.
		/// See remarks section at the attribute documentation for details.
		/// </param>
		public CompetitionMetadataAttribute(string metadataResourceName)
		{
			MetadataResourceName = metadataResourceName;
		}

		/// <summary>The name of the resource containing xml document with competition limits.</summary>
		/// <value>The name of the resource containing xml document with competition limits.</value>
		public string MetadataResourceName { get; }
	}
}