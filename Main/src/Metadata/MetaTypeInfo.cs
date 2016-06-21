#if !FW35
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeJam.Metadata
{
	internal class MetaTypeInfo
	{
		public MetaTypeInfo(string name, Dictionary<string,MetaMemberInfo> members, params AttributeInfo[] attributes)
		{
			Name       = name;
			Members    = members;
			Attributes = attributes;
		}

		public readonly string                            Name;
		public readonly Dictionary<string,MetaMemberInfo> Members;
		public readonly AttributeInfo[]                   Attributes;

		public AttributeInfo[] GetAttribute(Type type)
			=>
				Attributes.Where(a => a.Name == type.FullName).Concat(
				Attributes.Where(a => a.Name == type.Name).    Concat(
					type.Name.EndsWith("Attribute") ?
						Attributes.Where(a => a.Name == type.Name.Substring(0, type.Name.Length - "Attribute".Length)) :
						Enumerable.Empty<AttributeInfo>())
				).ToArray();
	}
}
#endif