using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>Suffix of the SuffixTree</summary>
	[PublicAPI]
	public struct Suffix
	{
		/// <summary>Buffer of all added strings</summary>
		private readonly string _buffer;
		/// <summary>Offset to the beginning of the suffix in the buffer</summary>
		private readonly int _offset;

		/// <summary>Constructs a new suffix</summary>
		/// <param name="buffer">Buffer with all added strings</param>
		/// <param name="sourceIndex">Source string index</param>
		/// <param name="offset">Offset of the suffix in the buffer</param>
		/// <param name="length">Length of the suffix</param>
		internal Suffix([NotNull] string buffer, int sourceIndex, int offset, int length)
		{
			DebugCode.NotNull(buffer, nameof(buffer));
			DebugCode.ValidIndex(sourceIndex, nameof(sourceIndex));
			DebugCode.ValidIndexAndCount(offset, nameof(offset), length, nameof(length), buffer.Length);
			_buffer = buffer;
			SourceIndex = sourceIndex;
			_offset = offset;
			Length = length;
		}

		/// <summary>
		/// The index of the source string in the order or addition to the Suffix tree
		/// <remarks>0 - for the first added string, 1 - for the second, etc</remarks>
		/// </summary>
		public int SourceIndex { get; }
		/// <summary>The length of the suffix</summary>
		public int Length { get; }
		/// <summary>The suffix value</summary>
		public string Value => _buffer.Substring(_offset, Length);

		/// <summary>String conversion operator</summary>
		/// <param name="suffix">The suffix to convert</param>
		public static implicit operator string(Suffix suffix) => suffix.Value;
	}
}
