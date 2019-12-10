using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using NUnit.Framework;

namespace CodeJam.IO
{
	[TestFixture(Category = "Assertions")]
	[SuppressMessage("ReSharper", "NotResolvedInText")]
	public class IoCodeTests
	{
		[Test]
		public void TestIsWellFormedPath()
		{
			var ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedPath(@"a:/b", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain(@"a:/b"));

			ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedPath("", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));

			ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedPath(null, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));

			Assert.DoesNotThrow(() => IoCode.IsWellFormedPath(@"\\valid\path", "arg00"));
		}

		[Test]
		public void TestDebugIsWellFormedPath()
		{
#if DEBUG
			var ex = Assert.Throws<ArgumentException>(() => DebugIoCode.IsWellFormedPath(@"a:/b", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain(@"a:/b"));
#else
			// ReSharper disable once InvocationIsSkipped
			Assert.DoesNotThrow(() =>  DebugIoCode.IsWellFormedPath(@"a:/b", "arg00"));
#endif

			// ReSharper disable once InvocationIsSkipped
			Assert.DoesNotThrow(() => IoCode.IsWellFormedPath("maybe/path", "arg00"));
		}

		[Test]
		public void TestIsWellFormedAbsolutePath()
		{
			var ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedAbsolutePath("maybe/path", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("maybe/path"));

			ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedAbsolutePath("d:maybe\\path", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("d:maybe\\path"));

			ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedAbsolutePath("/some/path", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("/some/path"));

			ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedAbsolutePath("~/some/path", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("~/some/path"));

			ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedAbsolutePath("", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));

			ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedAbsolutePath(null, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));


			Assert.DoesNotThrow(() => IoCode.IsWellFormedAbsolutePath("c:\\valid\\path", "arg00"));
		}

		[Test]
		public void TestIsWellFormedRelativePath()
		{
			var ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedRelativePath("d:maybe\\path", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain("d:maybe\\path"));

			ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedRelativePath("", "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));

			ex = Assert.Throws<ArgumentException>(() => IoCode.IsWellFormedRelativePath(null, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));

			Assert.DoesNotThrow(() => IoCode.IsWellFormedRelativePath("some/path", "arg00"));
			Assert.DoesNotThrow(() => IoCode.IsWellFormedRelativePath("~/some/path", "arg00"));
		}

		[Test]
		public void TestDirectoryExists()
		{
			Exception ex;
			string dirPath;
			using (var dir = TempData.CreateDirectory())
			{
				dirPath = dir.Path;
				Assert.DoesNotThrow(() => IoCode.DirectoryExists(dir.Path, "arg00"));

				ex = Assert.Throws<IOException>(() => IoCode.PathIsFree(dirPath));
				Assert.That(ex.Message, Does.Contain(dirPath));

				ex = Assert.Throws<FileNotFoundException>(() => IoCode.FileExists(dirPath, "arg00"));
				Assert.That(ex.Message, Does.Contain("arg00"));
				Assert.That(ex.Message, Does.Contain("is a directory"));
				Assert.That(ex.Message, Does.Contain(dirPath));
			}

			Assert.DoesNotThrow(() => IoCode.PathIsFree(dirPath));
			ex = Assert.Throws<DirectoryNotFoundException>(() => IoCode.DirectoryExists(dirPath, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain(dirPath));
		}


		[Test]
		public void TestFileExists()
		{
			Exception ex;
			string filePath;
			using (var file = TempData.CreateFile())
			{
				filePath = file.Path;
				Assert.DoesNotThrow(() => IoCode.FileExists(file.Path, "arg00"));

				ex = Assert.Throws<IOException>(() => IoCode.PathIsFree(filePath));
				Assert.That(ex.Message, Does.Contain(filePath));

				ex = Assert.Throws<DirectoryNotFoundException>(() => IoCode.DirectoryExists(filePath, "arg00"));
				Assert.That(ex.Message, Does.Contain("arg00"));
				Assert.That(ex.Message, Does.Contain("is a file"));
				Assert.That(ex.Message, Does.Contain(filePath));
			}

			Assert.DoesNotThrow(() => IoCode.PathIsFree(filePath));

			ex = Assert.Throws<FileNotFoundException>(() => IoCode.FileExists(filePath, "arg00"));
			Assert.That(ex.Message, Does.Contain("arg00"));
			Assert.That(ex.Message, Does.Contain(filePath));
		}
	}
}