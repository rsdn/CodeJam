using System;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

namespace CodeJam.Csv
{
	/// <summary>
	/// Prints tab data in CSV format
	/// </summary>
	[PublicAPI]
	public static class CsvPrinter
	{
		/// <summary>
		/// Returns true if value required escaping.
		/// </summary>
		private static bool IsEscapeRequired(string value)
		{
			if (string.IsNullOrEmpty(value))
				return false;
			return
				char.IsWhiteSpace(value[0])
				|| char.IsWhiteSpace(value[value.Length - 1])
				|| value.IndexOfAny(new[] { '\r', '\n', '"', ',' }) >= 0;
		}

		/// <summary>
		/// Returns length of escaped string
		/// </summary>
		private static int GetEscapedLength(string value)
		{
			if (!IsEscapeRequired(value))
				return value.Length;
			return value.Length + 2 + value.Count(c => c == '"');
		}

		/// <summary>
		/// Escapes value if required
		/// </summary>
		private static string EscapeValue(string value)
		{
			if (!IsEscapeRequired(value))
				return value;
			return '"' + value.Replace("\"", "\"\"") + '"';
		}

		/// <summary>
		/// Returns CSV line from array of values
		/// </summary>
		[NotNull]
		public static string GetLine([NotNull] string[] values)
		{
			if (values == null) throw new ArgumentNullException(nameof(values));
			return values.Select(EscapeValue).Join(", ");
		}

		/// <summary>
		/// Prints full CSV table
		/// </summary>
		[NotNull]
		public static string Print([NotNull] string[][] values, [NotNull] string indent)
		{
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (indent == null) throw new ArgumentNullException(nameof(indent));

			if (values.Length == 0)
				return "";
			var maxWidths =
				values[0]
					.Select((line, i) => values.Select(l => GetEscapedLength(l[i])).Max())
					.ToArray();
			var sb = new StringBuilder();
			var lineNum = 0;
			foreach (var line in values)
			{
				sb.Append(indent);
				var i = 0;
				var first = true;
				foreach (var v in line)
				{
					if (!first)
						sb.Append(", ");
					else
						first = false;
					var ev = EscapeValue(v);
					sb.Append(ev);
					if (i < line.Length - 1)
						sb.Append(new string(' ', maxWidths[i] - ev.Length));
					i++;
				}
				if (lineNum < values.Length - 1)
					sb.AppendLine();
				lineNum++;
			}
			return sb.ToString();
		}
	}
}