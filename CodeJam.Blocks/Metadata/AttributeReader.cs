#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Metadata
{
	internal class AttributeReader : IMetadataReader
	{
		public T[] GetAttributes<T>(Type type, bool inherit = true)
			where T : Attribute =>
				type.GetTypeInfo().GetCustomAttributes<T>(inherit).ToArray();

		public T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit = true)
			where T : Attribute =>
				memberInfo.GetCustomAttributes<T>(inherit).ToArray();
	}
}

#endif