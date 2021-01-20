using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

using JetBrains.Annotations;

#pragma warning disable CA1305 // Specify IFormatProvider

namespace CodeJam.ConnectionStrings
{
	/// <summary>
	/// Base class for connection strings
	/// </summary>
	[PublicAPI]
	public abstract partial class ConnectionStringBase : IDictionary<string, object>
	{
		/// <summary>
		/// Descriptor for connection string keyword
		/// </summary>
		[PublicAPI]
		protected class KeywordDescriptor
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="KeywordDescriptor"/> class.
			/// </summary>
			public KeywordDescriptor(string name, Type valueType, bool isRequired, bool isBrowsable)
			{
				Name = name;
				ValueType = valueType;
				IsRequired = isRequired;
				IsBrowsable = isBrowsable;
			}

			/// <summary>
			/// Gets the keyword name.
			/// </summary>
			public string Name { get; }

			/// <summary>
			/// Gets expected type of the keyword value.
			/// </summary>
			public Type ValueType { get; }

			/// <summary>
			/// Gets a value indicating whether this keyword is a mandatory keyword.
			/// </summary>
			public bool IsRequired { get; }

			/// <summary>
			/// Gets a value indicating whether this keyword is browsable (safe to log / display).
			/// </summary>
			public bool IsBrowsable { get; }
		}

		private readonly StringBuilderWrapper _wrapper;

		/// <summary>Initializes a new instance of the <see cref="ConnectionStringBase" /> class.</summary>
		/// <param name="connectionString">The connection string.</param>
		protected ConnectionStringBase(string? connectionString)
			=> _wrapper = new StringBuilderWrapper(connectionString, GetType());

		/// <summary>
		/// Gets all supported keywords for current connection.
		/// </summary>
		protected IReadOnlyDictionary<string, KeywordDescriptor> Keywords => _wrapper.Keywords;

		/// <summary>
		/// Gets or sets the connection string associated with the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.
		/// </summary>
		public string ConnectionString
		{
			get => _wrapper.ConnectionString;
			set => _wrapper.ConnectionString = value;
		}

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <returns>Value for the keyword</returns>
		[MustUseReturnValue]
		protected string? TryGetValue(string keyword) => _wrapper.GetStringValue(keyword);

		/// <summary>Set value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <param name="value">The value.</param>
		protected void SetValue(string keyword, object? value) => _wrapper[keyword] = value;

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <returns>Value for the keyword</returns>
		[MustUseReturnValue]
		protected bool TryGetBooleanValue(string keyword) =>
			_wrapper.TryGetValue(keyword, out var item) && Convert.ToBoolean(item);

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <returns>Value for the keyword</returns>
		[MustUseReturnValue]
		protected int? TryGetInt32Value(string keyword) =>
			_wrapper.TryGetValue(keyword, out var item) ? Convert.ToInt32(item) : default(int?);

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <returns>Value for the keyword</returns>
		[MustUseReturnValue]
		protected long? TryGetInt64Value(string keyword) =>
			_wrapper.TryGetValue(keyword, out var item) ? Convert.ToInt64(item) : default(long?);

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <returns>Value for the keyword</returns>
		[MustUseReturnValue]
		protected DateTimeOffset? TryGetDateTimeOffsetValue(string keyword) =>
			_wrapper.TryGetStringValue(keyword, out var item)
				? DateTimeOffset.Parse(item, CultureInfo.InvariantCulture, DateTimeStyles.None)
				: default(DateTimeOffset?);

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">The value for the keyword.</param>
		/// <returns>Value for the keyword</returns>
		[MustUseReturnValue]
		protected Guid? TryGetGuidValue(string keyword) =>
			_wrapper.TryGetStringValue(keyword, out var item) ? new Guid(item) : default(Guid?);

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">The value for the keyword.</param>
		/// <returns>Value for the keyword</returns>
		[MustUseReturnValue]
		protected TimeSpan? TryGetTimeSpanValue(string keyword) =>
			_wrapper.TryGetStringValue(keyword, out var item) ? TimeSpan.Parse(item) : default(TimeSpan?);

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">The value for the keyword.</param>
		/// <returns>Value for the keyword.</returns>
		[MustUseReturnValue]
		protected Uri? TryGetUriValue(string keyword) =>
			_wrapper.TryGetStringValue(keyword, out var item) ? new Uri(item) : null;

		/// <summary>
		/// Compares the connection information in this <see cref="ConnectionStringBase"/> object with the connection information in the supplied object..
		/// </summary>
		/// <param name="other">The other connection string.</param>
		/// <returns><c>true</c> if the connection information in both objects causes an equivalent connection string; otherwise <c>false</c>.</returns>
		public bool EquivalentTo(ConnectionStringBase other)
		{
			Code.NotNull(other, nameof(other));

			return _wrapper.EquivalentTo(other._wrapper);
		}

		/// <summary>
		/// Gets the browsable connection string.
		/// </summary>
		/// <param name="includeNonBrowsable">If set to <c>true</c>, non browsable values will be .</param>
		/// <returns>Browsable connection string</returns>
		[MustUseReturnValue]
		public string GetBrowsableConnectionString(bool includeNonBrowsable = false) =>
			_wrapper.GetBrowsableConnectionString(includeNonBrowsable);

		#region Implementation of IEnumerable
		/// <inheritdoc />
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _wrapper
			.Cast<KeyValuePair<string, object>>()
			.GetEnumerator();

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		#endregion

		#region Implementation of ICollection<KeyValuePair<string,object>>
		/// <inheritdoc />
		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) =>
			_wrapper.Add(item.Key, item.Value);

		/// <inheritdoc />
		void ICollection<KeyValuePair<string, object>>.Clear() => _wrapper.Clear();

		/// <inheritdoc />
		bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) =>
			_wrapper.TryGetValue(item.Key, out var value) && Equals(item.Value, value);

		/// <inheritdoc />
		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			Code.NotNull(array, nameof(array));
			Code.ValidIndex(arrayIndex, nameof(arrayIndex), array.Length);
			Code.ValidIndexAndCount(
				arrayIndex,
				nameof(arrayIndex),
				Count,
				nameof(Count),
				array.Length);

			var index = arrayIndex;
			foreach (var pair in this)
			{
				array[index++] = pair;
			}
		}

		/// <inheritdoc />
		bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
		{
			if (_wrapper.TryGetValue(item.Key, out var value) && Equals(item.Value, value))
				return _wrapper.Remove(item.Key);

			return false;
		}

		/// <inheritdoc />
		public int Count => _wrapper.Count;

		/// <inheritdoc />
		bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;
		#endregion

		#region Implementation of IDictionary<string,object>
		/// <inheritdoc />
		public bool ContainsKey(string key) => _wrapper.ContainsKey(key);

		/// <inheritdoc />
		public void Add(string key, object value) => _wrapper.Add(key, value);

		/// <inheritdoc />
		public bool Remove(string key) => _wrapper.Remove(key);

		/// <inheritdoc />
		public bool TryGetValue(
			string key,
#if NETCOREAPP30_OR_GREATER
			[MaybeNullWhen(false)]
#endif
			out object value) =>
			_wrapper.TryGetValue(key, out value);

		/// <inheritdoc />
		[AllowNull]
		public object this[string key]
		{
			get => _wrapper[key];
			set => _wrapper[key] = value;
		}

		/// <inheritdoc />
		ICollection<string> IDictionary<string, object>.Keys
		{
			get
			{
				DebugCode.BugIf(_wrapper.Keys == null, "_wrapper.Keys == null");
				return (ICollection<string>)_wrapper.Keys;
			}
		}

		/// <inheritdoc />
		ICollection<object> IDictionary<string, object>.Values => (ICollection<object>)_wrapper.Values;
		#endregion

		#region Overrides of Object
		/// <inheritdoc />
		public override string ToString() => _wrapper.ToString();
		#endregion
	}
}