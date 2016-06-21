#if !FW35
using System;

namespace CodeJam.Mapping
{
	internal interface IGenericInfoProvider
	{
		void SetInfo(MappingSchema mappingSchema);
	}
}
#endif