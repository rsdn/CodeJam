using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Reflection;
using CodeJam.Strings;

using JetBrains.Annotations;

using static CodeJam.PerfTests.Loggers.FilteringLogger;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Helper class for xml annotations
	/// </summary>
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	internal static class XmlAnnotationHelpers
	{
		#region XML doc constants
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

		#region XML doc helpers
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
		#endregion

		#region XML doc loading
		/// <summary>Parses xml annotation document.</summary>
		/// <param name="source">Stream that contains xml document.</param>
		/// <param name="sourceDescription">Source description to be used in messages.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>XML annotation document or <c>null</c> if parsing failed.</returns>
		[CanBeNull]
		public static XDocument TryParseXmlAnnotationDoc(
			[NotNull] TextReader source,
			[NotNull] string sourceDescription,
			[NotNull] IMessageLogger messageLogger)
		{
			Code.NotNull(source, nameof(source));

			return TryParseXmlAnnotationDocCore(
				() => XmlReader.Create(source, GetXmlReaderSettings()),
				sourceDescription,
				messageLogger);
		}

		/// <summary>Parses xml annotation document.</summary>
		/// <param name="source">Stream that contains xml document.</param>
		/// <param name="sourceDescription">Source description to be used in messages.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>XML annotation document or <c>null</c> if parsing failed.</returns>
		[CanBeNull]
		public static XDocument TryParseXmlAnnotationDoc(
			[NotNull] Stream source,
			[NotNull] string sourceDescription,
			[NotNull] IMessageLogger messageLogger)
		{
			Code.NotNull(source, nameof(source));

			return TryParseXmlAnnotationDocCore(
				() => XmlReader.Create(source, GetXmlReaderSettings()),
				sourceDescription,
				messageLogger);
		}

		/// <summary>Parses xml annotation document from the resource.</summary>
		/// <param name="annotationResourceKey">Name of the xml resource with competition limits.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>XML annotation document or <c>null</c> if parsing failed.</returns>
		[CanBeNull]
		public static XDocument TryParseXmlAnnotationDoc(
			ResourceKey annotationResourceKey,
			[NotNull] IMessageLogger messageLogger)
		{
			Code.NotNull(messageLogger, nameof(messageLogger));
			var assembly = annotationResourceKey.Assembly;
			var resourceName = annotationResourceKey.ResourceName;

			using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
			{
				if (resourceStream == null)
				{
					messageLogger.WriteSetupErrorMessage(
						$"XML annotation resource stream '{resourceName}' not found. Assembly {assembly.FullName}.");

					return null;
				}

				return TryParseXmlAnnotationDoc(resourceStream, $"Resource '{resourceName}'", messageLogger);
			}
		}

		[CanBeNull]
		private static XDocument TryParseXmlAnnotationDocCore(
			[NotNull] Func<XmlReader> readerFactory,
			[NotNull] string sourceDescription,
			[NotNull] IMessageLogger messageLogger)
		{
			Code.NotNull(messageLogger, nameof(messageLogger));
			Code.NotNullNorEmpty(sourceDescription, nameof(sourceDescription));

			XDocument xmlAnnotationDoc;
			using (var reader = readerFactory())
			{
				try
				{
					xmlAnnotationDoc = XDocument.Load(reader);
				}
				catch (ArgumentException ex)
				{
					messageLogger.WriteExceptionMessage(
						MessageSeverity.SetupError,
						$"{sourceDescription}: Could not parse annotation.", ex);
					return null;
				}
				catch (XmlException ex)
				{
					messageLogger.WriteExceptionMessage(
						MessageSeverity.SetupError,
						$"{sourceDescription}: Could not parse annotation.", ex);
					return null;
				}
			}
			var rootNode = xmlAnnotationDoc.Element(CompetitionBenchmarksRootNode);
			if (rootNode == null)
			{
				messageLogger.WriteSetupErrorMessage(
					$"{sourceDescription}: root node {CompetitionBenchmarksRootNode} not found.");

				return null;
			}

			return xmlAnnotationDoc;
		}

		/// <summary>Parses xml annotation documents from the log.</summary>
		/// <param name="logUri">The URI the log will be obtained from.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>Parsed xml annotation documents, if any or <c>null</c> if parsing failed.</returns>
		[CanBeNull]
		public static XDocument[] TryParseXmlAnnotationDocsFromLog(
			[NotNull] string logUri,
			[NotNull] IMessageLogger messageLogger)
		{
			Code.NotNullNorEmpty(logUri, nameof(logUri));
			Code.NotNull(messageLogger, nameof(messageLogger));

			messageLogger.Logger.WriteVerbose($"Downloading '{logUri}'.");

			using (var reader = IoHelpers.TryGetTextFromUri(logUri, TimeSpan.FromSeconds(15)))
			{
				if (reader == null)
				{
					messageLogger.WriteSetupErrorMessage($"Could not load log content from '{logUri}'.");
					return null;
				}

				messageLogger.Logger.WriteVerbose($"Parsing '{logUri}' content.");

				return ParseLogContent(logUri, reader, messageLogger);
			}
		}

		private static XDocument[] ParseLogContent(
			string logUri, TextReader reader,
			IMessageLogger messageLogger)
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
						messageLogger.WriteSetupErrorMessage(
							$"The log is damaged, annotation area start tag was repeated twice. Uri '{logUri}', lines {xmlStartLineNumber} and {lineNumber}.");
					}
					xmlStartLineNumber = lineNumber;
				}
				else if (logLine.StartsWith(LogAnnotationEnd, StringComparison.OrdinalIgnoreCase))
				{
					if (xmlStartLineNumber < 0)
					{
						messageLogger.WriteSetupErrorMessage(
							$"The log is damaged, annotation area start tag is missing. Uri '{logUri}', start tag at line {lineNumber}.");
					}
					else if (buffer.Length > 0)
					{
						var xmlAnnotationDoc = TryParseLogContentDoc(
							buffer.ToString(), logUri, xmlStartLineNumber, messageLogger);
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
				messageLogger.WriteSetupErrorMessage(
					$"The log is damaged, annotation area end tag is missing. Uri '{logUri}', start tag at line {xmlStartLineNumber}.");
			}

			return result.ToArray();
		}

		[CanBeNull]
		private static XDocument TryParseLogContentDoc(
			string logXmlText,
			string logUri, int logLine,
			[NotNull] IMessageLogger messageLogger)
		{
			using (var reader = new StringReader(logXmlText))
			{
				var xmlAnnotationDoc = TryParseXmlAnnotationDoc(
					reader,
					$"Log '{logUri}', line {logLine}",
					messageLogger);

				return xmlAnnotationDoc;
			}
		}
		#endregion

		#region XML doc saving
		/// <summary>Writes xml annotation document for the competition targets to the log.</summary>
		/// <param name="competitionTargets">The competition targets to log.</param>
		/// <param name="messageLogger">The message logger.</param>
		public static void LogXmlAnnotationDoc(
			[NotNull] IReadOnlyCollection<CompetitionTarget> competitionTargets,
			[NotNull] IMessageLogger messageLogger)
		{
			Code.NotNull(competitionTargets, nameof(competitionTargets));
			Code.NotNull(messageLogger, nameof(messageLogger));

			if (competitionTargets.Count == 0)
				return;

			// Create xml annotation doc
			var xmlAnnotationDoc = new XDocument(new XElement(CompetitionBenchmarksRootNode));
			var benchmarkType = competitionTargets.First().Target.Type;
			AddOrUpdateXmlAnnotation(xmlAnnotationDoc, competitionTargets, benchmarkType, true);

			// Dump it
			var xmlDocContent = new StringBuilder();
			using (var writer = XmlWriter.Create(xmlDocContent, GetXmlWriterSettings(true)))
			{
				xmlAnnotationDoc.Save(writer);
			}

			var logger = messageLogger.Logger;
			logger.WriteLineInfo(LogAnnotationStart);
			logger.WriteLineInfo(xmlDocContent.ToString());
			logger.WriteLineInfo(LogAnnotationEnd);
		}

		/// <summary>Saves the specified xml annotation document.</summary>
		/// <param name="xmlAnnotationDoc">The xml annotation document.</param>
		/// <param name="output">The stream the document will be saved to.</param>
		public static void Save(
			[NotNull] XDocument xmlAnnotationDoc,
			[NotNull] Stream output)
		{
			Code.NotNull(xmlAnnotationDoc, nameof(xmlAnnotationDoc));
			Code.NotNull(output, nameof(output));

			using (var writer = XmlWriter.Create(output, GetXmlWriterSettings()))
			{
				xmlAnnotationDoc.Save(writer);
			}
		}
		#endregion

		#region XML doc metric helpers
		[NotNull]
		private static string GetTargetTypeName(this Type targetType, bool useFullTypeName) =>
			useFullTypeName
				? targetType.GetShortAssemblyQualifiedName()
				: targetType.Name;

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

		#region XML doc metric loading
		/// <summary>Parses stored info for targets from the the xml annotation document.</summary>
		/// <param name="targets">The targets.</param>
		/// <param name="metrics">The metrics to parse.</param>
		/// <param name="xmlAnnotationDoc">The xml annotation document.</param>
		/// <param name="useFullTypeName">Use full type name in XML annotations.</param>
		/// <param name="analysis">State of the analysis.</param>
		/// <returns>Parsed stored info for targets.</returns>
		[NotNull]
		public static Dictionary<Target, StoredTargetInfo> TryGetStoredTargets(
			[NotNull] Target[] targets,
			[NotNull] IEnumerable<MetricInfo> metrics,
			[NotNull] XDocument xmlAnnotationDoc,
			bool useFullTypeName,
			[NotNull] IMessageLogger analysis)
		{
			Code.NotNull(targets, nameof(targets));
			Code.NotNull(metrics, nameof(metrics));
			Code.NotNull(xmlAnnotationDoc, nameof(xmlAnnotationDoc));
			Code.NotNull(analysis, nameof(analysis));

			var result = new Dictionary<Target, StoredTargetInfo>();
			if (targets.Length == 0)
				return result;

			var targetTypeName = targets[0].Type.GetTargetTypeName(useFullTypeName);

			var metricsByName = metrics.ToDictionary(m => m.DisplayName);
			var targetNodesByName =
				(from competition in xmlAnnotationDoc.Element(CompetitionBenchmarksRootNode)?.Elements(CompetitionNode)
				 where competition.Attribute(TargetAttribute)?.Value == targetTypeName
				 from candidate in competition.Elements()
				 select candidate)
				.ToDictionary(c => c.Name);

			foreach (var target in targets)
			{
				var targetMethodName = target.GetTargetMethodName();
				var targetNode = targetNodesByName.GetValueOrDefault(targetMethodName);
				var storedTarget = TryParseTargetMetrics(target, targetNode, metricsByName, analysis);
				if (storedTarget != null)
				{
					result.Add(target, storedTarget);
				}
			}

			return result;
		}

		[CanBeNull]
		private static StoredTargetInfo TryParseTargetMetrics(
			Target target, XElement targetNode,
			Dictionary<string, MetricInfo> metricsByName,
			IMessageLogger messageLogger)
		{
			if (targetNode == null)
			{
				return null;
			}

			var baseline = TryParseBooleanValue(target, targetNode, BaselineAttribute, messageLogger);
			var metrics = new List<StoredMetricValue>();

			var primaryMetric = metricsByName.Values.SingleOrDefault(m => m.IsPrimaryMetric);
			if (primaryMetric != null)
			{
				var storedMetric = TryParseTargetMetric(target, targetNode, primaryMetric, messageLogger);
				if (storedMetric != null)
				{
					metrics.Add(storedMetric);
				}
			}

			foreach (var metricNode in targetNode.Elements())
			{
				if (!metricsByName.TryGetValue(metricNode.Name.LocalName, out var metric))
				{
					messageLogger.WriteSetupErrorMessage(
						target,
						$"XML annotation contains unknown metric {metricNode.Name}, it will be skipped.");
					continue;
				}

				var storedMetric = TryParseTargetMetric(target, metricNode, metric, messageLogger);
				if (storedMetric != null)
				{
					metrics.Add(storedMetric);
				}
			}

			return new StoredTargetInfo(metrics.ToArray(), baseline);
		}

		private static StoredMetricValue TryParseTargetMetric(
			Target target, XElement targetNode,
			MetricInfo targetMetric,
			IMessageLogger messageLogger)
		{
			var min = TryParseDoubleValue(target, targetNode, MinAttribute, double.NaN, messageLogger);
			var max = TryParseDoubleValue(target, targetNode, MaxAttribute, double.NaN, messageLogger);

			if (min == null || max == null)
				return null;

			var minValue = min.Value;
			var maxValue = max.Value;
			if (maxValue.Equals(double.NaN))
			{
				minValue = double.NaN;
			}
			else if (minValue.Equals(double.NaN))
			{
				minValue = maxValue.GetMinMetricValue(targetMetric);
			}

			string unitName = targetNode.Attribute(UnitAttribute)?.Value;
			var unitValue = targetMetric.MetricUnits[unitName].EnumValue;
			if (unitName.NotNullNorEmpty())
			{
				unitValue = targetMetric.MetricUnits[unitName].EnumValue;

				if (unitValue == null)
				{
					messageLogger.WriteSetupErrorMessage(
						target,
						$"XML annotation contains metric {targetMetric} with invalid unit value {unitName}, skipped.");

					return null;
				}
			}

			return new StoredMetricValue(targetMetric.AttributeType, minValue, maxValue, unitValue);
		}

		// ReSharper disable ConvertClosureToMethodGroup
		private static double? TryParseDoubleValue(
			Target target, XElement competitionNode,
			string attribute, double fallbackValue,
			IMessageLogger messageLogger) =>
				TryParseCore(
					target, competitionNode,
					attribute, fallbackValue,
					s => XmlConvert.ToDouble(s),
					messageLogger);

		private static bool? TryParseBooleanValue(
			Target target, XElement competitionNode,
			string attribute, IMessageLogger messageLogger) =>
				TryParseCore(
					target, competitionNode,
					attribute, false,
					s => XmlConvert.ToBoolean(s),
					messageLogger);

		// ReSharper restore ConvertClosureToMethodGroup

		private static T? TryParseCore<T>(
			Target target, XElement competitionNode,
			string limitProperty,
			T fallbackValue,
			Func<string, T> parseCallback,
			IMessageLogger messageLogger)
			where T : struct
		{
			var limitText = competitionNode.Attribute(limitProperty)?.Value;
			if (limitText == null)
				return fallbackValue;
			try
			{
				return parseCallback(limitText);
			}
			catch (FormatException ex)
			{
				messageLogger.WriteExceptionMessage(
					MessageSeverity.SetupError,
					target,
					$"XML annotation for {target.MethodDisplayInfo}: could not parse {limitProperty}.",
					ex);
				return null;
			}
		}
		#endregion

		#region XML doc metric saving
		/// <summary>Adds or updates xml annotation for the competition targets.</summary>
		/// <param name="xmlAnnotationDoc">The xml annotation document that will be updated.</param>
		/// <param name="competitionTargets">The competition targets.</param>
		/// <param name="useFullTypeName">Use full type name in XML annotations.</param>
		/// <param name="benchmarkType">The type of the benchmark.</param>
		public static void AddOrUpdateXmlAnnotation(
			[NotNull] XDocument xmlAnnotationDoc,
			[NotNull] IReadOnlyCollection<CompetitionTarget> competitionTargets,
			[NotNull] Type benchmarkType,
			bool useFullTypeName)
		{
			Code.NotNull(xmlAnnotationDoc, nameof(xmlAnnotationDoc));
			Code.NotNull(competitionTargets, nameof(competitionTargets));

			var targetTypeName = benchmarkType.GetTargetTypeName(useFullTypeName);
			var competitionNode = xmlAnnotationDoc
				.Element(CompetitionBenchmarksRootNode)
				.GetOrAddElement(CompetitionNode, targetTypeName);
			foreach (var competitionTarget in competitionTargets)
			{
				var targetMethodName = competitionTarget.Target.GetTargetMethodName();
				var isBaseline = competitionTarget.Baseline;

				var targetNode = competitionNode.GetOrAddElement(targetMethodName);
				bool forceUpdate = !targetNode.HasElements;

				var baselineText = isBaseline ? XmlConvert.ToString(true) : null;
				targetNode.SetAttribute(BaselineAttribute, baselineText);

				var targetMetricValue = competitionTarget.MetricValues.SingleOrDefault(f => f.Metric.IsPrimaryMetric);
				UpdateStoredMetric(targetNode, targetMetricValue, forceUpdate);

				foreach (var metricValue in competitionTarget.MetricValues.Where(m => !m.Metric.IsPrimaryMetric))
				{
					var metricNode = GetOrAddElement(targetNode, metricValue.Metric.DisplayName);
					UpdateStoredMetric(metricNode, metricValue, forceUpdate);
				}
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
				metricValue.ValuesRange.GetMinMaxString(metricValue.DisplayMetricUnit, out var minValueText, out var maxValueText);
				if (double.IsInfinity(metricRange.Min))
					minValueText = XmlConvert.ToString(metricRange.Min);
				if (double.IsInfinity(metricRange.Max))
					maxValueText = XmlConvert.ToString(metricRange.Max);

				var unit = metricValue.DisplayMetricUnit;
				targetNode.SetAttribute(MinAttribute, minValueText);
				targetNode.SetAttribute(MaxAttribute, maxValueText);
				targetNode.SetAttribute(UnitAttribute, unit.IsEmpty ? null : unit.DisplayName);
			}
		}
		#endregion
	}
}