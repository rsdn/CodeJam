using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.UnitTesting
{
	/// <summary>
	/// Helper methods for <see cref="CompetitionTarget"/>
	/// </summary>
	internal static class CompetitionHelpers
	{
		#region Benchmark configuration
		/// <summary>
		/// Removes the default console logger, removes all exporters but default markdown exporter.
		/// </summary>
		// TODO: do not filter the exporters?
		public static ManualConfig CreateCompetitionTestConfig(IConfig template)
		{
			var result = new ManualConfig();
			result.Add(template.GetColumns().ToArray());
			//result.Add(template.GetExporters().ToArray());
			result.Add(template.GetExporters().Where(l => l == MarkdownExporter.Default).ToArray());
			result.Add(template.GetLoggers().Where(l => l != ConsoleLogger.Default).ToArray());
			result.Add(template.GetDiagnosers().ToArray());
			result.Add(template.GetAnalysers().ToArray());
			result.Add(template.GetJobs().ToArray());
			result.Add(template.GetValidators().ToArray());

			return result;
		}

		/// <summary>
		/// Checks that the assembly is build in debug mode.
		/// </summary>
		public static bool IsDebugAssembly(this Assembly assembly)
		{
			var optAtt = (DebuggableAttribute)Attribute.GetCustomAttribute(assembly, typeof(DebuggableAttribute));
			return optAtt != null && optAtt.IsJITOptimizerDisabled;
		}
		#endregion

		#region IO
		/// <summary>
		/// Writes file content without empty line at the end
		/// </summary>
		// BASEDON: http://stackoverflow.com/a/11689630
		public static void WriteFileContent(string path, string[] lines)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (lines == null)
				throw new ArgumentNullException(nameof(lines));

			using (var writer = File.CreateText(path))
			{
				if (lines.Length > 0)
				{
					for (var i = 0; i < lines.Length - 1; i++)
					{
						writer.WriteLine(lines[i]);
					}
					writer.Write(lines[lines.Length - 1]);
				}
			}
		}
		#endregion

		#region Competition benchmarks
		public static CompetitionParameters GetCompetitionParameters(this Summary summary) =>
			summary.Config.GetAnalysers()
				.OfType<CompetitionParameters>()
				.Single();

		public static CompetitionState GetCompetitionState(this Summary summary) =>
			summary.Config.GetAnalysers()
				.OfType<CompetitionState>()
				.Single();
		#endregion

		#region XML metadata constants
		public const string CompetitionBenchmarksRootNode = "CompetitionBenchmarks";
		public const string CompetitionNode = "Competition";
		public const string CandidateNode = "Candidate";
		public const string TargetAttribute = "Target";
		public const string MinRatioAttribute = "MinRatio";
		public const string MaxRatioAttribute = "MaxRatio";
		#endregion
	}
}