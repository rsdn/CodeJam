using System;
using System.Diagnostics;
using System.IO;

using CodeJam.Internal;

using JetBrains.Annotations;

using static CodeJam.Internal.CodeExceptionsHelper;

namespace CodeJam.IO
{
	/// <summary>IO exception factory class</summary>
	[PublicAPI]
	public static class IoCodeExceptions
	{
		#region Path
		/// <summary>Creates <see cref="ArgumentException"/> for invalid path.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="path">The path being checked.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static ArgumentException ArgumentNotWellFormedPath(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string path)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"The path '{path}' is not a valid absolute or not-rooted relative path.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for invalid full path.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="path">The path being checked.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static ArgumentException ArgumentNotWellFormedAbsolutePath(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string path)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"The path '{path}' is not a valid full path.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for invalid relative path.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="path">The path being checked.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static ArgumentException ArgumentRootedOrNotRelativePath(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string path)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"The path '{path}' is not a valid not-rooted relative path.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> for invalid file name.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="path">The path being checked.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static ArgumentException ArgumentNotFileName(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string path)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"The path '{path}' is not a valid name of the file or a directory.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="ArgumentException"/> if path does not ends with one of path separator chars.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="path">The path being checked.</param>
		/// <returns>Initialized instance of <see cref="ArgumentException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		public static ArgumentException ArgumentNotVolumeOrDirectoryPath(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string path)
		{
			BreakIfAttached();
			return new ArgumentException(
				$"The path '{path}' should end with volume or directory separator chars.",
				argumentName)
				.LogToCodeTraceSourceBeforeThrow();
		}
		#endregion

		#region File / Directory
		/// <summary>Creates <see cref="FileNotFoundException" /> for missing file when there is a dictionary.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="filePath">The file being checked.</param>
		/// <returns>Initialized instance of <see cref="FileNotFoundException" />.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static FileNotFoundException ArgumentDirectoryExistsFileExpected(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string filePath)
		{
			BreakIfAttached();
			return new FileNotFoundException(
				$"Argument {argumentName}. The path {filePath} is a directory, not a file.", filePath)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="DirectoryNotFoundException" /> for missing directory when there is a file.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="directoryPath">The directory being checked.</param>
		/// <returns>Initialized instance of <see cref="DirectoryNotFoundException" />.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static DirectoryNotFoundException ArgumentFileExistsDirectoryExpected(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string directoryPath)
		{
			BreakIfAttached();
			return new DirectoryNotFoundException(
				$"Argument {argumentName}. The path {directoryPath} is a file, not a directory.")
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="FileNotFoundException" /> for missing file.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="filePath">The file being checked.</param>
		/// <returns>Initialized instance of <see cref="FileNotFoundException" />.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static FileNotFoundException ArgumentFileNotFound(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string filePath)
		{
			BreakIfAttached();
			return new FileNotFoundException($"Argument {argumentName}. File {filePath} not found.", filePath)
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="DirectoryNotFoundException" /> for missing directory.</summary>
		/// <param name="argumentName">Name of the argument.</param>
		/// <param name="directoryPath">The directory being checked.</param>
		/// <returns>Initialized instance of <see cref="DirectoryNotFoundException" />.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static DirectoryNotFoundException ArgumentDirectoryNotFound(
			[NotNull, InvokerParameterName] string argumentName,
			[NotNull] string directoryPath)
		{
			BreakIfAttached();
			return new DirectoryNotFoundException($"Argument {argumentName}. Directory {directoryPath} not found.")
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="IOException"/>.</summary>
		/// <param name="messageFormat">The message format.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>Initialized instance of <see cref="IOException"/>.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		[StringFormatMethod("messageFormat")]
		// ReSharper disable once InconsistentNaming
		public static IOException IOException(
			[NotNull] string messageFormat,
			[CanBeNull] params object[] args)
		{
			BreakIfAttached();
			return new IOException(
				InvariantFormat(messageFormat, args))
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="IOException" /> for file that should not exist.</summary>
		/// <param name="filePath">The file being checked.</param>
		/// <returns>Initialized instance of <see cref="IOException" />.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static IOException FileExists([NotNull] string filePath)
		{
			BreakIfAttached();
			return new IOException($"File {filePath} exists while should not.")
				.LogToCodeTraceSourceBeforeThrow();
		}

		/// <summary>Creates <see cref="IOException" /> for directory that should not exist.</summary>
		/// <param name="directoryPath">The directory being checked.</param>
		/// <returns>Initialized instance of <see cref="IOException" />.</returns>
		[DebuggerHidden, NotNull, MustUseReturnValue]
		public static IOException DirectoryExists([NotNull] string directoryPath)
		{
			BreakIfAttached();
			return new IOException($"Directory {directoryPath} exists while should not.")
				.LogToCodeTraceSourceBeforeThrow();
		}
		#endregion
	}
}