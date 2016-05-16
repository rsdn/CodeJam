using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using JetBrains.Annotations;

using Microsoft.DiaSymReader;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.SourceAnnotations
{
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	internal static partial class AnnotateSourceHelper
	{
		/// <summary>
		/// BASEDON:
		///  http://sorin.serbans.net/blog/2010/08/how-to-read-pdb-files/257/
		///  http://stackoverflow.com/questions/13911069/how-to-get-global-variables-definition-from-symbols-tables
		///  http://referencesource.microsoft.com/#System.Management/Instrumentation/MetaDataInfo.cs,45
		///  https://github.com/dotnet/roslyn/blob/master/src/Test/PdbUtilities/Shared/SymUnmanagedReaderExtensions.cs#L483
		///  http://stackoverflow.com/questions/36649271/check-that-pdb-file-matches-to-the-source
		/// </summary>
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		private static class SymbolHelpers
		{
			// BASEDON: http://stackoverflow.com/questions/36649271/check-that-pdb-file-matches-to-the-source
			public static bool TryGetSourceInfo(
				MethodBase method,
				out string sourceFileName,
				out int firstCodeLine,
				out string validationMessage)
			{
				firstCodeLine = -1;
				sourceFileName = null;
				validationMessage = null;

				var methodSymbols = TryGetMethodSymbols(method);
				if (methodSymbols != null)
				{
					int[] startLines;
					ISymUnmanagedDocument[] documents;
					GetDocsAndLines(methodSymbols, out documents, out startLines);

					if (documents.Length == 0)
						throw new InvalidOperationException($"Method {method}, no PDB data available");

					Array.Sort(startLines, documents);
					var doc = new SymDocument(documents[0]);

					validationMessage = ValidateFileHash(doc);
					if (validationMessage == null)
					{
						sourceFileName = doc.Url;
						firstCodeLine = startLines[0];
					}
				}

				return validationMessage == null;
			}

			// ReSharper disable once SuggestBaseTypeForParameter
			private static ISymUnmanagedMethod TryGetMethodSymbols(MethodBase method)
			{
				try
				{
					// ReSharper disable once PossibleNullReferenceException
					var assembly = method.DeclaringType.Assembly;
					var codeBase = new Uri(assembly.CodeBase).LocalPath;
					var codeBaseDirectory = Path.GetDirectoryName(codeBase);

					var dispenser = (IMetaDataDispenser)new CorMetaDataDispenser();
					var import = dispenser.OpenScope(codeBase, 0, typeof(IMetaDataImportStub).GUID);
					var binder = (ISymUnmanagedBinder)new CorSymBinder();

					ISymUnmanagedReader reader;
					var hr2 = binder.GetReaderForFile(import, codeBase, codeBaseDirectory, out reader);
					ThrowExceptionForHR(hr2);

					ISymUnmanagedMethod methodSymbols;
					hr2 = reader.GetMethod(method.MetadataToken, out methodSymbols);
					ThrowExceptionForHR(hr2);
					return methodSymbols;
				}
				catch (COMException)
				{
					return null;
				}
			}

			private static void GetDocsAndLines(
				ISymUnmanagedMethod methodSymbols,
				out ISymUnmanagedDocument[] documents,
				out int[] startLines)
			{
				int numAvailable;
				var hr = methodSymbols.GetSequencePointCount(out numAvailable);
				ThrowExceptionForHR(hr);

				documents = new ISymUnmanagedDocument[numAvailable];
				startLines = new int[numAvailable];

				if (numAvailable > 0)
				{
					var offsets = new int[numAvailable];
					var startColumns = new int[numAvailable];
					var endLines = new int[numAvailable];
					var endColumns = new int[numAvailable];

					int numRead;
					hr = methodSymbols.GetSequencePoints(
						numAvailable, out numRead,
						offsets, documents,
						startLines, startColumns,
						endLines, endColumns);
					ThrowExceptionForHR(hr);

					if (numRead != numAvailable)
					{
						throw new InvalidOperationException($"Read only {numRead} of {numAvailable} sequence points.");
					}
				}
			}

			#region Validate checksum
			private enum ChecksumAlgorithmKind
			{
				[UsedImplicitly]
				Unknown = 0,
				Md5,
				Sha1
			}

			private static readonly Dictionary<string, byte[]> _md5Hashes = new Dictionary<string, byte[]>();
			private static readonly Dictionary<string, byte[]> _sha1Hashes = new Dictionary<string, byte[]>();

			private const string Sha1AlgName = "SHA1";
			private const string Md5AlgName = "Md5";

			private static string ToHexString([NotNull] byte[] data) =>
				string.Concat(data.Select(b => b.ToString("X2")));

			private static string ValidateCore(
				string file,
				IDictionary<string, byte[]> fileHashes,
				string hashAlgName,
				byte[] expectedChecksum)
			{
				byte[] actualChecksum;
				lock (fileHashes)
				{
					if (!fileHashes.TryGetValue(file, out actualChecksum))
					{
						using (var f = File.OpenRead(file))
						using (var h = HashAlgorithm.Create(hashAlgName))
						{
							// ReSharper disable once PossibleNullReferenceException
							actualChecksum = h.ComputeHash(f);
						}
					}
				}

				if (expectedChecksum.SequenceEqual(actualChecksum))
				{
					// ReSharper disable once RedundantAssignment
					return null;
				}

				var expected = ToHexString(expectedChecksum);
				var actual = ToHexString(actualChecksum);

				return $"{hashAlgName} checksum validation failed. File '{file}'.\r\nActual: 0x{actual}\r\nExpected: 0x{expected}";
			}

			private static string ValidateFileHash(
				SymDocument doc)
			{
				switch (doc.ChecksumAlgorithm)
				{
					case ChecksumAlgorithmKind.Md5:
						return ValidateCore(doc.Url, _md5Hashes, Md5AlgName, doc.Checksum);
					case ChecksumAlgorithmKind.Sha1:
						return ValidateCore(doc.Url, _sha1Hashes, Sha1AlgName, doc.Checksum);
					default:
						throw new ArgumentOutOfRangeException(nameof(doc.ChecksumAlgorithm), doc.ChecksumAlgorithm, null);
				}
			}
			#endregion

			#region COM interop
			// ReSharper disable IdentifierTypo
			// ReSharper disable CommentTypo
			// ReSharper disable InconsistentNaming
			private const int E_FAIL = unchecked((int)0x80004005);
			private const int E_NOTIMPL = unchecked((int)0x80004001);
			private static readonly IntPtr _ignoreIErrorInfo = new IntPtr(-1);

			// BASEDON https://github.com/dotnet/roslyn/blob/master/src/Test/PdbUtilities/Shared/SymUnmanagedReaderExtensions.cs#L483
			private static void ThrowExceptionForHR(int hr)
			{
				// E_FAIL indicates "no info".
				// E_NOTIMPL indicates a lack of ISymUnmanagedReader support (in a particular implementation).
				if (hr < 0 && hr != E_FAIL && hr != E_NOTIMPL)
				{
					Marshal.ThrowExceptionForHR(hr, _ignoreIErrorInfo);
				}
			}

			private class SymDocument
			{
				// guids are from corsym.h
				private static readonly Guid CorSym_SourceHash_MD5 = new Guid("406ea660-64cf-4c82-b6f0-42d48172a799");
				private static readonly Guid CorSym_SourceHash_SHA1 = new Guid("ff1816ec-aa5e-4d10-87f7-6f4963833460");

				public SymDocument(ISymUnmanagedDocument doc)
				{
					if (doc == null)
						throw new ArgumentNullException(nameof(doc));

					int len;
					var hr = doc.GetUrl(0, out len, null);
					ThrowExceptionForHR(hr);
					if (len > 0)
					{
						var urlChars = new char[len];
						hr = doc.GetUrl(len, out len, urlChars);
						ThrowExceptionForHR(hr);
						Url = new string(urlChars, 0, len - 1);
					}

					hr = doc.GetChecksum(0, out len, null);
					ThrowExceptionForHR(hr);

					if (len > 0)
					{
						Checksum = new byte[len];
						hr = doc.GetChecksum(len, out len, Checksum);
						ThrowExceptionForHR(hr);
					}

					var id = Guid.Empty;
					hr = doc.GetChecksumAlgorithmId(ref id);
					ThrowExceptionForHR(hr);

					ChecksumAlgorithmId = id;
					if (id == CorSym_SourceHash_MD5)
					{
						ChecksumAlgorithm = ChecksumAlgorithmKind.Md5;
					}
					else if (id == CorSym_SourceHash_SHA1)
					{
						ChecksumAlgorithm = ChecksumAlgorithmKind.Sha1;
					}
				}

				public string Url { get; }
				public byte[] Checksum { get; }
				[UsedImplicitly]
				private Guid ChecksumAlgorithmId { get; }
				public ChecksumAlgorithmKind ChecksumAlgorithm { get; }
			}

			/// <summary>
			/// CoClass for getting an ISymUnmanagedBinder
			/// </summary>
			[ComImport, Guid("0A29FF9E-7F9C-4437-8B11-F424491E3931")]
			internal class CorSymBinder { }

			/// <summary>
			/// CoClass for getting an IMetaDataDispenser
			/// </summary>
			[ComImport]
			[Guid("E5CB7A31-7512-11d2-89CE-0080C792E5D8")]
			[TypeLibType(TypeLibTypeFlags.FCanCreate)]
			[ClassInterface(ClassInterfaceType.None)]
			private class CorMetaDataDispenser { }

			[ComImport]
			[Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			[TypeLibType(TypeLibTypeFlags.FRestricted)]
			private interface IMetaDataImportStub
			{
				// ...
			}

			/// <summary>
			/// This version of the IMetaDataDispenser interface defines
			/// the interfaces so that the last parameter from cor.h
			/// is the return value of the methods.  The 'raw' way to
			/// implement these methods is as follows:
			///    void OpenScope(
			///        [In][MarshalAs(UnmanagedType.LPWStr)]  string   szScope,
			///        [In] UInt32 dwOpenFlags,
			///        [In] ref Guid riid,
			///        [Out] out IntPtr ppIUnk);
			/// The way to call this other version is as follows
			///    IntPtr unk;
			///    dispenser.OpenScope(assemblyName, 0, ref guidIMetaDataImport, out unk);
			///    importInterface = (IMetaDataImport)Marshal.GetObjectForIUnknown(unk);
			///    Marshal.Release(unk);
			/// </summary>
			[ComImport]
			[Guid("809C652E-7396-11D2-9771-00A0C9B4D50C")]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown /*0x0001*/)]
			[TypeLibType(TypeLibTypeFlags.FRestricted /*0x0200*/)]
			private interface IMetaDataDispenser
			{
				[return: MarshalAs(UnmanagedType.Interface)]
				object DefineScope(
					[In] ref Guid rclsid,
					[In] uint dwCreateFlags,
					[In] ref Guid riid);

				[return: MarshalAs(UnmanagedType.Interface)]
				object OpenScope(
					[In] [MarshalAs(UnmanagedType.LPWStr)] string szScope,
					[In] uint dwOpenFlags,
					[In] ref Guid riid);

				[return: MarshalAs(UnmanagedType.Interface)]
				object OpenScopeOnMemory(
					[In] IntPtr pData,
					[In] uint cbData,
					[In] uint dwOpenFlags,
					[In] ref Guid riid);
			}

			// ReSharper restore InconsistentNaming
			// ReSharper restore CommentTypo
			// ReSharper restore IdentifierTypo
			#endregion
		}
	}
}