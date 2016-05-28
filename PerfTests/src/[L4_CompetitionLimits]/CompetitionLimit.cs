using System;

using BenchmarkDotNet.Helpers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests
{
	/// <summary>
	/// Class that describes limits for benchmarks participating in competition.
	/// </summary>
	[PublicAPI]
	public class CompetitionLimit
	{
		/// <summary>Marks the competition limit as ignored.</summary>
		public const int IgnoreValue = -1;

		/// <summary>Marks the competition limit as unset.</summary>
		public const int NotSetValue = 0;

		private const int IgnoreUpperLimit = 0;

		public const string RatioFormat = "0.00";
		public const string ActualRatioFormat = "0.000";

		#region Limit helper methods (reusable methods for memory and timing limits)
		[AssertionMethod]
		protected static void AssertLimitArgument(double value, [InvokerParameterName] string argName)
		{
			if (IsInvalidValue(value))
			{
				throw CodeExceptions.Argument(argName, $"The {argName} value ({value}) should be a finite number.");
			}
		}

		protected static double AdjustLimitArgument(double value, [InvokerParameterName] string argName)
		{
			AssertLimitArgument(value, argName);
			return IsIgnored(value) ? IgnoreValue : value;
		}

		protected static bool IsInvalidValue(double value) => double.IsInfinity(value) || double.IsNaN(value);

		protected static bool IsIgnored(double value) => value < IgnoreUpperLimit;

		// ReSharper disable once CompareOfFloatsByEqualityOperator
		protected static bool IsUnset(double value) => value == NotSetValue;

		protected bool IsMinLimitOk(double minLimit, double value)
		{
			AssertLimitArgument(minLimit, nameof(minLimit));

			if (IsInvalidValue(value) || IsIgnored(value) || IsUnset(value))
				return true;

			if (IsIgnored(minLimit))
				return true;

			if (IsUnset(minLimit))
				return false;

			return value >= minLimit;
		}

		protected bool IsMaxLimitOk(double maxLimit, double value)
		{
			AssertLimitArgument(maxLimit, nameof(maxLimit));

			if (IsInvalidValue(value) || IsIgnored(value) || IsUnset(value))
				return true;

			if (IsIgnored(maxLimit))
				return true;

			if (IsUnset(maxLimit))
				return false;

			return value <= maxLimit;
		}

		protected bool ShouldBeUpdatedMin(double minLimit, double newMinLimit)
		{
			AssertLimitArgument(minLimit, nameof(minLimit));

			if (IsInvalidValue(newMinLimit) || IsIgnored(newMinLimit) || IsUnset(newMinLimit))
				return false;

			if (IsIgnored(minLimit))
				return false;

			if (IsUnset(minLimit))
				return true;

			return newMinLimit < minLimit;
		}

		protected bool ShouldBeUpdatedMax(double maxLimit, double newMaxLimit)
		{
			AssertLimitArgument(maxLimit, nameof(maxLimit));

			if (IsInvalidValue(newMaxLimit) || IsIgnored(newMaxLimit) || IsUnset(newMaxLimit))
				return false;

			if (IsIgnored(maxLimit))
				return false;

			if (IsUnset(maxLimit))
				return true;

			return newMaxLimit > maxLimit;
		}
		#endregion

		public static readonly CompetitionLimit Ignored = new CompetitionLimit(IgnoreValue, IgnoreValue);
		public static readonly CompetitionLimit Empty = new CompetitionLimit(NotSetValue, NotSetValue);

		public CompetitionLimit(
			double minRatio, double maxRatio)
		{
			MinRatio = AdjustLimitArgument(minRatio, nameof(minRatio));
			MaxRatio = AdjustLimitArgument(maxRatio, nameof(maxRatio));
		}

		public double MinRatio { get; protected set; }
		public double MaxRatio { get; protected set; }

		public bool IgnoreMin => IsIgnored(MinRatio);
		public bool IgnoreMax => IsIgnored(MaxRatio);

		public bool MinIsOk(double value) => IsMinLimitOk(MinRatio, value);
		public bool MaxIsOk(double value) => IsMaxLimitOk(MaxRatio, value);

		public string MinText => IgnoreMin
			? MinRatio.ToString(EnvironmentInfo.MainCultureInfo)
			: MinRatio.ToString(RatioFormat, EnvironmentInfo.MainCultureInfo);
		public string MaxText => IgnoreMax
			? MaxRatio.ToString(EnvironmentInfo.MainCultureInfo)
			: MaxRatio.ToString(RatioFormat, EnvironmentInfo.MainCultureInfo);

		public bool IsEmpty => IsUnset(MinRatio) && IsUnset(MaxRatio);
		public bool IgnoreAll => IgnoreMin && IgnoreMax;
	}
}