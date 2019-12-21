using System;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.IO
{
	/// <summary>
	/// Helpers for <see cref="Path"/>.
	/// </summary>
	[PublicAPI]
	public static class PathHelper
	{
		[NotNull]
		private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();

		[NotNull]
		private static readonly char[] _separatorChars =
		{
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar,
			Path.VolumeSeparatorChar
		};

		#region Checks
		/// <summary>
		/// Determines whether the path is either absolute or relative not rooted path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path is either absolute or relative not rooted path, <c>false</c>.</returns>
		[Pure]
		public static bool IsWellFormedPath([NotNull] string path)
		{
			Code.NotNullNorEmpty(path, nameof(path));
			try
			{
				// DONTTOUCH: order is important as GetFullPath performs path validation
				return Path.GetFullPath(path) == path || !Path.IsPathRooted(path);
			}
			catch (ArgumentException) { }
			catch (NotSupportedException) { }
			return false;
		}

		/// <summary>
		/// Determines whether the path is well-formed absolute path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path is well-formed absolute path; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsWellFormedAbsolutePath([NotNull] string path)
		{
			Code.NotNullNorEmpty(path, nameof(path));
			try
			{
				return Path.GetFullPath(path) == path;
			}
			catch (ArgumentException) { }
			catch (NotSupportedException) { }
			return false;
		}

		/// <summary>
		/// Determines whether the path is not rooted well-formed relative path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path is not rooted well-formed relative path; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsWellFormedRelativePath([NotNull] string path)
		{
			Code.NotNullNorEmpty(path, nameof(path));
			try
			{
				if (Path.IsPathRooted(path))
					return false;
				// DONTTOUCH:GetFullPath performs path validation
				return Path.GetFullPath(path) != path;
			}
			catch (ArgumentException) { }
			catch (NotSupportedException) { }
			return false;
		}

		/// <summary>
		/// Checks if the path ends with directory or volume separator chars.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path ends with separator char; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsWellFormedContainerPath([NotNull] string path) =>
			IsContainerPath(path) && IsWellFormedPath(path);

		/// <summary>
		/// Checks if the path ends with directory or volume separator chars.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path ends with separator char; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsWellFormedFileName([NotNull] string path) =>
			IsFileName(path) && IsWellFormedRelativePath(path);

		/// <summary>
		/// Determines whether the path is a file or directory name.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path is a file or directory name; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsFileName([NotNull] string path)
		{
			Code.NotNullNorEmpty(path, nameof(path));

			return path.IndexOfAny(_invalidFileNameChars) < 0;
		}

		/// <summary>
		/// Checks if the path ends with directory or volume separator chars.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path ends with separator char; otherwise, <c>false</c>.</returns>
		[Pure]
		public static bool IsContainerPath([NotNull] string path)
		{
			Code.NotNullNorEmpty(path, nameof(path));

			return _separatorChars.Contains(path[path.Length - 1]);
		}
		#endregion

		#region Manipulations
		/// <summary>
		/// Appends directory separator char if the path is not a volume or directory path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>Path that ends with one of path separator chars.</returns>
		[Pure]
		public static string EnsureContainerPath([NotNull] string path) =>
			IsContainerPath(path)
				? path
				: path + Path.DirectorySeparatorChar;
		#endregion
	}
}