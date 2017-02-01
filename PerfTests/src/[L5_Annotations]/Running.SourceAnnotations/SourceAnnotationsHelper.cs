using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

using CodeJam.Collections;
using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Running.Core;
using CodeJam.PerfTests.Running.Messages;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	internal static partial class SourceAnnotationsHelper
	{
		#region Annotation DOM
		[PublicAPI]
		private abstract class AnnotationFile
		{
			protected AnnotationFile(string path, bool parsed)
			{
				Code.NotNullNorEmpty(path, nameof(path));
				Path = path;
				Parsed = parsed;
			}

			public string Path { get; }
			public bool Parsed { get; }
			public bool HasChanges { get; private set; }

			[AssertionMethod]
			protected void AssertIsParsed()
			{
				if (!Parsed)
					throw CodeExceptions.InvalidOperation($"Trying to change non-parsed document {Path}.");
			}

			public void MarkAsChanged()
			{
				AssertIsParsed();
				HasChanges = true;
			}

			public void Save()
			{
				if (!Parsed)
					throw CodeExceptions.InvalidOperation($"Trying to save non-parsed document {Path}.");

				if (!HasChanges)
					return;

				SaveCore();
				HasChanges = false;
			}

			protected abstract void SaveCore();
		}

		private class AnnotationContext
		{
			// TODO: no global lock?
			private static readonly object _lockKey = new object();

			// Second-level cache; stores content of the files.
			// DONTTOUCH: the same file may be used in multiple assemblies. DO NOT change the caching schema.
			private readonly Dictionary<string, AnnotationFile> _filesCache =
				new Dictionary<string, AnnotationFile>();

			// First-level cache that stores match competition target => annotation document.
			private readonly Dictionary<TargetCacheKey, SourceCodeFile> _sourceAnnotationsCache =
				new Dictionary<TargetCacheKey, SourceCodeFile>();
			private readonly Dictionary<RuntimeTypeHandle, XmlAnnotationFile> _xmlAnnotationsCache =
				new Dictionary<RuntimeTypeHandle, XmlAnnotationFile>();

			public SourceCodeFile GetSourceCodeFile(
				[NotNull] CompetitionTarget target,
				[NotNull] CompetitionState competitionState)
			{
				lock (_lockKey)
				{
					return _sourceAnnotationsCache.GetOrAdd(target.TargetKey, t => GetSourceCodeFileCore(target, competitionState));
				}
			}

			[CanBeNull]
			private SourceCodeFile GetSourceCodeFileCore(
				[NotNull] CompetitionTarget target,
				[NotNull] CompetitionState competitionState)
			{
				var key = target.TargetKey;
				var sourcePath = SymbolHelper.TryGetSourcePath(MethodBase.GetMethodFromHandle(key.TargetMethod), competitionState);
				if (sourcePath == null)
					return null;

				var result = (SourceCodeFile)_filesCache.GetOrAdd(sourcePath, p => SourceAnnotation.Parse(target, sourcePath, competitionState));
				if (result.Parsed)
				{
					foreach (var method in result.BenchmarkMethods.Keys)
					{
						if (method != key.TargetMethod)
							_sourceAnnotationsCache.Add(new TargetCacheKey(target.TargetKey.TargetType, method), result);
					}
				}
				return result;
			}

			public XmlAnnotationFile GetXmlAnnotationFile(
				[NotNull] Type targetType,
				[NotNull] CompetitionMetadata competitionMetadata,
				[NotNull] CompetitionState competitionState)
			{
				lock (_lockKey)
				{
					return _xmlAnnotationsCache.GetOrAdd(
						targetType.TypeHandle,
						t => GetXmlAnnotationFileCore(targetType, competitionMetadata, competitionState));
				}
			}

			private XmlAnnotationFile GetXmlAnnotationFileCore(
				[NotNull] Type targetType,
				[NotNull] CompetitionMetadata competitionMetadata,
				[NotNull] CompetitionState competitionState)
			{
				var bf =
					BindingFlags.Static | BindingFlags.Instance |
					BindingFlags.Public | BindingFlags.NonPublic |
					BindingFlags.DeclaredOnly;

				// TODO: better sort?
				MethodBase anyMethod = targetType.GetMethods(bf).OrderBy(m => m.MetadataToken).FirstOrDefault();
				if (anyMethod == null)
					return null;

				anyMethod = targetType.GetConstructors(bf).OrderBy(m => m.MetadataToken).FirstOrDefault();
				if (anyMethod == null)
					return null;

				var sourcePath = SymbolHelper.TryGetSourcePath(anyMethod, competitionState);
				if (sourcePath == null)
					return null;

				// ReSharper disable once AssignNullToNotNullAttribute
				var resourceFileName = XmlAnnotation.GetResourcePath(sourcePath, competitionMetadata);

				return (XmlAnnotationFile)_filesCache.GetOrAdd(
					resourceFileName,
					p => XmlAnnotation.Parse(resourceFileName, competitionMetadata, competitionState));
			}


			public void Save()
			{
				lock (_lockKey)
				{
					foreach (var sourceDocument in _filesCache.Values)
					{
						sourceDocument.Save();
					}
				}
			}
		}

		private static class FileHashes
		{
			private const string Sha1AlgName = "SHA1";
			private const string Md5AlgName = "Md5";

			public static bool CheckResource(
				string file,
				ResourceKey resourceKey,
				CompetitionState competitionState)
			{
				var resourceChecksum = TryGetResourceSha1Checksum(resourceKey);
				return Check(file, ChecksumAlgorithm.Sha1, resourceChecksum, competitionState);
			}

			public static bool Check(
				string file,
				ChecksumAlgorithm checksumAlgorithm,
				byte[] expectedChecksum,
				CompetitionState competitionState)
			{
				switch (checksumAlgorithm)
				{
					case ChecksumAlgorithm.Md5:
						return CheckCore(file, Md5AlgName, expectedChecksum, competitionState);
					case ChecksumAlgorithm.Sha1:
						return CheckCore(file, Sha1AlgName, expectedChecksum, competitionState);
					default:
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.SetupError,
							$"Checksum validation failed, unknown hasing algorithm. File '{file}'.");
						return false;
				}
			}

			private static bool CheckCore(
				string file,
				string hashAlgName,
				byte[] expectedChecksum, CompetitionState competitionState)
			{
				var actualChecksum = TryGetChecksum(file, hashAlgName);
				if (expectedChecksum.EqualsTo(actualChecksum))
					return true;

				var expected = expectedChecksum.ToHexString();
				var actual = actualChecksum.ToHexString();
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.SetupError,
					$"{hashAlgName} checksum validation failed. File '{file}'." +
						$"{Environment.NewLine}\tActual: 0x{actual}" +
						$"{Environment.NewLine}\tExpected: 0x{expected}");

				return false;
			}

			[NotNull]
			private static byte[] TryGetChecksum(string file, string hashAlgName)
			{
				if (!File.Exists(file))
					return Array<byte>.Empty;

				using (var f = File.OpenRead(file))
				using (var h = HashAlgorithm.Create(hashAlgName))
				{
					// ReSharper disable once PossibleNullReferenceException
					return h.ComputeHash(f);
				}
			}

			[NotNull]
			private static byte[] TryGetResourceSha1Checksum(ResourceKey resourceKey)
			{
				using (var r = resourceKey.Assembly.GetManifestResourceStream(resourceKey.ResourceName))
				{
					if (r == null)
						return Array<byte>.Empty;
					using (var h = HashAlgorithm.Create(Sha1AlgName))
					{
						// ReSharper disable once PossibleNullReferenceException
						return h.ComputeHash(r);
					}
				}
			}
		}
		#endregion

		private static readonly Lazy<AnnotationContext> _annotationContext =
			Lazy.Create(() => new AnnotationContext());

		/// <summary>Tries to annotate source files with competition limits.</summary>
		/// <param name="targetsToAnnotate">Targets to annotate.</param>
		/// <param name="competitionMetadata">The competition metadata.</param>
		/// <param name="competitionState">State of the run.</param>
		/// <returns>Array of successfully annotated benchmarks.</returns>
		[NotNull]
		public static CompetitionTarget[] TryAnnotateBenchmarkFiles(
			[NotNull] CompetitionTarget[] targetsToAnnotate,
			[CanBeNull] CompetitionMetadata competitionMetadata,
			[NotNull] CompetitionState competitionState)
		{
			Code.NotNull(targetsToAnnotate, nameof(targetsToAnnotate));
			Code.NotNull(competitionState, nameof(competitionState));
			Code.BugIf(
				targetsToAnnotate.Any(t => t.Target.Type != competitionState.BenchmarkType),
				"Trying to annotate code that does not belongs to the benchmark.");

			var annotatedTargets = new List<CompetitionTarget>();
			var annContext = _annotationContext.Value;

			var xmlAnnotationDoc = competitionMetadata == null
				? null
				: annContext.GetXmlAnnotationFile(competitionState.BenchmarkType, competitionMetadata, competitionState);

			if (competitionMetadata != null && xmlAnnotationDoc == null)
			{
				competitionState.WriteMessage(
					MessageSource.Analyser, MessageSeverity.Warning,
					"Could not find XML annotation source file for the benchmark.");
				return new CompetitionTarget[0];
			}

			foreach (var targetToAnnotate in targetsToAnnotate)
			{
				var target = targetToAnnotate.Target;
				var targetMethodTitle = target.MethodDisplayInfo;

				var metrics = targetToAnnotate.MetricValues.Where(m => m.HasUnsavedChanges).ToArray();
				if (metrics.Length == 0)
					continue;

				foreach (var metricValue in metrics)
				{
					competitionState.WriteVerbose(
						$"Method {targetMethodTitle}: updating metric {metricValue.Metric} {metricValue}.");
				}

				if (competitionMetadata != null)
				{
					competitionState.WriteVerboseHint(
						$"Method {targetMethodTitle}: annotating resource file '{xmlAnnotationDoc.Path}'.");
					var annotated = XmlAnnotation.TryUpdate(xmlAnnotationDoc, competitionMetadata, targetToAnnotate);
					if (!annotated)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Warning,
							$"Method {targetMethodTitle}: could not find annotations in resource file '{xmlAnnotationDoc.Path}'.");
					}
					else
					{
						foreach (var metricValue in metrics)
						{
							competitionState.WriteVerboseHint(
								$"Method {targetMethodTitle}: metric {metricValue.Metric} {metricValue} updated.");
						}
						annotatedTargets.Add(targetToAnnotate);
					}
				}
				else
				{
					var doc = annContext.GetSourceCodeFile(targetToAnnotate, competitionState);
					if (doc == null)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Warning,
							$"Method {targetMethodTitle}: could not find source file for the method.");
						continue;
					}

					competitionState.WriteVerboseHint(
						$"Method {targetMethodTitle}: annotating file '{doc.Path}'");
					// TODO: log line???
					var annotated = SourceAnnotation.TryUpdate(doc, targetToAnnotate);
					if (!annotated)
					{
						competitionState.WriteMessage(
							MessageSource.Analyser, MessageSeverity.Warning,
							$"Method {targetMethodTitle}: could not find annotations in source file '{doc.Path}'.");
					}
					else
					{
						foreach (var metricValue in metrics)
						{
							competitionState.WriteVerboseHint(
								$"Method {targetMethodTitle}: metric {metricValue.Metric} {metricValue} updated.");
						}
						annotatedTargets.Add(targetToAnnotate);
					}
				}
			}

			annContext.Save();
			return annotatedTargets.ToArray();
		}
	}
}