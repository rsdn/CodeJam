using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	[SuppressMessage("ReSharper", "InvocationIsSkipped")]
	internal static partial class SourceAnnotationsHelper
	{
		/// <summary>
		/// BASEDON:
		///  http://referencesource.microsoft.com/#System.Management/Instrumentation/MetaDataInfo.cs,45
		///  https://github.com/dotnet/symreader/tree/master/src/Microsoft.DiaSymReader/Shared
		///  http://stackoverflow.com/questions/40616611/nuspec-non-transitive-package-dependencies
		/// </summary>
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		private static partial class SymbolHelpers
		{
			#region CoClasses
			/// <summary>
			/// CoClass for getting an ISymUnmanagedBinder
			/// </summary>
			[ComImport, Guid("0A29FF9E-7F9C-4437-8B11-F424491E3931")]
			private class CorSymBinder { }

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
			#endregion

			private static class InteropUtilities
			{
				#region Core
				private const int E_FAIL = unchecked((int)0x80004005);
				private const int E_NOTIMPL = unchecked((int)0x80004001);

				private static readonly IntPtr s_ignoreIErrorInfo = new IntPtr(-1);

				private static T[] NullToEmpty<T>(T[] items) => items ?? Array.Empty<T>();

				public static void ThrowExceptionForHR(int hr)
				{
					// E_FAIL indicates "no info".
					// E_NOTIMPL indicates a lack of ISymUnmanagedReader support (in a particular implementation).
					if (hr < 0 && hr != E_FAIL && hr != E_NOTIMPL)
					{
						Marshal.ThrowExceptionForHR(hr, s_ignoreIErrorInfo);
					}
				}

				// PERF: The purpose of all this code duplication is to avoid allocating any display class instances.
				// Effectively, we will use the stack frames themselves as display classes.
				// ReSharper disable TypeParameterCanBeVariant
				private delegate int CountGetter<TEntity>(TEntity entity, out int count);

				private delegate int ItemsGetter<TEntity, TItem>(TEntity entity, int bufferLength, out int count, TItem[] buffer);

				// ReSharper restore TypeParameterCanBeVariant

				private static string BufferToString(char[] buffer)
				{
					DebugCode.BugIf(buffer[buffer.Length - 1] != 0, "buffer[buffer.Length - 1] != 0");
					return new string(buffer, 0, buffer.Length - 1);
				}

				private static void ValidateItems(int actualCount, int bufferLength)
				{
					if (actualCount != bufferLength)
					{
						throw new InvalidOperationException($"Read only {actualCount} of {bufferLength} items.");
					}
				}

				private static TItem[] GetItems<TEntity, TItem>(
					TEntity entity,
					CountGetter<TEntity> countGetter,
					ItemsGetter<TEntity, TItem> itemsGetter)
				{
					int count;
					ThrowExceptionForHR(countGetter(entity, out count));
					if (count == 0)
					{
						return null;
					}

					var result = new TItem[count];
					ThrowExceptionForHR(itemsGetter(entity, count, out count, result));
					ValidateItems(count, result.Length);
					return result;
				}

				private static TItem[] GetItems<TEntity, TItem>(TEntity entity, ItemsGetter<TEntity, TItem> getter)
				{
					int count;
					ThrowExceptionForHR(getter(entity, 0, out count, null));
					if (count == 0)
					{
						return null;
					}

					var result = new TItem[count];
					ThrowExceptionForHR(getter(entity, count, out count, result));
					ValidateItems(count, result.Length);
					return result;
				}
				#endregion

				#region Method
				public static ISymUnmanagedDocument[] GetDocumentsForMethod(ISymUnmanagedMethod method)
				{
					if (method == null)
					{
						throw new ArgumentNullException(nameof(method));
					}

					return NullToEmpty(
						GetItems(
							(ISymEncUnmanagedMethod)method,
							(ISymEncUnmanagedMethod a, out int b) => a.GetDocumentsForMethodCount(out b),
							(ISymEncUnmanagedMethod a, int b, out int c, ISymUnmanagedDocument[] d) => a.GetDocumentsForMethod(b, out c, d)));
				}

				public static void GetSourceExtentInDocument(
					ISymUnmanagedMethod method, ISymUnmanagedDocument document,
					out int startLine, out int endLine)
				{
					if (method == null)
					{
						throw new ArgumentNullException(nameof(method));
					}

					ThrowExceptionForHR(
						((ISymEncUnmanagedMethod)method).GetSourceExtentInDocument(document, out startLine, out endLine));
				}
				#endregion

				#region Documents
				public static string GetName(ISymUnmanagedDocument document)
				{
					if (document == null)
					{
						throw new ArgumentNullException(nameof(document));
					}

					return BufferToString(
						GetItems(
							document,
							(ISymUnmanagedDocument a, int b, out int c, char[] d) => a.GetUrl(b, out c, d)));
				}

				public static byte[] GetChecksum(ISymUnmanagedDocument document)
				{
					if (document == null)
					{
						throw new ArgumentNullException(nameof(document));
					}

					return NullToEmpty(
						GetItems(
							document,
							(ISymUnmanagedDocument a, int b, out int c, byte[] d) => a.GetChecksum(b, out c, d)));
				}

				public static Guid GetHashAlgorithm(ISymUnmanagedDocument document)
				{
					if (document == null)
					{
						throw new ArgumentNullException(nameof(document));
					}

					var result = default(Guid);
					ThrowExceptionForHR(document.GetChecksumAlgorithmId(ref result));
					return result;
				}
				#endregion
			}

			#region COM interfaces
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

			[ComImport]
			[ComVisible(false)]
			[Guid("40DE4037-7C81-3E1E-B022-AE1ABFF2CA08")]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			public interface ISymUnmanagedDocument
			{
				[PreserveSig]
				int GetUrl(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] url);

				[PreserveSig]
				int GetDocumentType(ref Guid documentType);

				[PreserveSig]
				int GetLanguage(ref Guid language);

				[PreserveSig]
				int GetLanguageVendor(ref Guid vendor);

				[PreserveSig]
				int GetChecksumAlgorithmId(ref Guid algorithm);

				[PreserveSig]
				int GetChecksum(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] checksum);

				[PreserveSig]
				int FindClosestLine(int line, out int closestLine);

				[PreserveSig]
				int HasEmbeddedSource([MarshalAs(UnmanagedType.Bool)] out bool value);

				[PreserveSig]
				int GetSourceLength(out int length);

				[PreserveSig]
				int GetSourceRange(
					int startLine,
					int startColumn,
					int endLine,
					int endColumn,
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] source);
			}

			[ComImport]
			[Guid("B62B923C-B500-3158-A543-24F307A8B7E1")]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			[ComVisible(false)]
			public interface ISymUnmanagedMethod
			{
				[PreserveSig]
				int GetToken(out int methodToken);

				[PreserveSig]
				int GetSequencePointCount(out int count);

				[PreserveSig]
				int GetRootScope([MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedScope scope);

				[PreserveSig]
				int GetScopeFromOffset(int offset, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedScope scope);

				/// <summary>
				/// Gets the IL offset within the method that corresponds to the specified position.
				/// </summary>
				/// <param name="document">The document for which the offset is requested. </param>
				/// <param name="line">The document line corresponding to the offset. </param>
				/// <param name="column">The document column corresponding to the offset. </param>
				/// <param name="offset">The offset within the specified document.</param>
				/// <returns>HResult.</returns>
				[PreserveSig]
				int GetOffset(ISymUnmanagedDocument document, int line, int column, out int offset);

				/// <summary>
				/// Gets an array of start and end offset pairs that correspond to the ranges of IL that a given position covers within this method.
				/// </summary>
				[PreserveSig]
				int GetRanges(
					ISymUnmanagedDocument document,
					int line,
					int column,
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] int[] ranges);

				/// <summary>
				/// Gets method parameters.
				/// </summary>
				[PreserveSig]
				int GetParameters(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] parameters);

				[PreserveSig]
				int GetNamespace([MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedNamespace @namespace);

				/// <summary>
				/// Gets the start and end positions for the source of the current method.
				/// </summary>
				/// <param name="documents">The starting and ending source documents.</param>
				/// <param name="lines">The starting and ending lines in the corresponding source documents. </param>
				/// <param name="columns">The starting and ending columns in the corresponding source documents. </param>
				/// <param name="defined">true if the positions were defined; otherwise, false.</param>
				/// <returns>HResult</returns>
				[PreserveSig]
				int GetSourceStartEnd(
					ISymUnmanagedDocument[] documents,
					[In, Out, MarshalAs(UnmanagedType.LPArray)] int[] lines,
					[In, Out, MarshalAs(UnmanagedType.LPArray)] int[] columns,
					[MarshalAs(UnmanagedType.Bool)] out bool defined);

				[PreserveSig]
				int GetSequencePoints(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] offsets,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedDocument[] documents,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] startLines,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] startColumns,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] endLines,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] endColumns);
			}

			[ComImport]
			[Guid("85E891DA-A631-4c76-ACA2-A44A39C46B8C")]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			[ComVisible(false)]
			public interface ISymEncUnmanagedMethod
			{
				/// <summary>
				/// Get the file name for the line associated with offset dwOffset.
				/// </summary>
				[PreserveSig]
				int GetFileNameFromOffset(
					int offset,
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] name);

				/// <summary>
				/// Get the Line information associated with <paramref name="offset"/>.
				/// </summary>
				/// <remarks>
				/// If <paramref name="offset"/> is not a sequence point it is associated with the previous one.
				/// <paramref name="sequencePointOffset"/> provides the associated sequence point.
				/// </remarks>
				[PreserveSig]
				int GetLineFromOffset(
					int offset,
					out int startLine,
					out int startColumn,
					out int endLine,
					out int endColumn,
					out int sequencePointOffset);

				/// <summary>
				/// Get the number of Documents that this method has lines in.
				/// </summary>
				[PreserveSig]
				int GetDocumentsForMethodCount(out int count);

				/// <summary>
				/// Get the documents this method has lines in.
				/// </summary>
				[PreserveSig]
				int GetDocumentsForMethod(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedDocument[] documents);

				/// <summary>
				/// Get the smallest start line and largest end line, for the method, in a specific document.
				/// </summary>
				[PreserveSig]
				int GetSourceExtentInDocument(ISymUnmanagedDocument document, out int startLine, out int endLine);
			}

			[ComImport]
			[Guid("68005D0F-B8E0-3B01-84D5-A11A94154942")]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			[ComVisible(false)]
			public interface ISymUnmanagedScope
			{
				[PreserveSig]
				int GetMethod([MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedMethod method);

				[PreserveSig]
				int GetParent([MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedScope scope);

				[PreserveSig]
				int GetChildren(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedScope[] children);

				[PreserveSig]
				int GetStartOffset(out int offset);

				[PreserveSig]
				int GetEndOffset(out int offset);

				[PreserveSig]
				int GetLocalCount(out int count);

				[PreserveSig]
				int GetLocals(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] locals);

				[PreserveSig]
				int GetNamespaces(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedNamespace[] namespaces);
			}

			[ComImport]
			[Guid("9F60EEBE-2D9A-3F7C-BF58-80BC991C60BB")]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			[ComVisible(false)]
			public interface ISymUnmanagedVariable
			{
				[PreserveSig]
				int GetName(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] name);

				[PreserveSig]
				int GetAttributes(out int attributes);

				[PreserveSig]
				int GetSignature(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] signature);

				[PreserveSig]
				int GetAddressKind(out int kind);

				[PreserveSig]
				int GetAddressField1(out int value);

				[PreserveSig]
				int GetAddressField2(out int value);

				[PreserveSig]
				int GetAddressField3(out int value);

				[PreserveSig]
				int GetStartOffset(out int offset);

				[PreserveSig]
				int GetEndOffset(out int offset);
			}

			[ComImport]
			[Guid("0DFF7289-54F8-11d3-BD28-0000F80849BD")]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			[ComVisible(false)]
			public interface ISymUnmanagedNamespace
			{
				[PreserveSig]
				int GetName(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] name);

				[PreserveSig]
				int GetNamespaces(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedNamespace[] namespaces);

				[PreserveSig]
				int GetVariables(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] variables);
			}

			[ComImport]
			[Guid("AA544D42-28CB-11d3-BD22-0000F80849BD")]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			[ComVisible(false)]
			public interface ISymUnmanagedBinder
			{
				/// <summary>
				/// Given a metadata interface and a file name, returns a new instance of <see cref="ISymUnmanagedReader"/>
				/// that will read the debugging symbols associated with the specified PE file.
				/// </summary>
				/// <param name="metadataImporter">An instance of IMetadataImport providing metadata for the specified PE file.</param>
				/// <param name="fileName">PE file path.</param>
				/// <param name="searchPath">Alternate path to search for debug data.</param>
				/// <param name="reader">The new reader instance.</param>
				[PreserveSig]
				int GetReaderForFile(
					[MarshalAs(UnmanagedType.Interface)] object metadataImporter,
					[MarshalAs(UnmanagedType.LPWStr)] string fileName,
					[MarshalAs(UnmanagedType.LPWStr)] string searchPath,
					[MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedReader reader);

				/// <summary>
				/// Given a metadata interface and a stream that contains
				/// the symbol store, returns the <see cref="ISymUnmanagedReader"/>
				/// that will read the debugging symbols from the given
				/// symbol store.
				/// </summary>
				/// <param name="metadataImporter">An instance of IMetadataImport providing metadata for the corresponding PE file.</param>
				/// <param name="stream">PDB stream.</param>
				/// <param name="reader">The new reader instance.</param>
				[PreserveSig]
				int GetReaderFromStream(
					[MarshalAs(UnmanagedType.Interface)] object metadataImporter,
					[MarshalAs(UnmanagedType.Interface)] object stream,
					[MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedReader reader);
			}

			[ComImport]
			[Guid("B4CE6286-2A6B-3712-A3B7-1EE1DAD467B5")]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			[ComVisible(false)]
			public interface ISymUnmanagedReader
			{
				[PreserveSig]
				int GetDocument(
					[MarshalAs(UnmanagedType.LPWStr)] string url,
					Guid language,
					Guid languageVendor,
					Guid documentType,
					[MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedDocument document);

				[PreserveSig]
				int GetDocuments(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedDocument[] documents);

				[PreserveSig]
				int GetUserEntryPoint(out int methodToken);

				[PreserveSig]
				int GetMethod(int methodToken, [MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedMethod method);

				[PreserveSig]
				int GetMethodByVersion(
					int methodToken,
					int version,
					[MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedMethod method);

				[PreserveSig]
				int GetVariables(
					int methodToken,
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ISymUnmanagedVariable[] variables);

				[PreserveSig]
				int GetGlobalVariables(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedVariable[] variables);

				[PreserveSig]
				int GetMethodFromDocumentPosition(
					ISymUnmanagedDocument document,
					int line,
					int column,
					[MarshalAs(UnmanagedType.Interface)] out ISymUnmanagedMethod method);

				[PreserveSig]
				int GetSymAttribute(
					int methodToken,
					[MarshalAs(UnmanagedType.LPWStr)] string name,
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] customDebugInformation);

				[PreserveSig]
				int GetNamespaces(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ISymUnmanagedNamespace[] namespaces);

				[PreserveSig]
				int Initialize(
					[MarshalAs(UnmanagedType.Interface)] object metadataImporter,
					[MarshalAs(UnmanagedType.LPWStr)] string fileName,
					[MarshalAs(UnmanagedType.LPWStr)] string searchPath,
					IStream stream);

				[PreserveSig]
				int UpdateSymbolStore([MarshalAs(UnmanagedType.LPWStr)] string fileName, IStream stream);

				[PreserveSig]
				int ReplaceSymbolStore([MarshalAs(UnmanagedType.LPWStr)] string fileName, IStream stream);

				[PreserveSig]
				int GetSymbolStoreFileName(
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] name);

				[PreserveSig]
				int GetMethodsFromDocumentPosition(
					ISymUnmanagedDocument document,
					int line,
					int column,
					int bufferLength,
					out int count,
					[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] ISymUnmanagedMethod[] methods);

				[PreserveSig]
				int GetDocumentVersion(
					ISymUnmanagedDocument document, out int version, [MarshalAs(UnmanagedType.Bool)] out bool isCurrent);

				[PreserveSig]
				int GetMethodVersion(ISymUnmanagedMethod method, out int version);
			}
			#endregion
		}
	}
}