using System;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Metrics
{
	/// <summary>The metric measurement unit.</summary>
	public struct MetricUnit : IEquatable<MetricUnit>
	{
		#region Equality operators
		/// <summary>Implements the operator ==.</summary>
		/// <param name="a">The metric1.</param>
		/// <param name="b">The metric2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator ==(MetricUnit a, MetricUnit b) => a.Equals(b);

		/// <summary>Implements the operator !=.</summary>
		/// <param name="a">The metric1.</param>
		/// <param name="b">The metric2.</param>
		/// <returns>The result of the operator.</returns>
		public static bool operator !=(MetricUnit a, MetricUnit b) => !a.Equals(b);
		#endregion

		/// <summary>Empty metric measurement unit.</summary>
		public static readonly MetricUnit Empty = new MetricUnit();

		#region Fields & .ctor
		private const string EmptyMetricName = "(" + nameof(Empty) + ")";

		private readonly string _name;

		/// <summary>Initializes a new instance of the <see cref="MetricUnit"/> class.</summary>
		/// <param name="name">The name of the metric measurement unit.</param>
		/// <param name="enumValue">Enum value for the measurement unit.</param>
		/// <param name="scaleCoefficient">The scale coefficient for the metric measurement unit.</param>
		public MetricUnit([NotNull] string name, [NotNull] Enum enumValue, double scaleCoefficient)
		{
			Code.NotNullNorEmpty(name, nameof(name));
			Code.NotNull(enumValue, nameof(enumValue));
			Code.InRange(scaleCoefficient, nameof(scaleCoefficient), 0, double.PositiveInfinity);
			Code.AssertArgument(
				!scaleCoefficient.IsSpecialMetricValue(),
				nameof(scaleCoefficient),
				"The scale coefficient has to be a numeric value.");

			_name = name;
			EnumValue = enumValue;
			ScaleCoefficient = scaleCoefficient;
		}
		#endregion

		#region Properties
		/// <summary>Gets a value indicating whether this instance is empty metric unit.</summary>
		/// <value>
		///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
		/// </value>
		public bool IsEmpty => _name == null;

		/// <summary>Gets the name of the metric measurement unit.</summary>
		/// <value>The name of the metric measurement unit.</value>
		[NotNull]
		public string Name => _name ?? EmptyMetricName;

		/// <summary>Gets the enum value for the measurement unit.</summary>
		/// <value>The enum value for the measurement unit.</value>
		[CanBeNull]
		public Enum EnumValue { get; }

		/// <summary>Gets the scale coefficient for the metric measurement unit.</summary>
		/// <value>The scale coefficient for the metric measurement unit.</value>
		public double ScaleCoefficient { get; }
		#endregion

		#region Equality members
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(MetricUnit other) =>
			Equals(_name, other._name) &&
				Equals(EnumValue, other.EnumValue) &&
				ScaleCoefficient.Equals(other.ScaleCoefficient);

		/// <summary>Determines whether the <paramref name="obj"/> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the <paramref name="obj"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is MetricUnit && Equals((MetricUnit)obj);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(
			_name?.GetHashCode() ?? 0,
			EnumValue?.GetHashCode() ?? 0,
			ScaleCoefficient.GetHashCode());
		#endregion

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => Name;
	}
}