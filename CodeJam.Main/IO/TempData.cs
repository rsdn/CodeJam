using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Threading;

using JetBrains.Annotations;

namespace CodeJam.IO
{
	/// <summary>Methods to work with temporary data.</summary>
	[PublicAPI]
	public static class TempData
	{
		#region Temp file|directory holders
		/// <summary>
		/// Base class for temp file|directory objects.
		/// Contains logic to proof the removal will be performed even on resource leak.
		/// </summary>
		[PublicAPI]
		public abstract class TempBase :
#if !LESSTHAN_NETSTANDARD20 && !LESSTHAN_NETCOREAPP20
			CriticalFinalizerObject,
#endif
			IDisposable
		{
			/// <summary>Checks that the path is valid.</summary>
			/// <param name="path">The path.</param>
			[Conditional(DebugCode.DebugCondition)]
			protected static void DebugAssertValidPath([NotNull] string path) =>
				// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
				System.IO.Path.GetFullPath(path);

			private volatile string _path;
			private volatile bool _keepOnDispose;

			/// <summary>Assertion on object dispose</summary>
			protected void AssertNotDisposed() => Code.DisposedIfNull(_path, this);

			/// <summary>Initializes a new instance of the <see cref="TempBase"/> class.</summary>
			/// <param name="path">The path.</param>
			protected TempBase([NotNull] string path)
			{
				Code.NotNullNorEmpty(path, nameof(path));
				DebugAssertValidPath(path);

				_path = path;
			}

			/// <summary>Temp path.</summary>
			/// <value>The path.</value>
			[NotNull]
			public string Path
			{
				get
				{
					AssertNotDisposed();
					return _path;
				}
			}

			/// <summary>Suppresses item deletion on dispose.</summary>
			public void SuppressDelete() => _keepOnDispose = true;

			/// <summary>Finalize instance</summary>
			~TempBase()
			{
				Dispose(false);
			}

			/// <summary>Delete the temp file|directory</summary>
			public void Dispose()
			{
				if (_path != null) // Fast check
				{
					Dispose(true);

					// it's safe to call SuppressFinalize multiple times so it's ok if check above will be inaccurate.
					GC.SuppressFinalize(this);
				}
			}

			/// <summary>Dispose pattern implementation - overridable part</summary>
			/// <param name="disposing">
			/// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
			/// </param>
			protected void Dispose(bool disposing)
			{
#pragma warning disable 420 // Interlocked is safe to call on volatile fields.
				var path = Interlocked.Exchange(ref _path, null);
#pragma warning restore 420
				if (path == null || _keepOnDispose)
					return;

				DisposePath(path, disposing);
			}

			/// <summary>Temp path disposal</summary>
			/// <param name="path">The path.</param>
			/// <param name="disposing">
			/// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
			/// </param>
			protected abstract void DisposePath(string path, bool disposing);
		}

		/// <summary>Wraps reference on a temp directory meant to be deleted on dispose</summary>
		[PublicAPI]
		public sealed class TempDirectory : TempBase
		{
			private DirectoryInfo _info;

			/// <summary>Create an instance using an automatically constructed temp directory path.</summary>
			internal TempDirectory() : base(System.IO.Path.Combine(System.IO.Path.GetTempPath(), GetTempName())) { }

			/// <summary>Initializes a new instance of the <see cref="TempDirectory"/> class.</summary>
			/// <param name="path">The path.</param>
			internal TempDirectory([NotNull] string path) : base(path) { }

			/// <summary>DirectoryInfo object</summary>
			/// <value>The DirectoryInfo object.</value>
			[NotNull]
			public DirectoryInfo Info
			{
				get
				{
					AssertNotDisposed();
					return _info ??= new DirectoryInfo(Path);
				}
			}

			/// <summary>Temp path disposal</summary>
			/// <param name="path">The path.</param>
			/// <param name="disposing">
			/// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
			/// </param>
			protected override void DisposePath([NotNull] string path, bool disposing)
			{
				_info = null;
				Exception caught = null;
				try
				{
					Directory.Delete(path, true);
				}
				catch (ArgumentException ex)
				{
					caught = ex;
				}
				catch (IOException ex)
				{
					caught = ex;
				}
				catch (UnauthorizedAccessException ex)
				{
					caught = ex;
				}
				catch (NotSupportedException ex)
				{
					caught = ex;
				}

				if (disposing)
				{
					caught?.LogToCodeTraceSourceCaught();
				}
			}
		}

		/// <summary>Wraps reference on a temp file meant to be deleted on dispose</summary>
		/// <seealso cref="CodeJam.IO.TempData.TempBase"/>
		[PublicAPI]
		public sealed class TempFile : TempBase
		{
			private FileInfo _info;

			/// <summary>Create an instance using an automatically constructed temp file path.</summary>
			internal TempFile() : base(System.IO.Path.Combine(System.IO.Path.GetTempPath(), GetTempName())) { }

			/// <summary>Initialize instance.</summary>
			/// <param name="path">The path.</param>
			internal TempFile([NotNull] string path) : base(path) { }

			/// <summary>FileInfo object</summary>
			/// <value>The FileInfo object.</value>
			[NotNull]
			public FileInfo Info
			{
				get
				{
					AssertNotDisposed();
					return _info ??= new FileInfo(Path);
				}
			}

			/// <summary>Temp path disposal</summary>
			/// <param name="path">The path.</param>
			/// <param name="disposing">
			/// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
			/// </param>
			protected override void DisposePath([NotNull] string path, bool disposing)
			{
				_info = null;
				Exception caught = null;
				try
				{
					File.Delete(path);
				}
				catch (ArgumentException ex)
				{
					caught = ex;
				}
				catch (IOException ex)
				{
					caught = ex;
				}
				catch (UnauthorizedAccessException ex)
				{
					caught = ex;
				}
				catch (NotSupportedException ex)
				{
					caught = ex;
				}

				if (disposing)
				{
					caught?.LogToCodeTraceSourceCaught();
				}
			}
		}
		#endregion

		#region Factory methods
		/// <summary>Returns a random name for a temp file or directory.</summary>
		/// <returns>A random name</returns>
		/// <remarks>The resulting name is a local name (does not include a base path)</remarks>
		[NotNull] public static string GetTempName() => GetTempName(null);

		/// <summary>Returns a random name for a temp file or directory.</summary>
		/// <param name="extension">The extension for thew filename.</param>
		/// <returns>A random name</returns>
		/// <remarks>The resulting name is a local name (does not include a base path)</remarks>
		[NotNull] public static string GetTempName([CanBeNull] string extension) => Guid.NewGuid() + (extension ?? ".tmp");

		/// <summary>Creates temp directory and returns <see cref="IDisposable"/> to free it.</summary>
		/// <returns>Temp directory to be freed on dispose.</returns>
		[NotNull] public static TempDirectory CreateDirectory() => CreateDirectory(null, null);

		/// <summary>Creates temp directory and returns <see cref="IDisposable"/> to free it.</summary>
		/// <param name="dirPath">The dir path.</param>
		/// <returns>Temp directory to be freed on dispose.</returns>
		[NotNull] public static TempDirectory CreateDirectory([CanBeNull] string dirPath) => CreateDirectory(dirPath, null);

		/// <summary>Creates temp directory and returns <see cref="IDisposable"/> to free it.</summary>
		/// <param name="dirPath">The dir path.</param>
		/// <param name="directoryName">Name of the temp directory.</param>
		/// <returns>Temp directory to be freed on dispose.</returns>
		[NotNull]
		public static TempDirectory CreateDirectory([CanBeNull] string dirPath, [CanBeNull] string directoryName)
		{
			if (dirPath == null)
				dirPath = Path.GetTempPath();

			if (directoryName == null)
				directoryName = GetTempName();

			var directoryPath = Path.Combine(dirPath, directoryName);
			var result = new TempDirectory(directoryPath);
			Directory.CreateDirectory(directoryPath);
			return result;
		}

		/// <summary>Creates temp file and return disposable handle.</summary>
		/// <returns>Temp file to be freed on dispose.</returns>
		public static TempFile CreateFile() => CreateFile(null, null);

		/// <summary>Creates temp file and return disposable handle.</summary>
		/// <param name="dirPath">The dir path.</param>
		/// <returns>Temp file to be freed on dispose.</returns>
		[NotNull] public static TempFile CreateFile([CanBeNull] string dirPath) => CreateFile(dirPath, null);

		/// <summary>Creates temp file and return disposable handle.</summary>
		/// <param name="dirPath">The dir path.</param>
		/// <param name="fileName">Name of the temp file.</param>
		/// <returns>Temp file to be freed on dispose.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="dirPath"/> is null.</exception>
		[NotNull]
		public static TempFile CreateFile([CanBeNull] string dirPath, [CanBeNull] string fileName)
		{
			if (dirPath == null)
				dirPath = Path.GetTempPath();

			if (fileName == null)
				fileName = GetTempName();

			var filePath = Path.Combine(dirPath, fileName);
			var result = new TempFile(filePath);

			// DONTTOUCH: File.Create() requires existing directory and the filePath may be a relative path.
			Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? dirPath);
			using (File.Create(filePath)) { }
			return result;
		}

		/// <summary>Creates stream and returns disposable handler.</summary>
		/// <returns>Temp stream to be freed on dispose.</returns>
		[NotNull]
		public static FileStream CreateFileStream() =>
			CreateFileStream(null, null, FileAccess.ReadWrite);

		/// <summary>Creates stream and returns disposable handler.</summary>
		/// <param name="fileAccess">The file access.</param>
		/// <returns>Temp stream to be freed on dispose.</returns>
		[NotNull]
		public static FileStream CreateFileStream(FileAccess fileAccess) =>
			CreateFileStream(null, null, fileAccess);

		/// <summary> Creates stream and returns disposable handler.</summary>
		/// <param name="dirPath">The dir path.</param>
		/// <param name="fileName">Name of the temp file.</param>
		/// <param name="fileAccess">The file access.</param>
		/// <returns>Temp stream to be freed on dispose.</returns>
		[NotNull]
		public static FileStream CreateFileStream(
			[CanBeNull] string dirPath,
			[CanBeNull] string fileName = null,
			FileAccess fileAccess = FileAccess.ReadWrite)
		{
			const int bufferSize = 4096;

			if (dirPath == null)
				dirPath = Path.GetTempPath();

			if (fileName == null)
				fileName = GetTempName();

			var filePath = Path.Combine(dirPath, fileName);

			// DONTTOUCH: new FileStream() requires existing directory and the filePath may be a relative path.
			Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? dirPath);
			return new FileStream(
				filePath, FileMode.CreateNew,
				fileAccess, FileShare.Read, bufferSize,
				FileOptions.DeleteOnClose);
		}
		#endregion
	}
}