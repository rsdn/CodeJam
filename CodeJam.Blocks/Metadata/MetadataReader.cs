#if LESSTHAN_NET40 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
// Some expression types are missing if targeting to these frameworks
# else
using System;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.Metadata
{
	internal class MetadataReader : IMetadataReader
	{
		public static readonly MetadataReader Default =
			new MetadataReader(new AttributeReader());

		public MetadataReader([NotNull, ItemNotNull] params IMetadataReader[] readers) =>
			_readers = readers ?? throw new ArgumentNullException(nameof(readers));

		[NotNull][ItemNotNull] private readonly IMetadataReader[] _readers;

		[NotNull, ItemNotNull]
		public T[] GetAttributes<T>([NotNull] Type type, bool inherit) where T : Attribute =>
			_readers.SelectMany(r => r.GetAttributes<T>(type, inherit)).ToArray();

		[NotNull, ItemNotNull]
		public T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit) where T : Attribute =>
			_readers.SelectMany(r => r.GetAttributes<T>(memberInfo, inherit)).ToArray();
	}
}
#endif