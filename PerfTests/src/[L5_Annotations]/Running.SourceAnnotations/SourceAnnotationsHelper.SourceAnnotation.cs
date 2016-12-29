using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using BenchmarkDotNet.Helpers;
using CodeJam.Collections;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Ranges;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	internal static partial class SourceAnnotationsHelper
	{
		private sealed class SourceCodeFile : AnnotationFile
		{
			private readonly string[] _sourceLines;
			private readonly Dictionary<RuntimeMethodHandle, int> _benchmarkAttributeLines;

			public SourceCodeFile(
				[NotNull] string path,
				[NotNull] string[] sourceLines,
				[NotNull] Dictionary<RuntimeMethodHandle, int> benchmarkAttributeLines) :
				base(
					path,
					sourceLines.Length > 0 && benchmarkAttributeLines.Count > 0)
			{
				_sourceLines = sourceLines;
				_benchmarkAttributeLines = benchmarkAttributeLines;
			}

			public IReadOnlyList<string> SourceLines => _sourceLines;
			public IReadOnlyDictionary<RuntimeMethodHandle, int> BenchmarkAttributeLines => _benchmarkAttributeLines;

			public void ReplaceLine(int lineIndex, string newLine)
			{
				AssertIsParsed();

				_sourceLines[lineIndex] = newLine;
				MarkAsChanged();
			}

			protected override void SaveCore() => BenchmarkHelpers.WriteFileContent(Path, _sourceLines);
		}

		private static class SourceAnnotation
		{
			private static readonly Regex _attributeRegex = new Regex(
			@"
				(\[CompetitionBenchmark\(?)
				(
					(?:  \s*\-?\d+\.?\d*\s*)
					(?:\,\s*\-?\d+\.?\d*\s*)?
				)?
				(.*?\])",
			RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.Compiled);

			#region Parse
			[NotNull]
			public static SourceCodeFile Parse(
				CompetitionTarget target,
				string sourcePath,
				CompetitionState competitionState)
			{
				var attributeLines = new Dictionary<RuntimeMethodHandle, int>();
				var sourceLines = Array<string>.Empty;
				var documentInfo = SymbolHelper.TryGetSourceInfo(target.Target.Method, competitionState);
				if (documentInfo == null)
					return new SourceCodeFile(sourcePath, sourceLines, attributeLines);

				Code.BugIf(documentInfo.Path != sourcePath, "documentInfo.Path != sourcePath");

				if (!TryValidate(documentInfo, competitionState))
					return new SourceCodeFile(sourcePath, sourceLines, attributeLines);

				var map = documentInfo.MethodMap;
				var noCodeRanges = map.GetComplementation().MakeInclusive(i => i, i => i);
				var candidateLines = map.SubRanges.Zip(noCodeRanges.SubRanges, (r1, r2) => r2.WithKey(r1.Key)).ToCompositeRange();
				Code.BugIf(
					candidateLines.SubRanges.Count != map.SubRanges.Count,
					"candidateLines.SubRanges.Count != map.SubRanges.Count");

				sourceLines = TryReadFileContent(sourcePath, competitionState);
				if (sourceLines.Length == 0)
					return new SourceCodeFile(sourcePath, sourceLines, attributeLines);

				FillAttributeLines(attributeLines, candidateLines, sourceLines, sourcePath, competitionState);
				return new SourceCodeFile(sourcePath, sourceLines, attributeLines);
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

				if (documentInfo.MethodMap.IsEmpty)
				{
					// TODO: improve message
					competitionState.WriteMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"No methods found in document '{documentInfo.Path}'.");
					result = false;
				}

				if (!documentInfo.MethodMap.IsMerged)
				{
					var methodsIntersection = documentInfo.MethodMap.GetIntersections().FirstOrDefault(i => i.Ranges.Count > 1);
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

			private static void FillAttributeLines(
				Dictionary<RuntimeMethodHandle, int> attributeLines,
				CompositeRange<int, MethodBase> candidateLines,
				string[] sourceLines,
				string sourcePath,
				CompetitionState competitionState)
			{
				foreach (var candidateRange in candidateLines.SubRanges)
				{
					var attribute = candidateRange.Key.GetCustomAttribute<CompetitionBenchmarkAttribute>();
					// TODO: improve check
					if (attribute == null || attribute.GetType() != typeof(CompetitionBenchmarkAttribute))
						continue;

					var line = TryParseLine(candidateRange, sourceLines);
					if (line != null)
					{
						attributeLines.Add(candidateRange.Key.MethodHandle, line.GetValueOrDefault());
					}
					else
					{
						// TODO: improve message
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.SetupError,
							$"Cannot find attribute for {candidateRange}. File '{sourcePath}'.");
					}
				}
			}

			private static int? TryParseLine(
				Range<int, MethodBase> candidateRange, string[] lines)
			{
				for (var i = candidateRange.ToValue; i >= candidateRange.From.GetValueOrDefault(); i--)
				{
					// Line numbering starts from 1
					if (_attributeRegex.IsMatch(lines[i - 1]))
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

				if (!sourceCodeFile.BenchmarkAttributeLines.TryGetValue(
					competitionTarget.TargetKey.TargetMethod,
					out var attributeLine))
					return false;

				// Line numbering starts from 1
				attributeLine -= 1;

				var attributeFixed = false;

				var line = sourceCodeFile.SourceLines[attributeLine];

				var hasMatch = false;
				var line2 = _attributeRegex.Replace(
					line,
					m => FixAttributeContent(m, competitionTarget, out hasMatch),
					1);

				if (hasMatch)
				{
					if (line2 != line)
						sourceCodeFile.ReplaceLine(attributeLine, line2);

					attributeFixed = true;
				}
				return attributeFixed;
			}

			// ReSharper disable once SuggestBaseTypeForParameter
			private static string FixAttributeContent(Match m, CompetitionTarget competitionTarget, out bool hasMatch)
			{
				var attributeStartText = m.Groups[1].Value;
				var attributeEndText = m.Groups[3].Value;

				var attributeWithoutBraces = !attributeStartText.EndsWith("(");
				var attributeWithoutMinMax = !m.Groups[2].Success;
				var attributeHasAdditionalContent = !attributeEndText.StartsWith(")");

				var result = new StringBuilder(m.Length + 10);
				result.Append(attributeStartText);

				if (attributeWithoutBraces)
				{
					result.Append('(');
					AppendMinMax(result, competitionTarget);
					result.Append(')');
				}
				else
				{
					AppendMinMax(result, competitionTarget);
					if (attributeWithoutMinMax && attributeHasAdditionalContent)
					{
						result.Append(", ");
					}
				}

				result.Append(attributeEndText);

				hasMatch = true;
				return result.ToString();
			}

			// ReSharper disable once SuggestBaseTypeForParameter
			private static void AppendMinMax(StringBuilder result, CompetitionTarget competitionTarget)
			{
				var min = competitionTarget.Limits.MinRatioText;
				var max = competitionTarget.Limits.MaxRatioText;

				if (min.NotNullNorEmpty())
				{
					result.Append(min);
					result.Append(", ");
				}

				result.Append(max);
			}
			#endregion
		}
	}
}