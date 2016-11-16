using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Reflection;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	[SuppressMessage("ReSharper", "InvocationIsSkipped")]
	internal static partial class SourceAnnotationsHelper
	{
		/// <summary>
		/// BASEDON:
		///  http://sorin.serbans.net/blog/2010/08/how-to-read-pdb-files/257/
		///  http://stackoverflow.com/questions/13911069/how-to-get-global-variables-definition-from-symbols-tables
		///  http://stackoverflow.com/questions/36649271/check-that-pdb-file-matches-to-the-source
		/// </summary>
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
		private static partial class SymbolHelpers
		{
			public static bool TryGetSourceInfo(
				MethodBase method,
				CompetitionState competitionState,
				out string sourceFileName,
				out int firstCodeLine)
			{
				firstCodeLine = -1;
				sourceFileName = null;

				var methodSymbols = TryGetMethodSymbols(method, competitionState);
				if (methodSymbols != null)
				{
					int[] startLines;
					ISymUnmanagedDocument[] documents;

					if (TryGetDocsAndLines(methodSymbols, competitionState, out documents, out startLines))
					{
						// TODO: collect count of method lines and check for partial methods?
						DebugCode.BugIf(
							documents.Length > 1,
							"Temp assertion: current approach with sort is very naive. Use something better?");
						Array.Sort(startLines, documents);
						var doc = new SymDocument(documents[0]);

						if (TryValidateFileHash(doc, competitionState))
						{
							sourceFileName = doc.Url;
							firstCodeLine = startLines[0];
						}
					}
					else
					{
						// ReSharper disable once PossibleNullReferenceException
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.SetupError,
							$"Method {method.DeclaringType.Name}.{method.Name}, no PDB data available.");
					}
				}

				return sourceFileName != null;
			}

			// ReSharper disable once SuggestBaseTypeForParameter
			private static ISymUnmanagedMethod TryGetMethodSymbols(MethodBase method, CompetitionState competitionState)
			{
				try
				{
					// ReSharper disable once PossibleNullReferenceException
					var assembly = method.DeclaringType.Assembly;
					var assemblyPath = assembly.GetAssemblyPath();
					var codeBaseDirectory = Path.GetDirectoryName(assemblyPath);

					var dispenser = (IMetaDataDispenser)new CorMetaDataDispenser();
					var import = dispenser.OpenScope(assemblyPath, 0, typeof(IMetaDataImportStub).GUID);
					var binder = (ISymUnmanagedBinder)new CorSymBinder();

					ISymUnmanagedReader reader;
					var hr = binder.GetReaderForFile(import, assemblyPath, codeBaseDirectory, out reader);
					InteropUtilities.ThrowExceptionForHR(hr);

					ISymUnmanagedMethod methodSymbols;
					hr = reader.GetMethod(method.MetadataToken, out methodSymbols);
					InteropUtilities.ThrowExceptionForHR(hr);
					return methodSymbols;
				}
				catch (COMException ex)
				{
					// ReSharper disable once PossibleNullReferenceException
					competitionState.WriteExceptionMessage(
						MessageSource.Analyser, MessageSeverity.ExecutionError,
						$"Method {method.DeclaringType.Name}.{method.Name}, no PDB data available.", ex);

					return null;
				}
			}

			private static bool TryGetDocsAndLines(
				ISymUnmanagedMethod methodSymbols,
				CompetitionState competitionState,
				out ISymUnmanagedDocument[] documents,
				out int[] startLines)
			{
				try
				{
					documents = InteropUtilities.GetDocumentsForMethod(methodSymbols);
					startLines = new int[documents.Length];

					for (int i = 0; i < startLines.Length; i++)
					{
						int stub;
						InteropUtilities.GetSourceExtentInDocument(methodSymbols, documents[i], out startLines[i], out stub);
					}
				}
				catch (COMException ex)
				{
					documents = null;
					startLines = null;

					// ReSharper disable once PossibleNullReferenceException
					competitionState.WriteExceptionMessage(
						MessageSource.Analyser, MessageSeverity.ExecutionError,
						"Could not parse method symbols.", ex);

					return false;
				}

				return startLines.Length > 0;
			}

			#region Doc & validate checksum
			private class SymDocument
			{
				// ReSharper disable once CommentTypo
				// guids are from corsym.h
				// ReSharper disable InconsistentNaming
				private static readonly Guid CorSym_SourceHash_MD5 = new Guid("406ea660-64cf-4c82-b6f0-42d48172a799");
				private static readonly Guid CorSym_SourceHash_SHA1 = new Guid("ff1816ec-aa5e-4d10-87f7-6f4963833460");
				// ReSharper restore InconsistentNaming

				public SymDocument(ISymUnmanagedDocument doc)
				{
					if (doc == null)
						throw new ArgumentNullException(nameof(doc));

					Url = InteropUtilities.GetName(doc);
					Checksum = InteropUtilities.GetChecksum(doc);
					ChecksumAlgorithmId = InteropUtilities.GetHashAlgorithm(doc);
					if (ChecksumAlgorithmId == CorSym_SourceHash_MD5)
					{
						ChecksumAlgorithm = ChecksumAlgorithmKind.Md5;
					}
					else if (ChecksumAlgorithmId == CorSym_SourceHash_SHA1)
					{
						ChecksumAlgorithm = ChecksumAlgorithmKind.Sha1;
					}
				}

				public string Url { get; }
				public byte[] Checksum { get; }
				private Guid ChecksumAlgorithmId { get; }
				public ChecksumAlgorithmKind ChecksumAlgorithm { get; }
			}

			private enum ChecksumAlgorithmKind
			{
				[UsedImplicitly]
				Unknown = 0,
				Md5,
				Sha1
			}

			private static readonly Func<string, byte[]> _md5HashesCache = Algorithms.Memoize(
				(string f) => TryGetChecksum(f, Md5AlgName),
				true);

			private static readonly Func<string, byte[]> _sha1HashesCache = Algorithms.Memoize(
				(string f) => TryGetChecksum(f, Sha1AlgName),
				true);

			private const string Sha1AlgName = "SHA1";
			private const string Md5AlgName = "Md5";

			private static bool TryValidateFileHash(SymDocument doc, CompetitionState competitionState)
			{
				switch (doc.ChecksumAlgorithm)
				{
					case ChecksumAlgorithmKind.Md5:
						return ValidateCore(doc.Url, _md5HashesCache, Md5AlgName, doc.Checksum, competitionState);
					case ChecksumAlgorithmKind.Sha1:
						return ValidateCore(doc.Url, _sha1HashesCache, Sha1AlgName, doc.Checksum, competitionState);
					default:
						throw CodeExceptions.UnexpectedArgumentValue(nameof(doc.ChecksumAlgorithm), doc.ChecksumAlgorithm);
				}
			}

			private static bool ValidateCore(
				string file,
				Func<string, byte[]> fileHashesGetter,
				string hashAlgName,
				byte[] expectedChecksum, CompetitionState competitionState)
			{
				var actualChecksum = fileHashesGetter(file);
				if (expectedChecksum.EqualsTo(actualChecksum))
				{
					return true;
				}

				var expected = expectedChecksum.ToHexString();
				var actual = actualChecksum.ToHexString();

				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"{hashAlgName} checksum validation failed. File '{file}'." +
						$"{Environment.NewLine}\tActual: 0x{actual}" +
						$"{Environment.NewLine}\tExpected: 0x{expected}");

				return false;
			}

			[NotNull]
			private static byte[] TryGetChecksum(string file, string hashAlgName)
			{
				if (!File.Exists(file))
					return Array<byte>.Empty;

				using (var f = File.OpenRead(file))
				using (var h = HashAlgorithm.Create(hashAlgName))
				{
					// ReSharper disable once PossibleNullReferenceException
					return h.ComputeHash(f);
				}
			}
			#endregion
		}
	}
}