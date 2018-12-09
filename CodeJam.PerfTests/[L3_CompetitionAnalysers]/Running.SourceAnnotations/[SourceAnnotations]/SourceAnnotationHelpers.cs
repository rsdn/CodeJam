using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Helpers;
using CodeJam.Ranges;
using CodeJam.Reflection;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Helper lass for source annotations
	/// </summary>
	internal static class SourceAnnotationHelpers
	{
		#region Regex
		private const string RegexPrefixPart = "Prefix";
		private const string RegexArgsPart = "Args";
		private const string RegexSuffixPart = "Suffix";
		// language=regexp
		private const string RegexPatternFormat = @"
				(?<Prefix>\[(?:[^\]]+?\,\s+?)?{0})
				(?:
					\(
					(?<Args>.*?)
					\)
				)?
				(?<Suffix>(?:\s*\,[^\]]+?)?\])";

		private static readonly Func<Type, Regex> _regexCache = Algorithms.Memoize(
			(Type attributeType) => new Regex(
				string.Format(RegexPatternFormat, attributeType.GetShortAttributeName()),
				RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.Compiled),
			true);
		#endregion

		#region Parse
		/// <summary>Fills attribute lines.</summary>
		/// <param name="candidateLines">The candidate lines.</param>
		/// <param name="sourceLines">The source lines.</param>
		/// <param name="sourcePath">The source path.</param>
		/// <param name="messageLogger">The message logger.</param>
		/// <returns>Filled attribute lines.</returns>
		public static TargetSourceLines[] FillAttributeLines(
			CompositeRange<int, MethodBase> candidateLines,
			string[] sourceLines,
			string sourcePath,
			IMessageLogger messageLogger)
		{
			// TODO: same method => multiple ranges.
			var result = new List<TargetSourceLines>();
			foreach (var candidateRange in candidateLines.SubRanges)
			{
				var benchmarkMethodInfo = TryParseBenchmarkMethodInfo(
					candidateRange,
					sourceLines, sourcePath,
					messageLogger);
				if (benchmarkMethodInfo != null)
				{
					result.Add(benchmarkMethodInfo);
				}
			}
			return result.ToArray();
		}

		[CanBeNull]
		private static TargetSourceLines TryParseBenchmarkMethodInfo(
			Range<int, MethodBase> candidateRange,
			string[] sourceLines,
			string sourcePath,
			IMessageLogger messageLogger)
		{
			var primaryAttribute = candidateRange.Key.GetCustomAttribute<CompetitionBenchmarkAttribute>();
			if (primaryAttribute == null || primaryAttribute.GetType() != typeof(CompetitionBenchmarkAttribute))
			{
				primaryAttribute = candidateRange.Key.GetCustomAttribute<CompetitionBaselineAttribute>();
				if (primaryAttribute == null || primaryAttribute.GetType() != typeof(CompetitionBaselineAttribute))
					return null;
			}

			var attributeLines = new Dictionary<RuntimeTypeHandle, int>();
			foreach (var metricAttribute in candidateRange.Key.GetMetadataAttributes<IStoredMetricValue>(true))
			{
				var attributeType = metricAttribute.GetType();
				var attributeLine = TryParseBenchmarkAttributeLine(attributeType, candidateRange, sourceLines);
				if (attributeLine != null)
				{
					attributeLines.Add(attributeType.TypeHandle, attributeLine.Value);
				}
				else
				{
					// TODO: improve message
					messageLogger.WriteSetupErrorMessage(
						$"Cannot find attribute {attributeType} for {candidateRange}. File '{sourcePath}'.");
				}
			}

			if (!attributeLines.TryGetValue(primaryAttribute.GetType().TypeHandle, out var primaryAttributeLine))
			{
				return null;
			}

			return new TargetSourceLines(
				candidateRange.Key.MethodHandle,
				primaryAttributeLine, candidateRange.WithoutKey(),
				attributeLines);
		}

		private static int? TryParseBenchmarkAttributeLine(
			Type attributeType,
			Range<int, MethodBase> candidateRange,
			string[] sourceLines)
		{
			var regex = _regexCache(attributeType);

			for (var i = candidateRange.ToValue; i >= candidateRange.FromValue; i--)
			{
				// Line numbers start from 1
				if (regex.IsMatch(sourceLines[i - 1]))
					return i;
			}

			return null;
		}
		#endregion

		#region Source lines API
		public static bool TryUpdateExistingAttributeAnnotation(
			SourceAnnotationFile sourceFile, int attributeLineNumber,
			CompetitionMetricValue metricValue)
		{
			var line = sourceFile[attributeLineNumber];

			if (!TryUpdateLineCore(line, metricValue, out var newLine))
				return false;

			if (newLine != line)
				sourceFile.ReplaceLine(attributeLineNumber, newLine);
			return true;
		}

		public static bool TryAppendAttributeAnnotation(
			SourceAnnotationFile sourceCodeFile, int attributeLineNumber,
			TargetSourceLines benchmarkMethod,
			CompetitionMetricValue metricValue)
		{
			var line = sourceCodeFile[attributeLineNumber];

			var attributePosition = line.LastIndexOf(']');
			if (attributePosition < 0)
				return false;

			var appendText = GetAppendAnnotationText(metricValue);

			line = line.Insert(attributePosition, appendText);
			sourceCodeFile.ReplaceLine(attributeLineNumber, line);
			var attributeTypeHandle = metricValue.Metric.AttributeType.TypeHandle;
			benchmarkMethod.AddAttribute(attributeTypeHandle, attributeLineNumber);

			return true;
		}

		public static int InsertNewLineWithAttributeAnnotation(
			SourceAnnotationFile sourceCodeFile, int insertLineNumber,
			TargetSourceLines benchmarkMethod,
			CompetitionMetricValue metricValue)
		{
			var whitespacePrefix = GetWhitespacePrefix(sourceCodeFile[insertLineNumber]);

			var newLine = GetNewLineAnnotationText(whitespacePrefix, metricValue);

			var newLineNumber = insertLineNumber + 1;
			sourceCodeFile.InsertLine(newLineNumber, newLine);
			var attributeTypeHandle = metricValue.Metric.AttributeType.TypeHandle;
			benchmarkMethod.AddAttribute(attributeTypeHandle, newLineNumber);

			return newLineNumber;
		}

		#region Attribute helper methods
		private static string GetWhitespacePrefix(string line) =>
			new string(line.TakeWhile(c => c.IsWhiteSpace()).ToArray());

		private static string GetNewLineAnnotationText(string whitespacePrefix, CompetitionMetricValue metricValue)
		{
			var result = new StringBuilder();
			result.Append(whitespacePrefix);
			result.Append("[");
			result.Append(metricValue.Metric.AttributeType.GetShortAttributeName());
			result.Append("(");
			AppendFirstAttributeArgs(metricValue, result);
			result.Append(")]");
			return result.ToString();
		}

		private static string GetAppendAnnotationText(CompetitionMetricValue metricValue)
		{
			var result = new StringBuilder();
			result.Append(", ");
			result.Append(metricValue.Metric.AttributeType.GetShortAttributeName());
			result.Append("(");
			AppendFirstAttributeArgs(metricValue, result);
			result.Append(")");
			return result.ToString();
		}

		private static void AppendFirstAttributeArgs(CompetitionMetricValue metricValue, StringBuilder result)
		{
			var metric = metricValue.Metric;
			var metricRange = metricValue.ValuesRange;
			var metricUnit = metricValue.DisplayMetricUnit;

			if (metricRange.Min.Equals(0) && metricRange.Max.Equals(0))
			{
				result.Append("0");
			}
			else if (metricRange.IsNotEmpty)
			{
				metricRange.GetMinMaxString(metricUnit, out var minValueText, out var maxValueText);
				if (double.IsInfinity(metricRange.Min))
					minValueText = $"double.{nameof(double.NegativeInfinity)}";
				if (double.IsInfinity(metricRange.Max))
					maxValueText = $"double.{nameof(double.PositiveInfinity)}";

				if (metricRange.ShouldStoreMinMetricValue(metricUnit, metric))
				{
					result.Append(minValueText).Append(", ");
				}

				result.Append(maxValueText);

				if (!metric.MetricUnits.IsEmpty)
				{
					var metricEnumType = metric.MetricUnits.MetricEnumType;
					Code.BugIf(
						metricEnumType == null || metricUnit.EnumValue == null ||
							!Enum.IsDefined(metricEnumType, metricUnit.EnumValue),
						"Bad enum value.");

					result
						.Append(", ")
						.Append(metricEnumType.Name)
						.Append(".")
						.Append(metricUnit.EnumValue);
				}
			}
		}

		private static bool TryUpdateLineCore(string line, CompetitionMetricValue metricValue, out string newLine)
		{
			var hasMatch = false;

			var regex = _regexCache(metricValue.Metric.AttributeType);
			newLine = regex.Replace(
				line,
				m => FixAttributeContent(m, metricValue, out hasMatch),
				1);

			return hasMatch;
		}

		private static string FixAttributeContent(Match m, CompetitionMetricValue metricValue, out bool hasMatch)
		{
			var attributeStartText = m.Groups[RegexPrefixPart].Value;
			var attributeEndText = m.Groups[RegexSuffixPart].Value;
			var optionalArgsText = GetOptionalAttributeArgs(m.Groups[RegexArgsPart]?.Value);

			var result = new StringBuilder(m.Length + 10);
			result
				.Append(attributeStartText)
				.Append("(");

			AppendFirstAttributeArgs(metricValue, result);

			if (optionalArgsText.NotNullNorEmpty())
			{
				result
					.Append(",")
					.Append(optionalArgsText);
			}

			result
				.Append(")")
				.Append(attributeEndText);

			hasMatch = true;
			return result.ToString();
		}

		/// <summary>Gets optional attribute args or empty string if none.</summary>
		/// <param name="argsPart">The arguments part.</param>
		/// <returns>Optional attribute args or empty string if none.</returns>
		private static string GetOptionalAttributeArgs(string argsPart)
		{
			if (argsPart.IsNullOrEmpty())
				return string.Empty;

			var firstOptionalSeparator = argsPart.IndexOf('=');
			if (firstOptionalSeparator < 0)
				return string.Empty;

			var optionalStartIndex = argsPart.LastIndexOf(',', firstOptionalSeparator) + 1;
			return argsPart.Substring(optionalStartIndex);
		}
		#endregion

		#endregion
	}
}