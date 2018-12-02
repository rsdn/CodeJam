using System;
using System.IO;

using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.Parsing
{
	[PublicAPI]
	public struct CharReader
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

		public bool IsWhitespace => Char.IsWhiteSpace();

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

		public static CharReader Create(TextReader reader) => new CharReader(reader, Read(reader));

		public CharReader Next() => Create(_reader);

		public CharReader Peek() => new CharReader(_reader, _reader.Peek());
	}
}