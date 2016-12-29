using System;
using System.IO;
using System.Xml.Linq;

using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>Core logic for source annotations.</summary>
	internal static partial class SourceAnnotationsHelper
	{
		private sealed class XmlAnnotationFile : AnnotationFile
		{
			public XmlAnnotationFile(
				[NotNull] string path,
				[CanBeNull] XDocument xmlAnnotationDoc) : base(path, xmlAnnotationDoc != null)
			{
				XmlAnnotationDoc = xmlAnnotationDoc;
			}

			[CanBeNull]
			public XDocument XmlAnnotationDoc { get; }

			protected override void SaveCore()
			{
				using (var writer = new StreamWriter(Path))
				{
					// ReSharper disable once AssignNullToNotNullAttribute
					XmlAnnotations.Save(XmlAnnotationDoc, writer);
				}
			}
		}

		private static class XmlAnnotation
		{
			[NotNull]
			public static string GetResourcePath(
				[NotNull] string sourcePath,
				[NotNull] CompetitionMetadata competitionMetadata)
			{
				if (competitionMetadata.MetadataResourcePath.NotNullNorEmpty())
				{
					// ReSharper disable once AssignNullToNotNullAttribute
					return Path.Combine(
						Path.GetDirectoryName(sourcePath),
						competitionMetadata.MetadataResourcePath);
				}

				return Path.ChangeExtension(sourcePath, ".xml");
			}

			[NotNull]
			public static XmlAnnotationFile Parse(
				[NotNull] string resourcePath,
				[NotNull] CompetitionMetadata competitionMetadata,
				[NotNull] CompetitionState competitionState)
			{
				var resourceKey = competitionMetadata.MetadataResourceKey;
				if (!FileHashes.CheckResource(resourcePath, resourceKey, competitionState))
					return new XmlAnnotationFile(resourcePath, null);

				var xmlAnnotationDoc = TryParseXmlAnnotationDoc(resourcePath, competitionState);
				return new XmlAnnotationFile(resourcePath, xmlAnnotationDoc);
			}

			[CanBeNull]
			private static XDocument TryParseXmlAnnotationDoc(
				string resourcePath,
				CompetitionState competitionState)
			{
				try
				{
					using (var stream = File.OpenRead(resourcePath))
					{
						return XmlAnnotations.TryParseXmlAnnotationDoc(
							stream, competitionState,
							$"XML annotation '{resourcePath}'");
					}
				}
				catch (IOException ex)
				{
					competitionState.WriteExceptionMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Could not access file '{resourcePath}'.", ex);

					return null;
				}
				catch (UnauthorizedAccessException ex)
				{
					competitionState.WriteExceptionMessage(
						MessageSource.Analyser, MessageSeverity.SetupError,
						$"Could not access file '{resourcePath}'.", ex);

					return null;
				}
			}

			public static bool TryUpdate(
				[NotNull] XmlAnnotationFile xmlAnnotationFile,
				[NotNull] CompetitionTarget competitionTarget)
			{
				Code.NotNull(competitionTarget.CompetitionMetadata, nameof(competitionTarget.CompetitionMetadata));
				if (!xmlAnnotationFile.Parsed)
					return false;

				Code.NotNull(xmlAnnotationFile.XmlAnnotationDoc, nameof(xmlAnnotationFile.XmlAnnotationDoc));
				XmlAnnotations.AddOrUpdateXmlAnnotation(
					xmlAnnotationFile.XmlAnnotationDoc,
					competitionTarget,
					competitionTarget.CompetitionMetadata.UseFullTypeName);

				xmlAnnotationFile.MarkAsChanged();

				return true;
			}
		}
	}
}