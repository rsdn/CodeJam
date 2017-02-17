using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using BenchmarkDotNet.Helpers;

using CodeJam.Collections;
using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Metrics;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Ranges;
using CodeJam.Reflection;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	internal static partial class SourceAnnotationsHelper
	{
		private sealed class BenchmarkMethodInfo
		{
			private readonly Dictionary<RuntimeTypeHandle, int> _attributeLineNumbers;

			public BenchmarkMethodInfo(
				RuntimeMethodHandle method,
				int primaryAttributeLineNumber,
				Range<int> attributeCandidateLineNumbers,
				Dictionary<RuntimeTypeHandle, int> attributeLineNumbers)
			{
				Code.InRange(primaryAttributeLineNumber, nameof(primaryAttributeLineNumber), 1, int.MaxValue);

				Code.AssertArgument(
					Range.Create(1, int.MaxValue).Contains(attributeCandidateLineNumbers),
					nameof(attributeCandidateLineNumbers),
					"Incorrect line numbers range.");

				Code.AssertArgument(
					attributeCandidateLineNumbers.Contains(primaryAttributeLineNumber),
					nameof(primaryAttributeLineNumber),
					"Incorrect benchmark attribute line number.");

				Method = method;
				PrimaryAttributeLineNumber = primaryAttributeLineNumber;
				AttributeCandidateLineNumbers = attributeCandidateLineNumbers;
				_attributeLineNumbers = attributeLineNumbers;
			}

			public RuntimeMethodHandle Method { get; }
			public int PrimaryAttributeLineNumber { get; private set; }
			public Range<int> AttributeCandidateLineNumbers { get; private set; }

			public IReadOnlyDictionary<RuntimeTypeHandle, int> AttributeLineNumbers => _attributeLineNumbers;

			private int FixOnInsertFrom(int lineNumber, int insertLineNumber) =>
				insertLineNumber < lineNumber ? lineNumber + 1 : lineNumber;
			private int FixOnInsertTo(int lineNumber, int insertLineNumber) =>
				insertLineNumber <= lineNumber ? lineNumber + 1 : lineNumber;

			public void FixOnInsert(int insertLineNumber)
			{
				Code.InRange(insertLineNumber, nameof(insertLineNumber), 1, int.MaxValue);

				var range = AttributeCandidateLineNumbers;
				if (range.EndsBefore(insertLineNumber))
					return;

				PrimaryAttributeLineNumber = FixOnInsertTo(PrimaryAttributeLineNumber, insertLineNumber);
				int newFrom = FixOnInsertFrom(range.FromValue, insertLineNumber);
				int newTo = FixOnInsertTo(range.ToValue, insertLineNumber);
				AttributeCandidateLineNumbers = Range.Create(newFrom, newTo);

				foreach (var method in _attributeLineNumbers.Keys.ToArray())
				{
					var line = _attributeLineNumbers[method];
					if (insertLineNumber <= line)
					{
						_attributeLineNumbers[method] = line + 1;
					}
				}
			}

			public void AddAttribute(RuntimeTypeHandle attributeType, int attributeLineNumber)
			{
				Code.InRange(
					attributeLineNumber, nameof(attributeLineNumber),
					AttributeCandidateLineNumbers.FromValue, AttributeCandidateLineNumbers.ToValue);

				_attributeLineNumbers.Add(attributeType, attributeLineNumber);
			}
		}

		private sealed class SourceCodeFile : AnnotationFile
		{
			private readonly List<string> _sourceLines;
			private readonly Dictionary<RuntimeMethodHandle, BenchmarkMethodInfo> _benchmarkMethodInfo;

			public SourceCodeFile(
				[NotNull] string path,
				[NotNull] string[] sourceLines) : this(path, sourceLines, Array<BenchmarkMethodInfo>.Empty) { }

			public SourceCodeFile(
				[NotNull] string path,
				[NotNull] string[] sourceLines,
				[NotNull] BenchmarkMethodInfo[] benchmarkMethods) :
					base(
					path,
					sourceLines.Length > 0 && benchmarkMethods.Length > 0)
			{
				_sourceLines = sourceLines.ToList();
				_benchmarkMethodInfo = benchmarkMethods.ToDictionary(m => m.Method);
			}

			//public IReadOnlyList<string> SourceLines => _sourceLines;
			public IReadOnlyDictionary<RuntimeMethodHandle, BenchmarkMethodInfo> BenchmarkMethods => _benchmarkMethodInfo;

			public string this[int lineNumber] => _sourceLines[lineNumber - 1];
			public int LinesCount => _sourceLines.Count;

			public void ReplaceLine(int lineNumber, string newLine)
			{
				Code.InRange(lineNumber, nameof(lineNumber), 1, int.MaxValue);
				Code.NotNull(newLine, nameof(newLine));
				AssertIsParsed();

				_sourceLines[lineNumber - 1] = newLine;
				MarkAsChanged();
			}

			public void InsertLine(int insertLineNumber, string newLine)
			{
				Code.InRange(insertLineNumber, nameof(insertLineNumber), 1, int.MaxValue);
				Code.NotNull(newLine, nameof(newLine));
				AssertIsParsed();

				_sourceLines.Insert(insertLineNumber - 1, newLine);
				foreach (var benchmarkMethodInfo in _benchmarkMethodInfo.Values)
				{
					benchmarkMethodInfo.FixOnInsert(insertLineNumber);
				}
				MarkAsChanged();
			}

			protected override void SaveCore() => BenchmarkHelpers.WriteFileContent(Path, _sourceLines.ToArray());
		}

		private static class SourceAnnotation
		{
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
					string.Format(RegexPatternFormat, attributeType.GetAttributeName()),
					RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.Compiled));

			#region Parse
			[NotNull]
			public static SourceCodeFile Parse(
				CompetitionTarget target,
				string sourcePath,
				CompetitionState competitionState)
			{
				var sourceLines = Array<string>.Empty;
				var documentInfo = SymbolHelper.TryGetSourceInfo(target.Target.Method, competitionState);
				if (documentInfo == null)
					return new SourceCodeFile(sourcePath, sourceLines);

				Code.BugIf(documentInfo.Path != sourcePath, "documentInfo.Path != sourcePath");

				if (!TryValidate(documentInfo, competitionState))
					return new SourceCodeFile(sourcePath, sourceLines);

				var methodRanges = documentInfo.MethodLinesMap;
				var noCodeRanges = methodRanges.GetComplementation()
					.MakeInclusive(i => i, i => i)
					.TrimFrom(1);
				var candidateLines = methodRanges.SubRanges
					.Zip(
						noCodeRanges.SubRanges,
						(methodRange, noCodeRange) => noCodeRange.WithKey(methodRange.Key))
					.ToCompositeRange();
				Code.BugIf(
					candidateLines.SubRanges.Count != methodRanges.SubRanges.Count,
					"candidateLines.SubRanges.Count != map.SubRanges.Count");

				sourceLines = TryReadFileContent(sourcePath, competitionState);
				if (sourceLines.Length == 0)
					return new SourceCodeFile(sourcePath, sourceLines);

				var benchmarkMethods = FillAttributeLines(candidateLines, sourceLines, sourcePath, competitionState);
				return new SourceCodeFile(sourcePath, sourceLines, benchmarkMethods);
			}

			private static bool TryValidate(SourceFileInfo documentInfo, CompetitionState competitionState)
			{
				bool result = true;

				if (documentInfo.LanguageType != LanguageType.CSharp)
				{
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Unsupported document language {documentInfo.LanguageType}. File '{documentInfo.Path}'.");
					result = false;
				}

				if (documentInfo.MethodLinesMap.IsEmpty)
				{
					// TODO: improve message
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"No methods found in document '{documentInfo.Path}'.");
					result = false;
				}

				if (!documentInfo.MethodLinesMap.IsMerged)
				{
					var methodsIntersection = documentInfo.MethodLinesMap.GetIntersections().FirstOrDefault(i => i.Ranges.Count > 1);
					DebugCode.BugIf(methodsIntersection.IsEmpty, "methodsIntersection.IsEmpty");

					var methodNames = methodsIntersection.Ranges.Select(r => r.Key.Name).Join(", ");

					// TODO: improve message
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Some of methods in document share same source lines {methodsIntersection.IntersectionRange}: " +
							$"{methodNames}. Document '{documentInfo.Path}'.");
					result = false;
				}

				result &= FileHashes.Check(
					documentInfo.Path,
					documentInfo.ChecksumAlgorithm,
					documentInfo.Checksum,
					competitionState);

				return result;
			}

			[NotNull]
			private static string[] TryReadFileContent(string file, CompetitionState competitionState)
			{
				try
				{
					return BenchmarkHelpers.ReadFileContent(file);
				}
				catch (IOException ex)
				{
					competitionState.WriteExceptionMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Could not access file '{file}'.", ex);

					return Array<string>.Empty;
				}
				catch (UnauthorizedAccessException ex)
				{
					competitionState.WriteExceptionMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Could not access file '{file}'.", ex);

					return Array<string>.Empty;
				}
				catch (DecoderFallbackException ex)
				{
					competitionState.WriteExceptionMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Cannot detect encoding for file '{file}'. Try to save the file as UTF8 or UTF16.", ex);

					return Array<string>.Empty;
				}
			}

			private static BenchmarkMethodInfo[] FillAttributeLines(
				CompositeRange<int, MethodBase> candidateLines,
				string[] sourceLines,
				string sourcePath,
				CompetitionState competitionState)
			{
				// TODO: same method => multiple ranges.
				var result = new List<BenchmarkMethodInfo>();
				foreach (var candidateRange in candidateLines.SubRanges)
				{
					var benchmarkMethodInfo = TryParseBenchmarkMethodInfo(candidateRange, sourceLines, sourcePath, competitionState);
					if (benchmarkMethodInfo != null)
					{
						result.Add(benchmarkMethodInfo);
					}
				}
				return result.ToArray();
			}

			[CanBeNull]
			private static BenchmarkMethodInfo TryParseBenchmarkMethodInfo(
				Range<int, MethodBase> candidateRange,
				string[] sourceLines,
				string sourcePath,
				CompetitionState competitionState)
			{
				var primaryAttribute = candidateRange.Key.GetCustomAttribute<CompetitionBenchmarkAttribute>();
				if (primaryAttribute == null || primaryAttribute.GetType() != typeof(CompetitionBenchmarkAttribute))
				{
					primaryAttribute = candidateRange.Key.GetCustomAttribute<CompetitionBaselineAttribute>();
					if (primaryAttribute == null || primaryAttribute.GetType() != typeof(CompetitionBaselineAttribute))
						return null;
				}

				var attributeLines = new Dictionary<RuntimeTypeHandle, int>();
				foreach (var metricAttribute in candidateRange.Key.GetMetadataAttributes<IStoredMetricSource>(true))
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
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.SetupError,
							$"Cannot find attribute {attributeType} for {candidateRange}. File '{sourcePath}'.");
					}
				}

				int primaryAttributeLine;
				if (!attributeLines.TryGetValue(primaryAttribute.GetType().TypeHandle, out primaryAttributeLine))
				{
					return null;
				}

				return new BenchmarkMethodInfo(
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

			#region Update annotation
			public static bool TryUpdate(
				SourceCodeFile sourceCodeFile,
				CompetitionTarget competitionTarget)
			{
				if (!sourceCodeFile.Parsed)
					return false;

				if (!sourceCodeFile.BenchmarkMethods.TryGetValue(
					competitionTarget.TargetKey.TargetMethod,
					out var benchmarkMethod))
					return false;

				bool allFixed = true;

				var metricsByCategory = competitionTarget.MetricValues.GroupBy(m => m.Metric.Category);
				foreach (var metricGrouping in metricsByCategory.Select(x => x.ToArray()))
				{
					var metricAttributes = metricGrouping.Select(m => m.Metric.AttributeType.TypeHandle).ToHashSet();
					var attributeAppendLineNumber = benchmarkMethod.AttributeLineNumbers
						.Where(p => metricAttributes.Contains(p.Key))
						.MinOrDefault(p => p.Value, -1);
					var attributeInsertLineNumber = benchmarkMethod.AttributeLineNumbers
						.Where(p => metricAttributes.Contains(p.Key))
						.MaxOrDefault(p => p.Value, benchmarkMethod.PrimaryAttributeLineNumber);

					foreach (var metricValue in metricGrouping.Where(m => m.HasUnsavedChanges || m.ValuesRange.IsEmpty))
					{
						var attributeTypeHandle = metricValue.Metric.AttributeType.TypeHandle;

						if (benchmarkMethod.AttributeLineNumbers.TryGetValue(
							attributeTypeHandle,
							out var attributeLineNumber))
						{
							bool updated = TryUpdateLineWithAttribute(sourceCodeFile, attributeLineNumber, metricValue);

							if (updated && attributeAppendLineNumber <= 0)
							{
								attributeAppendLineNumber = attributeLineNumber;
							}
							allFixed &= updated;
						}
						else
						{
							bool inserted = false;
							if (metricValue.Metric.AnnotateInplace && attributeAppendLineNumber > 0)
							{
								inserted = TryInsertAttributeInplace(
									sourceCodeFile, attributeAppendLineNumber,
									benchmarkMethod, metricValue,
									attributeTypeHandle);
							}

							if (!inserted)
							{
								attributeInsertLineNumber = InsertLineWithAttribute(
									sourceCodeFile, attributeInsertLineNumber,
									benchmarkMethod, metricValue,
									attributeTypeHandle);
								if (attributeAppendLineNumber <= 0)
								{
									attributeAppendLineNumber = attributeInsertLineNumber;
								}
							}
						}
					}
				}

				return allFixed;
			}

			#region Update attribute
			private static bool TryUpdateLineWithAttribute(
				SourceCodeFile sourceCodeFile, int attributeLineNumber, CompetitionMetricValue metricValue)
			{
				var line = sourceCodeFile[attributeLineNumber];

				if (!TryUpdateLine(line, metricValue, out var newLine))
					return false;

				if (newLine != line)
					sourceCodeFile.ReplaceLine(attributeLineNumber, newLine);
				return true;
			}

			private static bool TryUpdateLine(string line, CompetitionMetricValue metricValue, out string newLine)
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
				var attributeRestArgsText = TrimFirstAttributeArgs(m.Groups[RegexArgsPart]?.Value, 3);

				var result = new StringBuilder(m.Length + 10);
				result
					.Append(attributeStartText)
					.Append("(");
				AppendFirstAttributeArgs(metricValue, result);
				if (attributeRestArgsText.NotNullNorEmpty())
				{
					result
						.Append(", ")
						.Append(attributeRestArgsText);
				}
				result
					.Append(")")
					.Append(attributeEndText);

				hasMatch = true;
				return result.ToString();
			}

			/// <summary>Removes first <paramref name="attributesToSkip"/> attributes from args text.</summary>
			/// <param name="argsPart">The arguments part.</param>
			/// <param name="attributesToSkip">The attributes to skip.</param>
			/// <returns>Args text without first N attributes.</returns>
			private static string TrimFirstAttributeArgs(string argsPart, int attributesToSkip)
			{
				if (argsPart.IsNullOrEmpty())
					return string.Empty;

				int lastIndex = 0;
				var separators = new[] { ',', '=' };
				for (int i = 0; i < attributesToSkip; i++) // skip up to N args
				{
					var separatorIndex = argsPart.IndexOfAny(separators, lastIndex);
					if (separatorIndex < 0) // all args skipped
						return string.Empty;

					if (argsPart[separatorIndex] == '=') // first property initializer found, 'A = b'. Return rest.
						break;

					lastIndex = separatorIndex + 1;
				}

				return argsPart.Substring(lastIndex); // return all but first N args.
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
					metricRange.GetStringMinMax(metricUnit, out var minValueText, out var maxValueText);
					if (double.IsInfinity(metricRange.Min))
						minValueText = $"double.{nameof(double.NegativeInfinity)}";
					if (double.IsInfinity(metricRange.Max))
						maxValueText = $"double.{nameof(double.PositiveInfinity)}";

					switch (metric.SingleValueMode)
					{
						case MetricSingleValueMode.FromInfinityToMax:
							if (!double.IsInfinity(metricRange.Min))
							{
								result.Append(minValueText).Append(", ");
							}
							break;
						case MetricSingleValueMode.BothMinAndMax:
							if (double.IsInfinity(metricRange.Min) || minValueText != maxValueText)
							{
								result.Append(minValueText).Append(", ");
							}
							break;
						default:
							throw CodeExceptions.UnexpectedValue(
								$"Unknown single value mode of metric {metric}: {metric.SingleValueMode}.");
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
			#endregion

			#region Insert attribute
			private static bool TryInsertAttributeInplace(
				SourceCodeFile sourceCodeFile, int inplaceLineNumber,
				BenchmarkMethodInfo benchmarkMethod, CompetitionMetricValue metricValue,
				RuntimeTypeHandle attributeTypeHandle)
			{
				var line = sourceCodeFile[inplaceLineNumber];
				var inplacePosition = line.LastIndexOf(']');
				if (inplacePosition < 0)
					return false;

				var appendText = GetAppendAnnotationText(metricValue);
				line = line.Insert(inplacePosition, appendText);
				sourceCodeFile.ReplaceLine(inplaceLineNumber, line);

				benchmarkMethod.AddAttribute(attributeTypeHandle, inplaceLineNumber);

				return true;
			}

			private static int InsertLineWithAttribute(
				SourceCodeFile sourceCodeFile, int insertLineNumber,
				BenchmarkMethodInfo benchmarkMethod, CompetitionMetricValue metricValue,
				RuntimeTypeHandle attributeTypeHandle)
			{
				var whitespacePrefix = GetWhitespacePrefix(sourceCodeFile[insertLineNumber]);
				var newLine = GetNewAnnotationLine(whitespacePrefix, metricValue);

				var newLineNumber = insertLineNumber + 1;
				sourceCodeFile.InsertLine(newLineNumber, newLine);
				benchmarkMethod.AddAttribute(attributeTypeHandle, newLineNumber);

				return newLineNumber;
			}

			private static string GetWhitespacePrefix(string line) =>
				new string(line.TakeWhile(c => c.IsWhiteSpace()).ToArray());

			private static string GetNewAnnotationLine(string whitespacePrefix, CompetitionMetricValue metricValue)
			{
				var result = new StringBuilder();
				result.Append(whitespacePrefix);
				result.Append("[");
				result.Append(metricValue.Metric.AttributeType.GetAttributeName());
				result.Append("(");
				AppendFirstAttributeArgs(metricValue, result);
				result.Append(")]");
				return result.ToString();
			}

			private static string GetAppendAnnotationText(CompetitionMetricValue metricValue)
			{
				var result = new StringBuilder();
				result.Append(", ");
				result.Append(metricValue.Metric.AttributeType.GetAttributeName());
				result.Append("(");
				AppendFirstAttributeArgs(metricValue, result);
				result.Append(")");
				return result.ToString();
			}
			#endregion
			#endregion
		}
	}
}