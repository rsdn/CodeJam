using System;

using BenchmarkDotNet.Running;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Cache key for the <see cref="Target"/>. Can be persisted in the current appdomain only.
	/// </summary>
	/// <seealso cref="System.IEquatable{TargetKey}"/>
	// DONTTOUCH: DO NOT mark the type as serializable & DO NOT add equality operators
	public struct TargetCacheKey : IEquatable<TargetCacheKey>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TargetCacheKey"/> struct.
		/// </summary>
		/// <param name="targetType">The target type.</param>
		/// <param name="targetMethod">The target method.</param>
		public TargetCacheKey(RuntimeTypeHandle targetType, RuntimeMethodHandle targetMethod)
		{
			TargetType = targetType;
			TargetMethod = targetMethod;
		}

		/// <summary>The target type.</summary>
		/// <value>The target type.</value>
		public RuntimeTypeHandle TargetType { get; }

		/// <summary>The target method.</summary>
		/// <value>The target method.</value>
		public RuntimeMethodHandle TargetMethod { get; }

		#region Equality members
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
		public bool Equals(TargetCacheKey other) =>
			TargetType.Equals(other.TargetType) && TargetMethod.Equals(other.TargetMethod);

		/// <summary>Determines whether the <paramref name="obj"/> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the <paramref name="obj"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is TargetCacheKey && Equals((TargetCacheKey)obj);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(TargetType.GetHashCode(), TargetMethod.GetHashCode());
		#endregion
	}
}