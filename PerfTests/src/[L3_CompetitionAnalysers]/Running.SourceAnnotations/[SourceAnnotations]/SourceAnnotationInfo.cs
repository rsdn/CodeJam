using System;
using System.Reflection;

using CodeJam.PerfTests.Running.Helpers;
using CodeJam.Ranges;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>Description of source file that contains method from the benchmark.</summary>
	internal class SourceAnnotationInfo
	{
		/// <summary>Initializes a new instance of the <see cref="SourceAnnotationInfo"/> class.</summary>
		/// <param name="path">The path to the source file.</param>
		/// <param name="methodLinesMap">Range that stores start/end lines for each method in the document.</param>
		/// <param name="sourceLanguage">Language of the source.</param>
		/// <param name="checksumAlgorithm">The checksum algorithm.</param>
		/// <param name="checksum">The checksum.</param>
		public SourceAnnotationInfo(
			string path,
			CompositeRange<int, MethodBase> methodLinesMap,
			SourceLanguage sourceLanguage,
			PdbChecksumAlgorithm checksumAlgorithm,
			byte[] checksum)
		{
			Code.NotNullNorEmpty(path, nameof(path));
			Code.NotNull(checksum, nameof(checksum));

			Path = path;
			MethodLinesMap = methodLinesMap;
			SourceLanguage = sourceLanguage;
			ChecksumAlgorithm = checksumAlgorithm;
			Checksum = checksum;
		}

		/// <summary>Gets path to the source file.</summary>
		/// <value>The path to the source file.</value>
		[NotNull]
		public string Path { get; }

		/// <summary>Gets range that stores start/end lines for each method in the document..</summary>
		/// <value>The range that stores start/end lines for each method in the document..</value>
		public CompositeRange<int, MethodBase> MethodLinesMap { get; }

		/// <summary>Gets language of the source.</summary>
		/// <value>The language of the source.</value>
		public SourceLanguage SourceLanguage { get; }

		/// <summary>Gets checksum algorithm.</summary>
		/// <value>The checksum algorithm.</value>
		public PdbChecksumAlgorithm ChecksumAlgorithm { get; }

		/// <summary>Gets checksum.</summary>
		/// <value>The checksum.</value>
		[NotNull]
		public byte[] Checksum { get; }
	}
}