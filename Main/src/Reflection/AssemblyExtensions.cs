using System;
using System.IO;
using System.Reflection;

using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.Reflection
{
	/// <summary>
	/// The <see cref="Assembly"/> extensions.
	/// </summary>
	[PublicAPI]
	public static class AssemblyExtensions
	{
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

		/// <summary>
		/// Returns path to assembly <paramref name="assembly"/> file.
		/// </summary>
		/// <param name="assembly">Assembly.</param>
		/// <returns>Path to <paramref name="assembly"/>.</returns>
		[NotNull]
		[Pure]
		public static string GetAssemblyPath([NotNull] this Assembly assembly)
		{
			Code.NotNull(assembly, nameof(assembly));

			var codeBase = assembly.CodeBase;
			if (codeBase == null)
				throw CodeExceptions.Argument(nameof(assembly), $"Assembly {assembly} has no physical code base.");

			var uri = new Uri(codeBase);
			if (uri.IsFile)
				return uri.LocalPath;

			throw CodeExceptions.Argument(nameof(assembly), $"Assembly '{assembly}' placed not on local disk.");
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
	}
}
