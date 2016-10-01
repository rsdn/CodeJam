using System;

using BenchmarkDotNet.Helpers;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Limits
{
	/// <summary>Class that describes limits for benchmarks participating in competition.</summary>
	[PublicAPI]
	public class CompetitionLimit
	{
		#region Static members
		/// <summary>Empty value for the competition limit.</summary>
		public const int EmptyValue = 0;

		/// <summary>Ignored value for the competition limit..</summary>
		public const int IgnoreValue = -1;

		/// <summary>Default format for ratio (relative) limits.</summary>
		private const string RatioFormat = "0.00";

		/// <summary>The empty competition limit.</summary>
		public static readonly CompetitionLimit Empty = new CompetitionLimit(EmptyValue, EmptyValue);

		/// <summary>The ignored (will not be checked) competition limit.</summary>
		public static readonly CompetitionLimit Ignored = new CompetitionLimit(IgnoreValue, IgnoreValue);

		#region Core logic for competition limits
		private static bool IsInvalidValue(double value) => double.IsInfinity(value) || double.IsNaN(value);

		private static bool IsIgnoredValue(double value) => value < EmptyValue;

		// ReSharper disable once CompareOfFloatsByEqualityOperator
		private static bool IsEmptyValue(double value) => value == EmptyValue;

		[AssertionMethod]
		private static void AssertLimitArgument(double value, [InvokerParameterName] string argName)
		{
			if (IsInvalidValue(value))
			{
				throw CodeExceptions.Argument(argName, $"The {argName} value ({value}) should be a finite number.");
			}
		}

		private static double AdjustLimitArgument(double value, [InvokerParameterName] string argName)
		{
			AssertLimitArgument(value, argName);

			return IsIgnoredValue(value) ? IgnoreValue : value;
		}
		#endregion

		#region Limit helper methods
		/// <summary>
		/// Helper method for checking the value against min limit.
		/// </summary>
		/// <param name="minLimit">The limit.</param>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c> if the value fits into limit.</returns>
		protected static bool IsMinLimitOk(double minLimit, double value)
		{
			AssertLimitArgument(minLimit, nameof(minLimit));

			if (IsInvalidValue(value) || IsIgnoredValue(value) || IsEmptyValue(value))
				return true;

			if (IsIgnoredValue(minLimit))
				return true;

			if (IsEmptyValue(minLimit))
				return false;

			return value >= minLimit;
		}

		/// <summary>
		/// Helper method for checking the value against max limit.
		/// </summary>
		/// <param name="maxLimit">The limit.</param>
		/// <param name="value">The value to check.</param>
		/// <returns><c>true</c> if the value fits into limit.</returns>
		protected static bool IsMaxLimitOk(double maxLimit, double value)
		{
			AssertLimitArgument(maxLimit, nameof(maxLimit));

			if (IsInvalidValue(value) || IsIgnoredValue(value) || IsEmptyValue(value))
				return true;

			if (IsIgnoredValue(maxLimit))
				return true;

			if (IsEmptyValue(maxLimit))
				return false;

			return value <= maxLimit;
		}

		/// <summary>
		/// Helper method for checking before extending the min limit.
		/// IMPORTANT: DO NOT replace with call to <see cref="IsMinLimitOk"/>.
		/// Implementation may change in the future.
		/// </summary>
		/// <param name="minLimit">The minimum limit.</param>
		/// <param name="newMinLimit">The new minimum limit.</param>
		/// <returns><c>true</c> if the current limit should be replaced with new one.</returns>
		protected static bool ShouldBeUpdatedMin(double minLimit, double newMinLimit) =>
			!IsMinLimitOk(minLimit, newMinLimit);

		/// <summary>
		/// Helper method for checking before extending the max limit.
		/// IMPORTANT: DO NOT replace with call to <see cref="IsMaxLimitOk"/>.
		/// Implementation may change in the future.
		/// </summary>
		/// <param name="maxLimit">The maximum limit.</param>
		/// <param name="newMaxLimit">The new maximum limit.</param>
		/// <returns><c>true</c> if the current limit should be replaced with new one.</returns>
		protected static bool ShouldBeUpdatedMax(double maxLimit, double newMaxLimit) =>
			!IsMaxLimitOk(maxLimit, newMaxLimit);
		#endregion

		#endregion

		/// <summary>Initializes a new instance of the <see cref="CompetitionLimit"/> class.</summary>
		/// <param name="minRatio">The minimum timing ratio relative to the baseline.</param>
		/// <param name="maxRatio">The maximum timing ratio relative to the baseline.</param>
		public CompetitionLimit(
			double minRatio, double maxRatio)
		{
			minRatio = AdjustLimitArgument(minRatio, nameof(minRatio));
			maxRatio = AdjustLimitArgument(maxRatio, nameof(maxRatio));

			if (!IsMinLimitOk(minRatio, maxRatio))
			{
				throw CodeExceptions.Argument(
					nameof(maxRatio),
					$"Please check competition limits. The {nameof(minRatio)} ({minRatio.ToString(HostEnvironmentInfo.MainCultureInfo)}) "
						+
						$"should not be greater than the {nameof(maxRatio)} ({maxRatio.ToString(HostEnvironmentInfo.MainCultureInfo)}).");
			}

			MinRatio = minRatio;
			MaxRatio = maxRatio;
		}

		/// <summary>All limits are empty.</summary>
		/// <value><c>true</c> if all limits are is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => IsEmptyValue(MinRatio) && IsEmptyValue(MaxRatio);

		/// <summary>All limits are ignored.</summary>
		/// <value><c>true</c> if all limits are ignored; otherwise, <c>false</c>.</value>
		public bool IgnoreAll => IgnoreMinRatio && IgnoreMaxRatio;

		/// <summary>The minimum timing ratio relative to the baseline.</summary>
		/// <value>The minimum timing ratio relative to the baseline.</value>
		public double MinRatio { get; protected set; }

		/// <summary>The maximum timing ratio relative to the baseline.</summary>
		/// <value>The maximum timing ratio relative to the baseline.</value>
		public double MaxRatio { get; protected set; }

		/// <summary>The minimum timing ratio (rounded, two digits).</summary>
		/// <value>The minimum timing ratio (rounded, two digits).</value>
		public double MinRatioRounded => Math.Round(MinRatio, 2);

		/// <summary>The maximum timing ratio (rounded, two digits).</summary>
		/// <value>The maximum timing ratio (rounded, two digits).</value>
		public double MaxRatioRounded => Math.Round(MaxRatio, 2);

		/// <summary>The minimum timing ratio limit is ignored.</summary>
		/// <value><c>true</c> if the minimum timing ratio limit is ignored; otherwise, <c>false</c>.</value>
		public bool IgnoreMinRatio => IsIgnoredValue(MinRatio);

		/// <summary>The maximum timing ratio limit is ignored.</summary>
		/// <value><c>true</c> if the maximum timing ratio limit is ignored; otherwise, <c>false</c>.</value>
		public bool IgnoreMaxRatio => IsIgnoredValue(MaxRatio);

		/// <summary>The string representation of minimum timing ratio limit.</summary>
		/// <value>The string representation of minimum timing ratio limit.</value>
		public string MinRatioText => IgnoreMinRatio
			? MinRatioRounded.ToString(HostEnvironmentInfo.MainCultureInfo)
			: MinRatioRounded.ToString(RatioFormat, HostEnvironmentInfo.MainCultureInfo);

		/// <summary>The string representation of maximum timing ratio limit.</summary>
		/// <value>The string representation of maximum timing ratio limit.</value>
		public string MaxRatioText => IgnoreMaxRatio
			? MaxRatioRounded.ToString(HostEnvironmentInfo.MainCultureInfo)
			: MaxRatioRounded.ToString(RatioFormat, HostEnvironmentInfo.MainCultureInfo);

		/// <summary>Checks if actual values fits into limits represented by this instance.</summary>
		/// <param name="actualValues">The limits for actual values.</param>
		/// <returns><c>true</c> if the the actual values fits into limits.</returns>
		public bool CheckLimitsFor([NotNull] CompetitionLimit actualValues)
		{
			Code.NotNull(actualValues, nameof(actualValues));

			return IsMinLimitOk(MinRatio, actualValues.MinRatioRounded) &&
				IsMaxLimitOk(MaxRatio, actualValues.MaxRatioRounded);
		}

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => $"[{MinRatioText}..{MaxRatioText}]";
	}
}