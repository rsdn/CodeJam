using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

using NUnit.Framework;

namespace CodeJam.IO
{
	[TestFixture(Category = "IO")]
	public class TempDataTests
	{
		#region Test helpers
		private static void AssertDisposed<T>(Func<T> memberCallback) =>
			Assert.Throws<ObjectDisposedException>(() => memberCallback());
		#endregion

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string CreateAndLeakTempDir(string s)
		{
			var dir2 = TempData.CreateDirectory();
			var dir2Path = dir2.Path;
			Assert.AreNotEqual(s, dir2Path, "Path should not match");
			Assert.IsNotNull(dir2.Info, "Info is null");
			Assert.IsTrue(dir2.Info.Exists, "Directory should exist");
			GC.KeepAlive(dir2);
			return dir2Path;
		}

		[Test]
		public void TestDirectory()
		{
			var tempPath = Path.GetTempPath();
			string dirPath;
			using (var dir = TempData.CreateDirectory())
			{
				dirPath = dir.Path;
				Assert.IsTrue(Directory.Exists(dirPath), "Directory should exist");
				Assert.That(dirPath, Does.StartWith(tempPath));

				var dir2 = TempData.CreateDirectory();
				Assert.AreNotEqual(dir.Path, dir2.Path, "Path should not match");
				Assert.IsTrue(dir2.Info.Exists, "Directory should exist");
				dir2.Dispose();
				AssertDisposed(() => dir2.Info);
			}
			Assert.IsFalse(Directory.Exists(dirPath), "Directory should NOT exist");

			// test for cleanup if leaked
			{
				var dir2Path = CreateAndLeakTempDir(dirPath);
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				Assert.IsFalse(Directory.Exists(dir2Path), "Directory should NOT exist");
			}

			// test for SuppressDelete()
			{
				var dir2 = TempData.CreateDirectory();
				var dir2Path = dir2.Path;
				try
				{
					dir2.SuppressDelete();
					Assert.AreNotEqual(dirPath, dir2Path, "Path should not match");
					Assert.IsNotNull(dir2.Info, "Info is null");
					Assert.IsTrue(dir2.Info.Exists, "Directory should exist");
					dir2.Dispose();
					Assert.IsTrue(Directory.Exists(dir2Path), "Directory should exist");
				}
				finally
				{
					Directory.Delete(dir2Path);
				}
			}
		}

		[Test]
		public void TestDirectoryNestedContent()
		{
			string dirPath;
			string nestedFile;
			string nestedDir;
			using (var dir = TempData.CreateDirectory())
			{
				dirPath = dir.Path;

				Assert.IsNotNull(dir.Info, "Info is null");
				nestedDir = dir.Info.CreateSubdirectory("test.dir").FullName;

				nestedFile = Path.Combine(dirPath, "test.tmp");
				File.WriteAllText(nestedFile, "O La La");
				var content = File.ReadAllText(nestedFile);
				Assert.AreEqual(content, "O La La");
			}
			Assert.IsFalse(Directory.Exists(dirPath), "Directory should NOT exist");
			Assert.IsFalse(Directory.Exists(nestedDir), "Directory should NOT exist");
			Assert.IsFalse(File.Exists(nestedFile), "File should NOT exist");
		}

		[Test]
		public void TestDirectorySpecificPath()
		{
			var tempPath = Path.GetTempPath();
			var dirName = TempData.GetTempName();
			var dirPath = Path.Combine(tempPath, dirName);

			using (var dir = TempData.CreateFile(tempPath, dirName))
			{
				Assert.AreEqual(dir.Path, dirPath);
				Assert.IsNotNull(dir.Info, "Info is null");
				Assert.IsTrue(dir.Info.Exists, "Directory should exist");
			}
			Assert.IsFalse(File.Exists(dirPath), "Directory should NOT exist");

			using (var dir = TempData.CreateFile(null, dirName))
			{
				Assert.AreEqual(dir.Path, dirPath);
				Assert.IsNotNull(dir.Info, "Info is null");
				Assert.IsTrue(dir.Info.Exists, "Directory should exist");
			}
			Assert.IsFalse(File.Exists(dirPath), "Directory should NOT exist");
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string CreateAndLeakTempFile(string filePath)
		{
			var file2 = TempData.CreateFile();
			var file2Path = file2.Path;
			Assert.AreNotEqual(filePath, file2Path, "Path should not match");
			Assert.IsNotNull(file2.Info, "Info is null");
			Assert.IsTrue(file2.Info.Exists, "File should exist");
			GC.KeepAlive(file2);

			return file2Path;
		}

		[Test]
		public void TestFile()
		{
			var tempPath = Path.GetTempPath();
			string filePath;
			using (var file = TempData.CreateFile())
			{
				filePath = file.Path;
				Assert.IsTrue(File.Exists(filePath), "File should exist");
				Assert.That(tempPath, Does.StartWith(tempPath));

				var file2 = TempData.CreateFile();
				Assert.AreNotEqual(file.Path, file2.Path, "Path should not match");
				Assert.IsTrue(file2.Info.Exists, "File should exist");
				file2.Dispose();
				AssertDisposed(() => file2.Info);
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");

			// test for cleanup if leaked
			{
				var file2Path = CreateAndLeakTempFile(filePath);
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();

				Assert.IsFalse(File.Exists(file2Path), "File should NOT exist");
			}

			// test for SuppressDelete()
			{
				var file2 = TempData.CreateFile();
				var file2Path = file2.Path;
				try
				{
					file2.SuppressDelete();
					Assert.AreNotEqual(filePath, file2Path, "Path should not match");
					Assert.IsNotNull(file2.Info, "Info is null");
					Assert.IsTrue(file2.Info.Exists, "File should exist");
					file2.Dispose();
					Assert.IsTrue(File.Exists(file2Path), "File should exist");
				}
				finally
				{
					File.Delete(file2Path);
				}
			}
		}

		[Test]
		public void TestFileContent()
		{
			string filePath;
			using (var file = TempData.CreateFile())
			{
				filePath = file.Path;

				Assert.IsNotNull(file.Info, "Info is null");
				using (var textWriter = file.Info.AppendText())
					textWriter.Write("O La La");

				var content = File.ReadAllText(filePath);
				Assert.AreEqual(content, "O La La");
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");
		}

		[Test]
		public void TestFileSpecificPath()
		{
			var tempPath = Path.GetTempPath();
			var fileName = TempData.GetTempName();
			var filePath = Path.Combine(tempPath, fileName);

			using (var file = TempData.CreateFile(tempPath, fileName))
			{
				Assert.AreEqual(file.Path, filePath);
				Assert.IsNotNull(file.Info, "Info is null");
				Assert.IsTrue(file.Info.Exists, "File should exist");
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");

			using (var file = TempData.CreateFile(null, fileName))
			{
				Assert.AreEqual(file.Path, filePath);
				Assert.IsNotNull(file.Info, "Info is null");
				Assert.IsTrue(file.Info.Exists, "File should exist");
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");
		}

		[Test]
		public void TestFileSpecificPathSubDirectory()
		{
			var tempPath = Path.Combine(Path.GetTempPath(), TempData.GetTempName());
			Assert.IsFalse(Directory.Exists(tempPath), "Directory should NOT exist");
			var fileName = TempData.GetTempName();
			var filePath = Path.Combine(tempPath, fileName);

			using (var file = TempData.CreateFile(tempPath, fileName))
			{
				Assert.AreEqual(file.Path, filePath);
				Assert.IsNotNull(file.Info, "Info is null");
				Assert.IsTrue(file.Info.Exists, "File should exist");
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");
			Directory.Delete(tempPath);
		}

		[Test]
		public void TestFileStreamSpecificPathSubDirectory()
		{
			var tempPath = Path.Combine(Path.GetTempPath(), TempData.GetTempName());
			Assert.IsFalse(Directory.Exists(tempPath), "Directory should NOT exist");
			var fileName = TempData.GetTempName();
			var filePath = Path.Combine(tempPath, fileName);

			using (var file = TempData.CreateFileStream(tempPath, fileName))
			{
				Assert.AreEqual(file.Name, filePath);
				Assert.IsTrue(File.Exists(filePath), "File should exist");
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");
			Directory.Delete(tempPath);
		}
		[Test]
		public void TestDirectorySpecificPathSubDirectory()
		{
			var tempPath = Path.Combine(Path.GetTempPath(), TempData.GetTempName());
			Assert.IsFalse(Directory.Exists(tempPath), "Directory should NOT exist");
			var dirName = TempData.GetTempName();
			var dirPath = Path.Combine(tempPath, dirName);

			using (var directory = TempData.CreateDirectory(tempPath, dirName))
			{
				Assert.AreEqual(directory.Path, dirPath);
				Assert.IsNotNull(directory.Info, "Info is null");
				Assert.IsTrue(directory.Info.Exists, "Directory should exist");
			}
			Assert.IsFalse(File.Exists(dirPath), "Directory should NOT exist");
			Directory.Delete(tempPath);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static string CreateAndLeakTempStream(string filePath)
		{
			var file2 = TempData.CreateFileStream();
			var file2Path = file2.Name;
			Assert.AreNotEqual(filePath, file2Path, "Path should not match");
			Assert.IsTrue(File.Exists(file2.Name), "FileStream should exist");
			GC.KeepAlive(file2);

			return file2Path;
		}

		[Test]
		public void TestFileStream()
		{
			var tempPath = Path.GetTempPath();
			string filePath;
			using (var file = TempData.CreateFileStream())
			{
				filePath = file.Name;
				Assert.IsTrue(File.Exists(filePath), "FileStream should exist");
				Assert.That(tempPath, Does.StartWith(tempPath));

				var file2 = TempData.CreateFileStream();
				Assert.AreNotEqual(file.Name, file2.Name, "Path should not match");
				Assert.IsTrue(File.Exists(file2.Name), "File should exist");
				file2.Dispose();
				AssertDisposed(() => file2.Length);
			}
			Assert.IsFalse(File.Exists(filePath), "FileStream should NOT exist");

			// test for cleanup if leaked
			{
				var file2Path = CreateAndLeakTempStream(filePath);
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				Assert.IsFalse(File.Exists(file2Path), "FileStream should NOT exist");
			}
		}

#if !LESSTHAN_NET45
		[Test]
		public void TestFileStreamContent()
		{
			string filePath;
			using (var fileStream = TempData.CreateFileStream())
			{
				filePath = fileStream.Name;

				using (var textWriter = new StreamWriter(fileStream, Encoding.UTF8, 4096, true))
					textWriter.Write("O La La");

				string content;
				fileStream.Position = 0;
				using (var textReader = new StreamReader(fileStream, Encoding.UTF8, true, 4096, true))
					content = textReader.ReadToEnd();
				Assert.AreEqual(content, "O La La");
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");
		}
#endif

		[Test]
		public void TestFileStreamSpecificPath()
		{
			var tempPath = Path.GetTempPath();
			var fileName = Guid.NewGuid() + ".tmp";
			var filePath = Path.Combine(tempPath, fileName);

			using (var fileStream = TempData.CreateFileStream(tempPath, fileName))
			{
				Assert.AreEqual(fileStream.Name, filePath);
				Assert.IsTrue(File.Exists(filePath), "File should exist");
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");
		}

		[Test]
		public void TestFileWrapperWithDefaultPath()
		{
			string filePath;
			using (var file = new TempData.TempFile())
			{
				filePath = file.Path;
				Assert.IsNotNull(file.Info, "Info is null");
				Assert.IsFalse(file.Info.Exists, "File should not exist");

				using (File.Create(filePath))
				{ }
				file.Info.Refresh();
				Assert.IsTrue(file.Info.Exists, "File should exist");
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");
		}

		[Test]
		public void TestFileWrapperWithCustomPath()
		{
			var tempPath = Path.GetTempPath();
			var fileName = TempData.GetTempName();
			var filePath = Path.Combine(tempPath, fileName);
			using (var file = new TempData.TempFile(filePath))
			{
				Assert.AreEqual(filePath, file.Path);
				Assert.IsNotNull(file.Info, "Info is null");
				Assert.IsFalse(file.Info.Exists, "File should not exist");

				using (File.Create(filePath))
				{ }
				file.Info.Refresh();
				Assert.IsTrue(file.Info.Exists, "File should exist");
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");
		}
	}
}
