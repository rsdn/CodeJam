using System;
using System.IO;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Loggers
{
	// BASEDON: https://github.com/PerfDotNet/BenchmarkDotNet/blob/master/BenchmarkDotNet/Loggers/StreamLogger.cs
	/// <summary>Implementation of <see cref="ILogger"/> that supports flush method.</summary>
	/// <seealso cref="BenchmarkDotNet.Loggers.ILogger"/>
	[PublicAPI]
	public class FlushableStreamLogger : IFlushableLogger
	{
		private readonly StreamWriter _writer;

		/// <summary>Initializes a new instance of the <see cref="FlushableStreamLogger"/> class.</summary>
		/// <param name="writer">The writer the log output will be redirected.</param>
		public FlushableStreamLogger([NotNull] StreamWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			_writer = writer;
		}

		/// <summary>Initializes a new instance of the <see cref="FlushableStreamLogger"/> class.</summary>
		/// <param name="filePath">The file path for the log.</param>
		/// <param name="append">
		/// if set to <c>true</c> the log will be append to existing file; if <c>false</c> the log wil be overwritten.
		/// </param>
		public FlushableStreamLogger([NotNull] string filePath, bool append = false)
		{
			_writer = new StreamWriter(filePath, append);
		}

		/// <summary>Write the text.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public void Write(LogKind logKind, string text) => _writer.Write(text);

		/// <summary>Write empty line.</summary>
		public void WriteLine() => _writer.WriteLine();

		/// <summary>Write the line.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public void WriteLine(LogKind logKind, string text) => _writer.WriteLine(text);

		/// <summary>Flushes the log.</summary>
		public void Flush() => _writer.Flush();
	}
}