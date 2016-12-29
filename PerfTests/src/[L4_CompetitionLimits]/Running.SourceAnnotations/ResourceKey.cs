using System;
using System.Reflection;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Key for assembly resources. Can be persisted in the current appdomain only.
	/// </summary>
	/// <seealso cref="System.IEquatable{TargetKey}" />
	// DONTTOUCH: DO NOT mark the type as serializable & DO NOT add equality operators
	public struct ResourceKey : IEquatable<ResourceKey>
	{
		/// <summary>Initializes a new instance of the <see cref="ResourceKey" /> struct.</summary>
		/// <param name="assembly">The assembly that contains resource.</param>
		/// <param name="resourceName">The name of the resource.</param>
		public ResourceKey(Assembly assembly, string resourceName)
		{
			Assembly = assembly;
			ResourceName = resourceName;
		}

		/// <summary>Gets the assembly that contains resource.</summary>
		/// <value>The assembly that contains resource.</value>
		public Assembly Assembly { get; }
		/// <summary>Gets the name of the resource.</summary>
		/// <value>The name of the resource.</value>
		public string ResourceName { get; }

		#region Equality members
		/// <summary>Equalses the specified other.</summary>
		/// <param name="other">The other.</param>
		/// <returns>
		///   <c>true</c> if the <paramref name="other"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(ResourceKey other) =>
			Equals(Assembly, other.Assembly) && Equals(ResourceName, other.ResourceName);

		/// <summary>Determines whether the <paramref name="obj"/> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the <paramref name="obj"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is ResourceKey && Equals((ResourceKey)obj);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(
			Assembly?.GetHashCode()??0,
			ResourceName?.GetHashCode() ?? 0);
		#endregion
	}
}