#if FW35
// BASEDON: https://github.com/dotnet/corert/blob/4376c55dd018d8b7b9fed82449728711231ba266/src/System.Private.CoreLib/src/System/Collections/IStructuralComparable.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Collections
{
	/// <summary>
	/// Supports the structural comparison of collection objects.
	/// </summary>
	public interface IStructuralComparable
	{
		/// <summary>
		/// Determines whether the current collection object precedes, occurs in the same position as,
		/// or follows another object in the sort order.
		/// </summary>
		/// <param name="other">The object to compare with the current instance.</param>
		/// <param name="comparer">
		/// An object that compares members of the current collection object with the corresponding members of
		/// <paramref name="other"/>.
		/// </param>
		/// <returns>
		/// An integer that indicates the relationship of the current collection object to <paramref name="other"/>.
		/// </returns>
		int CompareTo(object other, IComparer comparer);
	}
}
#endif