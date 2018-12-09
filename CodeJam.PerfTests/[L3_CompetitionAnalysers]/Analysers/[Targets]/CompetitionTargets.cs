using BenchmarkDotNet.Running;
using CodeJam.PerfTests.Running.SourceAnnotations;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodeJam.PerfTests.Analysers
{
	// TODO: check if it is safe to init from RunStateKey<T> .ctor.
	/// <summary>Storage class for competition descriptors.</summary>
	internal sealed class CompetitionTargets : IReadOnlyCollection<CompetitionTarget>
	{
		private class TargetsCollection : KeyedCollection<RuntimeMethodHandle, CompetitionTarget>
		{
			/// <summary>When implemented in a derived class, extracts the key from the specified element.</summary>
			/// <returns>The key for the specified element.</returns>
			/// <param name="item">The element from which to extract the key.</param>
			protected override RuntimeMethodHandle GetKeyForItem(CompetitionTarget item) =>
				item.Descriptor.WorkloadMethod.MethodHandle;
		}

		private readonly TargetsCollection _targets = new TargetsCollection();

		/// <summary>Gets a value indicating whether the collection was filled with descriptors.</summary>
		/// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
		public bool Initialized => AnnotationStorage != null;

		/// <summary>Gets annotation storage for competition descriptors.</summary>
		/// <value>The annotation storage for competition descriptors.</value>
		[CanBeNull]
		public IAnnotationStorage AnnotationStorage { get; private set; }

		/// <summary>Gets number of descriptors in the competition.</summary>
		public int Count => _targets.Count;

		/// <summary>Gets a value indicating whether there's a descriptor with unsaved changes.</summary>
		/// <value>
		/// <c>true</c> if there's a descriptor with unsaved changes; otherwise, <c>false</c>.
		/// </value>
		public bool HasUnsavedChanges =>
			Initialized && _targets.Any(t => t.MetricValues.Any(m => m.HasUnsavedChanges));

		/// <summary>Gets <see cref="CompetitionTarget"/> for the specified descriptor.</summary>
		/// <value>The <see cref="CompetitionTarget"/>.</value>
		/// <param name="descriptor">The descriptor.</param>
		/// <returns>Competition descriptor.</returns>
		[CanBeNull]
		public CompetitionTarget this[Descriptor descriptor]
		{
			get
			{
				var key = descriptor.WorkloadMethod.MethodHandle;
				return _targets.Contains(key) ? _targets[key] : null;
			}
		}

		/// <summary>Adds the specified competition descriptor.</summary>
		/// <param name="competitionTarget">The competition descriptor.</param>
		public void Add(CompetitionTarget competitionTarget)
		{
			Code.AssertState(!Initialized, "The descriptors is initialized already.");
			_targets.Add(competitionTarget);
		}

		/// <summary>Marks as initialized.</summary>
		/// <param name="annotationStorage">Annotation storage for the competition descriptors.</param>
		public void SetInitialized(IAnnotationStorage annotationStorage)
		{
			Code.AssertState(!Initialized, "The descriptors is initialized already.");
			AnnotationStorage = annotationStorage;
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<CompetitionTarget> GetEnumerator() => _targets.GetEnumerator();

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}