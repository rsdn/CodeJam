#if !LESSTHAN_NET40
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeJam.Metadata
{
	internal class MetaTypeInfo
	{
		public MetaTypeInfo(string name, Dictionary<string,MetaMemberInfo> members, params AttributeInfo[] attributes)
		{
			Name        = name;
			Members     = members;
			_attributes = attributes;
		}

		public readonly string                            Name;
		public readonly Dictionary<string,MetaMemberInfo> Members;

		private readonly AttributeInfo[] _attributes;

		public AttributeInfo[] GetAttribute(Type type)
			=>
				_attributes.Where(a => a.Name == type.FullName).Concat(
				_attributes.Where(a => a.Name == type.Name).    Concat(
					type.Name.EndsWith("Attribute") ?
						_attributes.Where(a => a.Name == type.Name.Substring(0, type.Name.Length - "Attribute".Length)) :
						Enumerable.Empty<AttributeInfo>())
				).ToArray();
	}
}
#endif