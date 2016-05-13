using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.UnitTesting
{
	/// <summary>
	/// Fills min..max values for [CompetitionBenchmark] attribute
	/// DANGER: this will try to update sources. May fail.
	/// </summary>
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

			public IReadOnlyList<string> GetFileLines(string file)
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
				_sourceLines[file][lineIndex] = newLine;
				MarkAsChanged(file);
			}

			public void Save()
			{
				if (!HasChanges)
					return;

				foreach (var pair in _sourceLines)
				{
					if (_changedFiles.Contains(pair.Key))
						CompetitionHelpers.WriteFileContent(pair.Key, pair.Value);
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
		private static bool TryGetSourceInfo(
			CompetitionTarget competitionTarget,
			out string sourceFileName, out int firstCodeLine) =>
				SymbolHelpers.TryGetSourceInfo(
					competitionTarget.Target.Method, out sourceFileName, out firstCodeLine);
		#endregion

		public static void AnnotateBenchmarkFiles(Summary summary, List<IWarning> warnings)
		{
			var competitionState = summary.GetCompetitionState();
			var competitionParameters = summary.GetCompetitionParameters();
			var logger = summary.Config.GetCompositeLogger();

			var annContext = new AnnotateContext();
			var competitionTargets = competitionState.GetCompetitionTargets(summary);
			var changedTargets = competitionState.GetCompetitionTargetsToUpdate(summary);
			if (changedTargets.Length == 0)
			{
				logger.WriteLineInfo("All competition benchmarks do not require annotation. Skipping.");
				return;
			}

			foreach (var newTarget in changedTargets)
			{
				var targetMethodName = newTarget.CandidateName;

				logger.WriteLineInfo(
					$"Method {targetMethodName}: new relative time limits [{newTarget.MinText},{newTarget.MaxText}].");

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
						$"Method {targetMethodName}: annotating resource file {resourceFileName}.");
					bool annotated = TryFixBenchmarkResource(annContext, resourceFileName, newTarget);
					if (!annotated)
					{
						throw new InvalidOperationException(
							$"Method {targetMethodName}: could not annotate resource file {resourceFileName}.");
					}
				}
				else
				{
					logger.WriteLineInfo($"Method {targetMethodName}: annotating file {fileName}, line {firstCodeLine}.");
					bool annotated = TryFixBenchmarkAttribute(annContext, fileName, firstCodeLine, newTarget);
					if (!annotated)
					{
						logger.WriteError($"Method {targetMethodName}: could not annotate source file {fileName}.");
						throw new InvalidOperationException(
							$"Method {targetMethodName}: could not annotate source file {fileName}.");
					}
				}

				if (competitionParameters.RerunIfModified && !competitionState.LastRun)
				{
					var message = $"Method {targetMethodName} annotation updated, benchmark has to be restarted";
					logger.WriteLineInfo(message);
					warnings.Add(new Warning(nameof(AnnotateSourceHelper), message, null));

					competitionTargets[newTarget.Target] = newTarget;
					competitionState.RerunCount += 2;
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