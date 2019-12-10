using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

namespace CodeJam.IO
{
	[TestFixture(Category = "IO")]
	[SuppressMessage("ReSharper", "HeapView.BoxingAllocation")]
	public class PathHelpersTests
	{
		public enum PathKind
		{
			Throws,
			Invalid,
			ValidAbsolutePath,
			ValidRelativePath,
			ValidAbsoluteContainerPath,
			ValidRelativeContainerPath,
			ValidSimpleName
		}

		[TestCase("", PathKind.Throws)]
		[TestCase(null, PathKind.Throws)]
		[TestCase(@"a", PathKind.ValidSimpleName)]
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
		[TestCase(@".a", PathKind.ValidSimpleName)]
		[TestCase(@".a.b", PathKind.ValidSimpleName)]
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
#if (TARGETS_NET && !LESSTHAN_NET462) || TARGETS_NETCOREAPP
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
		[TestCase(@"~", PathKind.ValidSimpleName)]
		[TestCase(@"a", PathKind.ValidSimpleName)]
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
#if TARGETS_NET || LESSTHAN_NETCOREAPP21
		[TestCase(@"\\a\", PathKind.Invalid)]
		[TestCase(@"\\a", PathKind.Invalid)]
		[TestCase(@":", PathKind.Invalid)]
		[TestCase(@"|", PathKind.Invalid)]
		[TestCase(@"|a", PathKind.Invalid)]
		[TestCase(@"a|", PathKind.Invalid)]
		[TestCase(@"a|a", PathKind.Invalid)]
		[TestCase(@":a", PathKind.Invalid)]
		[TestCase(@"a:\:a", PathKind.Invalid)]
		[TestCase(@"a:\|a", PathKind.Invalid)]
		[TestCase(@"a\:a", PathKind.Invalid)]
		[TestCase(@"a\|a", PathKind.Invalid)]
#else
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
#endif
		[TestCase(@"a:\\a", PathKind.Invalid)]
		[TestCase(@"a:\/a", PathKind.Invalid)]
		[TestCase(@"a\\a", PathKind.ValidRelativePath)]
		[TestCase(@"a\/a", PathKind.ValidRelativePath)]
		[TestCase(@"com0", PathKind.ValidSimpleName)]
		[TestCase(@"aux", PathKind.ValidSimpleName)]
		public void TestIsWellFormedPath(string path, PathKind pathState)
		{
			switch (pathState)
			{
				case PathKind.Throws:
					Assert.Throws<ArgumentException>(() => PathHelpers.IsWellFormedPath(path));
					Assert.Throws<ArgumentException>(() => PathHelpers.IsWellFormedAbsolutePath(path));
					Assert.Throws<ArgumentException>(() => PathHelpers.IsWellFormedRelativePath(path));
					Assert.Throws<ArgumentException>(() => PathHelpers.IsWellFormedContainerPath(path));
					Assert.Throws<ArgumentException>(() => PathHelpers.IsWellFormedSimpleName(path));
					break;
				case PathKind.Invalid:
					Assert.AreEqual(PathHelpers.IsWellFormedPath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedAbsolutePath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedRelativePath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedContainerPath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedSimpleName(path), false);
					break;
				case PathKind.ValidAbsolutePath:
					Assert.AreEqual(PathHelpers.IsWellFormedPath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedAbsolutePath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedRelativePath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedContainerPath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedSimpleName(path), false);
					break;
				case PathKind.ValidAbsoluteContainerPath:
					Assert.AreEqual(PathHelpers.IsWellFormedPath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedAbsolutePath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedRelativePath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedContainerPath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedSimpleName(path), false);
					break;
				case PathKind.ValidRelativePath:
					Assert.AreEqual(PathHelpers.IsWellFormedPath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedAbsolutePath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedRelativePath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedContainerPath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedSimpleName(path), false);
					break;
				case PathKind.ValidRelativeContainerPath:
					Assert.AreEqual(PathHelpers.IsWellFormedPath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedAbsolutePath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedRelativePath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedContainerPath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedSimpleName(path), false);
					break;
				case PathKind.ValidSimpleName:
					Assert.AreEqual(PathHelpers.IsWellFormedPath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedAbsolutePath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedRelativePath(path), true);
					Assert.AreEqual(PathHelpers.IsWellFormedContainerPath(path), false);
					Assert.AreEqual(PathHelpers.IsWellFormedSimpleName(path), true);
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
					Assert.Throws<ArgumentException>(() => PathHelpers.IsContainerPath(path));
					break;
				case true:
					Assert.AreEqual(PathHelpers.IsContainerPath(path), true);
					break;
				case false:
					Assert.AreEqual(PathHelpers.IsContainerPath(path), false);
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
				Assert.Throws<ArgumentException>(() => PathHelpers.IsContainerPath(path));
			else
				Assert.AreEqual(PathHelpers.EnsureContainerPath(path), result);
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
		public void TestIsSimpleName(string path, bool? state)
		{
			switch (state)
			{
				case null:
					Assert.Throws<ArgumentException>(() => PathHelpers.IsSimpleName(path));
					break;
				case true:
					Assert.AreEqual(PathHelpers.IsSimpleName(path), true);
					break;
				case false:
					Assert.AreEqual(PathHelpers.IsSimpleName(path), false);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(state), state, null);
			}
		}
	}
}
