using System;
using System.IO;
using System.Threading;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Loggers
{
	// BASEDON: https://github.com/dotnet/BenchmarkDotNet/blob/master/src/BenchmarkDotNet.Core/Loggers/StreamLogger.cs
	/// <summary>
	/// Implementation of <see cref="ILogger"/> that supports lazy initialization and prevents output interleaving.
	/// </summary>
	/// <seealso cref="ILogger"/>
	[PublicAPI]
	public sealed class LazySynchronizedStreamLogger : IFlushableLogger
	{
		private readonly Lazy<TextWriter> _writerLazy;

		/// <summary>Initializes a new instance of the <see cref="LazySynchronizedStreamLogger"/> class.</summary>
		/// <param name="writerFactory">Factory method for the writer the log output will be redirected.</param>
		public LazySynchronizedStreamLogger([NotNull] Func<TextWriter> writerFactory)
		{
			if (writerFactory == null)
				throw new ArgumentNullException(nameof(writerFactory));

			_writerLazy = new Lazy<TextWriter>(
				() => TextWriter.Synchronized(writerFactory()),
				LazyThreadSafetyMode.ExecutionAndPublication);
		}

		/// <summary>Initializes a new instance of the <see cref="LazySynchronizedStreamLogger"/> class.</summary>
		/// <param name="filePath">The file path for the log.</param>
		/// <param name="append">
		/// if set to <c>true</c> the log will be appended to existing file; if <c>false</c> the log wil be overwritten.
		/// </param>
		public LazySynchronizedStreamLogger([NotNull] string filePath, bool append = false)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));

			_writerLazy = new Lazy<TextWriter>(
				() => TextWriter.Synchronized(new StreamWriter(filePath, append)),
				LazyThreadSafetyMode.ExecutionAndPublication);
		}

		/// <summary>Write the text.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public void Write(LogKind logKind, string text) => _writerLazy.Value.Write(text);

		/// <summary>Write empty line.</summary>
		public void WriteLine() => _writerLazy.Value.WriteLine();

		/// <summary>Write the line.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public void WriteLine(LogKind logKind, string text) => _writerLazy.Value.WriteLine(text);

		/// <summary>Flushes the log.</summary>
		public void Flush()
		{
			if (_writerLazy.IsValueCreated)
			{
				_writerLazy.Value.Flush();
			}
		}
	}
}