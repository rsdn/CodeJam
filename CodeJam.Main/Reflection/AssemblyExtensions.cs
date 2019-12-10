using System;
using System.IO;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	/// The <see cref="Assembly"/> extensions.
	/// </summary>
	[PublicAPI]
	public static class AssemblyExtensions
	{
#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20 // PUBLIC_API_CHANGES
		// IsJITOptimizerDisabled is missing if targeting to these frameworks
#else
		/// <summary>
		/// Checks that the assembly is build with <see cref="System.Diagnostics.DebuggableAttribute.IsJITOptimizerDisabled"/>
		/// set to <c>false</c>.
		/// </summary>
		/// <param name="assembly">The assembly to check.</param>
		/// <returns><c>true</c> if the assembly was build with optimizations disabled.</returns>
		[Pure]
		public static bool IsDebugAssembly([NotNull] this Assembly assembly)
		{
			Code.NotNull(assembly, nameof(assembly));
			return assembly.GetCustomAttribute<System.Diagnostics.DebuggableAttribute>()?.IsJITOptimizerDisabled ?? false;
		}

#endif

		/// <summary>
		/// Loads the specified manifest resource from this assembly, and checks if it exists.
		/// </summary>
		/// <param name="assembly">Resource assembly.</param>
		/// <param name="name">The case-sensitive name of the manifest resource being requested.</param>
		/// <returns>The manifest resource.</returns>
		/// <exception cref="ArgumentNullException">The name parameter is null.</exception>
		/// <exception cref="ArgumentException">Resource with specified name not found</exception>
		[NotNull, Pure]
		public static Stream GetRequiredResourceStream([NotNull] this Assembly assembly, [NotNull] string name)
		{
			Code.NotNull(assembly, nameof(assembly));
			Code.NotNullNorWhiteSpace(name, nameof(name));

			var result = assembly.GetManifestResourceStream(name);
			if (result == null)
				throw new ArgumentException($"Resource '{name}' not found in assembly '{assembly}'.");

			return result;
		}

#if LESSTHAN_NET20 || LESSTHAN_NETSTANDARD15 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES
		// CodeBase is missing if targeting to these frameworks
#else
		/// <summary>
		/// Returns path to the <paramref name="assembly"/> file.
		/// </summary>
		/// <param name="assembly">Assembly.</param>
		/// <returns>Path to <paramref name="assembly"/>.</returns>
		[NotNull]
		[Pure]
		public static string GetAssemblyPath([NotNull] this Assembly assembly)
		{
			Code.NotNull(assembly, nameof(assembly));

			// DONTTOUCH: we cannot use Location to detect the path as it may refer to shadow-copied assembly.
			// DONTTOUCH: at the same time we cannot check CodeBase as it may refer to the loader codebase:
			//   If the assembly was loaded as a byte array, using an overload of the Load method that takes an array of bytes,
			//   this property returns the location of the caller of the method, not the location of the loaded assembly.
			// (c) https://msdn.microsoft.com/en-us/library/system.reflection.assembly.codebase(v=vs.110).aspx
			if (string.IsNullOrEmpty(assembly.Location))
				throw CodeExceptions.Argument(nameof(assembly), $"Assembly {assembly} has no physical code base.");

			var uri = new Uri(assembly.CodeBase);
			if (uri.IsFile)
				return uri.LocalPath;

			throw CodeExceptions.Argument(nameof(assembly), $"Assembly '{assembly}' has no local path.");
		}

		/// <summary>
		/// Returns directory part of path to assembly <paramref name="assembly"/> file.
		/// </summary>
		/// <param name="assembly">Assembly.</param>
		/// <returns>Folder part of path to <paramref name="assembly"/>.</returns>
		[CanBeNull]
		[Pure]
		public static string GetAssemblyDirectory([NotNull] this Assembly assembly) =>
			Path.GetDirectoryName(GetAssemblyPath(assembly));
#endif
	}
}