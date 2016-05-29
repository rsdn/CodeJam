using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Loggers;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	// TODO: needs code review
	/// <summary>
	/// Helper class for XML annotations
	/// </summary>
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	internal static class XmlAnnotations
	{
		private sealed class UseFullNameAnnotation
		{
			public static readonly UseFullNameAnnotation Instance = new UseFullNameAnnotation();

			private UseFullNameAnnotation() { }
		}

		#region XML metadata constants
		private const string LogAnnotationStart = HostLogger.LogImportantAreaStart +
			"------xml_annotation_begin------";

		private const string LogAnnotationEnd = HostLogger.LogImportantAreaEnd +
			"-------xml_annotation_end-------";

		private const string CompetitionBenchmarksRootNode = "CompetitionBenchmarks";
		private const string CompetitionNode = "Competition";
		private const string CandidateNode = "Candidate";
		private const string TargetAttribute = "Target";
		#endregion

		#region Core logic for XML annotations
		[NotNull]
		// ReSharper disable once SuggestBaseTypeForParameter
		private static string GetCompetitionName(this Target target, XDocument benchmarksDoc) =>
			benchmarksDoc.Annotations<UseFullNameAnnotation>().Any()
				? target.Type.FullName + ", " + target.Type.Assembly.GetName().Name
				: target.Type.Name;

		[NotNull]
		private static string GetCandidateName(this Target target) =>
			target.Method.Name;

		[NotNull]
		// ReSharper disable once SuggestBaseTypeForParameter
		private static XElement GetOrAddElement(this XElement element, XName elementName, string targetName)
		{
			if (targetName == null)
				throw new ArgumentNullException(nameof(targetName));

			var result = element
				.Elements(elementName)
				.SingleOrDefault(e => e.Attribute(TargetAttribute)?.Value == targetName);

			if (result == null)
			{
				result = new XElement(elementName);
				result.SetAttribute(TargetAttribute, targetName);
				element.Add(result);
			}

			return result;
		}

		[CanBeNull]
		// ReSharper disable once UnusedMethodReturnValue.Local
		private static XAttribute SetAttribute(this XElement element, XName attributeName, string attributeValue)
		{
			if (attributeValue == null)
			{
				element.Attribute(attributeName)?.Remove();
				return null;
			}

			var result = element.Attribute(attributeName);

			if (result == null)
			{
				result = new XAttribute(attributeName, attributeValue);
				element.Add(result);
			}
			else
			{
				result.Value = attributeValue;
			}

			return result;
		}

		private static XmlWriterSettings GetXmlWriterSettings(bool omitXmlDeclaration = false) =>
			new XmlWriterSettings
			{
				OmitXmlDeclaration = omitXmlDeclaration,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n"
			};

		private static XDocument CreateEmptyBenchmarksDoc(bool useFullCompetitionName)
		{
			var benchmarksDoc = new XDocument(new XElement(CompetitionBenchmarksRootNode));
			if (useFullCompetitionName)
			{
				benchmarksDoc.AddAnnotation(UseFullNameAnnotation.Instance);
			}
			return benchmarksDoc;
		}
		#endregion

		[CanBeNull]
		private static XDocument TryParseBenchmarksDoc(
			TextReader source,
			CompetitionState competitionState,
			bool useFullCompetitionName,
			string origin)
		{
			XDocument benchmarksDoc;
			try
			{
				benchmarksDoc = XDocument.Load(source);
				if (useFullCompetitionName)
				{
					benchmarksDoc.AddAnnotation(UseFullNameAnnotation.Instance);
				}
			}
			catch (ArgumentException ex)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"{origin}: Could not parse annotation from log. Exception: {ex}.");
				return null;
			}
			catch (XmlException ex)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"{origin}: Could not parse annotation from log. Exception: {ex}.");
				return null;
			}

			var rootNode = benchmarksDoc.Element(CompetitionBenchmarksRootNode);
			if (rootNode == null)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"origin: root node {CompetitionBenchmarksRootNode} not found.");

				return null;
			}

			return benchmarksDoc;
		}

		#region Logging & xml annotations
		public static void LogCompetitionTargets(
			[NotNull] IEnumerable<CompetitionTarget> competitionTargets,
			[NotNull] CompetitionState competitionState)
		{
			Code.NotNull(competitionTargets, nameof(competitionTargets));
			Code.NotNull(competitionState, nameof(competitionState));

			var hasData = false;
			var benchmarksDoc = CreateEmptyBenchmarksDoc(true);
			foreach (var competitionTarget in competitionTargets)
			{
				AddOrUpdateCompetitionTarget(benchmarksDoc, competitionTarget);
				hasData = true;
			}

			var logger = competitionState.Logger;
			if (hasData)
			{
				logger.WriteLineInfo(LogAnnotationStart);

				var tmp = new StringBuilder();
				using (var writer = XmlWriter.Create(tmp, GetXmlWriterSettings(true)))
				{
					benchmarksDoc.Save(writer);
				}
				logger.WriteLineInfo(tmp.ToString());

				logger.WriteLineInfo(LogAnnotationEnd);
			}
			else
			{
				logger.WriteLineInfo(
					"// No competition annotations were updated, nothing to export.");
			}
		}

		public static XDocument[] TryParseBenchmarkDocsFromLog(
			[NotNull] string logUri,
			[NotNull] CompetitionState competitionState)
		{
			Code.NotNullNorEmpty(logUri, nameof(logUri));
			Code.NotNull(competitionState, nameof(competitionState));

			var result = new List<XDocument>();

			using (var reader = BenchmarkHelpers.TryGetTextFromUri(logUri))
			{
				if (reader == null)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Could not load log from {logUri}.");
					return null;
				}

				var buffer = new StringBuilder();
				string logLine;
				int lineNumber = 0, xmlStartLineNumber = -1;
				while ((logLine = reader.ReadLine()) != null)
				{
					lineNumber++;

					if (logLine.StartsWith(LogAnnotationStart, StringComparison.OrdinalIgnoreCase))
					{
						xmlStartLineNumber = lineNumber;
					}
					else if (logLine.StartsWith(LogAnnotationEnd, StringComparison.OrdinalIgnoreCase))
					{
						if (buffer.Length > 0)
						{
							var benchmarksDoc = TryParseBenchmarksDocFromLogCore(
								buffer.ToString(), competitionState, logUri, xmlStartLineNumber);
							if (benchmarksDoc != null)
							{
								result.Add(benchmarksDoc);
							}
						}
						buffer.Length = 0;

						xmlStartLineNumber = -1;
					}
					else if (xmlStartLineNumber >= 0)
					{
						buffer.Append(logLine);
					}
				}

				if (buffer.Length > 0)
				{
					var xDoc = TryParseBenchmarksDocFromLogCore(buffer.ToString(), competitionState, logUri, xmlStartLineNumber);
					if (xDoc != null)
					{
						result.Add(xDoc);
					}
				}
			}

			return result.ToArray();
		}

		private static XDocument TryParseBenchmarksDocFromLogCore(
			string logXmlText,
			CompetitionState competitionState,
			string logUri, int logLine)
		{
			using (var reader = new StringReader(logXmlText))
			{
				return TryParseBenchmarksDoc(
					reader, competitionState, true,
					$"Log {logUri}, line {logLine}");
			}
		}
		#endregion

		public static XDocument TryParseBenchmarksDocFromResource(
			[NotNull] Target target,
			[NotNull] string targetResourceName,
			[NotNull] CompetitionState competitionState)
		{
			Code.NotNull(target, nameof(target));
			Code.NotNullNorEmpty(targetResourceName, nameof(targetResourceName));
			Code.NotNull(competitionState, nameof(competitionState));

			using (var resourceStream = target.Type.Assembly.GetManifestResourceStream(targetResourceName))
			{
				if (resourceStream == null)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Benchmark {target.Type.Name}: resource stream {targetResourceName} not found");

					return null;
				}

				using (var reader = new StreamReader(resourceStream))
				{
					return TryParseBenchmarksDoc(
						reader, competitionState, false,
						$"Resource {targetResourceName}");
				}
			}
		}

		public static CompetitionTarget TryParseCompetitionTarget(
			[NotNull] Target target,
			[NotNull] XDocument benchmarksDoc,
			[NotNull] CompetitionState competitionState)
		{
			Code.NotNull(target, nameof(target));
			Code.NotNull(benchmarksDoc, nameof(benchmarksDoc));
			Code.NotNull(competitionState, nameof(competitionState));

			var competitionName = target.GetCompetitionName(benchmarksDoc);
			var candidateName = target.GetCandidateName();

			var matchingNodes =
				// ReSharper disable once PossibleNullReferenceException
				from competition in benchmarksDoc.Element(CompetitionBenchmarksRootNode).Elements(CompetitionNode)
				where competition.Attribute(TargetAttribute)?.Value == competitionName
				from candidate in competition.Elements(CandidateNode)
				where candidate.Attribute(TargetAttribute)?.Value == candidateName
				select candidate;

			var competitionNode = matchingNodes.SingleOrDefault();
			if (competitionNode == null)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"Xml anotations for {competitionName}.{candidateName}: no annotation exists.");

				return null;
			}

			var minRatio = TryParseLimitValue(
				target, competitionNode,
				CompetitionLimitProperties.MinRatio,
				competitionState);
			var maxRatio = TryParseLimitValue(
				target, competitionNode,
				CompetitionLimitProperties.MaxRatio,
				competitionState);

			if (minRatio == null && maxRatio != null)
			{
				minRatio = CompetitionLimit.IgnoreValue;
			}
			else if (maxRatio == null && minRatio != null)
			{
				maxRatio = CompetitionLimit.IgnoreValue;
			}

			return new CompetitionTarget(target, minRatio ?? 0, maxRatio ?? 0, true);
		}

		private static double? TryParseLimitValue(
			Target target, XElement competitionNode,
			CompetitionLimitProperties limitProperty,
			CompetitionState competitionState)
		{
			double? result = 0;

			var limitText = competitionNode.Attribute(limitProperty.ToString())?.Value;
			if (limitText != null)
			{
				var culture = EnvironmentInfo.MainCultureInfo;
				double parsed;
				if (double.TryParse(limitText, NumberStyles.Any, culture, out parsed))
				{
					result = parsed;
				}
				else
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.Informational,
						$"Xml anotations for {target.Type.Name}.{target.Method.Name}: could not parse {limitProperty}.");
				}
			}

			return result;
		}

		public static void AddOrUpdateCompetitionTarget(XDocument benchmarksDoc, CompetitionTarget competitionTarget)
		{
			var competitionName = competitionTarget.Target.GetCompetitionName(benchmarksDoc);
			var candidateName = competitionTarget.Target.GetCandidateName();

			var competition = benchmarksDoc
				.Element(CompetitionBenchmarksRootNode)
				.GetOrAddElement(CompetitionNode, competitionName);
			var candidate = competition.GetOrAddElement(CandidateNode, candidateName);

			var minText = !competitionTarget.IgnoreMinRatio ? competitionTarget.MinRatioText : null;
			// MaxText should be specified even if ignored.
			var maxText = competitionTarget.MaxRatioText;

			candidate.SetAttribute(CompetitionLimitProperties.MinRatio.ToString(), minText);
			candidate.SetAttribute(CompetitionLimitProperties.MaxRatio.ToString(), maxText);
		}

		public static void Save(XDocument benchmarksDoc, TextWriter output)
		{
			var writerSettings = GetXmlWriterSettings();
			using (var writer = XmlWriter.Create(output, writerSettings))
			{
				benchmarksDoc.Save(writer);
			}
		}
	}
}