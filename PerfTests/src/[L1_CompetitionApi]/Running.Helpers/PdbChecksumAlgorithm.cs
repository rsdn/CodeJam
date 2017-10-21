using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Helpers
{
	/// <summary>
	/// Known PDB algorithms
	/// </summary>
	public enum PdbChecksumAlgorithm
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