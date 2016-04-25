using JetBrains.Annotations;

namespace CodeJam.TableData
{
	/// <summary>
	/// Formatter interface.
	/// </summary>
	[PublicAPI]
	public interface ITableDataFormatter
	{
		/// <summary>
		/// Prints line of table data.
		/// </summary>
		/// <param name="values">Line values.</param>
		/// <param name="columnWidths">Array of column widths.</param>
		/// <returns>String representation of values</returns>
		[NotNull]
		string FormatLine([NotNull] string[] values, [NotNull] int[] columnWidths);
	}
}