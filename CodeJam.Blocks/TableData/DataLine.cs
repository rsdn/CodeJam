
using System;

using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.TableData
{
	/// <summary>
	/// Line of data.
	/// </summary>
	[PublicAPI]
	public struct DataLine : IEquatable<DataLine>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object" /> class.
		/// </summary>
		/// <param name="lineNum">The line number.</param>
		/// <param name="values">Line values.</param>
		public DataLine(int lineNum, string[] values)
		{
			LineNum = lineNum;
			Values = values;
		}

		/// <summary>
		/// Line number.
		/// </summary>
		public int LineNum { get; }

		/// <summary>
		/// Line values.
		/// </summary>
		public string[] Values { get; }

		#region Overrides of ValueType
		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>A <see cref="String" /> containing a fully qualified type name.</returns>
		public override string ToString() => $"({LineNum}) {Values.Join(", ")}";
		#endregion

		#region Equality members
		/// <inheritdoc/>
		public bool Equals(DataLine other) => LineNum == other.LineNum && Values.Equals(other.Values);

		/// <inheritdoc/>
		public override bool Equals(object? obj) => obj is DataLine other && Equals(other);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (LineNum * 397) ^ Values.GetHashCode();
			}
		}

		/// <summary>
		/// Operator ==
		/// </summary>
		/// <param name="left">Left operand</param>
		/// <param name="right">Right operand</param>
		/// <returns>True if operands equal</returns>
		public static bool operator ==(DataLine left, DataLine right) => left.Equals(right);

		/// <summary>
		/// Operator !=
		/// </summary>
		/// <param name="left">Left operand</param>
		/// <param name="right">Right operand</param>
		/// <returns>True if operands not equal</returns>
		public static bool operator !=(DataLine left, DataLine right) => !left.Equals(right);
		#endregion
	}
}