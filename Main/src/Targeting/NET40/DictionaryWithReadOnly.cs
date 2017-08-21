#if SUPPORTS_NET40
using System.Runtime.Serialization;

using JetBrains.Annotations;

namespace System.Collections.Generic
{
	/// <summary>
	/// <see cref="Dictionary{TKey,TValue}"/> that implements <see cref="IReadOnlyDictionary{TKey,TValue}"/>.
	/// </summary>
	/// <typeparam name="TKey">Key type.</typeparam>
	/// <typeparam name="TValue">Value type.</typeparam>
	/// <remarks>For SUPPORTS_NET40 targeting purposes only.</remarks>
	public class DictionaryWithReadOnly<TKey, TValue> : Dictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
	{
		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the default equality comparer for the key type.</summary>
		public DictionaryWithReadOnly() { }

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the default equality comparer for the key type.</summary>
		/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="capacity" /> is less than 0.</exception>
		public DictionaryWithReadOnly(int capacity) : base(capacity) { }

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the default initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
		public DictionaryWithReadOnly(IEqualityComparer<TKey> comparer) : base(comparer) { }

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that is empty, has the specified initial capacity, and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
		/// <param name="capacity">The initial number of elements that the <see cref="T:System.Collections.Generic.Dictionary`2" /> can contain.</param>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// <paramref name="capacity" /> is less than 0.</exception>
		public DictionaryWithReadOnly(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the default equality comparer for the key type.</summary>
		/// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="dictionary" /> is null.</exception>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="dictionary" /> contains one or more duplicate keys.</exception>
		public DictionaryWithReadOnly([NotNull] IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class that contains elements copied from the specified <see cref="T:System.Collections.Generic.IDictionary`2" /> and uses the specified <see cref="T:System.Collections.Generic.IEqualityComparer`1" />.</summary>
		/// <param name="dictionary">The <see cref="T:System.Collections.Generic.IDictionary`2" /> whose elements are copied to the new <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
		/// <param name="comparer">The <see cref="T:System.Collections.Generic.IEqualityComparer`1" /> implementation to use when comparing keys, or null to use the default <see cref="T:System.Collections.Generic.EqualityComparer`1" /> for the type of the key.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="dictionary" /> is null.</exception>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="dictionary" /> contains one or more duplicate keys.</exception>
		public DictionaryWithReadOnly([NotNull] IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.Dictionary`2" /> class with serialized data.</summary>
		/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object containing the information required to serialize the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
		/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure containing the source and destination of the serialized stream associated with the <see cref="T:System.Collections.Generic.Dictionary`2" />.</param>
		protected DictionaryWithReadOnly(SerializationInfo info, StreamingContext context) : base(info, context) { }

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;
	}
}
#endif