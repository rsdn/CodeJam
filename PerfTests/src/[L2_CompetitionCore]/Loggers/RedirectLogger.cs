using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using BenchmarkDotNet.Loggers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Loggers
{
	/// <summary>Logger that redirects output to the supplied <see cref="TextWriter"/>.</summary>
	/// <seealso cref="ILogger"/>
	[PublicAPI]
	[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
	public sealed class RedirectLogger : IFlushableLogger
	{
		private readonly TextWriter _output;

		/// <summary>Initializes a new instance of the <see cref="RedirectLogger"/> class.</summary>
		/// <param name="output">The output.</param>
		public RedirectLogger([NotNull] TextWriter output)
		{
			Code.NotNull(output, nameof(output));
			_output = output;
		}

		/// <summary>Write empty line.</summary>
		public void WriteLine() => _output.WriteLine();

		/// <summary>Writes line.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public void WriteLine(LogKind logKind, string text) => _output.WriteLine(text);

		/// <summary>Writes text.</summary>
		/// <param name="logKind">Kind of text.</param>
		/// <param name="text">The text to write.</param>
		public void Write(LogKind logKind, string text) => _output.Write(text);

		/// <summary>Flushes the log.</summary>
		public void Flush() => _output.Flush();
	}
}