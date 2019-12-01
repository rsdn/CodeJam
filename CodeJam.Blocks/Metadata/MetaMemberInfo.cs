#if LESSTHAN_NET40 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
// Some expression types are missing if targeting to these frameworks
#else
using System;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Strings;

using JetBrains.Annotations;

namespace CodeJam.Metadata
{
	internal class MetaMemberInfo
	{
		public MetaMemberInfo(
			[NotNull] string name,
			[NotNull, ItemNotNull] params AttributeInfo[] attributes)
		{
			Name        = name;
			_attributes = attributes;
		}

		public readonly string Name;

		[NotNull, ItemNotNull]
		private readonly AttributeInfo[] _attributes;

		[NotNull, ItemNotNull]
		public IEnumerable<AttributeInfo> GetAttribute([NotNull] Type type)
		{
			var attrs = _attributes
				.Where(a => a.Name == type.FullName)
				.Concat(_attributes.Where(a => a.Name == type.Name));

			var typeName = type.Name.TrimSuffix("Attribute");

			if (typeName.Length != type.Name.Length)
				attrs = attrs.Concat(_attributes.Where(a => a.Name == typeName));

			return attrs;
		}
	}
}
#endif
