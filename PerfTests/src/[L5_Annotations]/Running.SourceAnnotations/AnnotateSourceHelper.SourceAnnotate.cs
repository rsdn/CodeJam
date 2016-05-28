using System;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	internal static partial class AnnotateSourceHelper
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
			CompetitionTarget competitionTarget)
		{
			var result = false;
			var sourceFileLines = annotateContext.GetFileLines(fileName);

			for (var i = firstCodeLine - 2; i >= 0; i--)
			{
				var line = sourceFileLines[i];
				if (_breakIfRegex.IsMatch(line))
					break;

				var line2 = _attributeRegex.Replace(
					line,
					m => FixAttributeContent(m, competitionTarget), 1);
				if (line2 != line)
				{
					annotateContext.ReplaceLine(fileName, i, line2);
					result = true;
					break;
				}
			}
			return result;
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		private static string FixAttributeContent(Match m, CompetitionTarget competitionTarget)
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
			return result.ToString();
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		private static void AppendMinMax(StringBuilder result, CompetitionTarget competitionTarget)
		{
			if (!competitionTarget.IgnoreMin)
			{
				result.Append(competitionTarget.MinText);
				result.Append(", ");
			}

			// MaxText should be specified even if ignored.
			result.Append(competitionTarget.MaxText);
		}
	}
}