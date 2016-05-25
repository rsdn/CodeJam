using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running.Competitions.Core;
using BenchmarkDotNet.Running.Messages;

using JetBrains.Annotations;

namespace BenchmarkDotNet.Running.Competitions.SourceAnnotations
{
	// TODO: needs code review
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	internal static class XmlAnnotations
	{
		private class UseFullNameAnnotation
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
		private const string MinRatioAttribute = "MinRatio";
		private const string MaxRatioAttribute = "MaxRatio";
		#endregion

		#region Helpers
		[NotNull]
		// ReSharper disable once SuggestBaseTypeForParameter
		private static string GetCompetitionName(this Target target, XDocument document) =>
			document.Annotations<UseFullNameAnnotation>().Any()
				? target.Type.FullName + ", " + target.Type.Assembly.GetName().Name
				: target.Type.Name;

		[NotNull]
		private static string GetCandidateName(this Target target) =>
			target.Method.Name;

		// ReSharper disable once SuggestBaseTypeForParameter
		[NotNull]
		private static XElement GetOrAddElement(this XElement element, XName name, string targetName)
		{
			if (targetName == null)
				throw new ArgumentNullException(nameof(targetName));

			var result = element
				.Elements(name)
				.SingleOrDefault(e => e.Attribute(TargetAttribute)?.Value == targetName);

			if (result == null)
			{
				result = new XElement(name);
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
		#endregion

		// TODO: ConcurrentDictionary?
		private static readonly Dictionary<string, XDocument[]> _previousLogData = new Dictionary<string, XDocument[]>();

		public static XDocument[] GetDocumentsFromLog(string previousLogUri)
		{
			lock (_previousLogData)
			{
				XDocument[] result;
				if (!_previousLogData.TryGetValue(previousLogUri, out result))
				{
					result = GetDocumentsFromLogCore(previousLogUri);
					_previousLogData.Add(previousLogUri, result);
				}
				return result.ToArray();
			}
		}

		private static XDocument[] GetDocumentsFromLogCore(string previousLogUri)
		{
			var result = new List<XDocument>();

			using (var reader = BenchmarkHelpers.TryGetTextFromUri(previousLogUri))
			{
				if (reader == null)
					return new XDocument[0];

				var buffer = new StringBuilder();
				var append = false;
				string logLine;
				while ((logLine = reader.ReadLine()) != null)
				{
					if (logLine.StartsWith(LogAnnotationStart, StringComparison.OrdinalIgnoreCase))
					{
						append = true;
					}
					else if (logLine.StartsWith(LogAnnotationEnd, StringComparison.OrdinalIgnoreCase))
					{
						if (buffer.Length > 0)
						{
							var xDoc = TryParse(buffer.ToString(), true);
							if (xDoc != null)
							{
								result.Add(xDoc);
							}
						}
						buffer.Length = 0;
						append = false;
					}
					else if (append)
					{
						buffer.Append(logLine);
					}
				}
				if (buffer.Length > 0)
				{
					var xDoc = TryParse(buffer.ToString(), true);
					if (xDoc != null)
					{
						result.Add(xDoc);
					}
				}
			}

			return result.ToArray();
		}

		private static XDocument TryParse(string xDocText, bool useFullCompetitionName)
		{
			try
			{
				var result = XDocument.Parse(xDocText);
				if (useFullCompetitionName)
				{
					result.AddAnnotation(UseFullNameAnnotation.Instance);
				}
				return result;
			}
			catch (XmlException)
			{
				return null;
			}
		}

		private static XDocument CreateEmptyResourceDoc(bool useFullCompetitionName)
		{
			var result = new XDocument(new XElement(CompetitionBenchmarksRootNode));
			if (useFullCompetitionName)
			{
				result.AddAnnotation(UseFullNameAnnotation.Instance);
			}
			return result;
		}

		public static XDocument TryLoadResourceDoc(
			Type targetType, string targetResourceName, CompetitionState competitionState)
		{
			var resourceStream = targetType.Assembly.GetManifestResourceStream(targetResourceName);

			if (resourceStream == null)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"Benchmark {targetType.Name}: resource stream {targetResourceName} not found");

				return null;
			}
			XDocument resourceDoc;
			try
			{
				resourceDoc = XDocument.Load(resourceStream);
			}
			catch (XmlException ex)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"Benchmark {targetType.Name}: could not parse resource stream {targetResourceName} not found. Exception: {ex}.");
				return null;
			}

			var rootNode = resourceDoc.Element(CompetitionBenchmarksRootNode);
			if (rootNode == null)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"Resource {targetResourceName}: root node {CompetitionBenchmarksRootNode} not found.");

				return null;
			}

			return resourceDoc;
		}

		public static CompetitionTarget TryParseCompetitionTarget(XDocument resourceDoc, Target target, CompetitionState competitionState)
		{
			if (resourceDoc.Element(CompetitionBenchmarksRootNode) == null)
				return null;

			var competitionName = target.GetCompetitionName(resourceDoc);
			var candidateName = target.GetCandidateName();

			var matchingNodes =
				// ReSharper disable once PossibleNullReferenceException
				from competition in resourceDoc.Element(CompetitionBenchmarksRootNode).Elements(CompetitionNode)
				where competition.Attribute(TargetAttribute)?.Value == competitionName
				from candidate in competition.Elements(CandidateNode)
				where candidate.Attribute(TargetAttribute)?.Value == candidateName
				select candidate;

			var resultNode = matchingNodes.SingleOrDefault();
			if (resultNode == null)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Informational,
					$"Xml anotations for {candidateName}: no annotation exists.");

				return null;
			}

			var minText = resultNode.Attribute(MinRatioAttribute)?.Value;
			var maxText = resultNode.Attribute(MaxRatioAttribute)?.Value;

			double min;
			double max;
			var culture = EnvironmentInfo.MainCultureInfo;
			double.TryParse(minText, NumberStyles.Any, culture, out min);
			double.TryParse(maxText, NumberStyles.Any, culture, out max);

			// Only one attribute set
			if (minText == null && maxText != null)
			{
				min = CompetitionLimitConstants.IgnoreValue;
			}
			else if (maxText == null && minText != null)
			{
				max = CompetitionLimitConstants.IgnoreValue;
			}

			return new CompetitionTarget(target, min, max, true);
		}

		public static void AddOrUpdateCompetitionTarget(XDocument resourceDoc, CompetitionTarget competitionTarget)
		{
			var competitionName = competitionTarget.Target.GetCompetitionName(resourceDoc);
			var candidateName = competitionTarget.Target.GetCandidateName();

			var competition = resourceDoc.Element(CompetitionBenchmarksRootNode).GetOrAddElement(CompetitionNode, competitionName);
			var candidate = competition.GetOrAddElement(CandidateNode, candidateName);

			var minText = !competitionTarget.IgnoreMin ? competitionTarget.MinText : null;
			// MaxText should be specified even if ignored.
			var maxText = competitionTarget.MaxText;

			candidate.SetAttribute(MinRatioAttribute, minText);
			candidate.SetAttribute(MaxRatioAttribute, maxText);
		}

		private static XmlWriterSettings GetXmlWriterSettings() =>
			new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n"
			};

		private static StringBuilder SaveToString(XDocument resourceDoc)
		{
			var tmp = new StringWriter();
			Save(resourceDoc, tmp);
			return tmp.GetStringBuilder();
		}

		public static void Save(XDocument resourceDoc, TextWriter output)
		{
			var writerSettings = GetXmlWriterSettings();
			using (var writer = XmlWriter.Create(output, writerSettings))
			{
				resourceDoc.Save(writer);
			}
		}

		public static void LogAdjustedCompetitionTargets(
			IEnumerable<CompetitionTarget> competitionTargets,
			ILogger logger)
		{
			var updated = false;
			var xDoc = CreateEmptyResourceDoc(true);
			foreach (var competitionTarget in competitionTargets)
			{
				AddOrUpdateCompetitionTarget(xDoc, competitionTarget);
				updated = true;
			}

			if (updated)
			{
				logger.WriteLineInfo(LogAnnotationStart);

				var tmp = SaveToString(xDoc);
				logger.WriteLineInfo(tmp.ToString());

				logger.WriteLineInfo(LogAnnotationEnd);
			}
			else
			{
				logger.WriteLineInfo(
					"// No competition annotations were updated, nothing to export.");
			}
		}
	}
}