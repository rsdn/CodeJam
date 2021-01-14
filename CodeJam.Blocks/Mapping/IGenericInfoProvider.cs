#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	internal interface IGenericInfoProvider
	{
		void SetInfo(MappingSchema mappingSchema);
	}
}

#endif