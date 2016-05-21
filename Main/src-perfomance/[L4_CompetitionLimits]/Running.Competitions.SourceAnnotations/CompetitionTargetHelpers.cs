using System;

using BenchmarkDotNet.Competitions;

namespace BenchmarkDotNet.Running.Competitions.SourceAnnotations
{
	internal static class CompetitionTargetHelpers
	{
		public static bool MergeWith(this CompetitionTarget competitionTarget, CompetitionLimit newProperties)
		{
			bool result = false;
			result |= competitionTarget.UnionWithMin(newProperties.Min);
			result |= competitionTarget.UnionWithMax(newProperties.Max);
			return result;
		}

		// TODO:  propertiesToLoose
		public static void LooseLimits(
			this CompetitionTarget competitionTarget,
			int percent)
		{
			if (percent < 0 || percent > 100)
			{
				throw new ArgumentOutOfRangeException(
					nameof(percent), percent,
					$"The {nameof(percent)} should be in range (0..100).");
			}

			if (competitionTarget.IsChanged(CompetitionTargetProperties.MinRatio))
			{
				var newValue = Math.Floor(competitionTarget.Min * (100 - percent)) / 100;
				competitionTarget.UnionWithMin(newValue);
			}
			if (competitionTarget.IsChanged(CompetitionTargetProperties.MaxRatio))
			{
				var newValue = Math.Ceiling(competitionTarget.Max * (100 + percent)) / 100;
				competitionTarget.UnionWithMin(newValue);
			}
		}
	}
}