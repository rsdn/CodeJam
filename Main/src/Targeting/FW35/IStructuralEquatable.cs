#if FW35
// BASEDON: https://github.com/dotnet/corert/blob/4376c55dd018d8b7b9fed82449728711231ba266/src/System.Private.CoreLib/src/System/Collections/IStructuralEquatable.cs

namespace System.Collections
{
	/// <summary>
	/// Defines methods to support the comparison of objects for structural equality.
	/// </summary>
	public interface IStructuralEquatable
	{
		/// <summary>
		/// Determines whether an object is structurally equal to the current instance.
		/// </summary>
		/// <param name="other">The object to compare with the current instance.</param>
		/// <param name="comparer">
		/// An object that determines whether the current instance and <paramref name="other"/> are equal.
		/// </param>
		/// <returns><c>true</c> if the two objects are equal; otherwise, <c>false</c>.</returns>
		bool Equals(object other, IEqualityComparer comparer);

		/// <summary>
		/// Returns a hash code for the current instance.
		/// </summary>
		/// <param name="comparer">An object that computes the hash code of the current object.</param>
		/// <returns>The hash code for the current instance.</returns>
		int GetHashCode(IEqualityComparer comparer);
	}
}
#endif