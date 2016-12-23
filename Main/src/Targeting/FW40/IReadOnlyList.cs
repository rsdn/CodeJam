#if FW40
// BASEDON: https://github.com/dotnet/coreclr/blob/master/src/mscorlib/src/System/Collections/Generic/IReadOnlyList.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** Interface:  IReadOnlyList<T>
** 
** 
**
** Purpose: Base interface for read-only generic lists.
** 
===========================================================*/
using System;

namespace System.Collections.Generic
{

	// Provides a read-only, covariant view of a generic list.

	// Note that T[] : IReadOnlyList<T>, and we want to ensure that if you use
	// IList<YourValueType>, we ensure a YourValueType[] can be used 
	// without jitting.  Hence the TypeDependencyAttribute on SZArrayHelper.
	// This is a special workaround internally though - see VM\compile.cpp.
	// The same attribute is on IList<T>, IEnumerable<T>, ICollection<T> and IReadOnlyCollection<T>.
	// If we ever implement more interfaces on IReadOnlyList, we should also update RuntimeTypeCache.PopulateInterfaces() in rttype.cs
	/// <summary>
	/// Represents a read-only collection of elements that can be accessed by index.
	/// </summary>
	/// <typeparam name="T">The type of elements in the read-only list.</typeparam>
	public interface IReadOnlyList<
#if !FW35
	out
#endif
	T> : IReadOnlyCollection<T>
	{
		/// <summary>
		/// Gets the element at the specified index in the read-only list.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get.</param>
		/// <returns>The element at the specified index in the read-only list.</returns>
		T this[int index] { get; }
	}
}
#endif