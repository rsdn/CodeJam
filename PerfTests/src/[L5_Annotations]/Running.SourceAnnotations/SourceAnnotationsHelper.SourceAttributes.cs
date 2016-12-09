using System;
using System.Text;
using System.Text.RegularExpressions;

using CodeJam.PerfTests.Running.Core;
using CodeJam.Strings;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	internal static partial class SourceAnnotationsHelper
	{
		private static readonly Regex _breakIfRegex = new Regex(
			@"///|\sclass\s|\}|\;",
			RegexOptions.CultureInvariant | RegexOptions.Compiled);

		private static readonly Regex _attributeRegex = new Regex(
			@"
				(\[CompetitionBenchmark\(?)
				(
					(?:  \s*\-?\d+\.?\d*\s*)
					(?:\,\s*\-?\d+\.?\d*\s*)?
				)?
				(.*?\])",
			RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		private static bool TryFixBenchmarkAttribute(
			AnnotateContext annotateContext,
			string fileName, int firstCodeLine,
			CompetitionTarget competitionTarget,
			CompetitionState competitionState)
		{
			var sourceFileLines = annotateContext.TryGetFileLines(fileName, competitionState);
			if (sourceFileLines.Count == 0)
				return false;

			var attributeFixed = false;
			for (var i = firstCodeLine - 2; i >= 0; i--)
			{
				var line = sourceFileLines[i];
				if (_breakIfRegex.IsMatch(line))
					break;

				var hasMatch = false;
				var line2 = _attributeRegex.Replace(
					line,
					m => FixAttributeContent(m, competitionTarget, out hasMatch), 1);

				if (hasMatch)
				{
					if (line2 != line)
						annotateContext.ReplaceLine(fileName, i, line2);

					attributeFixed = true;
					break;
				}
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
	}
}