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
using CodeJam.PerfTests.Configs;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Helpers;
using CodeJam.Reflection;
using CodeJam.Strings;

using JetBrains.Annotations;

using static BenchmarkDotNet.Loggers.FilteringLogger;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Helper class for xml annotations
	/// </summary>
	[SuppressMessage("ReSharper", "ArrangeBraces_using")]
	internal static class XmlAnnotationHelpers
	{
		#region XML doc constants
		private const string LogAnnotationStart = ImportantLogScopeStartPrefix +
			" ------ xml_annotation_begin ------";

		private const string LogAnnotationEnd = ImportantLogScopeEndPrefix +
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

		private const string IndentChars = "\t";
		private const string NewLineChars = "\r\n";

		[NotNull]
		private static XmlWriterSettings GetXmlWriterSettings(bool omitXmlDeclaration = false)
		{
			return new XmlWriterSettings
			{
				OmitXmlDeclaration = omitXmlDeclaration,
				NewLineOnAttributes = false,
				Indent = true,
				IndentChars = IndentChars,
				NewLineChars = NewLineChars
			};
		}
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
		/// <param name="annotationResourceKey">Name of the xml resource with xml annotation document.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>XML annotation document or <c>null</c> if parsing failed.</returns>
		[CanBeNull]
		public static XDocument TryParseXmlAnnotationDoc(
			ResourceKey annotationResourceKey,
			[NotNull] IMessageLogger messageLogger)
		{
			Code.NotNull(messageLogger, nameof(messageLogger));
			Code.BugIf(annotationResourceKey.Assembly == null, "annotationResourceKey.Assembly == null");

			var assembly = annotationResourceKey.Assembly;
			var resourceName = annotationResourceKey.ResourceName;
			using (var resourceStream = annotationResourceKey.TryGetResourceStream())
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

		/// <summary>Parses xml annotation document from the file.</summary>
		/// <param name="resourcePath">The path to xml annotation document.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>XML annotation document or <c>null</c> if parsing failed.</returns>
		[CanBeNull]
		public static XDocument TryParseXmlAnnotationDoc(
			string resourcePath,
			IMessageLogger messageLogger)
		{
			try
			{
				using (var stream = File.OpenRead(resourcePath))
				{
					return TryParseXmlAnnotationDoc(
						stream,
						$"XML annotation '{resourcePath}'",
						messageLogger);
				}
			}
			catch (IOException ex)
			{
				messageLogger.WriteExceptionMessage(
					MessageSeverity.SetupError,
					$"Could not access file '{resourcePath}'.", ex);

				return null;
			}
			catch (UnauthorizedAccessException ex)
			{
				messageLogger.WriteExceptionMessage(
					MessageSeverity.SetupError,
					$"Could not access file '{resourcePath}'.", ex);

				return null;
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
					xmlAnnotationDoc = XDocument.Load(reader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
				}
				catch (ArgumentException ex)
				{
					messageLogger.WriteExceptionMessage(
						MessageSeverity.SetupError,
						$"{sourceDescription}: Could not parse XML annotation.", ex);
					return null;
				}
				catch (XmlException ex)
				{
					messageLogger.WriteExceptionMessage(
						MessageSeverity.SetupError,
						$"{sourceDescription}: Could not parse XML annotation.", ex);
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

			messageLogger.Logger.WriteVerboseLine($"{sourceDescription}: XML annotation parsed.");
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

			messageLogger.Logger.WriteVerboseLine($"Downloading '{logUri}'.");

			using (var reader = IoHelpers.TryGetTextFromUri(logUri, TimeSpan.FromSeconds(15)))
			{
				if (reader == null)
				{
					messageLogger.WriteSetupErrorMessage($"Could not load log content from '{logUri}'.");
					return null;
				}

				messageLogger.Logger.WriteVerboseLine($"Parsing '{logUri}' content.");

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
			try
			{
				logger.WriteLineInfo(xmlDocContent.ToString());
			}
			finally
			{
				logger.WriteLineInfo(LogAnnotationEnd);
			}
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
		private static XElement GetOrAddTargetElement(this XElement parent, XName elementName, string targetName)
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
				parent.AddPreserveFormat(result, null);
			}

			return result;
		}

		[NotNull]
		// ReSharper disable once SuggestBaseTypeForParameter
		private static XElement GetOrAddElement(
			this XElement parent, XName elementName, [CanBeNull] XElement insertAfterElement)
		{
			Code.NotNull(elementName, nameof(elementName));

			var result = parent.Element(elementName);
			if (result == null)
			{
				result = new XElement(elementName);
				parent.AddPreserveFormat(result, insertAfterElement);
			}

			return result;
		}

		[NotNull]
		private static IEnumerable<XNode> ScanUp([NotNull] this XNode node) =>
			node
				.NodesBeforeSelf().Prepend(node)
				.Concat(
					node
						.Ancestors()
						.SelectMany(n => n.NodesBeforeSelf().Prepend(n)));

		[CanBeNull]
		private static XText FirstIndentNodeOrDefault([NotNull] this IEnumerable<XNode> nodes) =>
			nodes
				.OfType<XText>()
				.FirstOrDefault(n => n.Value.Contains('\n'));

		private static void AddPreserveFormat(
			[NotNull] this XElement parentNode,
			[NotNull] XElement newElement,
			[CanBeNull] XElement insertAfterElement)
		{
			Code.NotNull(parentNode, nameof(parentNode));
			Code.NotNull(newElement, nameof(newElement));
			Code.AssertState(parentNode.Document != null, "The parent node should be attached to document.");
			Code.AssertState(newElement.Document == null, "The new node should not be attached to document.");

			// The node after that we'll insert new one
			var insertNode = insertAfterElement ?? parentNode.Nodes().LastOrDefault(n => !(n is XText));

			// Find indentation text for item and for closing parent node (lazily)
			var closeTagIndentText = Lazy.Create(
				() =>
					parentNode.ScanUp().FirstIndentNodeOrDefault()?.Value
						?? NewLineChars);
			var indentText =
				insertNode?.NodesBeforeSelf().FirstIndentNodeOrDefault()?.Value
					?? (closeTagIndentText.Value + IndentChars);
			var indentNode = new XText(indentText);

			if (insertNode != null)
			{
				// There's element or comment in parent: add indent and new node after it
				insertNode.AddAfterSelf(indentNode);
				indentNode.AddAfterSelf(newElement);
			}
			else
			{
				// Parent has no elements: search new line indent for closing parent node
				var closeTagIndentNode = parentNode.Nodes().FirstIndentNodeOrDefault();

				if (closeTagIndentNode != null)
				{
					// There is indent for closing parent node: insert before it
					closeTagIndentNode.AddBeforeSelf(indentNode);
					indentNode.AddAfterSelf(newElement);
				}
				else
				{
					// Add indent, new node and indent for closing parent node
					parentNode.Add(indentNode);
					indentNode.AddAfterSelf(newElement);
					newElement.AddAfterSelf(new XText(closeTagIndentText.Value));
				}
			}
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
					.ToLookup(c => c.Name);

			foreach (var target in targets)
			{
				var targetMethodName = target.GetTargetMethodName();
				var targetNodes = targetNodesByName[targetMethodName].ToArray();
				var storedTarget = TryParseTargetMetrics(target, targetNodes, metricsByName, analysis);
				if (storedTarget != null)
				{
					result.Add(target, storedTarget);
				}
			}

			return result;
		}

		[CanBeNull]
		private static StoredTargetInfo TryParseTargetMetrics(
			Target target, XElement[] targetNodes,
			Dictionary<string, MetricInfo> metricsByName,
			IMessageLogger messageLogger)
		{
			if (targetNodes.IsNullOrEmpty())
			{
				return null;
			}

			bool? baseline = null;
			var metrics = new List<StoredMetricValue>();
			var primaryMetric = metricsByName.Values.SingleOrDefault(m => m.IsPrimaryMetric);
			foreach (var targetNode in targetNodes)
			{
				baseline = baseline ?? TryParseBooleanValue(target, targetNode, BaselineAttribute, messageLogger);
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
						messageLogger.WriteWarningMessage(
							target,
							$"XML annotation contains metric {metricNode.Name} not listed in config; the metric is ignored.",
							$"List of metrics is exposed as {nameof(ICompetitionConfig)}.{nameof(ICompetitionConfig.GetMetrics)}().");
						continue;
					}

					var storedMetric = TryParseTargetMetric(target, metricNode, metric, messageLogger);
					if (storedMetric != null)
					{
						metrics.Add(storedMetric);
					}
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
			string xmlAttributeName, double fallbackValue,
			IMessageLogger messageLogger) =>
				TryParseCore(
					target, competitionNode,
					xmlAttributeName,
					s => XmlConvert.ToDouble(s),
					fallbackValue,
					messageLogger);

		private static bool? TryParseBooleanValue(
			Target target, XElement competitionNode,
			string xmlAttributeName, IMessageLogger messageLogger) =>
				TryParseCore(
					target, competitionNode,
					xmlAttributeName,
					s => XmlConvert.ToBoolean(s),
					false,
					messageLogger);

		// ReSharper restore ConvertClosureToMethodGroup

		private static T? TryParseCore<T>(
			Target target, XElement competitionNode,
			string xmlAttributeName,
			Func<string, T> parseCallback,
			T fallbackValue,
			IMessageLogger messageLogger)
			where T : struct
		{
			var attributeValue = competitionNode.Attribute(xmlAttributeName)?.Value;
			if (attributeValue == null)
				return fallbackValue;

			try
			{
				return parseCallback(attributeValue);
			}
			catch (FormatException ex)
			{
				messageLogger.WriteExceptionMessage(
					MessageSeverity.SetupError,
					target,
					$"XML annotation for {target.MethodDisplayInfo}: could not parse {xmlAttributeName}.",
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
				.GetOrAddTargetElement(CompetitionNode, targetTypeName);

			XElement lastTargetNode = null;
			foreach (var competitionTarget in competitionTargets)
			{
				var targetMethodName = competitionTarget.Target.GetTargetMethodName();
				var isBaseline = competitionTarget.Baseline;

				lastTargetNode = competitionNode.GetOrAddElement(targetMethodName, lastTargetNode);
				bool forceUpdate = !lastTargetNode.HasElements;

				var baselineText = isBaseline ? XmlConvert.ToString(true) : null;
				lastTargetNode.SetAttribute(BaselineAttribute, baselineText);

				var targetMetricValue = competitionTarget.MetricValues.SingleOrDefault(f => f.Metric.IsPrimaryMetric);
				UpdateStoredMetric(lastTargetNode, targetMetricValue, forceUpdate);

				XElement lastMetricNode = null;
				foreach (var metricValue in competitionTarget.MetricValues.Where(m => !m.Metric.IsPrimaryMetric))
				{
					lastMetricNode = GetOrAddElement(lastTargetNode, metricValue.Metric.DisplayName, lastMetricNode);
					UpdateStoredMetric(lastMetricNode, metricValue, forceUpdate);
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