using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

using CodeJam.Collections;
using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.TableData
{
	/// <summary>
	/// CSV format support.
	/// </summary>
	[PublicAPI]
	public static class CsvFormat
	{
		private static readonly char[] _conflictChars = { '\r', '\n', '"', ',' };

		private static bool IsEscapeRequired(char value) => value.IsWhiteSpace() || _conflictChars.Contains(value);

		/// <summary>Escapes csv value.</summary>
		/// <param name="value">The value.</param>
		/// <returns>Escaped value.</returns>
		public static string EscapeValue(string value)
		{
			Code.NotNull(value, nameof(value));

			StringBuilder? escaped = null;
			for (var i = 0; i < value.Length; i++)
			{
				var chr = value[i];
				if (escaped != null)
					if (chr == '"')
						escaped.Append("\"\"");
					else
						escaped.Append(chr);
				else if (IsEscapeRequired(chr))
					escaped = new StringBuilder(chr == '"' ? value.Substring(0, i) + "\"\"" : value.Substring(0, i + 1));
			}
			return escaped?.ToString() ?? value;
		}

		#region Parser
		/// <summary>
		/// Creates RFC4180 compliant CSV parser.
		/// </summary>
		/// <param name="allowEscaping">If true, allows values escaping.</param>
		/// <param name="columnSeparator">Char to use as column separator</param>
		/// <returns>Parser to use with <see cref="TableDataParser.Parse(Parser,string)"/></returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static Parser CreateParser(bool allowEscaping = true, char columnSeparator = ',') =>
			allowEscaping
				? (Parser)((TextReader rdr, ref int ln) => ParseCsv(rdr, ref ln, columnSeparator))
				: (TextReader rdr, ref int ln) => ParseCsvNoEscape(rdr, ref ln, columnSeparator);

		/// <summary>Parses CSV data.</summary>
		/// <param name="reader">Text to parse</param>
		/// <param name = "allowEscaping" > If true, allows values escaping.</param>
		/// <param name="columnSeparator">Char to use as column separator</param>
		/// <returns>Enumeration of <see cref="DataLine" /> contained parsed data.</returns>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static IEnumerable<DataLine> Parse(
			TextReader reader,
			bool allowEscaping = true,
			char columnSeparator = ',') =>
				CreateParser(allowEscaping, columnSeparator).Parse(reader);

		[return: MaybeNull]
		private static string[] ParseCsv(TextReader reader, ref int lineNum, char separator)
		{
			var curChar = CharReader.Create(reader);
			if (curChar.IsEof)
				return null; // EOF reached
			if (curChar.IsEol)
			{
				lineNum++;
				return Array<string>.Empty;
			}

			var result = new List<string>();
			StringBuilder? curField = null;
			var state = ParserState.ExpectField;
			var column = 1;
			while (true)
			{
				var skip = false;

				while (!skip)
					switch (state)
					{
						case ParserState.ExpectField:
							if (curChar.IsEof || curChar.IsEol)
							{
								if (result.Count > 0) // Special case - empty string not treated as single empty value
									result.Add("");
								return result.ToArray();
							}

							if (curChar.Char == separator)
							{
								result.Add("");
								state = ParserState.AfterField;
								break;
							}

							skip = true;
							if (curChar.IsWhitespace)
								break;

							curField = new StringBuilder();

							if (curChar.IsDoubleQuota)
							{
								state = ParserState.QuotedField;
								break;
							}

							state = ParserState.Field;
							curField.Append(curChar.Char);
							break;

						case ParserState.Field:
							DebugCode.BugIf(curField == null, "curField should be not null");
							if (curChar.IsEof || curChar.IsEol || curChar.Char == separator)
							{
								result.Add(curField.ToString().Trim());
								state = ParserState.AfterField;
								break;
							}

							skip = true;
							curField.Append(curChar.Char);
							break;

						case ParserState.QuotedField:
							DebugCode.BugIf(curField == null, "curField should be not null");
							if (curChar.IsEof)
								throw new FormatException($"Unexpected EOF at line {lineNum} column {column}");

							skip = true;
							if (curChar.IsEol)
							{
								curField.Append("\r\n");
								break;
							}
							if (curChar.IsDoubleQuota)
							{
								var peek = curChar.Peek();
								if (peek.IsDoubleQuota) // Escaped '"'
								{
									curField.Append('"');
									curChar = curChar.Next();
								}
								else
								{
									result.Add(curField.ToString());
									state = ParserState.AfterField;
								}
								break;
							}
							curField.Append(curChar.Char);
							break;

						case ParserState.AfterField:
							if (curChar.IsEof || curChar.IsEol)
								return result.ToArray();
							skip = true;
							if (curChar.IsWhitespace)
								continue;
							if (curChar.Char == separator)
							{
								state = ParserState.ExpectField;
								break;
							}
							throw new FormatException($"Unexpected char '{curChar.Char}' at line {lineNum} column {column}");

						default:
							throw new ArgumentOutOfRangeException($"State has unexpected value {state}");
					}

				curChar = curChar.Next();
				column++;
				if (curChar.IsEol)
				{
					lineNum++;
					column = 1;
				}
			}
		}

		[return: MaybeNull]
		private static string[] ParseCsvNoEscape(TextReader reader, ref int lineNum, char separator)
		{
			var line = reader.ReadLine();
			if (line == null)
				return null;
			lineNum++;
			var parts = line.Split(separator);
			// Special case - whitespace lines are ignored
			if (parts.Length == 1 && parts[0].IsNullOrWhiteSpace())
				return Array<string>.Empty;
			return parts;
		}

		#region CharReader struct
		private struct CharReader
		{
			private const int _eof = -1;
			private const int _eol = -2;

			private readonly TextReader _reader;
			private readonly int _code;

			private CharReader(TextReader reader, int code)
			{
				_reader = reader;
				_code = code;
			}

			public char Char => (char)_code;

			public bool IsEof => _code == _eof;

			public bool IsEol => _code == _eol;

			public bool IsWhitespace => char.IsWhiteSpace(Char);

			public bool IsDoubleQuota => _code == '"';

			private static int Read(TextReader reader)
			{
				var code = reader.Read();
				if (code == '\r' || code == '\n')
				{
					if (code == '\r' && reader.Peek() == '\n')
						reader.Read();
					return _eol;
				}
				return code;
			}

			public static CharReader Create(TextReader reader) => new(reader, Read(reader));

			public CharReader Next() => Create(_reader);

			public CharReader Peek() => new(_reader, _reader.Peek());
		}
		#endregion

		#region ParserState enum
		private enum ParserState
		{
			ExpectField,
			Field,
			QuotedField,
			AfterField
		}
		#endregion

		#endregion

		#region Formatter
		/// <summary>
		/// Prints full data table
		/// </summary>
		/// <param name="writer">Instance of <see cref="TextWriter"/> to write to.</param>
		/// <param name="data">Data to write.</param>
		/// <param name="indent">The indent.</param>
		/// <param name="allowEscaping">If true, allows values escaping.</param>
		public static void Print(
			TextWriter writer,
			IEnumerable<string[]> data,
			string? indent = null,
			bool allowEscaping = true)
		{
			Code.NotNull(writer, nameof(writer));
			Code.NotNull(data, nameof(data));

			var formatter =
				allowEscaping
					? (ITableDataFormatter)new CsvFormatter()
					: new CsvNoEscapeFormatter();
			var first = true;
			var widths = Array.Empty<int>();
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
						.Select(
							(val, i) =>
							{
								var len = formatter.GetValueLength(val);
								return i >= widths.Length ? len : Math.Max(len, widths[i]);
							})
						.ToArray();
				writer.Write(formatter.FormatLine(line, widths));
			}
		}

		/// <summary>
		/// Formatter interface.
		/// </summary>
		private interface ITableDataFormatter
		{
			int GetValueLength([AllowNull] string value);

			/// <summary>
			/// Prints line of table data.
			/// </summary>
			/// <param name="values">Line values.</param>
			/// <param name="columnWidths">Array of column widths.</param>
			/// <returns>String representation of values</returns>
			string FormatLine(string[] values, int[] columnWidths);
		}

		private class CsvFormatter : ITableDataFormatter
		{
			public int GetValueLength([AllowNull] string? value)
			{
				var len = value?.Length;
				if (!len.HasValue || len == 0)
					return 0;

				var append = 0;
				foreach (var chr in value!)
				{
					if (append == 0)
					{
						if (IsEscapeRequired(chr))
						{
							append = 2; // add two quotes
							if (chr == '"')
								append++; // +1 for double quota escape
						}
					}
					else if (chr == '"')
						append++; // +1 for double quota escape
				}
				return len.Value + append;
			}

			public string FormatLine(string[] values, int[] columnWidths)
			{
				Code.NotNull(values, nameof(values));
				Code.NotNull(columnWidths, nameof(columnWidths));
				Code.AssertArgument(
					values.Length <= columnWidths.Length,
					nameof(columnWidths),
					"columnWidth array to short");

				return values
					.Select(EscapeValue).Zip(columnWidths, (s, w) => s.PadRight(w))
					.Join(", ");
			}
		}

		private class CsvNoEscapeFormatter : ITableDataFormatter
		{
			#region Implementation of ITableDataFormatter
			/// <summary>
			/// Returns length of formatted value.
			/// </summary>
			/// <param name="value">Value.</param>
			/// <returns>Length of formatted value representation.</returns>
			public int GetValueLength([AllowNull] string value) => (value?.Length).GetValueOrDefault();

			/// <summary>
			/// Prints line of table data.
			/// </summary>
			/// <param name="values">Line values.</param>
			/// <param name="columnWidths">Array of column widths. If null - value is ignored.</param>
			/// <returns>String representation of values</returns>
			public string FormatLine(string[] values, int[] columnWidths) =>
				values.Zip(columnWidths, (s, w) => s.PadLeft(w)).Join(", ");
			#endregion
		}
		#endregion
	}
}