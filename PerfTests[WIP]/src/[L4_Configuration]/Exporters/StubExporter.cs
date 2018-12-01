using System;
using System.Collections.Generic;

using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Validators;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Exporters
{
	/// <summary>
	/// Stub exporter to shut up the <see cref="ConfigValidator"/>
	/// </summary>
	/// <seealso cref="IExporter" />
	[PublicAPI]
	public class StubExporter : IExporter
	{
		#region Implementation of IExporter
		/// <summary>Exports to log.</summary>
		/// <param name="summary">The summary.</param>
		/// <param name="logger">The logger.</param>
		public void ExportToLog(Summary summary, ILogger logger)
		{
			// Do nothing
		}

		/// <summary>Exports to files.</summary>
		/// <param name="summary">The summary.</param>
		/// <param name="consoleLogger">The logger.</param>
		/// <returns>Export output.</returns>
		public IEnumerable<string> ExportToFiles(Summary summary, ILogger consoleLogger)
		{
			// Do nothing
			yield break;
		}

		/// <summary>Gets the exporter name.</summary>
		/// <value>The exporter name.</value>
		public string Name => nameof(StubExporter);
		#endregion
	}
}