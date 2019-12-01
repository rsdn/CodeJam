#if LESSTHAN_NET40 || LESSTHAN_NETSTANDARD10 || LESSTHAN_NETCOREAPP10 // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
// Some expression types are missing if targeting to these frameworks
#else
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