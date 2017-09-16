#if LESSTHAN_NET40
// BASEDON: https://github.com/dotnet/coreclr/blob/76c62b72ef2642c3ad91209acf02db6c8b42aff7/src/mscorlib/src/System/Action.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System
{
	/// <summary>
	/// Encapsulates a method that has five parameters and returns a value of the type specified
	/// by the <typeparamref name="TResult"/> parameter.
	/// </summary>
	/// <typeparam name="TArg1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg4">The type of the forth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
	/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg4">The forth parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
	/// <returns></returns>
	public delegate TResult Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(
		TArg1 arg1,
		TArg2 arg2,
		TArg3 arg3,
		TArg4 arg4,
		TArg5 arg5);

	/// <summary>
	/// Encapsulates a method that has five parameters and returns a value of the type specified
	/// by the <typeparamref name="TResult"/> parameter.
	/// </summary>
	/// <typeparam name="TArg1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg4">The type of the forth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
	/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg4">The forth parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
	/// <returns></returns>
	public delegate TResult Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(
		TArg1 arg1,
		TArg2 arg2,
		TArg3 arg3,
		TArg4 arg4,
		TArg5 arg5,
		TArg6 arg6);

	/// <summary>
	/// Encapsulates a method that has five parameters and returns a value of the type specified
	/// by the <typeparamref name="TResult"/> parameter.
	/// </summary>
	/// <typeparam name="TArg1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg4">The type of the forth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg7">The type of the seventh parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
	/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg4">The forth parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg7">The seventh parameter of the method that this delegate encapsulates.</param>
	/// <returns></returns>
	public delegate TResult Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(
		TArg1 arg1,
		TArg2 arg2,
		TArg3 arg3,
		TArg4 arg4,
		TArg5 arg5,
		TArg6 arg6,
		TArg7 arg7);

	/// <summary>
	/// Encapsulates a method that has five parameters and returns a value of the type specified
	/// by the <typeparamref name="TResult"/> parameter.
	/// </summary>
	/// <typeparam name="TArg1">The type of the first parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg4">The type of the forth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg7">The type of the seventh parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TArg8">The type of the eighth parameter of the method that this delegate encapsulates.</typeparam>
	/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.</typeparam>
	/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg4">The forth parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg7">The seventh parameter of the method that this delegate encapsulates.</param>
	/// <param name="arg8">The eighth parameter of the method that this delegate encapsulates.</param>
	/// <returns></returns>
	public delegate TResult Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(
		TArg1 arg1,
		TArg2 arg2,
		TArg3 arg3,
		TArg4 arg4,
		TArg5 arg5,
		TArg6 arg6,
		TArg7 arg7,
		TArg8 arg8);
}
#endif