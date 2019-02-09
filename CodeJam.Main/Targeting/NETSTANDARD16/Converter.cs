// BASEDON: https://github.com/dotnet/coreclr/blob/a9b57bd4fe194b30b3c6e9a85a316fc218f474be/src/System.Private.CoreLib/shared/System/Action.cs#L29

#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20

namespace System
{
	public delegate TOutput Converter<in TInput, out TOutput>(TInput input);
}

#endif