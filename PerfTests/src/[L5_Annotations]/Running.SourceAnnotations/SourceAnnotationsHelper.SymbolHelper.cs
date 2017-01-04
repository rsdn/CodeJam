using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Ranges;
using CodeJam.Reflection;

using JetBrains.Annotations;

using static CodeJam.PerfTests.Running.SourceAnnotations.SymbolInterop;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{

	internal static partial class SourceAnnotationsHelper
	{
		/// <summary>
		/// Known hashing algorithms
		/// </summary>
		private enum ChecksumAlgorithm
		{
			/// <summary>Unknown</summary>
			[UsedImplicitly]
			Unknown = 0,
			/// <summary>MD5</summary>
			Md5,
			/// <summary>SHA1</summary>
			Sha1
		}
		/// <summary>
		/// Known language types
		/// </summary>
		private enum LanguageType
		{
			[UsedImplicitly]
			Unknown = 0,
			/// <summary>C#</summary>
			CSharp,
			/// <summary>Visual Basic</summary>
			VisualBasic
		}

		/// <summary>
		/// Source file info.
		/// </summary>
		private class SourceFileInfo
		{
			/// <summary>Initializes a new instance of the <see cref="SourceFileInfo"/> class.</summary>
			/// <param name="path">The path to the source file.</param>
			/// <param name="methodMap">Range that stores start/end lines for each method in the document.</param>
			/// <param name="languageType">Type of the language.</param>
			/// <param name="checksumAlgorithm">The checksum algorithm.</param>
			/// <param name="checksum">The checksum.</param>
			public SourceFileInfo(
				string path,
				CompositeRange<int, MethodBase> methodMap,
				LanguageType languageType,
				ChecksumAlgorithm checksumAlgorithm,
				byte[] checksum)
			{
				Code.NotNullNorEmpty(path, nameof(path));
				Code.NotNull(checksum, nameof(checksum));

				Path = path;
				MethodMap = methodMap;
				LanguageType = languageType;
				ChecksumAlgorithm = checksumAlgorithm;
				Checksum = checksum;
			}

			/// <summary>Path to the source file.</summary>
			/// <value>Path to the source file.</value>
			[NotNull]
			public string Path { get; }
			/// <summary>Range that stores start/end lines for each method in the document..</summary>
			/// <value>Range that stores start/end lines for each method in the document..</value>
			public CompositeRange<int, MethodBase> MethodMap { get; }
			/// <summary>The type of the language.</summary>
			/// <value>The type of the language.</value>
			public LanguageType LanguageType { get; }
			/// <summary>The checksum algorithm.</summary>
			/// <value>The checksum algorithm.</value>
			public ChecksumAlgorithm ChecksumAlgorithm { get; }
			/// <summary>The checksum.</summary>
			/// <value>The checksum.</value>
			[NotNull]
			public byte[] Checksum { get; }
		}

		/// <summary>
		/// BASEDON:
		///  http://sorin.serbans.net/blog/2010/08/how-to-read-pdb-files/257/
		///  http://stackoverflow.com/questions/13911069/how-to-get-global-variables-definition-from-symbols-tables
		///  http://stackoverflow.com/questions/36649271/check-that-pdb-file-matches-to-the-source
		/// </summary>
		private static class SymbolHelper
		{
			// Guids are from https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/PEWriter/DebugSourceDocument.cs
			private static readonly Guid _corSymLanguageTypeCSharp = new Guid("3f5162f8-07c6-11d3-9053-00c04fa302a1");
			private static readonly Guid _corSymLanguageTypeBasic = new Guid("3a12d0b8-c26c-11d0-b442-00a0244a1dd2");

			// ReSharper disable once CommentTypo
			// guids are from corsym.h
			private static readonly Guid _corSymSourceHashMd5 = new Guid("406ea660-64cf-4c82-b6f0-42d48172a799");
			private static readonly Guid _corSymSourceHashSha1 = new Guid("ff1816ec-aa5e-4d10-87f7-6f4963833460");

			/// <summary>Tries to get path to the source file for the method.</summary>
			/// <param name="method">The method.</param>
			/// <param name="competitionState">State of the run.</param>
			/// <returns>
			/// Path to the source file, or <c>null</c> if there is no PDB info for the method
			/// </returns>
			[CanBeNull]
			public static string TryGetSourcePath(
				[NotNull] MethodBase method,
				[NotNull] CompetitionState competitionState)
			{
				Code.NotNull(method, nameof(method));
				Code.NotNull(competitionState, nameof(competitionState));

				string result = null;
				try
				{
					var reader = GetReader(method);

					var documentInfo = TryGetDocumentInfo(method, reader, competitionState);
					result = documentInfo?.GetName();

					if (result == null)
					{
						// ReSharper disable once PossibleNullReferenceException
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.SetupError,
							$"Method {method.DeclaringType.Name}.{method.Name}, no PDB data available.");
					}
				}
				catch (COMException ex)
				{
					// ReSharper disable once PossibleNullReferenceException
					competitionState.WriteExceptionMessage(
						MessageSource.Analyser, MessageSeverity.ExecutionError,
						"Could not parse method symbols.", ex);
				}

				return result;
			}

			/// <summary>Tries to get source file information.</summary>
			/// <param name="method">The method.</param>
			/// <param name="competitionState">State of the run.</param>
			/// <returns>Source file info.</returns>
			[CanBeNull]
			public static SourceFileInfo TryGetSourceInfo(
				MethodBase method,
				CompetitionState competitionState)
			{
				SourceFileInfo result = null;
				try
				{
					result = TryGetSourceInfoCore(method, competitionState);
					if (result == null)
					{
						// ReSharper disable once PossibleNullReferenceException
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.SetupError,
							$"Method {method.DeclaringType.Name}.{method.Name}, no PDB data available.");
					}
				}
				catch (COMException ex)
				{
					// ReSharper disable once PossibleNullReferenceException
					competitionState.WriteExceptionMessage(
						MessageSource.Analyser, MessageSeverity.ExecutionError,
						"Could not parse method symbols.", ex);
				}

				return result;
			}

			[CanBeNull]
			private static SourceFileInfo TryGetSourceInfoCore(MethodBase method, CompetitionState competitionState)
			{
				var reader = GetReader(method);
				var documentInfo = TryGetDocumentInfo(method, reader, competitionState);
				if (documentInfo == null)
					return null;

				var path = documentInfo.GetName();
				// ReSharper disable once PossibleNullReferenceException
				var methodMap = GetMethodMap(documentInfo, reader, method.DeclaringType.Assembly);
				var languageType = LanguageType.Unknown;
				var checksumAlgorithm = ChecksumAlgorithm.Unknown;
				var checksum = documentInfo.GetChecksum();

				var checksumAlgorithmId = documentInfo.GetHashAlgorithm();
				if (checksumAlgorithmId == _corSymSourceHashMd5)
				{
					checksumAlgorithm = ChecksumAlgorithm.Md5;
				}
				else if (checksumAlgorithmId == _corSymSourceHashSha1)
				{
					checksumAlgorithm = ChecksumAlgorithm.Sha1;
				}

				var languageTypeId = documentInfo.GetLanguage();
				if (languageTypeId == _corSymLanguageTypeCSharp)
				{
					languageType = LanguageType.CSharp;
				}
				else if (languageTypeId == _corSymLanguageTypeBasic)
				{
					languageType = LanguageType.VisualBasic;
				}

				return new SourceFileInfo(path, methodMap, languageType, checksumAlgorithm, checksum);
			}

			#region Helpers
			// ReSharper disable once SuggestBaseTypeForParameter
			[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
			[NotNull]
			private static ISymUnmanagedReader GetReader(MethodBase method)
			{
				// ReSharper disable once PossibleNullReferenceException
				var assembly = method.DeclaringType.Assembly;

				// TODO: fix it?
				if (assembly.Modules.Skip(1).Any())
					throw new NotSupportedException("Multi-module assemblies are not supported.");

				var assemblyPath = assembly.GetAssemblyPath();
				var codeBaseDirectory = Path.GetDirectoryName(assemblyPath);

				var dispenser = (IMetaDataDispenser)new CorMetaDataDispenser();
				var import = dispenser.OpenScope(assemblyPath, 0, typeof(IMetaDataImportStub).GUID);
				var binder = (ISymUnmanagedBinder)new CorSymBinder();

				var hr = binder.GetReaderForFile(import, assemblyPath, codeBaseDirectory, out var reader);
				ThrowExceptionForHR(hr);

				return reader;
			}

			#region TryGetDocumentInfo
			[CanBeNull]
			private static ISymUnmanagedDocument TryGetDocumentInfo(
				MethodBase method, ISymUnmanagedReader reader,
				CompetitionState competitionState)
			{
				var implMethod = ResolveBestMethodInfo(method);

				var methodSymbols = reader.GetMethod(implMethod.MetadataToken);
				var documents = methodSymbols.GetDocumentsForMethod();

				if (documents.Length == 0)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						// ReSharper disable once PossibleNullReferenceException
						$"Method {method.DeclaringType.Name}.{method.Name}: no code found for the method.");
					return null;
				}
				if (documents.Length > 1)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						// ReSharper disable once PossibleNullReferenceException
						$"Method {method.DeclaringType.Name}.{method.Name}: method code spans multiple documents, this is not supported for now.");
					return null;
				}
				return documents[0];
			}

			// BASEDON: https://github.com/aspnet/Testing/issues/138
			// BASEDON: https://github.com/aspnet/dnx/blob/bebc991012fe633ecac69675b2e892f568b927a5/src/Microsoft.Dnx.TestHost/TestAdapter/SourceInformationProvider.cs#L88-L104
			private static MethodBase ResolveBestMethodInfo(MethodBase methodBase)
			{
				if (!(methodBase is MethodInfo method))
					return methodBase;

				// If a method has a StateMachineAttribute, then all of the user code will show up
				// in the symbols associated with the compiler-generated code. So, we need to look
				// for the 'MoveNext' on the generated type and resolve symbols for that.
				var attribute = method.GetCustomAttribute<StateMachineAttribute>();
				if (attribute?.StateMachineType == null)
					return method;

				return attribute.StateMachineType.GetMethod(
					nameof(IEnumerator.MoveNext),
					BindingFlags.Instance | BindingFlags.NonPublic);
			}
			#endregion

			#region Method map
			/// <summary>Returns range that stores start/end lines for each method in the document.</summary>
			/// <param name="documentInfo">Pdb document.</param>
			/// <param name="reader">Pdb reader.</param>
			/// <param name="assembly">Assembly that contains methods.</param>
			/// <returns>Range that stores start/end lines for each method in the document.</returns>
			private static CompositeRange<int, MethodBase> GetMethodMap(
				ISymUnmanagedDocument documentInfo,
				ISymUnmanagedReader reader,
				Assembly assembly)
			{
				/*
					Need to collect all sequence points as they may interleave. As example:
					```
					class C                      // line 1
					{
						int a = 11;              // line 3
						int M() => a;
						int b;
						public C() { a = 123; }  // line 6
					}
					```
					sequence points for C() - lines 3 & 6, sequence points for M() - line 4.
				*/
				var module = assembly.ManifestModule;
				var methods =
					(from m in reader.GetMethodsInDocument(documentInfo)
					 select module.ResolveMethod(m.GetToken()) into method
					 orderby IsCompilerGenerated(method) ? 0 : 1 // non-generated wins
					 select method)
					.ToArray();

				var methodLineMapping = new Dictionary<int, MethodBase>();
				foreach (var method in methods)
				{
					AddLineMapping(method, documentInfo, methodLineMapping, reader);
				}

				return methodLineMapping
					.OrderBy(p => p.Key)
					.GroupWhileEquals(p => p.Value, p => p.Key)
					.Select(g => Range.Create(g.First(), g.Last(), g.Key)).ToCompositeRange();
			}

			private static bool IsCompilerGenerated(MethodBase method)
			{
				var compilerGenerated = typeof(CompilerGeneratedAttribute);
				// ReSharper disable once AssignNullToNotNullAttribute
				return Attribute.IsDefined(method, compilerGenerated) ||
					Attribute.IsDefined(method.DeclaringType, compilerGenerated);
			}

			private static void AddLineMapping(
				MethodBase method,
				ISymUnmanagedDocument documentInfo,
				Dictionary<int, MethodBase> methodLineMapping,
				ISymUnmanagedReader reader)
			{
				var implMethod = ResolveBestMethodInfo(method);

				var docName = documentInfo.GetName();
				var points = reader.GetMethod(implMethod.MetadataToken).GetSequencePoints();
				foreach (var point in points)
				{
					if (point.IsHidden)
						continue;

					if (point.Document.GetName() != docName)
						continue;

					for (var i = point.StartLine; i <= point.EndLine; i++)
					{
						// DONTTOUCH: DO NOT change to .Add(). Last method wins.
						methodLineMapping[i] = method;
					}
				}
			}
			#endregion
			#endregion
		}
	}
}