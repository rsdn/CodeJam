using System;
using System.Reflection;

namespace CodeJam.Metadata
{
	public interface IMetadataReader
	{
		T[] GetAttributes<T>(Type type,             bool inherit = true) where T : Attribute;
		T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit = true) where T : Attribute;
	}
}
