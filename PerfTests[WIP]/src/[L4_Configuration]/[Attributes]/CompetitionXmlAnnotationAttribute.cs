using System;

using BenchmarkDotNet.Helpers;

using CodeJam.PerfTests.Running.SourceAnnotations;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>Attribute for benchmark classes that stores source annotations as embedded xml resource.</summary>
	/// <remarks>
	/// In case the <see cref="ResourcePath"/> is not set:
	/// Let's say there's benchmark class with full type name 'MyNamespace.MyBenchmark'
	/// and it is located at
	/// <code>%project_root%\Some Dir\AnotherDir\Benchmarks.cs</code>.
	/// The default namespace for the project is 'MyAmazingNamespace'.
	/// The resource containing source annotations should be located at
	/// <code>%project_root%\Some Dir\AnotherDir\Benchmarks.xml</code>.
	/// And the attribute should be declared as
	/// <code>
	/// [CompetitionXmlAnnotation("MyAmazingNamespace.Some_Dir.AnotherDir.Benchmarks.xml")]
	/// </code>
	/// Note that all non-alphanumeric symbols are replaced with '_'.
	/// </remarks>
	// DONTTOUCH: DO NOT change Inherited = false as it will break annotation system
	// ReSharper disable once RedundantAttributeUsageProperty
	[PublicAPI, MeansImplicitUse]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
	public class CompetitionXmlAnnotationAttribute : Attribute, IAnnotationStorageSource
	{
		private readonly AttributeValue<IAnnotationStorage> _attributeValue;

		/// <summary>Constructor for xml annotation attribute.</summary>
		/// <param name="resourceName">
		/// The name of the resource containing xml document with source annotations.
		/// If the <see cref="ResourcePath"/> is not set
		/// the resource file should be placed in the direcory with the source file for the benchmark.
		/// See remarks section for class-level documentation for detailed example.
		/// </param>
		public CompetitionXmlAnnotationAttribute([NotNull] string resourceName)
		{
			Code.NotNullNorEmpty(resourceName, nameof(resourceName));

			ResourceName = resourceName;

			_attributeValue = new AttributeValue<IAnnotationStorage>(
				() => new XmlAnnotationStorage(ResourceName, ResourcePath, UseFullTypeName));
		}

		/// <summary>
		/// Gets name of the resource containing xml document with source annotations.
		/// </summary>
		/// <value>The name of the resource containing xml document with source annotations.</value>
		[NotNull]
		public string ResourceName { get; }

		/// <summary>
		/// Gets or sets path to the resource containing xml document with source annotations.
		/// Should be relative to the source file the attribute is applied to.
		/// If not set then path to the resource should be same as path to the source file (resource's extension should be '.xml').
		/// </summary>
		/// <value>The relative path to the resource containing xml document with source annotations.</value>
		[CanBeNull]
		public string ResourcePath { get; set; }

		/// <summary>Use full type name to search for the XML annotation.</summary>
		/// <value><c>true</c> if full type name should be used in annotations; otherwise, <c>false</c>.</value>
		public bool UseFullTypeName { get; set; }

		/// <summary>Gets annotation storage.</summary>
		/// <value>The annotation storage.</value>
		IAnnotationStorage IAnnotationStorageSource.AnnotationStorage => _attributeValue.Value;
	}
}