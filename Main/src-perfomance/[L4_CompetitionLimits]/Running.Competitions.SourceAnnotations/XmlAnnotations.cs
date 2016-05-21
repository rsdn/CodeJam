using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Competitions;
using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running.Competitions.Core;
using BenchmarkDotNet.Running.Messages;

using JetBrains.Annotations;

using static BenchmarkDotNet.Competitions.CompetitionLimitConstants;

namespace BenchmarkDotNet.Running.Competitions.SourceAnnotations
{
	internal static class XmlAnnotations
	{
		private class UseFullNameAnnotation
		{
			public static readonly UseFullNameAnnotation Instance = new UseFullNameAnnotation();
			private UseFullNameAnnotation() { }
		}

		#region XML metadata constants
		public const string LogAnnotationStart = HostLogger.LogImportantAreaStart + "------xml_annotation_begin------";
		public const string LogAnnotationEnd = HostLogger.LogImportantAreaEnd + "------xml_annotation_end------";

		private const string CompetitionBenchmarksRootNode = "CompetitionBenchmarks";
		private const string CompetitionNode = "Competition";
		private const string CandidateNode = "Candidate";
		private const string TargetAttribute = "Target";
		private const string MinRatioAttribute = "MinRatio";
		private const string MaxRatioAttribute = "MaxRatio";
		#endregion

		#region Helpers
		[NotNull]
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

			var buffer = new StringBuilder();
			bool append = false;
			foreach (var logLine in File.ReadLines(previousLogUri))
			{
				if (logLine.StartsWith(LogAnnotationStart, StringComparison.OrdinalIgnoreCase))
				{
					append = true;
				}
				else if (logLine.StartsWith(LogAnnotationEnd, StringComparison.OrdinalIgnoreCase))
				{
					if (buffer.Length > 0)
					{
						result.Add(Parse(buffer, true));
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
				result.Add(Parse(buffer, true));
			}

			return result.ToArray();
		}

		private static XDocument Parse(StringBuilder buffer, bool competitionFullName)
		{
			var result = XDocument.Parse(buffer.ToString());
			if (competitionFullName)
			{
				result.AddAnnotation(UseFullNameAnnotation.Instance);
			}
			return result;
		}

		public static XDocument CreateEmptyResourceDoc(bool competitionFullName)
		{
			var result = new XDocument(new XElement(CompetitionBenchmarksRootNode));
			if (competitionFullName)
			{
				result.AddAnnotation(UseFullNameAnnotation.Instance);
			}
			return result;
		}

		public static XDocument TryLoadResourceDoc(
			Target target, string targetResourceName, List<IWarning> warnings)
		{
			var resourceStream = target.Type.Assembly.GetManifestResourceStream(targetResourceName);

			if (resourceStream == null)
			{
				warnings.AddWarning(
					MessageSeverity.SetupError,
					$"Method {target.Method.Name}: resource stream {targetResourceName} not found");

				return null;
			}

			var resourceDoc = XDocument.Load(resourceStream);
			var rootNode = resourceDoc.Element(CompetitionBenchmarksRootNode);
			if (rootNode == null)
			{
				warnings.AddWarning(
					MessageSeverity.SetupError,
					$"Resource {targetResourceName}: root node {CompetitionBenchmarksRootNode} not found.");

				return null;
			}

			return resourceDoc;
		}

		public static CompetitionTarget TryParseCompetitionTarget(
			Target target, XDocument resourceDoc)
		{
			var competitionName = target.GetCompetitionName(resourceDoc);
			var candidateName = target.GetCandidateName();

			var matchingNodes =
				// ReSharper disable once PossibleNullReferenceException
				from competition in resourceDoc.Root.Elements(CompetitionNode)
				where competition.Attribute(TargetAttribute)?.Value == competitionName
				from candidate in competition.Elements(CandidateNode)
				where candidate.Attribute(TargetAttribute)?.Value == candidateName
				select candidate;

			var resultNode = matchingNodes.SingleOrDefault();
			if (resultNode == null)
			{
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
				min = IgnoreValue;
			}
			else if (maxText == null && minText != null)
			{
				max = IgnoreValue;
			}

			return new CompetitionTarget(target, min, max, true);
		}

		public static void SaveCompetitionTarget(
			CompetitionTarget competitionTarget,
			XDocument resourceDoc)
		{
			var competitionName = competitionTarget.Target.GetCompetitionName(resourceDoc);
			var candidateName = competitionTarget.Target.GetCandidateName();

			var competition = resourceDoc.Root.GetOrAddElement(CompetitionNode, competitionName);
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

		public static StringBuilder SaveToString(XDocument resourceDoc)
		{
			var tmp = new StringWriter();
			SaveTo(tmp, resourceDoc);
			return tmp.GetStringBuilder();
		}

		public static void SaveTo(TextWriter output, XDocument resourceDoc)
		{
			var writerSettings = GetXmlWriterSettings();
			using (var writer = XmlWriter.Create(output, writerSettings))
			{
				resourceDoc.Save(writer);
			}
		}
	}

}