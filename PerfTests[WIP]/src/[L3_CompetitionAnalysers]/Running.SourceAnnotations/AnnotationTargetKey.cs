using System;
using System.Collections.Generic;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>
	/// Key object for mapping annotation documents to the targets.
	/// Depending on the caching schema the key may include 
	/// <see cref="TargetMethod"/> only
	/// or <see cref="BenchmarkType"/> only
	/// or both of them
	/// Can be persisted in the current appdomain only.
	/// </summary>
	/// <seealso cref="System.IEquatable{AnnotationDocumentKey}"/>
	// DONTTOUCH: DO NOT mark the type as serializable & DO NOT add equality operators
	internal struct AnnotationTargetKey : IEquatable<AnnotationTargetKey>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AnnotationTargetKey"/> struct for method-level annotations.
		/// </summary>
		/// <param name="targetMethod">The target method.</param>
		public AnnotationTargetKey(RuntimeMethodHandle targetMethod)
		{
			TargetMethod = targetMethod;
			BenchmarkType = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnnotationTargetKey"/> struct for type-level annotations.
		/// </summary>
		/// <param name="benchmarkType">The benchmark type.</param>
		public AnnotationTargetKey(RuntimeTypeHandle benchmarkType)
		{
			TargetMethod = null;
			BenchmarkType = benchmarkType;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AnnotationTargetKey"/> struct for method-level annotations
		/// (annotations are not inherited).
		/// </summary>
		/// <param name="targetMethod">The target method.</param>
		/// <param name="benchmarkType">The benchmark type. Should be used if annotations are not inherited.</param>
		public AnnotationTargetKey(RuntimeMethodHandle targetMethod, RuntimeTypeHandle benchmarkType)
		{
			TargetMethod = targetMethod;
			BenchmarkType = benchmarkType;
		}

		/// <summary>Gets a value indicating whether this instance is empty.</summary>
		/// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
		public bool IsEmpty => TargetMethod == null && BenchmarkType == null;

		/// <summary>The target method.</summary>
		/// <value>The target method.</value>
		public RuntimeMethodHandle? TargetMethod { get; }

		/// <summary>Gets benchmark type. Should be used if annotations are not inherited.</summary>
		/// <value>The benchmark type.</value>
		public RuntimeTypeHandle? BenchmarkType { get; }

		#region Equality members
		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
		public bool Equals(AnnotationTargetKey other) =>
			EqualityComparer<RuntimeMethodHandle?>.Default.Equals(TargetMethod, other.TargetMethod) &&
			EqualityComparer<RuntimeTypeHandle?>.Default.Equals(BenchmarkType, other.BenchmarkType);

		/// <summary>Determines whether the <paramref name="obj"/> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		/// <c>true</c> if the <paramref name="obj"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj) => obj is AnnotationTargetKey other && Equals(other);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode() =>
			HashCode.Combine(
				TargetMethod?.GetHashCode() ?? 0,
				BenchmarkType?.GetHashCode() ?? 0);
		#endregion
	}
}