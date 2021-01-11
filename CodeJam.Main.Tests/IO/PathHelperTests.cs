using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

namespace CodeJam.IO
{
	[TestFixture(Category = "IO")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public class PathHelperTests
	{
		public enum PathKind
		{
			Throws,
			Invalid,
			ValidAbsolutePath,
			ValidRelativePath,
			ValidAbsoluteContainerPath,
			ValidRelativeContainerPath,
			ValidFileName
		}

		[TestCase("", PathKind.Throws)]
		[TestCase(null, PathKind.Throws)]
		[TestCase(@"a", PathKind.ValidFileName)]
		[TestCase(@"a ", PathKind.ValidFileName)]
		[TestCase(@"a    ", PathKind.ValidFileName)]
		[TestCase(@"a\t", PathKind.ValidRelativePath)]
		[TestCase(@"a...", PathKind.ValidFileName)]
		[TestCase(@"a.", PathKind.ValidFileName)]

		[TestCase(@" a", PathKind.ValidFileName)]
		[TestCase(@"    a", PathKind.ValidFileName)]
		[TestCase(@"\ta", PathKind.Invalid)]
		[TestCase(@"...a", PathKind.ValidFileName)]

		[TestCase(@"a\b", PathKind.ValidRelativePath)]
		[TestCase(@"a\b\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"a/b", PathKind.ValidRelativePath)]
		[TestCase(@"a/b/", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"a/b\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"a\b/", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"a\\b", PathKind.ValidRelativePath)]
		[TestCase(@"a:\b", PathKind.ValidAbsolutePath)]
		[TestCase(@"a:\b\", PathKind.ValidAbsoluteContainerPath)]
		[TestCase(@"a:/b", PathKind.Invalid)]
		[TestCase(@"a:\\b", PathKind.Invalid)]
		[TestCase(@"a:\b\\", PathKind.Invalid)]
		[TestCase(@"a:/b/", PathKind.Invalid)]
		[TestCase(@"\\a\b", PathKind.ValidAbsolutePath)]
		[TestCase(@"\\a\b\", PathKind.ValidAbsoluteContainerPath)]
		[TestCase(@"\\a\\b\", PathKind.Invalid)]
		[TestCase(@"\\a\b/", PathKind.Invalid)]
		[TestCase(@".a", PathKind.ValidFileName)]
		[TestCase(@".a.b", PathKind.ValidFileName)]
		[TestCase(@"..a\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"..a.b\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"..a..\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"..a..b\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"..\a\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"..\a.b\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"..\a..\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"..\a..b\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"a:\.a\", PathKind.ValidAbsoluteContainerPath)]
		[TestCase(@"a:\.a.b\", PathKind.ValidAbsoluteContainerPath)]
		[TestCase(@"a:\a\..a\", PathKind.ValidAbsoluteContainerPath)]
		[TestCase(@"a:\a\..a.b\", PathKind.ValidAbsoluteContainerPath)]
		// SEE https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/retargeting/4.6.1-4.6.2#changes-in-path-normalization
#if NET462_OR_GREATER || TARGETS_NETCOREAPP
		[TestCase(@"a:\a\..a..\", PathKind.ValidAbsoluteContainerPath)]
#else
		[TestCase(@"a:\a\..a..\", PathKind.Invalid)]
#endif
		[TestCase(@"a:\a\..a..b\", PathKind.ValidAbsoluteContainerPath)]
		[TestCase(@"a:\a\..\a\", PathKind.Invalid)]
		[TestCase(@"a:\a\..\a.b\", PathKind.Invalid)]
		[TestCase(@"a:\a\..\a..\", PathKind.Invalid)]
		[TestCase(@"a:\a\..\a..b\", PathKind.Invalid)]
		[TestCase(@"\", PathKind.Invalid)]
		[TestCase(@"/", PathKind.Invalid)]
		[TestCase(@"~", PathKind.ValidFileName)]
		[TestCase(@"a", PathKind.ValidFileName)]
		[TestCase(@"a:", PathKind.Invalid)]
		[TestCase(@"a\", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"a/", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"\a", PathKind.Invalid)]
		[TestCase(@"/a", PathKind.Invalid)]
		[TestCase(@"a:a", PathKind.Invalid)]
		[TestCase(@"a\a", PathKind.ValidRelativePath)]
		[TestCase(@"a/a", PathKind.ValidRelativePath)]
		[TestCase(@"a::a", PathKind.Invalid)]
		[TestCase(@"a:\a", PathKind.ValidAbsolutePath)]
		[TestCase(@"a:|a", PathKind.Invalid)]
		[TestCase(@"a:/a", PathKind.Invalid)]
#if NETCOREAPP21_OR_GREATER
		[TestCase(@"\\a\", PathKind.ValidAbsoluteContainerPath)]
		[TestCase(@"\\a", PathKind.ValidAbsolutePath)]
		[TestCase(@":", PathKind.ValidRelativeContainerPath)]
		[TestCase(@"|", PathKind.ValidRelativePath)]
		[TestCase(@"|a", PathKind.ValidRelativePath)]
		[TestCase(@"a|", PathKind.ValidRelativePath)]
		[TestCase(@"a|a", PathKind.ValidRelativePath)]
		[TestCase(@":a", PathKind.ValidRelativePath)]
		[TestCase(@"a:\:a", PathKind.ValidAbsolutePath)]
		[TestCase(@"a:\|a", PathKind.ValidAbsolutePath)]
		[TestCase(@"a\:a", PathKind.ValidRelativePath)]
		[TestCase(@"a\|a", PathKind.ValidRelativePath)]
#else
		[TestCase(@":", PathKind.Invalid)]
		[TestCase(@":a", PathKind.Invalid)]
		[TestCase(@"\\a", PathKind.Invalid)]
		[TestCase(@"\\a\", PathKind.Invalid)]
		[TestCase(@"a:\:a", PathKind.Invalid)]
		[TestCase(@"a:\|a", PathKind.Invalid)]
		[TestCase(@"a\:a", PathKind.Invalid)]
		[TestCase(@"a\|a", PathKind.Invalid)]
		[TestCase(@"|", PathKind.Invalid)]
		[TestCase(@"|a", PathKind.Invalid)]
		[TestCase(@"a|", PathKind.Invalid)]
		[TestCase(@"a|a", PathKind.Invalid)]
#endif
		[TestCase(@"a:\\a", PathKind.Invalid)]
		[TestCase(@"a:\/a", PathKind.Invalid)]
		[TestCase(@"a\\a", PathKind.ValidRelativePath)]
		[TestCase(@"a\/a", PathKind.ValidRelativePath)]
		[TestCase(@"com0", PathKind.ValidFileName)]
		[TestCase(@"aux", PathKind.ValidFileName)]
		public void TestIsWellFormedPath(string path, PathKind pathState)
		{
			switch (pathState)
			{
				case PathKind.Throws:
					Assert.Throws<ArgumentException>(() => PathHelper.IsWellFormedPath(path));
					Assert.Throws<ArgumentException>(() => PathHelper.IsWellFormedAbsolutePath(path));
					Assert.Throws<ArgumentException>(() => PathHelper.IsWellFormedRelativePath(path));
					Assert.Throws<ArgumentException>(() => PathHelper.IsWellFormedContainerPath(path));
					Assert.Throws<ArgumentException>(() => PathHelper.IsWellFormedFileName(path));
					break;
				case PathKind.Invalid:
					Assert.AreEqual(PathHelper.IsWellFormedPath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedAbsolutePath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedRelativePath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedContainerPath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedFileName(path), false);
					break;
				case PathKind.ValidAbsolutePath:
					Assert.AreEqual(PathHelper.IsWellFormedPath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedAbsolutePath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedRelativePath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedContainerPath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedFileName(path), false);
					break;
				case PathKind.ValidAbsoluteContainerPath:
					Assert.AreEqual(PathHelper.IsWellFormedPath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedAbsolutePath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedRelativePath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedContainerPath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedFileName(path), false);
					break;
				case PathKind.ValidRelativePath:
					Assert.AreEqual(PathHelper.IsWellFormedPath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedAbsolutePath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedRelativePath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedContainerPath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedFileName(path), false);
					break;
				case PathKind.ValidRelativeContainerPath:
					Assert.AreEqual(PathHelper.IsWellFormedPath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedAbsolutePath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedRelativePath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedContainerPath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedFileName(path), false);
					break;
				case PathKind.ValidFileName:
					Assert.AreEqual(PathHelper.IsWellFormedPath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedAbsolutePath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedRelativePath(path), true);
					Assert.AreEqual(PathHelper.IsWellFormedContainerPath(path), false);
					Assert.AreEqual(PathHelper.IsWellFormedFileName(path), true);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(pathState), pathState, null);
			}
		}

		[TestCase(null, null)]
		[TestCase(@"", null)]
		[TestCase(@":", true)]
		[TestCase(@"\", true)]
		[TestCase(@"/", true)]
		[TestCase(@"a::", true)]
		[TestCase(@"a:\", true)]
		[TestCase(@"a:/", true)]
		[TestCase(@"a::a", false)]
		[TestCase(@"a:\a", false)]
		[TestCase(@"a:/a", false)]
		public void TestIsContainerPath(string path, bool? state)
		{
			switch (state)
			{
				case null:
					Assert.Throws<ArgumentException>(() => PathHelper.IsContainerPath(path));
					break;
				case true:
					Assert.AreEqual(PathHelper.IsContainerPath(path), true);
					break;
				case false:
					Assert.AreEqual(PathHelper.IsContainerPath(path), false);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
		}

		[TestCase(null, null)]
		[TestCase(@"", null)]
		[TestCase(@":", @":")]
		[TestCase(@"\", @"\")]
		[TestCase(@"/", @"/")]
		[TestCase(@"a::", @"a::")]
		[TestCase(@"a:\", @"a:\")]
		[TestCase(@"a:/", @"a:/")]
		[TestCase(@"a::a", @"a::a\")]
		[TestCase(@"a:\a", @"a:\a\")]
		[TestCase(@"a:/a", @"a:/a\")]
		public void TestEnsureContainerPath(string path, string result)
		{
			if (result == null)
				Assert.Throws<ArgumentException>(() => PathHelper.IsContainerPath(path));
			else
				Assert.AreEqual(PathHelper.EnsureContainerPath(path), result);
		}

		[TestCase(null, null)]
		[TestCase(@"", null)]
		[TestCase(@":", false)]
		[TestCase(@"\", false)]
		[TestCase(@"/", false)]
		[TestCase(@"a", true)]
		[TestCase(@"a.", true)]
		[TestCase(@"a.txt", true)]
		[TestCase(@"a..", true)]
		[TestCase(@"..a..", true)]
		[TestCase(@"..a..b", true)]
		[TestCase(@"..\a..b", false)]
		[TestCase(@"a?a", false)]
		[TestCase(@"a:/", false)]
		[TestCase(@"a:\a", false)]
		[TestCase(@"a:/a", false)]
		public void TestIsFileName(string path, bool? state)
		{
			switch (state)
			{
				case null:
					Assert.Throws<ArgumentException>(() => PathHelper.IsFileName(path));
					break;
				case true:
					Assert.AreEqual(PathHelper.IsFileName(path), true);
					break;
				case false:
					Assert.AreEqual(PathHelper.IsFileName(path), false);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
		}
	}
}
