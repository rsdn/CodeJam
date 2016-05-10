using System;
using System.Linq;

using JetBrains.Annotations;

namespace CodeJam.Metadata
{
	class MetaMemberInfo
	{
		public MetaMemberInfo(string name, params AttributeInfo[] attributes)
		{
			Name       = name;
			Attributes = attributes;
		}

		public readonly string          Name;
		public readonly AttributeInfo[] Attributes;

		public AttributeInfo[] GetAttribute(Type type)
			=>
				Attributes.Where(a => a.Name == type.FullName).Concat(
				Attributes.Where(a => a.Name == type.Name)).   Concat(
					type.Name.EndsWith("Attribute") ?
						Attributes.Where(a => a.Name == type.Name.Substring(0, type.Name.Length - "Attribute".Length)) :
						Enumerable.Empty<AttributeInfo>()
				).ToArray();
	}
}
