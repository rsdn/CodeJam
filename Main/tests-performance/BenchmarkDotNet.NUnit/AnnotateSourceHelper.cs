using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using BenchmarkDotNet.Analyzers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.NUnit
{
	/// <summary>
	/// Fills min..max values for [CompetitionBenchmark] attribute
	/// DANGER: this will try to update sources. May fail.
	/// </summary>
	[PublicAPI]
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	internal static partial class AnnotateSourceHelper
	{
		#region Helper types
		private class AnnotateContext
		{
			private readonly Dictionary<string, string[]> _sourceLines = new Dictionary<string, string[]>();
			private readonly Dictionary<string, XDocument> _xmlAnnotations = new Dictionary<string, XDocument>();
			private readonly HashSet<string> _changedFiles = new HashSet<string>();

			// ReSharper disable once MemberCanBePrivate.Local
			public bool HasChanges => _changedFiles.Any();

			public string[] GetFileLines(string file)
			{
				if (_xmlAnnotations.ContainsKey(file))
					throw new InvalidOperationException($"File {file} already loaded as XML annotation");

				string[] result;
				if (!_sourceLines.TryGetValue(file, out result))
				{
					result = File.ReadAllLines(file);
					_sourceLines[file] = result;
				}
				return result;
			}

			public XDocument GetXmlAnnotation(string file)
			{
				if (_sourceLines.ContainsKey(file))
					throw new InvalidOperationException($"File {file} already loaded as source lines");

				XDocument result;
				if (!_xmlAnnotations.TryGetValue(file, out result))
				{
					result = XDocument.Load(file);
					_xmlAnnotations[file] = result;
				}
				return result;
			}

			public void MarkAsChanged(string file)
			{
				if (!_sourceLines.ContainsKey(file) && !_xmlAnnotations.ContainsKey(file))
					throw new InvalidOperationException($"File {file} not loaded yet");

				_changedFiles.Add(file);
			}

			public void ReplaceLine(string file, int lineIndex, string newLine)
			{
				GetFileLines(file)[lineIndex] = newLine;
				MarkAsChanged(file);
			}

			public void Save()
			{
				if (!HasChanges)
					return;

				foreach (var pair in _sourceLines)
				{
					if (_changedFiles.Contains(pair.Key))
						BenchmarkHelpers.WriteFileContent(pair.Key, pair.Value);
				}

				var saveSettings = new XmlWriterSettings
				{
					Indent = true,
					IndentChars = "\t"
				};
				foreach (var pair in _xmlAnnotations)
				{
					if (_changedFiles.Contains(pair.Key))
					{
						using (var writer = XmlWriter.Create(pair.Key, saveSettings))
							pair.Value.Save(writer);
					}
				}
			}
		}
		#endregion

		#region Helper methods
		private static bool TryGetSourceInfo(CompetitionTarget competitionTarget, out string fileName, out int firstCodeLine)
		{
			fileName = null;
			firstCodeLine = 0;
			var methodSymbols = SymbolHelpers.TryGetSymbols(competitionTarget.Target.Method);
			if (methodSymbols != null)
			{
				var count = methodSymbols.SequencePointCount;
				var docs = new ISymbolDocument[count];
				var offsets = new int[count];
				var lines = new int[count];
				var columns = new int[count];
				var endlines = new int[count];
				var endcolumns = new int[count];
				methodSymbols.GetSequencePoints(offsets, docs, lines, columns, endlines, endcolumns);

				fileName = docs[0].URL;
				firstCodeLine = lines.Min();
			}
			return fileName != null;
		}
		#endregion

		public static void AnnotateBenchmarkFiles(Summary summary, List<IWarning> warnings)
		{
			var competitionState = summary.Config.GetAnalysers().OfType<CompetitionStateAnalyser>().Single();
			var competitionParameters = summary.Config.GetAnalysers().OfType<CompetitionParametersAnalyser>().Single();
			var logger = summary.Config.GetCompositeLogger();

			var annContext = new AnnotateContext();
			var competitionTargets = competitionState.GetCompetitionTargets(summary);
			var newTargets = competitionState.GetNewCompetitionTargets(summary);
			if (newTargets.Length == 0)
			{
				logger.WriteLineInfo("All competition benchmarks are in boundary.");
				return;
			}

			foreach (var newTarget in newTargets)
			{
				var targetMethodName = newTarget.CandidateName;

				logger.WriteLineInfo($"Method {targetMethodName}: new boundary [{newTarget.MinText},{newTarget.MaxText}].");

				int firstCodeLine;
				string fileName;
				bool hasSource = TryGetSourceInfo(newTarget, out fileName, out firstCodeLine);
				if (!hasSource)
				{
					throw new InvalidOperationException($"Method {targetMethodName}: could not annotate. Source file not found.");
				}

				if (newTarget.UsesResourceAnnotation)
				{
					var resourceFileName = Path.ChangeExtension(fileName, ".xml");
					logger.WriteLineInfo(
						$"Method {targetMethodName}: annotate resource file {resourceFileName}.");
					bool annotated = TryFixBenchmarkResource(annContext, resourceFileName, newTarget);
					if (!annotated)
					{
						throw new InvalidOperationException(
							$"Method {targetMethodName}: could not annotate resource file {resourceFileName}.");
					}
				}
				else
				{
					logger.WriteLineInfo($"Method {targetMethodName}: annotate at line {firstCodeLine}, file {fileName}.");
					bool annotated = TryFixBenchmarkAttribute(annContext, fileName, firstCodeLine, newTarget);
					if (!annotated)
					{
						throw new InvalidOperationException($"Method {targetMethodName}: could not annotate. Source file {fileName}.");
					}
				}

				if (competitionParameters.RerunIfModified && !competitionState.LastRun)
				{
					var message = $"Method {targetMethodName} annotation updated, benchmark has to be restarted";
					logger.WriteLineInfo(message);
					warnings.Add(new Warning(nameof(AnnotateSourceHelper), message, null));

					competitionTargets[newTarget.Target] = newTarget;
					competitionState.RerunRequested = true;
				}
				else
				{
					var message = $"Method {targetMethodName} annotation updated.";
					logger.WriteLineInfo(message);
					warnings.Add(new Warning(nameof(AnnotateSourceHelper), message, null));
				}
			}

			annContext.Save();
		}
	}
}