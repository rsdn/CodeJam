#if LESSTHAN_NET40 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
// Some expression types are missing if targeting to these frameworks
#else
using System;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// Mapping value.
	/// </summary>
	public class MapValue
	{
		/// <summary>
		/// Creates <see cref="MapValue"/> instance.
		/// </summary>
		/// <param name="origValue">Original value.</param>
		/// <param name="mapValues">Mapping value.</param>
		public MapValue(object origValue, params MapValueAttribute[] mapValues)
		{
			Code.NotNullAndItemNotNull(mapValues, nameof(mapValues));

			OrigValue = origValue;
			MapValues = mapValues;
		}

		/// <summary>
		/// Original value.
		/// </summary>
		public object OrigValue { get; }

		/// <summary>
		/// Mapping value.
		/// </summary>
		[NotNull]
		[ItemNotNull]
		public MapValueAttribute[] MapValues { get; }
	}
}
#endif