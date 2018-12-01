using System;
using System.IO;
using System.Security.Cryptography;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Helpers
{
	/// <summary>
	/// PDB helpers
	/// </summary>
	public static class PdbHelpers
	{
		private const string Sha1AlgName = "SHA1";
		private const string Md5AlgName = "Md5";

		[NotNull]
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
		[NotNull]
		public static byte[] TryGetChecksum([NotNull] string file, PdbChecksumAlgorithm checksumAlgorithm)
		{
			Code.NotNullNorEmpty(file, nameof(file));

			var algName = GetChecksumName(checksumAlgorithm);
			if (!File.Exists(file))
				return Array<byte>.Empty;

			using (var stream = File.OpenRead(file))
			using (var hashAlgorithm = HashAlgorithm.Create(algName))
			{
				// ReSharper disable once PossibleNullReferenceException
				return hashAlgorithm.ComputeHash(stream);
			}
		}

		/// <summary>Gets checksum for resource if it exists.</summary>
		/// <param name="resourceKey">The resource key.</param>
		/// <param name="checksumAlgorithm">The checksum algorithm to use.</param>
		/// <returns>Checksum for resource or empty byte array if the resource does not exist.</returns>
		[NotNull]
		public static byte[] TryGetChecksum(ResourceKey resourceKey, PdbChecksumAlgorithm checksumAlgorithm)
		{
			Code.AssertArgument(!resourceKey.IsEmpty, nameof(resourceKey), "The resource key should be non empty.");

			var algName = GetChecksumName(checksumAlgorithm);
			using (var stream = resourceKey.TryGetResourceStream())
			{
				if (stream == null)
					return Array<byte>.Empty;

				using (var hashAlgorithm = HashAlgorithm.Create(algName))
				{
					// ReSharper disable once PossibleNullReferenceException
					return hashAlgorithm.ComputeHash(stream);
				}
			}
		}
	}
}