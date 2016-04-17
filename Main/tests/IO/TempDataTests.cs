using System;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace CodeJam.IO
{
	[TestFixture(Category = "Temp data")]
	public class TempDataTests
	{
		private static void AssertDisposed<T>(Func<T> memberCallback) =>
			Assert.Throws<ObjectDisposedException>(() => memberCallback());

		[Test]
		public void Test01Directory()
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
				var dir2 = TempData.CreateDirectory();
				var dir2Path = dir2.Path;
				Assert.AreNotEqual(dirPath, dir2Path, "Path should not match");
				Assert.IsNotNull(dir2.Info, "Info is null");
				Assert.IsTrue(dir2.Info.Exists, "Directory should exist");
				GC.KeepAlive(dir2);

				// clear GC root for a debug build
				// ReSharper disable once RedundantAssignment
				dir2 = null;
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				Assert.IsFalse(Directory.Exists(dir2Path), "Directory should NOT exist");
			}
		}

		[Test]
		public void Test02DirectoryNestedContent()
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
			Assert.IsFalse(Directory.Exists(nestedFile), "File should NOT exist");
		}

		[Test]
		public void Test03File()
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
				var file2 = TempData.CreateFile();
				var file2Path = file2.Path;
				Assert.AreNotEqual(filePath, file2Path, "Path should not match");
				Assert.IsNotNull(file2.Info, "Info is null");
				Assert.IsTrue(file2.Info.Exists, "File should exist");
				GC.KeepAlive(file2);

				// clear GC root for a debug build
				// ReSharper disable once RedundantAssignment
				file2 = null;
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				Assert.IsFalse(File.Exists(file2Path), "File should NOT exist");
			}
		}

		[Test]
		public void Test04FileContent()
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
		public void Test05FileSpecificPath()
		{
			var tempPath = Path.GetTempPath();
			var fileName = Guid.NewGuid() + ".tmp";
			var filePath = Path.Combine(tempPath, fileName);

			using (var file = TempData.CreateFile(tempPath, fileName))
			{
				Assert.AreEqual(file.Path, filePath);
				Assert.IsNotNull(file.Info, "Info is null");
				Assert.IsTrue(file.Info.Exists, "File should exist");
			}
			Assert.IsFalse(File.Exists(filePath), "File should NOT exist");
		}

		[Test]
		public void Test06FileStream()
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
				var file2 = TempData.CreateFileStream();
				var file2Path = file2.Name;
				Assert.AreNotEqual(filePath, file2Path, "Path should not match");
				Assert.IsTrue(File.Exists(file2.Name), "FileStream should exist");
				GC.KeepAlive(file2);

				// clear GC root for a debug build
				// ReSharper disable once RedundantAssignment
				file2 = null;
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				Assert.IsFalse(File.Exists(file2Path), "FileStream should NOT exist");
			}
		}

		[Test]
		public void Test07FileStreamContent()
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

		[Test]
		public void Test08FileStreamSpecificPath()
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
	}
}