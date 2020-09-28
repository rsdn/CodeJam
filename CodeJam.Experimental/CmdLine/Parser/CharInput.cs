using System;

using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	///<summary>
	/// Character input.
	///</summary>
	internal class CharInput : ICharInput
	{
		///<summary>
		/// End of file char.
		///</summary>
		public const char Eof = '\0';

		[NotNull] private readonly string _source;

		///<summary>
		/// Initialize instance.
		///</summary>
		public CharInput([NotNull] string source) : this(source, 0)
		{}

		private CharInput([NotNull] string source, [NonNegativeValue] int position)
		{
			_source = source;
			Position = position;
			Current = position < source.Length ? source[position] : Eof;
		}

		#region Implementation of ICharInput
		public char Current { get; }

		public int Position { get; }

		public ICharInput GetNext()
		{
			if (Current == Eof)
				throw new InvalidOperationException("Atempt to read beyond end of file");
			return new CharInput(_source, Position + 1);
		}
		#endregion
	}
}