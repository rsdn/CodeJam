// BASEDON: https://github.com/dotnet/coreclr/blob/a9b57bd4fe194b30b3c6e9a85a316fc218f474be/src/System.Private.CoreLib/shared/System/Action.cs#L29

#if LESSTHAN_NETSTANDARD20 || LESSTHAN_NETCOREAPP20

// ReSharper disable once CheckNamespace

namespace System
{
	/// <summary>Represents a method that converts an object from one type to another type.</summary>
	/// <returns>The <typeparamref name="TOutput" /> that represents the converted <typeparamref name="TInput" />.</returns>
	/// <param name="input">The object to convert.</param>
	/// <typeparam name="TInput">The type of object that is to be converted.</typeparam>
	/// <typeparam name="TOutput">The type the input object is to be converted to.</typeparam>
	/// <filterpriority>1</filterpriority>
	public delegate TOutput Converter<in TInput, out TOutput>(TInput input);
}

#endif
