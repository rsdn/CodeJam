#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Linq;
using System.Reflection;

namespace CodeJam.Metadata
{
	internal class MetadataReader : IMetadataReader
	{
		public static readonly MetadataReader Default =
			new(new AttributeReader());

		public MetadataReader(params IMetadataReader[] readers) =>
			_readers = readers ?? throw new ArgumentNullException(nameof(readers));

		private readonly IMetadataReader[] _readers;

		public T[] GetAttributes<T>(Type type, bool inherit) where T : Attribute =>
			_readers.SelectMany(r => r.GetAttributes<T>(type, inherit)).ToArray();

		public T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit) where T : Attribute =>
			_readers.SelectMany(r => r.GetAttributes<T>(memberInfo, inherit)).ToArray();
	}
}
#endif