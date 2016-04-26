#if FW40
namespace System.Collections.Generic
{

	/// <summary>
	/// <see cref="Dictionary{TKey,TValue}"/> that implements <see cref="IReadOnlyDictionary{TKey,TValue}"/>.
	/// </summary>
	/// <typeparam name="TKey">Key type.</typeparam>
	/// <typeparam name="TValue">Value type.</typeparam>
	/// <remarks>For FW40 targeting puporses only.</remarks>
	public class DictionaryWithReadOnly<TKey, TValue> : Dictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
	{
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
	}
}
#endif