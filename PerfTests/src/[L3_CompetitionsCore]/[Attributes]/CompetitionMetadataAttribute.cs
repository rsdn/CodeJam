using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Attribute for benchmark classes that stores competition limits as embedded xml resource.</summary>
	/// <remarks>
	/// In case the <see cref="MetadataResourcePath"/> is not set:
	/// Let's say there's benchmark class with full type name 'MyNamespace.MyBenchmark'
	/// and it's located at
	/// <code>%project_root%\Some Dir\AnotherDir\Benchmarks.cs</code>.
	/// The default namespace for the project is 'MyAmazingNamespace'.
	/// So, the resource containing competition limits should be located at
	/// <code>%project_root%\Some Dir\AnotherDir\Benchmarks.xml</code>.
	/// And the attribute should be declared as
	/// <code>
	/// [CompetitionMetadata("MyAmazingNamespace.Some_Dir.AnotherDir.Benchmarks.xml")]
	/// </code>
	/// Note that all non-alphanumeric symbols are replaced with '_'.
	/// </remarks>
	// DONTTOUCH: DO NOT change Inherited = false as it will break annotation system
	// ReSharper disable once RedundantAttributeUsageProperty
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
	[PublicAPI, MeansImplicitUse]
	public class CompetitionMetadataAttribute : Attribute
	{
		/// <summary>Constructor for competition metadata attribute.</summary>
		/// <param name="metadataResourceName">
		/// The name of the resource containing xml document with competition limits.
		/// If the <see cref="MetadataResourcePath"/> is not set
		/// the resource file should be placed in the direcory with the source file for the benchmark.
		/// See remarks section at attribute documentation for detailed example.
		/// </param>
		public CompetitionMetadataAttribute([NotNull] string metadataResourceName)
		{
			Code.NotNullNorEmpty(metadataResourceName, nameof(metadataResourceName));

			MetadataResourceName = metadataResourceName;
		}

		/// <summary>
		/// The name of the resource containing xml document with competition limits.
		/// </summary>
		/// <value>The name of the resource containing xml document with competition limits.</value>
		[NotNull]
		public string MetadataResourceName { get; }

		/// <summary>
		/// The path to the resource containing xml document with competition limits.
		/// Should be relative to the source file the attribute is applied to.
		/// If not set then path to the resource should be same as path to the source file (resource's extension should be '.xml').
		/// </summary>
		/// <value>The relative path to the resource containing xml document with competition limits.</value>
		[CanBeNull]
		public string MetadataResourcePath { get; set; }

		/// <summary>Use full type name in XML annotations.</summary>
		/// <value><c>true</c> if full type name should be used in annotations; otherwise, <c>false</c>.</value>
		public bool UseFullTypeName { get; set; }
	}
}