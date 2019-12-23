#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
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