using System;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Collections;
using CodeJam.PerfTests.Running.Helpers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Source annotation document
	/// </summary>
	/// <seealso cref="AnnotationDocument" />
	internal sealed class SourceAnnotationFile : AnnotationDocument
	{
		private readonly List<string> _sourceLines;
		private readonly Dictionary<RuntimeMethodHandle, TargetSourceLines> _targetSourceLines;

		public SourceAnnotationFile(
			[NotNull] string path,
			[NotNull] string[] sourceLines) : this(path, sourceLines, Array<TargetSourceLines>.Empty) { }

		public SourceAnnotationFile(
			[NotNull] string path,
			[NotNull] string[] sourceLines,
			[NotNull] TargetSourceLines[] benchmarkMethods) :
					base(
					path,
					sourceLines.Length > 0 && benchmarkMethods.Length > 0)
		{
			_sourceLines = sourceLines.ToList();
			_targetSourceLines = benchmarkMethods.ToDictionary(m => m.TargetMethodHandle);
		}

		public IReadOnlyDictionary<RuntimeMethodHandle, TargetSourceLines> BenchmarkMethods => _targetSourceLines;

		public int LinesCount => _sourceLines.Count;

		public string this[int lineNumber] => _sourceLines[lineNumber - 1];

		public void ReplaceLine(int lineNumber, string newLine)
		{
			Code.InRange(lineNumber, nameof(lineNumber), 1, LinesCount);
			Code.NotNull(newLine, nameof(newLine));
			AssertIsParsed();

			_sourceLines[lineNumber - 1] = newLine;
			MarkAsChanged();
		}

		public void InsertLine(int insertLineNumber, string newLine)
		{
			Code.InRange(insertLineNumber, nameof(insertLineNumber), 1, LinesCount);
			Code.NotNull(newLine, nameof(newLine));
			AssertIsParsed();

			_sourceLines.Insert(insertLineNumber - 1, newLine);
			foreach (var benchmarkMethodInfo in _targetSourceLines.Values)
			{
				benchmarkMethodInfo.FixOnInsert(insertLineNumber);
			}
			MarkAsChanged();
		}

		/// <summary>Saves the document.</summary>
		protected override void SaveCore() => IoHelpers.WriteFileContent(Origin, _sourceLines.ToArray());

		/// <summary>Releases unmanaged and - optionally - managed resources.</summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			_sourceLines.Clear();
			_targetSourceLines.Clear();
		}
	}
}