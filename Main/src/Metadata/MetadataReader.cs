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

		public MetadataReader([NotNull] params IMetadataReader[] readers)
		{
			if (readers == null) throw new ArgumentNullException(nameof(readers));

			_readers = readers;
		}

		private readonly IMetadataReader[] _readers;

		public T[] GetAttributes<T>(Type type, bool inherit) where T : Attribute =>
			_readers.SelectMany(r => r.GetAttributes<T>(type, inherit)).ToArray();

		public T[] GetAttributes<T>(MemberInfo memberInfo, bool inherit) where T : Attribute =>
			_readers.SelectMany(r => r.GetAttributes<T>(memberInfo, inherit)).ToArray();
	}
}
