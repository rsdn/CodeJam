using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>Description of embedded resource containing xml document with competition limits.</summary>
	internal class CompetitionMetadata
	{
		/// <summary>Initializes a new instance of the <see cref="CompetitionMetadata"/> class.</summary>
		/// <param name="metadataResourceName">The name of the resource containing xml document with competition limits.</param>
		/// <param name="metadataResourcePath">The relative path to the resource containing xml document with competition limits.</param>
		/// <param name="useFullTypeName">Use full type name in XML annotations.</param>
		public CompetitionMetadata(
			[NotNull] string metadataResourceName,
			[CanBeNull] string metadataResourcePath,
			bool useFullTypeName)
		{
			Code.NotNullNorEmpty(metadataResourceName, nameof(metadataResourceName));

			MetadataResourceName = metadataResourceName;
			MetadataResourcePath = metadataResourcePath;
			UseFullTypeName = useFullTypeName;
		}

		/// <summary>
		/// The name of the resource containing xml document with competition limits.
		/// If not set then path to the resource should be same as path to the source file (resource's extension should be '.xml').
		/// </summary>
		/// <value>The name of the resource containing xml document with competition limits.</value>
		[NotNull]
		public string MetadataResourceName { get; }

		/// <summary>
		/// The path to the resource containing xml document with competition limits.
		/// Should be relative to the source file the attribute is applied to.
		/// </summary>
		/// <value>The relative path to the resource containing xml document with competition limits.</value>
		[CanBeNull]
		public string MetadataResourcePath { get; }

		/// <summary>Use full type name in XML annotations.</summary>
		/// <value><c>true</c> if full type name should be used in annotations; otherwise, <c>false</c>.</value>
		public bool UseFullTypeName { get; }
	}
}