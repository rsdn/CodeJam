using System;
using System.Reflection;

using BenchmarkDotNet.Competitions;

namespace BenchmarkDotNet.Running.Competitions.SourceAnnotations
{
	internal static class AttributeAnnotations
	{
		public static string TryGetTargetResourceName(Target target)
		{
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

		public static CompetitionTarget ParseCompetitionTarget(
			Target target, CompetitionBenchmarkAttribute competitionAttribute) =>
				new CompetitionTarget(
					target,
					competitionAttribute.MinRatio,
					competitionAttribute.MaxRatio,
					false);
	}
}