using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Attribute for benchmark classes that stores competition limits as embedded XML resource.</summary>
	/// <remarks>
	/// Let's say there's benchmark class named MyNamespace.MyBenchmark
	/// and it's located at
	/// <code>%project_root%\Some Dir\AnotherDir\Benchmarks.cs</code>.
	/// The default namespace for the project is MyAmazingNamespace.
	/// So, the resource containing competition limits should be located at
	/// <code>%project_root%\Some Dir\AnotherDir\Benchmarks.xml</code>.
	/// And the attribute should be declared as
	/// <code>
	/// [CompetitionMetadata("MyAmazingNamespace.Some_Dir.AnotherDir.Benchmarks.xml")]
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
		/// The resource file should be placed in the direcory with the source file for the benchmark.
		/// Path to the resource should be same as path to the source file (file ext should be '.xml').
		/// See remarks section at attribute documentation for detailed example.
		/// </param>
		public CompetitionMetadataAttribute([NotNull] string metadataResourceName)
		{
			Code.NotNullNorEmpty(metadataResourceName, nameof(metadataResourceName));

			MetadataResourceName = metadataResourceName;
		}

		/// <summary>The name of the resource containing xml document with competition limits.</summary>
		/// <value>The name of the resource containing xml document with competition limits.</value>
		public string MetadataResourceName { get; }
	}
}