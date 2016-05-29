using System;
using System.Reflection;

using BenchmarkDotNet.Running;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Helper class for attribute annotations
	/// </summary>
	internal static class AttributeAnnotations
	{
		/// <summary>
		/// Returns the name of target resource if defined in <seealso cref="CompetitionMetadataAttribute"/>.
		/// If the target type is nested all container types are checked too.
		/// </summary>
		/// <param name="target">The target to get resource name for.</param>
		/// <returns>
		/// Name of the resource containing xml document with competition limits
		/// or <c>null</c> if the target is not annotated with <seealso cref="CompetitionMetadataAttribute"/>
		/// </returns>
		public static string TryGetTargetResourceName([NotNull] Target target)
		{
			Code.NotNull(target, nameof(target));

			string targetResourceName = null;

			var targetType = target.Type;
			while (targetType != null && targetResourceName == null)
			{
				targetResourceName = targetType
					.GetCustomAttribute<CompetitionMetadataAttribute>()
					?.MetadataResourceName;

				targetType = targetType.DeclaringType;
			}

			return targetResourceName;
		}

		/// <summary>
		/// Creates <seealso cref="CompetitionTarget"/> from <seealso cref="CompetitionBenchmarkAttribute"/>.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="competitionAttribute">The attribute with competition limits.</param>
		/// <returns>
		/// A new instance of the <see cref="CompetitionTarget"/> class
		/// filled with the properties from <seealso cref="CompetitionBenchmarkAttribute"/>
		/// </returns>
		public static CompetitionTarget ParseCompetitionTarget(
			[NotNull] Target target,
			[NotNull] CompetitionBenchmarkAttribute competitionAttribute)
		{
			Code.NotNull(target, nameof(target));
			Code.NotNull(competitionAttribute, nameof(competitionAttribute));

			return new CompetitionTarget(
				target,
				competitionAttribute.MinRatio,
				competitionAttribute.MaxRatio,
				false);
		}
	}
}