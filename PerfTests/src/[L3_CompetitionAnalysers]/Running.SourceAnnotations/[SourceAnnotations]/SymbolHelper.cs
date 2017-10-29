using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Helpers;
using CodeJam.Ranges;
using CodeJam.Reflection;

using JetBrains.Annotations;

using static CodeJam.PerfTests.Running.SourceAnnotations.SymbolInterop;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// BASEDON:
	/// http://sorin.serbans.net/blog/2010/08/how-to-read-pdb-files/257/
	/// http://stackoverflow.com/questions/13911069/how-to-get-global-variables-definition-from-symbols-tables
	/// http://stackoverflow.com/questions/36649271/check-that-pdb-file-matches-to-the-source
	/// </summary>
	internal static class SymbolHelper
	{
		// Guids are from https://github.com/dotnet/roslyn/blob/master/src/Compilers/Core/Portable/PEWriter/DebugSourceDocument.cs
		private static readonly Guid _corSymLanguageTypeCSharp = new Guid("3f5162f8-07c6-11d3-9053-00c04fa302a1");
		private static readonly Guid _corSymLanguageTypeBasic = new Guid("3a12d0b8-c26c-11d0-b442-00a0244a1dd2");

		// ReSharper disable once CommentTypo
		// guids are from corsym.h
		private static readonly Guid _corSymSourceHashMd5 = new Guid("406ea660-64cf-4c82-b6f0-42d48172a799");
		private static readonly Guid _corSymSourceHashSha1 = new Guid("ff1816ec-aa5e-4d10-87f7-6f4963833460");

		/// <summary>Tries to get path to the source file for the method.</summary>
		/// <param name="target">The target.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>Path to the source file, or <c>null</c> if there is no PDB info for the method</returns>
		[CanBeNull]
		public static string TryGetSourcePath(
			[NotNull] Target target,
			[NotNull] IMessageLogger messageLogger)
		{
			Code.NotNull(target, nameof(target));
			Code.NotNull(messageLogger, nameof(messageLogger));

			string result = null;
			try
			{
				// ReSharper disable once PossibleNullReferenceException
				var reader = GetReader(target.Method.DeclaringType.Module);

				var documentInfo = TryGetDocumentInfo(target, reader, messageLogger);
				result = documentInfo?.GetName();

				if (result == null)
				{
					messageLogger.WriteSetupErrorMessage(
						target,
						"No PDB data available.",
						$"Ensure that there is actual pdb file for the assembly '{target.Method.DeclaringType?.Module.GetModulePath()}'.");
				}
			}
			catch (COMException ex)
			{
				messageLogger.WriteExceptionMessage(
					MessageSeverity.ExecutionError, target,
					"Could not parse method symbols.", ex);
			}

			return result;
		}

		/// <summary>Tries to get source file information.</summary>
		/// <param name="target">The target.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>Source file info.</returns>
		[CanBeNull]
		public static SourceAnnotationInfo TryGetSourceInfo(
			[NotNull] Target target,
			[NotNull] IMessageLogger messageLogger)
		{
			SourceAnnotationInfo result = null;
			try
			{
				// ReSharper disable once PossibleNullReferenceException
				var reader = GetReader(target.Method.DeclaringType.Module);

				result = TryGetSourceInfoCore(target, reader, messageLogger);
				if (result == null)
				{
					messageLogger.WriteSetupErrorMessage(
						target,
						"No PDB data available.",
						$"Ensure that there is actual pdb file for the assembly '{target.Method.DeclaringType?.Module.GetModulePath()}'.");
				}
			}
			catch (COMException ex)
			{
				messageLogger.WriteExceptionMessage(
					MessageSeverity.ExecutionError, target,
					"Could not parse method symbols.", ex);
			}

			return result;
		}

		[CanBeNull]
		private static SourceAnnotationInfo TryGetSourceInfoCore(
			[NotNull] Target target,
			[NotNull] ISymUnmanagedReader reader,
			[NotNull] IMessageLogger messageLogger)
		{
			var documentInfo = TryGetDocumentInfo(target, reader, messageLogger);
			if (documentInfo == null)
				return null;

			var path = documentInfo.GetName();
			// ReSharper disable once PossibleNullReferenceException
			var methodLinesMap = GetMethodLinesMap(documentInfo, reader, target.Method.DeclaringType.Assembly);
			var sourceLanguage = SourceLanguage.Unknown;
			var checksumAlgorithm = PdbChecksumAlgorithm.Unknown;
			var checksum = documentInfo.GetChecksum();

			var checksumAlgorithmId = documentInfo.GetHashAlgorithm();
			if (checksumAlgorithmId == _corSymSourceHashMd5)
			{
				checksumAlgorithm = PdbChecksumAlgorithm.Md5;
			}
			else if (checksumAlgorithmId == _corSymSourceHashSha1)
			{
				checksumAlgorithm = PdbChecksumAlgorithm.Sha1;
			}

			var languageId = documentInfo.GetLanguage();
			if (languageId == _corSymLanguageTypeCSharp)
			{
				sourceLanguage = SourceLanguage.CSharp;
			}
			else if (languageId == _corSymLanguageTypeBasic)
			{
				sourceLanguage = SourceLanguage.VisualBasic;
			}

			return new SourceAnnotationInfo(path, methodLinesMap, sourceLanguage, checksumAlgorithm, checksum);
		}

		#region Helpers
		// ReSharper disable once SuggestBaseTypeForParameter
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		[NotNull]
		private static ISymUnmanagedReader GetReader([NotNull] Module module)
		{
			var modulePath = module.GetModulePath();
			var codeBaseDirectory = Path.GetDirectoryName(modulePath);

			var dispenser = (IMetaDataDispenser)new CorMetaDataDispenser();
			var import = dispenser.OpenScope(modulePath, 0, typeof(IMetaDataImportStub).GUID);
			var binder = (ISymUnmanagedBinder)new CorSymBinder();

			var hr = binder.GetReaderForFile(import, modulePath, codeBaseDirectory, out var reader);
			ThrowExceptionForHR(hr);

			return reader;
		}

		#region TryGetDocumentInfo
		[CanBeNull]
		private static ISymUnmanagedDocument TryGetDocumentInfo(
			[NotNull] Target target,
			[NotNull] ISymUnmanagedReader reader,
			[NotNull] IMessageLogger messageLogger)
		{
			var implMethod = ResolveBestMethodInfo(target.Method);

			var methodSymbols = reader.GetMethod(implMethod.MetadataToken);
			var documents = methodSymbols.GetDocumentsForMethod();

			if (documents.Length == 0)
			{
				messageLogger.WriteSetupErrorMessage(
					target,
					"No code found for the method.");
				return null;
			}
			if (documents.Length > 1)
			{
				messageLogger.WriteSetupErrorMessage(
					target,
					"Method code spans multiple documents, this is not supported for now.");
				return null;
			}

			return documents[0];
		}

		// BASEDON: https://github.com/aspnet/Testing/issues/138
		// BASEDON: https://github.com/aspnet/dnx/blob/bebc991012fe633ecac69675b2e892f568b927a5/src/Microsoft.Dnx.TestHost/TestAdapter/SourceInformationProvider.cs#L88-L104
		[NotNull]
		private static MethodBase ResolveBestMethodInfo([NotNull] MethodBase methodBase)
		{
			Code.NotNull(methodBase, nameof(methodBase));

			if (!(methodBase is MethodInfo method))
				return methodBase;

			// If a method has a StateMachineAttribute, then all of the user code will show up
			// in the symbols associated with the compiler-generated code. So, we need to look
			// for the 'MoveNext' on the generated type and resolve symbols for that.
			var attribute = method.GetCustomAttribute<StateMachineAttribute>();
			if (attribute?.StateMachineType == null)
				return method;

			var stateMachineMethod = attribute.StateMachineType.GetMethod(
				nameof(IEnumerator.MoveNext),
				BindingFlags.Instance | BindingFlags.NonPublic);
			Code.BugIf(stateMachineMethod == null, "stateMachineMethod == null");
			return stateMachineMethod;
		}
		#endregion

		#region Method map
		/// <summary>Returns range that stores start/end lines for each method in the document.</summary>
		/// <param name="documentInfo">Pdb document.</param>
		/// <param name="reader">Pdb reader.</param>
		/// <param name="assembly">Assembly that contains methods.</param>
		/// <returns>Range that stores start/end lines for each method in the document.</returns>
		private static CompositeRange<int, MethodBase> GetMethodLinesMap(
			[NotNull] ISymUnmanagedDocument documentInfo,
			[NotNull] ISymUnmanagedReader reader,
			[NotNull] Assembly assembly)
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
				 select module.ResolveMethod(m.GetToken())
					into method
				 orderby method.IsCompilerGenerated() ? 0 : 1 // non-generated wins
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

		private static bool IsCompilerGenerated(this MethodBase method)
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
					// DONTTOUCH: DO NOT change to .Add(). Mapping may be overwritten; last method wins.
					methodLineMapping[i] = method;
				}
			}
		}
		#endregion

		#endregion
	}
}