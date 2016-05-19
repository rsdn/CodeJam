using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using BenchmarkDotNet.Competitions;

using JetBrains.Annotations;

using static BenchmarkDotNet.Competitions.CompetitionLimitConstants;

// ReSharper disable CheckNamespace

namespace BenchmarkDotNet.Running.Competitions.SourceAnnotations
{
	[SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
	internal static partial class AnnotateSourceHelper
	{
		private class UseFullNameAnnotation
		{
			public static readonly UseFullNameAnnotation Instance = new UseFullNameAnnotation();
			private UseFullNameAnnotation() { }
		}

		public static XmlWriterSettings GetXmlWriterSettings() =>
			new XmlWriterSettings
				{
					Indent = true,
					IndentChars = "\t",
					NewLineChars = "\r\n"
				};


		private static bool TryFixBenchmarkResource(
			AnnotateContext annotateContext, string xmlFileName,
			CompetitionTarget competitionTarget)
		{
			var xDoc = annotateContext.GetXmlAnnotation(xmlFileName);
			UpdateXmlAnnotation(competitionTarget, xDoc);

			annotateContext.MarkAsChanged(xmlFileName);

			return true;
		}

		public static XDocument CreateXmlAnnotationDoc(bool competitionFullName)
		{
			var result = new XDocument(new XElement(CompetitionBenchmarksRootNode));
			if (competitionFullName)
			{
				result.AddAnnotation(UseFullNameAnnotation.Instance);
			}
			return result;
		}

		public static void UpdateXmlAnnotation(
			CompetitionTarget competitionTarget,
			XDocument xDoc)
		{
			var competitionName = xDoc.Annotations<UseFullNameAnnotation>().Any()
				? competitionTarget.CompetitionFullName
				: competitionTarget.CompetitionName;
			var candidateName = competitionTarget.CandidateName;
			var competition = GetOrAddForTarget(xDoc.Root, CompetitionNode, competitionName);
			var candidate = GetOrAddForTarget(competition, CandidateNode, candidateName);

			var minText = !competitionTarget.IgnoreMin ? competitionTarget.MinText : null;
			// MaxText should be specified even if ignored.
			var maxText = competitionTarget.MaxText;

			SetAttribute(candidate, MinRatioAttribute, minText);
			SetAttribute(candidate, MaxRatioAttribute, maxText);
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		[NotNull]
		private static XElement GetOrAddForTarget(XElement element, XName name, string targetName)
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