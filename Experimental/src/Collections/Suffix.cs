using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>Suffix of the SuffixTree</summary>
	[PublicAPI]
	public struct Suffix
	{
		/// <summary>Buffer of all added strings</summary>
		private readonly string _buffer;
		/// <summary>Offset to the beginning of the source string in the buffer</summary>
		private readonly int _sourceOffset;

		/// <summary>Constructs a new suffix</summary>
		/// <param name="buffer">Buffer with all added strings</param>
		/// <param name="sourceIndex">Source string index</param>
		/// <param name="sourceOffset">Offset of the source string inside the buffer</param>
		/// <param name="offset">Offset of the suffix from the sourceOffset</param>
		/// <param name="length">Length of the suffix</param>
		internal Suffix([NotNull] string buffer, int sourceIndex, int sourceOffset, int offset, int length)
		{
			DebugCode.NotNull(buffer, nameof(buffer));
			DebugCode.ValidIndex(sourceIndex, nameof(sourceIndex));
			DebugCode.ValidIndex(sourceOffset, nameof(sourceOffset), buffer.Length);
			DebugCode.ValidIndexAndCount(sourceOffset + offset, nameof(offset), length, nameof(length), buffer.Length);
			_buffer = buffer;
			SourceIndex = sourceIndex;
			_sourceOffset = sourceOffset;
			Offset = offset;
			Length = length;
		}

		/// <summary>
		/// The index of the source string in the order or addition to the Suffix tree
		/// <remarks>0 - for the first added string, 1 - for the second, etc</remarks>
		/// </summary>
		public int SourceIndex { get; }
		/// <summary>The offset of the suffix from the beginning of the source string</summary>
		public int Offset { get; }
		/// <summary>The length of the suffix</summary>
		public int Length { get; }
		/// <summary>The suffix value</summary>
		public string Value => _buffer.Substring(_sourceOffset + Offset, Length);

		/// <summary>String conversion operator</summary>
		/// <param name="suffix">The suffix to convert</param>
		public static implicit operator string(Suffix suffix) => suffix.Value;
	}
}
