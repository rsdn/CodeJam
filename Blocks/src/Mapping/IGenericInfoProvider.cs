#if !SUPPORTS_NET35
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