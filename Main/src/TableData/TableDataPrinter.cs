using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.TableData
{
	/// <summary>
	/// Prints table data.
	/// </summary>
	[PublicAPI]
	public static class TableDataPrinter
	{
		/// <summary>
		/// Prints full CSV table
		/// </summary>
		/// <param name="formatter">The formatter.</param>
		/// <param name="writer">Instance of <see cref="TextWriter"/> to write to.</param>
		/// <param name="data">Data to write.</param>
		/// <param name="indent">The indent.</param>
		public static void Print(
			[NotNull] this ITableDataFormatter formatter,
			[NotNull] TextWriter writer,
			[NotNull] IEnumerable<string[]> data,
			[CanBeNull] string indent = null)
		{
			Code.NotNull(formatter, nameof(formatter));
			Code.NotNull(writer, nameof(writer));
			Code.NotNull(data, nameof(data));

			var first = true;
			var widths = new int[0];
			foreach (var line in data)
			{
				if (!first)
					writer.WriteLine();
				else
					first = false;
				if (indent != null)
					writer.Write(indent);

				widths =
					line
						.Select((ln, i) => i >= widths.Length ? ln.Length : Math.Max(ln.Length, widths[i]))
						.ToArray();
				writer.Write(formatter.FormatLine(line, widths));
			}
		}
	}
}