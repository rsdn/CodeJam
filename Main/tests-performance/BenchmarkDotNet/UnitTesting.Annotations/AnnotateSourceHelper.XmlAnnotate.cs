using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

using JetBrains.Annotations;

using static BenchmarkDotNet.UnitTesting.CompetitionHelpers;

// ReSharper disable CheckNamespace
namespace BenchmarkDotNet.UnitTesting
{
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	internal static partial class AnnotateSourceHelper
	{
		private static bool TryFixBenchmarkResource(
			AnnotateContext annotateContext, string xmlFileName,
			CompetitionTarget competitionTarget)
		{
			var competitionName = competitionTarget.CompetitionName;
			var candidateName = competitionTarget.CandidateName;

			var xdoc = annotateContext.GetXmlAnnotation(xmlFileName);
			var competition = GetOrAdd(xdoc.Root, CompetitionNode, competitionName);
			var candidate = GetOrAdd(competition, CandidateNode, candidateName);

			var minText = !competitionTarget.IgnoreMin ? competitionTarget.MinText : null;
			// Always prints
			var maxText = competitionTarget.MaxText;

			SetAttribute(candidate, MinRatioAttribute, minText);
			SetAttribute(candidate, MaxRatioAttribute, maxText);

			annotateContext.MarkAsChanged(xmlFileName);

			return true;
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		private static XElement GetOrAdd(XElement element, XName name, string targetName)
		{
			if (targetName == null)
				throw new ArgumentNullException(nameof(targetName));

			var result = element
				.Elements(name)
				.SingleOrDefault(e => e.Attribute(TargetAttribute)?.Value == targetName);

			if (result == null)
			{
				result = new XElement(name);
				SetAttribute(result, TargetAttribute, targetName);
				element.Add(result);
			}

			return result;
		}

		[CanBeNull]
		// ReSharper disable once UnusedMethodReturnValue.Local
		private static XAttribute SetAttribute(XElement element, XName attributeName, string attributeValue)
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
	}
}