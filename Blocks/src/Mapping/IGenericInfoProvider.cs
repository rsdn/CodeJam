#if !LESSTHAN_NET40
using System;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	internal interface IGenericInfoProvider
	{
		void SetInfo([NotNull] MappingSchema mappingSchema);
	}
}
#endif