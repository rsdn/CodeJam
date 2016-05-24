using System;
using System.Linq;

using CodeJam.Strings;

namespace CodeJam.Metadata
{
	internal class MetaMemberInfo
	{
		public MetaMemberInfo(string name, params AttributeInfo[] attributes)
		{
			Name       = name;
			Attributes = attributes;
		}

		public readonly string          Name;
		public readonly AttributeInfo[] Attributes;

		public AttributeInfo[] GetAttribute(Type type)
		{
			var attrs =
				Attributes
					.Where(a => a.Name == type.FullName)
					.Concat(Attributes.Where(a => a.Name == type.Name));
			var typeName = type.Name.TrimSuffix("Attribute");
			if (typeName.Length != type.Name.Length)
				attrs = attrs.Concat(Attributes.Where(a => a.Name == typeName));
			return attrs.ToArray();
		}
	}
}
