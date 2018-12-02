using System;

using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.TableData
{
	/// <summary>
	/// Line of data.
	/// </summary>
	[PublicAPI]
	public struct DataLine
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Object" /> class.
		/// </summary>
		/// <param name="lineNum">The line number.</param>
		/// <param name="values">Line values.</param>
		public DataLine(int lineNum, [NotNull, ItemNotNull] string[] values)
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
		[NotNull]
		[ItemNotNull]
		public string[] Values { get; }

		#region Overrides of ValueType
		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
		public override string ToString() => $"({LineNum}) {Values.Join(", ")}";
		#endregion
	}
}