using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Strings;

using JetBrains.Annotations;

using static CodeJam.PerfTests.Loggers.HostLogger;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Helper class for xml annotations
	/// </summary>
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	internal static class XmlAnnotations
	{
		#region XML metadata constants
		private const string LogAnnotationStart = LogImportantAreaStart +
			" ------ xml_annotation_begin ------";

		private const string LogAnnotationEnd = LogImportantAreaEnd +
			" ------- xml_annotation_end -------";

		private const string CompetitionBenchmarksRootNode = "CompetitionBenchmarks";
		private const string CompetitionNode = "Competition";
		private const string TargetAttribute = "Target";
		private const string BaselineAttribute = "Baseline";
		private const string MinAttribute = "Min";
		private const string MaxAttribute = "Max";
		private const string UnitAttribute = "Unit";
		#endregion

		#region XML doc loading / saving

		#region Core logic for XML annotations
		[NotNull]
		private static XmlReaderSettings GetXmlReaderSettings() =>
			new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Prohibit
			};

		[NotNull]
		private static XmlWriterSettings GetXmlWriterSettings(bool omitXmlDeclaration = false) =>
			new XmlWriterSettings
			{
				OmitXmlDeclaration = omitXmlDeclaration,
				Indent = true,
				IndentChars = "\t",
				NewLineChars = "\r\n"
			};

		[NotNull]
		// ReSharper disable once SuggestBaseTypeForParameter
		private static string GetTargetTypeName(this Target target, bool useFullTypeName) =>
			useFullTypeName
				? target.Type.GetShortAssemblyQualifiedName()
				: target.Type.Name;

		[NotNull]
		private static string GetTargetMethodName(this Target target) =>
			target.Method.Name;

		[NotNull]
		// ReSharper disable once SuggestBaseTypeForParameter
		private static XElement GetOrAddElement(this XElement parent, XName elementName, string targetName)
		{
			Code.NotNull(elementName, nameof(elementName));
			Code.NotNull(parent, nameof(parent));

			var result = parent
				.Elements(elementName)
				.SingleOrDefault(e => e.Attribute(TargetAttribute)?.Value == targetName);

			if (result == null)
			{
				result = new XElement(elementName);
				result.SetAttribute(TargetAttribute, targetName);
				parent.Add(result);
			}

			return result;
		}

		[NotNull]
		// ReSharper disable once SuggestBaseTypeForParameter
		private static XElement GetOrAddElement(this XElement parent, XName elementName)
		{
			Code.NotNull(elementName, nameof(elementName));

			var result = parent.Element(elementName);
			if (result == null)
			{
				result = new XElement(elementName);
				parent.Add(result);
			}

			return result;
		}

		[CanBeNull]
		// ReSharper disable once UnusedMethodReturnValue.Local
		private static XAttribute SetAttribute(this XElement parent, XName attributeName, object attributeValue)
		{
			if (attributeValue == null)
			{
				parent.Attribute(attributeName)?.Remove();
				return null;
			}

			var result = parent.Attribute(attributeName);

			if (result == null)
			{
				result = new XAttribute(attributeName, attributeValue);
				parent.Add(result);
			}
			else
			{
				result.SetValue(attributeValue);
			}

			return result;
		}
		#endregion

		#region XML annotations
		/// <summary>Parses xml annotation document.</summary>
		/// <param name="source">Stream that contains xml document.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <param name="sourceDescription">Source description to be used in messages.</param>
		/// <returns>XML annotation document or <c>null</c> if parsing failed.</returns>
		[CanBeNull]
		public static XDocument TryParseXmlAnnotationDoc(
			[NotNull] Stream source,
			[NotNull] CompetitionState competitionState,
			[NotNull] string sourceDescription)
		{
			Code.NotNull(source, nameof(source));
			Code.NotNull(competitionState, nameof(competitionState));
			Code.NotNullNorEmpty(sourceDescription, nameof(sourceDescription));

			XDocument xmlAnnotationDoc;
			try
			{
				using (var reader = XmlReader.Create(source, GetXmlReaderSettings()))
				{
					xmlAnnotationDoc = XDocument.Load(reader);
				}
			}
			catch (ArgumentException ex)
			{
				competitionState.WriteExceptionMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"{sourceDescription}: Could not parse annotation.", ex);
				return null;
			}
			catch (XmlException ex)
			{
				competitionState.WriteExceptionMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"{sourceDescription}: Could not parse annotation.", ex);
				return null;
			}

			var rootNode = xmlAnnotationDoc.Element(CompetitionBenchmarksRootNode);
			if (rootNode == null)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"{sourceDescription}: root node {CompetitionBenchmarksRootNode} not found.");

				return null;
			}

			return xmlAnnotationDoc;
		}

		/// <summary>Parses xml annotation document from the resource.</summary>
		/// <param name="annotationResourceKey">Name of the xml resource with competition limits.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <returns>XML annotation document or <c>null</c> if parsing failed.</returns>
		[CanBeNull]
		public static XDocument TryParseXmlAnnotationDoc(
			ResourceKey annotationResourceKey,
			[NotNull] CompetitionState competitionState)
		{
			Code.NotNull(competitionState, nameof(competitionState));
			var assembly = annotationResourceKey.Assembly;
			var resourceName = annotationResourceKey.ResourceName;

			using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
			{
				if (resourceStream == null)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"XML annotation resource stream '{resourceName}' not found. Assembly {assembly.FullName}.");

					return null;
				}

				return TryParseXmlAnnotationDoc(resourceStream, competitionState, $"Resource '{resourceName}'");
			}
		}

		/// <summary>Parses xml annotation documents from the log.</summary>
		/// <param name="logUri">The URI the log will be obtained from.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <returns>Parsed xml annotation documents, if any or <c>null</c> if parsing failed.</returns>
		[CanBeNull]
		public static XDocument[] TryParseXmlAnnotationDocsFromLog(
			[NotNull] string logUri,
			[NotNull] CompetitionState competitionState)
		{
			Code.NotNullNorEmpty(logUri, nameof(logUri));
			Code.NotNull(competitionState, nameof(competitionState));

			competitionState.WriteVerbose($"Downloading '{logUri}'.");

			using (var reader = BenchmarkHelpers.TryGetTextFromUri(logUri, TimeSpan.FromSeconds(15)))
			{
				if (reader == null)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.Warning,
						$"Could not load log content from '{logUri}'.");
					return null;
				}

				competitionState.WriteVerbose($"Parsing '{logUri}' content.");

				return ParseLogContent(logUri, reader, competitionState);
			}
		}

		private static XDocument[] ParseLogContent(
			string logUri, TextReader reader,
			CompetitionState competitionState)
		{
			var result = new List<XDocument>();

			var buffer = new StringBuilder();
			int lineNumber = 0, xmlStartLineNumber = -1;
			string logLine;
			while ((logLine = reader.ReadLine()) != null)
			{
				lineNumber++;

				if (logLine.StartsWith(LogAnnotationStart, StringComparison.OrdinalIgnoreCase))
				{
					if (xmlStartLineNumber >= 0)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Warning,
							$"The log is damaged, annotation area start tag was repeated twice. Uri '{logUri}', lines {xmlStartLineNumber} and {lineNumber}.");
					}
					xmlStartLineNumber = lineNumber;
				}
				else if (logLine.StartsWith(LogAnnotationEnd, StringComparison.OrdinalIgnoreCase))
				{
					if (xmlStartLineNumber < 0)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Warning,
							$"The log is damaged, annotation area start tag is missing. Uri '{logUri}', start tag at line {lineNumber}.");
					}
					else if (buffer.Length > 0)
					{
						var xmlAnnotationDoc = TryParseLogContentDoc(
							buffer.ToString(), competitionState, logUri, xmlStartLineNumber);
						if (xmlAnnotationDoc != null)
						{
							result.Add(xmlAnnotationDoc);
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

			if (xmlStartLineNumber >= 0)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Warning,
					$"The log is damaged, annotation area end tag is missing. Uri '{logUri}', start tag at line {xmlStartLineNumber}.");
			}

			return result.ToArray();
		}

		[CanBeNull]
		private static XDocument TryParseLogContentDoc(
			string logXmlText,
			CompetitionState competitionState,
			string logUri, int logLine)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(logXmlText)))
			{
				var xmlAnnotationDoc = TryParseXmlAnnotationDoc(
					stream,
					competitionState,
					$"Log '{logUri}', line {logLine}");

				return xmlAnnotationDoc;
			}
		}

		/// <summary>Writes xml annotation document for the competition targets to the log.</summary>
		/// <param name="competitionTargets">The competition targets to log.</param>
		/// <param name="competitionState">State of the run.</param>
		public static void LogXmlAnnotationDoc(
			[NotNull] IReadOnlyCollection<CompetitionTarget> competitionTargets,
			[NotNull] CompetitionState competitionState)
		{
			Code.NotNull(competitionTargets, nameof(competitionTargets));
			Code.NotNull(competitionState, nameof(competitionState));

			if (competitionTargets.Count == 0)
				return;

			// Create xml annotation doc
			var xmlAnnotationDoc = new XDocument(new XElement(CompetitionBenchmarksRootNode));
			foreach (var competitionTarget in competitionTargets)
			{
				AddOrUpdateXmlAnnotation(xmlAnnotationDoc, competitionTarget, true);
			}

			// Dump it
			var tmp = new StringBuilder();
			using (var writer = XmlWriter.Create(tmp, GetXmlWriterSettings(true)))
			{
				xmlAnnotationDoc.Save(writer);
			}

			var logger = competitionState.Logger;
			using (BeginLogImportant(competitionState.Config))
			{
				logger.WriteLineInfo(LogAnnotationStart);
				logger.WriteLineInfo(tmp.ToString());
				logger.WriteLineInfo(LogAnnotationEnd);
			}
		}
		#endregion

		/// <summary>Saves the specified xml annotation document.</summary>
		/// <param name="xmlAnnotationDoc">The xml annotation document.</param>
		/// <param name="output">The writer the document will be saved to.</param>
		public static void Save(
			[NotNull] XDocument xmlAnnotationDoc,
			[NotNull] TextWriter output)
		{
			Code.NotNull(xmlAnnotationDoc, nameof(xmlAnnotationDoc));
			Code.NotNull(output, nameof(output));

			using (var writer = XmlWriter.Create(output, GetXmlWriterSettings()))
			{
				xmlAnnotationDoc.Save(writer);
			}
		}
		#endregion

		#region Competition metrics-related
		#region Parse
		/// <summary>Parses metrics for target from the the xml annotation document.</summary>
		/// <param name="target">The target.</param>
		/// <param name="xmlAnnotationDoc">The xml annotation document.</param>
		/// <param name="useFullTypeName">Use full type name in XML annotations.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <returns>Parsed metrics for target.</returns>
		[NotNull]
		public static IStoredMetricSource[] TryParseMetrics(
			[NotNull] Target target,
			[NotNull] XDocument xmlAnnotationDoc,
			bool useFullTypeName,
			[NotNull] CompetitionState competitionState)
		{
			Code.NotNull(target, nameof(target));
			Code.NotNull(xmlAnnotationDoc, nameof(xmlAnnotationDoc));
			Code.NotNull(competitionState, nameof(competitionState));

			var targetTypeName = target.GetTargetTypeName(useFullTypeName);
			var targetMethodName = target.GetTargetMethodName();

			var matchingNodes =
				// ReSharper disable once PossibleNullReferenceException
				from competition in xmlAnnotationDoc.Element(CompetitionBenchmarksRootNode).Elements(CompetitionNode)
				where competition.Attribute(TargetAttribute)?.Value == targetTypeName
				from candidate in competition.Elements(targetMethodName)
				select candidate;

			var targetNode = matchingNodes.SingleOrDefault();
			if (targetNode == null)
			{
				return Array<IStoredMetricSource>.Empty;
			}

			var baseline = TryParseBooleanValue(target, targetNode, BaselineAttribute, competitionState);
			if (baseline.GetValueOrDefault() != target.Baseline)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"XML annotation for {target.MethodDisplayInfo}: baseline flag on the method and in the annotation do not match.");
				return Array<IStoredMetricSource>.Empty;
			}

			var metricsByName = competitionState.Config.GetMetrics().ToDictionary(m => m.Name);
			var primaryMetric = AttributeAnnotations.PrimaryMetric;
			var result = new List<IStoredMetricSource>(metricsByName.Count);
			if (metricsByName.TryGetValue(primaryMetric.Name, out var metricInfo))
			{
				result.Add(
					ParseStoredMetric(target, targetNode, metricInfo, competitionState));
			}

			foreach (var metricNode in targetNode.Elements())
			{
				if (!metricsByName.TryGetValue(metricNode.Name.LocalName, out metricInfo))
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.Informational,
						$"XML annotation for {target.MethodDisplayInfo}, unknown metric {metricNode.Name}, skipped.");
					continue;
				}

				result.Add(
					ParseStoredMetric(target, metricNode, metricInfo, competitionState));
			}
			return result.ToArray();
		}

		private static IStoredMetricSource ParseStoredMetric(
			Target target, XElement targetNode,
			CompetitionMetricInfo targetMetric,
			CompetitionState competitionState)
		{
			var min = TryParseDoubleValue(target, targetNode, MinAttribute, competitionState);
			var max = TryParseDoubleValue(target, targetNode, MaxAttribute, competitionState);
			var unitName = targetNode.Attribute(UnitAttribute)?.Value;

			if (max == null)
			{
				max = double.NaN;
				min = double.NaN;
			}
			else if (min == null)
			{
				min = max.Value.GetMinMetricValue(targetMetric);
			}

			Enum unitValue = AttributeAnnotations.ParseUnitValue(
				target, unitName, targetMetric, competitionState).EnumValue;
			if (unitName.NotNullNorEmpty())
			{
				unitValue = targetMetric.MetricUnits[unitName].EnumValue;

				if (unitValue == null)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"XML annotation for {target.MethodDisplayInfo}, metric {targetMetric}: could not parse unit value {unitName}.");
				}
			}

			return new StoredMetricValue(targetMetric.AttributeType, min.Value, max.Value, unitValue);
		}

		// ReSharper disable ConvertClosureToMethodGroup
		private static double? TryParseDoubleValue(
			Target target, XElement competitionNode, string limitProperty, CompetitionState competitionState) =>
				TryParseCore(
					target, competitionNode,
					limitProperty, competitionState,
					s => XmlConvert.ToDouble(s));

		private static bool? TryParseBooleanValue(
			Target target, XElement competitionNode, string limitProperty, CompetitionState competitionState) =>
				TryParseCore(
					target, competitionNode,
					limitProperty, competitionState,
					s => XmlConvert.ToBoolean(s));
		// ReSharper restore ConvertClosureToMethodGroup

		private static T? TryParseCore<T>(
			Target target, XElement competitionNode,
			string limitProperty,
			CompetitionState competitionState,
			Func<string, T> parseCallback)
			where T : struct
		{
			T? result = null;

			var limitText = competitionNode.Attribute(limitProperty)?.Value;
			if (limitText != null)
			{
				try
				{
					result = parseCallback(limitText);
				}
				catch (FormatException ex)
				{
					competitionState.WriteExceptionMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"XML annotation for {target.MethodDisplayInfo}: could not parse {limitProperty}.",
						ex);
				}
			}

			return result;
		}
		#endregion

		/// <summary>Adds or updates xml annotation for the competition target.</summary>
		/// <param name="xmlAnnotationDoc">The xml annotation document that will be updated.</param>
		/// <param name="competitionTarget">The competition target.</param>
		/// <param name="useFullTypeName">Use full type name in XML annotations.</param>
		public static void AddOrUpdateXmlAnnotation(
			[NotNull] XDocument xmlAnnotationDoc,
			[NotNull] CompetitionTarget competitionTarget,
			bool useFullTypeName)
		{
			Code.NotNull(xmlAnnotationDoc, nameof(xmlAnnotationDoc));
			Code.NotNull(competitionTarget, nameof(competitionTarget));

			var targetTypeName = competitionTarget.Target.GetTargetTypeName(useFullTypeName);
			var targetMethodName = competitionTarget.Target.GetTargetMethodName();
			var isBaseline = competitionTarget.Baseline;

			var competitionNode = xmlAnnotationDoc
				.Element(CompetitionBenchmarksRootNode)
				.GetOrAddElement(CompetitionNode, targetTypeName);

			var targetNode = competitionNode.GetOrAddElement(targetMethodName);
			bool forceUpdate = !targetNode.HasElements;

			var baselineText = isBaseline ? XmlConvert.ToString(true) : null;
			targetNode.SetAttribute(BaselineAttribute, baselineText);

			var primaryMetric = AttributeAnnotations.PrimaryMetric;
			var targetMetricValue = competitionTarget.MetricValues.FirstOrDefault(f => f.Metric == primaryMetric);
			UpdateStoredMetric(targetNode, targetMetricValue, forceUpdate);

			foreach (var metricValue in competitionTarget.MetricValues.Where(m => m.Metric != primaryMetric))
			{
				var metricNode = GetOrAddElement(targetNode, metricValue.Metric.Name);
				UpdateStoredMetric(metricNode, metricValue, forceUpdate);
			}
		}

		private static void UpdateStoredMetric(
			[NotNull] XElement targetNode, [CanBeNull] CompetitionMetricValue metricValue, bool forceUpdate)
		{
			if (metricValue == null || metricValue.ValuesRange.IsEmpty)
			{
				targetNode.SetAttribute(MinAttribute, null);
				targetNode.SetAttribute(MaxAttribute, null);
				targetNode.SetAttribute(UnitAttribute, null);
			}
			else if (forceUpdate || metricValue.HasUnsavedChanges || !targetNode.HasAttributes)
			{
				var metricRange = metricValue.ValuesRange;
				metricValue.ValuesRange.GetStringMinMax(metricValue.DisplayMetricUnit, out var minValueText, out var maxValueText);
				if (double.IsInfinity(metricRange.Min))
					minValueText = XmlConvert.ToString(metricRange.Min);
				if (double.IsInfinity(metricRange.Max))
					maxValueText = XmlConvert.ToString(metricRange.Max);

				var unit = metricValue.DisplayMetricUnit;
				targetNode.SetAttribute(MinAttribute, minValueText);
				targetNode.SetAttribute(MaxAttribute, maxValueText);
				targetNode.SetAttribute(UnitAttribute, unit.IsEmpty ? null : unit.Name);
			}
		}
	}
	#endregion
}