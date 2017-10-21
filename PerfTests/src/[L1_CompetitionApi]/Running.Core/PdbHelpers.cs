
using System.IO;
using System.Security.Cryptography;

using CodeJam.Collections;

namespace CodeJam.PerfTests.Running.Core
{
	/// <summary>
	/// PDB helpers
	/// </summary>
	public static class PdbHelpers
	{

		private const string Sha1AlgName = "SHA1";
		private const string Md5AlgName = "Md5";

		private static string GetChecksumName(PdbChecksumAlgorithm checksumAlgorithm)
		{
			switch (checksumAlgorithm)
			{
				case PdbChecksumAlgorithm.Md5:
					return Md5AlgName;
				case PdbChecksumAlgorithm.Sha1:
					return Sha1AlgName;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(checksumAlgorithm), checksumAlgorithm);
			}
		}

		/// <summary>Gets checksum for file if it exists.</summary>
		/// <param name="file">The file.</param>
		/// <param name="checksumAlgorithm">The checksum algorithm to use.</param>
		/// <returns>Checksum for file or empty byte array if the file does not exist.</returns>
		public static byte[] TryGetChecksum(
			string file,
			PdbChecksumAlgorithm checksumAlgorithm)
		{
			var algName = GetChecksumName(checksumAlgorithm);

			if (!File.Exists(file))
				return Array<byte>.Empty;

			using (var f = File.OpenRead(file))
			using (var h = HashAlgorithm.Create(algName))
			{
				// ReSharper disable once PossibleNullReferenceException
				return h.ComputeHash(f);
			}
		}

		/// <summary>Gets checksum for resource if it exists.</summary>
		/// <param name="resourceKey">The resource key.</param>
		/// <param name="checksumAlgorithm">The checksum algorithm to use.</param>
		/// <returns>Checksum for resource or empty byte array if the resource does not exist.</returns>
		public static byte[] TryGetChecksum(
			ResourceKey resourceKey,
			PdbChecksumAlgorithm checksumAlgorithm)
		{
			var algName = GetChecksumName(checksumAlgorithm);

			using (var s = resourceKey.TryGetResourceStream())
			{
				if (s == null)
					return Array<byte>.Empty;

				using (var h = HashAlgorithm.Create(algName))
				{
					// ReSharper disable once PossibleNullReferenceException
					return h.ComputeHash(s);
				}
			}
		}
	}
}
