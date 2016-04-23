using System;
using System.IO;
using System.Linq;

using CodeJam.Collections;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.TableData
{
	/// <summary>
	/// Fixed width format support.
	/// </summary>
	[PublicAPI]
	public static class FixedWidthFormat
	{
		/// <summary>
		/// Creates fixed width format parser.
		/// </summary>
		/// <param name="widths">Array of column widths</param>
		/// <returns>Parser to use with <see cref="TableDataParser.Parse(Parser,string)"/></returns>
		[NotNull]
		[Pure]
		public static Parser CreateParser([NotNull] int[] widths)
		{
			Code.NotNull(widths, nameof(widths));
			Code.AssertArgument(widths.Length > 0, nameof(widths), "At least one column must be specified");
			Code.AssertArgument(widths.All(w => w > 0), nameof(widths), "Column width must be greater than 0");

			return (TextReader rdr, ref int ln) => Parse(rdr, ref ln, widths);
		}

		private static string[] Parse(TextReader reader, ref int lineNum, int[] widths)
		{
			var line = reader.ReadLine();
			if (line == null)
				return null;
			if (line.IsNullOrWhiteSpace())
#if !FW452
				return Array.Empty<string>();
#else
				return Array<string>.Empty;
#endif

			var pos = 0;
			var result = new string[widths.Length];
			for (var i = 0; i < widths.Length; i++)
			{
				var width = widths[i];
				var len = Math.Min(width, line.Length - pos);
				if (len <= 0)
					throw new FormatException($"Line {lineNum} too short");
				result[i] = line.Substring(pos, len).Trim();
				pos += len;
			}
			lineNum++;
			return result;
		}
	}
}