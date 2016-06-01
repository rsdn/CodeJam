using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

using CodeJam.PerfTests.Running.Core;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	[SuppressMessage("ReSharper", "SuggestVarOrType_BuiltInTypes")]
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
			CompetitionTarget competitionTarget,
			CompetitionState competitionState)
		{
			var sourceFileLines = annotateContext.TryGetFileLines(fileName, competitionState);
			if (sourceFileLines == null)
				return false;

			bool attributeFixed = false;
			for (var i = firstCodeLine - 2; i >= 0; i--)
			{
				var line = sourceFileLines[i];
				if (_breakIfRegex.IsMatch(line))
					break;

				bool hasMatch = false;
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
			if (!competitionTarget.IgnoreMinRatio)
			{
				result.Append(competitionTarget.MinRatioText);
				result.Append(", ");
			}

			// MaxText should be specified even if ignored.
			result.Append(competitionTarget.MaxRatioText);
		}
	}
}