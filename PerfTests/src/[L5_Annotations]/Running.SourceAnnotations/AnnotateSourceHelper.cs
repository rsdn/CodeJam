using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;

using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Fills min..max values for [CompetitionBenchmark] attribute
	/// DANGER: this will try to update sources. May fail.
	/// </summary>
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	[SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
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
				if (!_sourceLines.ContainsKey(file))
					throw new InvalidOperationException($"File {file} not loaded yet");

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
					{
						BenchmarkHelpers.WriteFileContent(pair.Key, pair.Value);
					}
				}

				foreach (var pair in _xmlAnnotations)
				{
					if (_changedFiles.Contains(pair.Key))
					{
						using (var writer = new StreamWriter(pair.Key))
						{
							XmlAnnotations.Save(pair.Value, writer);
						}
					}
				}
			}
		}
		#endregion

		// ReSharper disable once ParameterTypeCanBeEnumerable.Global
		public static CompetitionTarget[] TryAnnotateBenchmarkFiles(
			CompetitionState competitionState, CompetitionTarget[] targetsToAnnotate, ILogger logger)
		{
			var annotatedTargets = new List<CompetitionTarget>();

			var annContext = new AnnotateContext();
			foreach (var targetToAnnotate in targetsToAnnotate)
			{
				var targetMethodName = targetToAnnotate.CandidateName;

				logger.WriteLineInfo(
					$"// Method {targetMethodName}: new relative time limits [{targetToAnnotate.MinText}, {targetToAnnotate.MaxText}].");

				// DONTTOUCH: the source should be loaded for checksum validation even if target uses resource annotation
				int firstCodeLine;
				string fileName;
				string validationMessage;
				bool hasSource = SymbolHelpers.TryGetSourceInfo(
					targetToAnnotate.Target.Method,
					out fileName, out firstCodeLine, out validationMessage);

				if (!hasSource)
				{
					validationMessage = validationMessage ?? "Source file not found.";
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.Warning,
						$"Method {targetMethodName}: could not annotate. {validationMessage}");
					continue;
				}

				if (targetToAnnotate.UsesResourceAnnotation)
				{
					var resourceFileName = Path.ChangeExtension(fileName, ".xml");
					logger.WriteLineInfo($"// Method {targetMethodName}: annotating resource file {resourceFileName}.");
					var annotated = TryFixBenchmarkResource(annContext, resourceFileName, targetToAnnotate);
					if (!annotated)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Warning,
							$"Method {targetMethodName}: could not annotate resource file {resourceFileName}.", null);
						continue;
					}
				}
				else
				{
					logger.WriteLineInfo($"// Method {targetMethodName}: annotating file {fileName}, line {firstCodeLine}.");
					var annotated = TryFixBenchmarkAttribute(annContext, fileName, firstCodeLine, targetToAnnotate);
					if (!annotated)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Warning,
							$"Method {targetMethodName}: could not annotate source file {fileName}.");
						continue;
					}
				}

				logger.WriteLineInfo($"// !Method {targetMethodName} annotation updated.");
				annotatedTargets.Add(targetToAnnotate);
			}

			annContext.Save();
			return annotatedTargets.ToArray();
		}

		private static bool TryFixBenchmarkResource(
			AnnotateContext annotateContext, string xmlFileName,
			CompetitionTarget competitionTarget)
		{
			var xDoc = annotateContext.GetXmlAnnotation(xmlFileName);
			XmlAnnotations.AddOrUpdateCompetitionTarget(xDoc, competitionTarget);

			annotateContext.MarkAsChanged(xmlFileName);

			return true;
		}
	}
}