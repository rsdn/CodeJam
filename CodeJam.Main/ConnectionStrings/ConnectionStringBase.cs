using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using CodeJam.Collections;
using CodeJam.Reflection;
using CodeJam.Strings;
using CodeJam.Targeting;

using JetBrains.Annotations;

namespace CodeJam.ConnectionStrings
{
	/// <summary>
	/// Base class for connection strings
	/// </summary>
	[PublicAPI]
	public abstract class ConnectionStringBase : DbConnectionStringBuilder
	{
		private const string _nonBrowsableValue = "...";

		/// <summary>
		/// Descriptor for connection string keyword
		/// </summary>
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

		private static IReadOnlyDictionary<string, KeywordDescriptor> GetDescriptorsCore(Type type)
		{
			KeywordDescriptor GetDescriptor(PropertyInfo property) =>
				new KeywordDescriptor(
					property.Name,
					property.PropertyType,
#if NET35_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
					property.IsRequired(),
#else
					false,
#endif
					property.IsBrowsable());

			// Explicit ordering from most derived to base. Explanation:
			// The GetProperties method does not return properties in a particular order, such as alphabetical or declaration order.
			// Your code must not depend on the order in which properties are returned, because that order varies.
			var typeChain = Sequence.CreateWhileNotNull(
				type,
				t => t.GetBaseType() is var baseType && baseType != typeof(DbConnectionStringBuilder)
					? baseType
					: null);
			var properties = typeChain
				.SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly));
#if NET45_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
			var result = new Dictionary<string, KeywordDescriptor>(StringComparer.OrdinalIgnoreCase);
#else
			var result = new DictionaryEx<string, KeywordDescriptor>(StringComparer.OrdinalIgnoreCase);
#endif
			foreach (var prop in properties)
			{
				// DONTTOUCH: most derived property wins
				if (result.ContainsKey(prop.Name))
					continue;

				result[prop.Name] = GetDescriptor(prop);
			}
			return result;
		}

		private static readonly Func<Type, IReadOnlyDictionary<string, KeywordDescriptor>> _keywordsCache = Algorithms.Memoize(
			(Type type) => GetDescriptorsCore(type),
			LazyThreadSafetyMode.ExecutionAndPublication);

		/// <summary>Initializes a new instance of the <see cref="ConnectionStringBase" /> class.</summary>
		/// <param name="connectionString">The connection string.</param>
		protected ConnectionStringBase([CanBeNull] string connectionString)
		{
			if (connectionString != null)
				ConnectionString = connectionString;
		}

		/// <summary>
		/// Gets all supported keywords for current connection.
		/// </summary>
		[NotNull]
		protected IReadOnlyDictionary<string, KeywordDescriptor> Keywords => _keywordsCache(GetType());

		/// <summary>
		/// Gets or sets the connection string associated with the <see cref="T:System.Data.Common.DbConnectionStringBuilder" />.
		/// </summary>
		[NotNull]
		public new string ConnectionString
		{
			get => base.ConnectionString;
			set
			{
				base.ConnectionString = value;
				if (value.NotNullNorEmpty())
				{
					foreach (var nameRequiredPair in Keywords.Where(p => p.Value.IsRequired))
					{
						if (!ContainsKey(nameRequiredPair.Key))
							throw CodeExceptions.Argument(
								nameof(ConnectionString),
								$"The value of required {nameRequiredPair.Key} connection string parameter is empty.");
					}
				}
			}
		}

		/// <summary>
		/// Gets the browsable connection string.
		/// </summary>
		/// <param name="includeNonBrowsable">If set to <c>true</c>, non browsable values will be .</param>
		/// <returns></returns>
		[NotNull, MustUseReturnValue]
		public string GetBrowsableConnectionString(bool includeNonBrowsable = false)
		{
			var builder = new StringBuilder();
			foreach (var browsablePair in Keywords)
			{
				if (!browsablePair.Value.IsBrowsable && !includeNonBrowsable)
					continue;

				if (ShouldSerialize(browsablePair.Key) && TryGetValue(browsablePair.Key, out var value))
				{
					if (!browsablePair.Value.IsBrowsable)
						value = _nonBrowsableValue;
					var keyValue = Convert.ToString(value, CultureInfo.InvariantCulture);
					AppendKeyValuePair(builder, browsablePair.Key, keyValue);
				}
			}

			return builder.ToString();
		}

		#region Use only allowed keywords
		/// <inheritdoc />
		public override ICollection Keys => _keywordsCache(GetType()).Keys.ToArray();

		/// <inheritdoc />
		public override object this[string keyword]
		{
			get
			{
				return base[keyword];
			}
			set
			{
				if (Keywords.ContainsKey(keyword))
					base[keyword] = value;
			}
		}
		#endregion

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <returns>Value for the keyword</returns>
		[CanBeNull, MustUseReturnValue]
		protected string TryGetValue(string keyword) => ContainsKey(keyword) ? (string)base[keyword] : null;

		/// <summary>Set value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <param name="value">The value.</param>
		protected void SetValue(string keyword, object value) =>
			base[keyword] = value switch
			{
				DateTimeOffset x => x.ToInvariantString(),
				Guid x => x.ToInvariantString(),
#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
				TimeSpan x => x.ToInvariantString(),
#else
				TimeSpan x => x.ToString(),
#endif
				Uri x => x.ToString(),
				_ => value
			};

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <returns>Value for the keyword</returns>
		[MustUseReturnValue]
		protected bool TryGetBooleanValue(string keyword) => ContainsKey(keyword) && Convert.ToBoolean(base[keyword]);

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <returns>Value for the keyword</returns>
		[CanBeNull, MustUseReturnValue]
		protected int? TryGetInt32Value(string keyword) => ContainsKey(keyword) ? Convert.ToInt32(base[keyword]) : default(int?);

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <returns>Value for the keyword</returns>
		[CanBeNull, MustUseReturnValue]
		protected long? TryGetInt64Value(string keyword) => ContainsKey(keyword) ? Convert.ToInt64(base[keyword]) : default(long?);


		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">Name of keyword</param>
		/// <returns>Value for the keyword</returns>
		[CanBeNull, MustUseReturnValue]
		protected DateTimeOffset? TryGetDateTimeOffsetValue(string keyword) => TryGetValue(keyword).ToDateTimeOffsetInvariant();

#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES
		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">The value for the keyword.</param>
		/// <returns>Value for the keyword</returns>
		[CanBeNull, MustUseReturnValue]
		protected Guid? TryGetGuidValue(string keyword) => TryGetValue(keyword).ToGuid();

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">The value for the keyword.</param>
		/// <returns>Value for the keyword</returns>
		[CanBeNull, MustUseReturnValue]
		protected TimeSpan? TryGetTimeSpanValue(string keyword) => TryGetValue(keyword).ToTimeSpanInvariant();

		/// <summary>Gets the value for the keyword.</summary>
		/// <param name="keyword">The value for the keyword.</param>
		/// <returns>Value for the keyword.</returns>
		[CanBeNull, MustUseReturnValue]
		protected Uri TryGetUriValue(string keyword) => TryGetValue(keyword).ToUri();
#endif
	}
}