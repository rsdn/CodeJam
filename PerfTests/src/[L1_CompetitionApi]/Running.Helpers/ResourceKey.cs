using System;
using System.Reflection;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.Helpers
{
	/// <summary>
	/// Key for assembly resources. Can be persisted in the current appdomain only.
	/// </summary>
	/// <seealso cref="System.IEquatable{TargetKey}"/>
	// DONTTOUCH: DO NOT mark the type as serializable.
	// DONTTOUCH: DO NOT add equality operators. Resource keys are not intended to be comparable.
	public struct ResourceKey : IEquatable<ResourceKey>
	{
		/// <summary>Initializes a new instance of the <see cref="ResourceKey"/> struct.</summary>
		/// <param name="assembly">The assembly that contains resource.</param>
		/// <param name="resourceName">The name of the resource.</param>
		public ResourceKey([NotNull] Assembly assembly, [NotNull] string resourceName)
		{
			Code.NotNull(assembly, nameof(assembly));
			Code.NotNullNorEmpty(resourceName, nameof(resourceName));

			Assembly = assembly;
			ResourceName = resourceName;
		}

		/// <summary>Gets a value indicating whether this instance is empty.</summary>
		/// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => Assembly == null && ResourceName == null;

		/// <summary>Gets the assembly that contains resource.</summary>
		/// <value>The assembly that contains resource.</value>
		[CanBeNull]
		public Assembly Assembly { get; }

		/// <summary>Gets the name of the resource.</summary>
		/// <value>The name of the resource.</value>
		[CanBeNull]
		public string ResourceName { get; }

		#region Equality members
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
		public bool Equals(ResourceKey other) =>
			Equals(Assembly, other.Assembly) && Equals(ResourceName, other.ResourceName);

		/// <summary>Determines whether the <paramref name="obj"/> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the <paramref name="obj"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is ResourceKey other && Equals(other);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode() => HashCode.Combine(
			Assembly?.GetHashCode() ?? 0,
			ResourceName?.GetHashCode() ?? 0);
		#endregion

		/// <summary>Returns a <see cref="string"/> that represents this instance.</summary>
		/// <returns>A <see cref="string"/> that represents this instance.</returns>
		public override string ToString() => IsEmpty ? "N/A" : $"{ResourceName}@{Assembly?.GetName().Name}";
	}
}