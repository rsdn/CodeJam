#if LESSTHAN_NETCOREAPP20 || LESSTHAN_NETSTANDARD21 || TARGETS_NET
using System.Collections.Generic;

namespace CodeJam.Targeting
{
	internal static class DictionaryHelper
	{
		/// <summary>
		/// https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.keyvaluepair-2.deconstruct
		/// Applies to
		/// .NET
		/// 5.0
		/// .NET Core
		/// 3.1 3.0 2.2 2.1 2.0
		/// .NET Standard
		/// 2.1
		/// </summary>
		// ReSharper disable once UseDeconstructionOnParameter
		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
		{
			key = pair.Key;
			value = pair.Value;
		}
	}
}
#endif