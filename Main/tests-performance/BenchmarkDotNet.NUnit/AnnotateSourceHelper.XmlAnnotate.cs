using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Linq;

using JetBrains.Annotations;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.NUnit
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
			var competition = GetOrAdd(xdoc.Root, CompetitionTargetHelpers.CompetitionNode, competitionName);
			var candidate = GetOrAdd(competition, CompetitionTargetHelpers.CandidateNode, candidateName);

			var minText = !competitionTarget.IgnoreMin ? competitionTarget.MinText : null;
			// Always prints
			var maxText = competitionTarget.MaxText;

			SetAttribute(candidate, CompetitionTargetHelpers.MinRatioAttribute, minText);
			SetAttribute(candidate, CompetitionTargetHelpers.MaxRatioAttribute, maxText);

			annotateContext.MarkAsChanged(xmlFileName);

			return true;
		}

		private static XElement GetOrAdd(XElement element, XName name, string targetName)
		{
			if (targetName == null)
				throw new ArgumentNullException(nameof(targetName));

			var result = element
				.Elements(name)
				.SingleOrDefault(e => e.Attribute(CompetitionTargetHelpers.TargetAttribute)?.Value == targetName);

			if (result == null)
			{
				result = new XElement(name);
				SetAttribute(result, CompetitionTargetHelpers.TargetAttribute, targetName);
				element.Add(result);
			}

			return result;
		}

		[CanBeNull]
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