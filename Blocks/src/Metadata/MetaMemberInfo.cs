#if !FW35
using System;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Strings;

namespace CodeJam.Metadata
{
	internal class MetaMemberInfo
	{
		public MetaMemberInfo(string name, params AttributeInfo[] attributes)
		{
			Name        = name;
			_attributes = attributes;
		}

		public readonly string Name;

		private readonly AttributeInfo[] _attributes;

		public IEnumerable<AttributeInfo> GetAttribute(Type type)
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
