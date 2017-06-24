using System;
using System.IO;
using System.Linq;

namespace CodeJam.IO
{
	/// <summary>
	/// Helpers for <see cref="Path"/>
	/// </summary>
	public static class PathHelpers
	{
		private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
		private static readonly char[] _separatorChars = new[]
		{
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar,
			Path.VolumeSeparatorChar
		};
		private static readonly char[] _directorySeparatorChars = new[]
		{
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar
		};
		#region Checks

		/// <summary>
		/// Determines whether the path is either absolute or relative not rooted path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path is either absolute or relative not rooted path, <c>false</c>.</returns>
		public static bool IsWellFormedPath(string path)
		{
			Code.NotNullNorEmpty(path, nameof(path));
			try
			{
				// DONTTOUCH: order is important as GetFullPath performs path validation
				return Path.GetFullPath(path) == path || !Path.IsPathRooted(path);
			}
			catch (ArgumentException)
			{
			}
			catch (NotSupportedException)
			{
			}
			return false;
		}

		/// <summary>
		/// Determines whether the path is well-formed absolute path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path is well-formed absolute path; otherwise, <c>false</c>.</returns>
		public static bool IsWellFormedAbsolutePath(string path)
		{
			Code.NotNullNorEmpty(path, nameof(path));
			try
			{
				return Path.GetFullPath(path) == path;
			}
			catch (ArgumentException)
			{
			}
			catch (NotSupportedException)
			{
			}
			return false;
		}

		/// <summary>
		/// Determines whether the path is not rooted well-formed relative path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path is not rooted well-formed relative path; otherwise, <c>false</c>.</returns>
		public static bool IsWellFormedRelativePath(string path)
		{
			Code.NotNullNorEmpty(path, nameof(path));
			try
			{
				if (Path.IsPathRooted(path))
					return false;
				// DONTTOUCH:GetFullPath performs path validation
				return Path.GetFullPath(path) != path;
			}
			catch (ArgumentException)
			{
			}
			catch (NotSupportedException)
			{
			}
			return false;
		}

		/// <summary>
		/// Checks if the path ends with directory or volume separator chars.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path ends with separator char; otherwise, <c>false</c>.</returns>
		public static bool IsWellFormedContainerPath(string path) =>
			IsContainerPath(path) && IsWellFormedPath(path);

		/// <summary>
		/// Checks if the path ends with directory or volume separator chars.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path ends with separator char; otherwise, <c>false</c>.</returns>
		public static bool IsWellFormedSimpleName(string path) =>
			IsSimpleName(path) && IsWellFormedRelativePath(path);

		/// <summary>
		/// Determines whether the path is a simple file or directory name.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path isa simple file or directory name; otherwise, <c>false</c>.</returns>
		public static bool IsSimpleName(string path)
		{
			Code.NotNullNorEmpty(path, nameof(path));

			return path.IndexOfAny(_invalidFileNameChars) < 0;
		}

		/// <summary>
		/// Checks if the path ends with directory or volume separator chars.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns><c>true</c> if the path ends with separator char; otherwise, <c>false</c>.</returns>
		public static bool IsContainerPath(string path)
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
		public static string EnsureContainerPath(string path) =>
			IsContainerPath(path)
				? path
				: path + Path.DirectorySeparatorChar;
		#endregion
	}
}