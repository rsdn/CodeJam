#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
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