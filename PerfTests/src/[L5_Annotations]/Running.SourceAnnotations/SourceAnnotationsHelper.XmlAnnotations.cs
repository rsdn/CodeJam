using System;
using System.IO;

using CodeJam.PerfTests.Running.Core;
using CodeJam.Strings;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>Core logic for source annotations.</summary>
	internal static partial class SourceAnnotationsHelper
	{
		private static string GetResourceFileName(string fileName, CompetitionMetadata competitionMetadata)
		{
			if (competitionMetadata.MetadataResourcePath.NotNullNorEmpty())
			{
				var dir = Path.GetDirectoryName(fileName);

				return dir.IsNullOrEmpty()
					? competitionMetadata.MetadataResourcePath
					: Path.Combine(dir, competitionMetadata.MetadataResourcePath);
			}

			return Path.ChangeExtension(fileName, ".xml");
		}

		private static bool TryFixBenchmarkXmlAnnotation(
			AnnotateContext annotateContext, string xmlFileName,
			CompetitionTarget competitionTarget,
			CompetitionState competitionState)
		{
			Code.AssertArgument(
				competitionTarget.CompetitionMetadata != null, nameof(competitionTarget),
				"Competition metadata cannot be null for xml annotations.");

			var xmlAnnotationDoc = annotateContext.TryGetXmlAnnotation(
				xmlFileName,
				competitionTarget.CompetitionMetadata.UseFullTypeName,
				competitionState);
			if (xmlAnnotationDoc == null)
				return false;

			XmlAnnotations.AddOrUpdateXmlAnnotation(xmlAnnotationDoc, competitionTarget);
			annotateContext.MarkAsChanged(xmlFileName);

			return true;
		}
	}
}