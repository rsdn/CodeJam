using System;
using System.Linq;

using NUnit.Framework;

namespace CodeJam.Collections
{
	public partial class EnumerableExtensionTests
	{
		[TestCase(1)]
		[TestCase(3)]
		[TestCase(6)]
		[TestCase(7)]
		public void SplitReturnNonEmptyEnumerableTest(int size)
		{
			var ranges = Enumerable.Range(1, size);
			var chunks = ranges.Split(3);

			Assert.That(chunks, Is.Not.Empty);
		}

		[TestCase(1, 1, 1, 1)]
		[TestCase(3, 1, 3, 3)]
		[TestCase(6, 2, 3, 3)]
		[TestCase(7, 3, 3, 1)]
		public void SplitSizeTest(int size, int totalChunks, int chunkSize, int lastChunkSize)
		{
			var ranges = Enumerable.Range(1, size);
			var chunks = ranges.Split(3).ToArray();

			Assert.That(chunks.Length,        Is.EqualTo(totalChunks),   "#1");
			Assert.That(chunks[0].Length,     Is.EqualTo(chunkSize),     "#2");
			Assert.That(chunks.Last().Length, Is.EqualTo(lastChunkSize), "#3");
		}

		[Test]
		public void SplitEmptyCollectionTest()
		{
			var ranges = new int[0];
			var chunks = ranges.Split(3).ToArray();
			Assert.That(chunks, Is.Empty);
		}

		[TestCase( 0)]
		[TestCase(-1)]
		public void SplitWithInvalidChunkSizeMustThrowExceptionTest(int chunkSize) =>
			// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
			Assert.Throws<ArgumentOutOfRangeException>(() => new int[0].Split(chunkSize));
	}
}
