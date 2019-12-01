#if LESSTHAN_NET40 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
// Some expression types are missing if targeting to these frameworks
#else
using System;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Metadata
{
	internal class AttributeReader : IMetadataReader
	{
		[NotNull]
		public T[] GetAttributes<T>([NotNull] Type type, bool inherit = true)
			where T : Attribute =>
			type.GetTypeInfo().GetCustomAttributes<T>(inherit).ToArray();

		[NotNull]
		public T[] GetAttributes<T>([NotNull] MemberInfo memberInfo, bool inherit = true)
			where T : Attribute =>
			memberInfo.GetCustomAttributes<T>(inherit).ToArray();
	}
}
#endif