using System;
using System.IO;
using System.Threading;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Loggers
{
	// BASEDON: https://github.com/PerfDotNet/BenchmarkDotNet/blob/master/BenchmarkDotNet/Loggers/StreamLogger.cs
	/// <summary>Implementation of <see cref="ILogger"/> that supports lazy initialization.</summary>
	/// <seealso cref="BenchmarkDotNet.Loggers.ILogger"/>
	[PublicAPI]
	public class LazyStreamLogger : IFlushableLogger
	{
		private readonly Lazy<StreamWriter> _writerLazy;

		/// <summary>Initializes a new instance of the <see cref="LazyStreamLogger"/> class.</summary>
		/// <param name="writerFactory">Factory method for the writer the log output will be redirected.</param>
		public LazyStreamLogger([NotNull] Func<StreamWriter> writerFactory)
		{
			if (writerFactory == null)
				throw new ArgumentNullException(nameof(writerFactory));
			_writerLazy = new Lazy<StreamWriter>(
				writerFactory,
				LazyThreadSafetyMode.ExecutionAndPublication);
		}

		/// <summary>Initializes a new instance of the <see cref="LazyStreamLogger"/> class.</summary>
		/// <param name="filePath">The file path for the log.</param>
		/// <param name="append">
		/// if set to <c>true</c> the log will be appended to existing file; if <c>false</c> the log wil be overwritten.
		/// </param>
		public LazyStreamLogger([NotNull] string filePath, bool append = false)
		{
			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));

			_writerLazy = new Lazy<StreamWriter>(
				() => new StreamWriter(filePath, append),
				LazyThreadSafetyMode.ExecutionAndPublication);
		}

		/// <summary>Write the text.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public void Write(LogKind logKind, string text)
		{
			lock (_writerLazy)
			{
				_writerLazy.Value.Write(text);
			}
		}

		/// <summary>Write empty line.</summary>
		public void WriteLine()
		{
			lock (_writerLazy)
			{
				_writerLazy.Value.WriteLine();
			}
		}

		/// <summary>Write the line.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public void WriteLine(LogKind logKind, string text)
		{
			lock (_writerLazy)
			{
				_writerLazy.Value.WriteLine(text);
			}
		}

		/// <summary>Flushes the log.</summary>
		public void Flush()
		{
			if (_writerLazy.IsValueCreated)
			{
				lock (_writerLazy)
				{
					_writerLazy.Value.Flush();
				}
			}
		}
	}
}