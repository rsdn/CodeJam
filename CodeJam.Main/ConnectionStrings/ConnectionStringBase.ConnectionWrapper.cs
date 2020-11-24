using System;
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
	partial class ConnectionStringBase
	{
		private class StringBuilderWrapper : DbConnectionStringBuilder
		{
			private const string _nonBrowsableValue = "...";

			private static IReadOnlyDictionary<string, KeywordDescriptor> GetDescriptorsCore(Type type)
			{
				static KeywordDescriptor GetDescriptor(PropertyInfo property) =>
					new(
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
					t => t.GetBaseType() is var baseType && baseType != typeof(ConnectionStringBase)
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

			private static readonly Func<Type, IReadOnlyDictionary<string, KeywordDescriptor>> _keywordsCache = Algorithms
				.Memoize(
					(Type type) => GetDescriptorsCore(type),
					LazyThreadSafetyMode.ExecutionAndPublication);

			private readonly Type _descriptorType;

			public StringBuilderWrapper(string connectionString, Type descriptorType)
			{
				_descriptorType = descriptorType;
				if (connectionString != null)
					ConnectionString = connectionString;
			}

			[NotNull]
			public IReadOnlyDictionary<string, KeywordDescriptor> Keywords => _keywordsCache(_descriptorType);

			[NotNull]
			public new string ConnectionString
			{
				get => base.ConnectionString;
				set
				{
					base.ConnectionString = value;
					if (value.NotNullNorEmpty())
					{
						foreach (var (key, _) in Keywords.Where(p => p.Value.IsRequired))
						{
							if (!ContainsKey(key))
								throw CodeExceptions.Argument(
									nameof(ConnectionString),
									$"The value of required {key} connection string parameter is empty.");
						}
					}
				}
			}

			/// <returns></returns>
			[NotNull, MustUseReturnValue]
			public string GetBrowsableConnectionString(bool includeNonBrowsable = false)
			{
				var builder = new StringBuilder();
				foreach (var (key, val) in Keywords)
				{
					if (!val.IsBrowsable && !includeNonBrowsable)
						continue;

					if (ShouldSerialize(key) && TryGetValue(key, out var value))
					{
						if (!val.IsBrowsable)
							value = _nonBrowsableValue;
						var keyValue = Convert.ToString(value, CultureInfo.InvariantCulture);
						AppendKeyValuePair(builder, key, keyValue);
					}
				}

				return builder.ToString();
			}

			public string GetStringValue(string keyword) => (string)this[keyword];

			public bool TryGetStringValue(string keyword, out string value)
			{
				value = GetStringValue(keyword);
				return value != null;
			}

			#region Use only allowed keywords
			/// <inheritdoc />
			public override object this[string keyword]
			{
				get
				{
					if (Keywords.ContainsKey(keyword))
					{
						TryGetValue(keyword, out var value);
						return value;
					}
					return base[keyword]; // exception for not supported keyword.
				}
				set
				{
					if (Keywords.TryGetValue(keyword, out var descriptor))
					{
						base[descriptor.Name] = value switch
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
					}
				}
			}
			#endregion
		}
	}
}