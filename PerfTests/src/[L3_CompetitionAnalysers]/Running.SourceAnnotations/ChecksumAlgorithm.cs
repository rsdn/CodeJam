using System;
using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Known hashing algorithms
	/// </summary>
	internal enum ChecksumAlgorithm
	{
		/// <summary>Unknown</summary>
		[UsedImplicitly]
		Unknown = 0,

		/// <summary>MD5</summary>
		Md5,

		/// <summary>SHA1</summary>
		Sha1
	}
}