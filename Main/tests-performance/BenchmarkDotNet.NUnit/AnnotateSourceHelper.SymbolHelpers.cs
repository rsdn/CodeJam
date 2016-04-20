using System;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.NUnit
{
	internal static partial class AnnotateSourceHelper
	{
		/// <summary>
		/// BASEDON:
		///  http://sorin.serbans.net/blog/2010/08/how-to-read-pdb-files/257/
		///  http://stackoverflow.com/questions/13911069/how-to-get-global-variables-definition-from-symbols-tables
		///  http://referencesource.microsoft.com/#System.Management/Instrumentation/MetaDataInfo.cs,45
		/// </summary>
		private static class SymbolHelpers
		{
			// ReSharper disable once SuggestBaseTypeForParameter
			[CanBeNull]
			public static ISymbolMethod TryGetSymbols(MethodBase target)
			{
				// ReSharper disable once PossibleNullReferenceException
				var assembly = target.DeclaringType.Assembly;
				var codeBase = new Uri(assembly.CodeBase).LocalPath;
				var codeBaseDirectory = Path.GetDirectoryName(codeBase);
				var symbolReader = TryGetSymbolReaderForFile(assembly.Location, codeBaseDirectory);

				return symbolReader?.GetMethod(new SymbolToken(target.MetadataToken));
			}

			[CanBeNull]
			private static ISymbolReader TryGetSymbolReaderForFile(string pathModule, string searchPath) =>
				TryGetSymbolReaderForFile(new SymBinder(), pathModule, searchPath);

			// ReSharper disable once SuggestBaseTypeForParameter
			[CanBeNull]
			private static ISymbolReader TryGetSymbolReaderForFile(
				SymBinder binder, string pathModule, string searchPath)
			{
				ISymbolReader reader;

				IMetaDataDispenser dispenser = null;
				IMetaDataImportStub importer = null;
				var importerPtr = IntPtr.Zero;
				try
				{
					// ReSharper disable once SuspiciousTypeConversion.Global
					dispenser = (IMetaDataDispenser)new CorMetaDataDispenser();
					var importerGuid = new Guid(InterfaceMetaDataImportGuid);
					importer = (IMetaDataImportStub)dispenser.OpenScope(pathModule, 0, ref importerGuid);

					// This will manually AddRef the underlying object, so we need to 
					// be very careful to Release it.
					importerPtr = Marshal.GetComInterfaceForObject(importer, typeof(IMetaDataImportStub));

					try
					{
						reader = binder.GetReader(importerPtr, pathModule, searchPath);
					}
					catch (COMException)
					{
						reader = null;
					}
				}
				finally
				{
					if (importerPtr != IntPtr.Zero)
						Marshal.Release(importerPtr);

					if (importer != null)
						Marshal.ReleaseComObject(importer);

					if (dispenser != null)
						Marshal.ReleaseComObject(dispenser);
				}

				return reader;
			}

			#region COM interop
			private const string InterfaceMetaDataImportGuid = "7DAC8207-D3AE-4c75-9B67-92801A497D44";
			private const string InterfaceMetaDataDispenserGuid = "809C652E-7396-11D2-9771-00A0C9B4D50C";
			private const string MetaDataDispenserGuid = "E5CB7A31-7512-11d2-89CE-0080C792E5D8";

			[ComImport]
			[Guid(InterfaceMetaDataImportGuid)]
			[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
			[TypeLibType(TypeLibTypeFlags.FRestricted)]
			private interface IMetaDataImportStub
			{
				// ...
			}

			/// <summary>
			/// CoClass for getting an IMetaDataDispenser
			/// </summary>
			[ComImport]
			[Guid(MetaDataDispenserGuid)]
			[TypeLibType(TypeLibTypeFlags.FCanCreate)]
			[ClassInterface(ClassInterfaceType.None)]
			private class CorMetaDataDispenser { }

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
			[Guid(InterfaceMetaDataDispenserGuid)]
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
			#endregion
		}
	}
}