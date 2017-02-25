using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

using BenchmarkDotNet.Helpers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using CodeJam.Collections;
using CodeJam.PerfTests.Analysers;
using CodeJam.PerfTests.Metrics;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Helpers for <see cref="IAnnotationStorage"/> implementations
	/// </summary>
	internal static class AnnotationHelpers
	{
		/// <summary>Gets the benchmark targets.</summary>
		/// <param name="summary">Summary for the run.</param>
		/// <returns></returns>
		public static Target[] GetBenchmarkTargets(this Summary summary) =>
			summary.GetSummaryOrderBenchmarks()
				.Select(b => b.Target)
				.Distinct()
				.ToArray();

		/// <summary>Converts <see cref="IStoredMetricValue"/> to the competition metric value.</summary>
		/// <param name="storedMetric">The stored metric.</param>
		/// <param name="metric">The metric information.</param>
		/// <returns>The competition metric value.</returns>
		public static CompetitionMetricValue ToMetricValue(
			this IStoredMetricValue storedMetric,
			MetricInfo metric)
		{
			Code.BugIf(
				storedMetric.MetricAttributeType != metric.AttributeType,
				"storedMetric.MetricAttributeType != metric.AttributeType");

			var metricUnit = metric.MetricUnits[storedMetric.UnitOfMeasurement];

			var metricValues = MetricValueHelpers.CreateMetricRange(storedMetric.Min, storedMetric.Max, metricUnit);
			return new CompetitionMetricValue(
				metric,
				metricValues,
				metricUnit);
		}

		/// <summary>Gets resource stream.</summary>
		/// <param name="resourceKey">The resource key.</param>
		/// <returns>A resource stream.</returns>
		[CanBeNull]
		public static Stream TryGetResourceStream(this ResourceKey resourceKey) =>
			resourceKey.Assembly.GetManifestResourceStream(resourceKey.ResourceName);

		#region Hashes
		private const string Sha1AlgName = "SHA1";
		private const string Md5AlgName = "Md5";

		private static string GetChecksumName(ChecksumAlgorithm checksumAlgorithm)
		{
			switch (checksumAlgorithm)
			{
				case ChecksumAlgorithm.Md5:
					return Md5AlgName;
				case ChecksumAlgorithm.Sha1:
					return Sha1AlgName;
				default:
					throw CodeExceptions.UnexpectedArgumentValue(nameof(checksumAlgorithm), checksumAlgorithm);
			}
		}

		/// <summary>Gets checksum for file if it exists.</summary>
		/// <param name="file">The file.</param>
		/// <param name="checksumAlgorithm">The checksum algorithm to use.</param>
		/// <returns>Checksum for file or empty byte array if the file does not exist.</returns>
		public static byte[] TryGetChecksum(
			string file,
			ChecksumAlgorithm checksumAlgorithm)
		{
			var algName = GetChecksumName(checksumAlgorithm);

			if (!File.Exists(file))
				return Array<byte>.Empty;

			using (var f = File.OpenRead(file))
			using (var h = HashAlgorithm.Create(algName))
			{
				// ReSharper disable once PossibleNullReferenceException
				return h.ComputeHash(f);
			}
		}

		/// <summary>Gets checksum for resource if it exists.</summary>
		/// <param name="resourceKey">The resource key.</param>
		/// <param name="checksumAlgorithm">The checksum algorithm to use.</param>
		/// <returns>Checksum for resource or empty byte array if the resource does not exist.</returns>
		public static byte[] TryGetChecksum(
			ResourceKey resourceKey,
			ChecksumAlgorithm checksumAlgorithm)
		{
			var algName = GetChecksumName(checksumAlgorithm);

			using (var s = resourceKey.TryGetResourceStream())
			{
				if (s == null)
					return Array<byte>.Empty;

				using (var h = HashAlgorithm.Create(algName))
				{
					// ReSharper disable once PossibleNullReferenceException
					return h.ComputeHash(s);
				}
			}
		}
		#endregion
	}
}