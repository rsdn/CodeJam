using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using BenchmarkDotNet.Running;

using CodeJam.PerfTests.Running.SourceAnnotations;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Analysers
{
	// TODO: check if it is safe to init from RunStateKey<T> .ctor.
	/// <summary>Storage class for competition targets.</summary>
	internal sealed class CompetitionTargets : IReadOnlyCollection<CompetitionTarget>
	{
		private class TargetsCollection : KeyedCollection<RuntimeMethodHandle, CompetitionTarget>
		{
			/// <summary>When implemented in a derived class, extracts the key from the specified element.</summary>
			/// <returns>The key for the specified element.</returns>
			/// <param name="item">The element from which to extract the key.</param>
			protected override RuntimeMethodHandle GetKeyForItem(CompetitionTarget item) =>
				item.Target.Method.MethodHandle;
		}

		private readonly TargetsCollection _targets = new TargetsCollection();

		/// <summary>Gets a value indicating whether the collection was filled with targets.</summary>
		/// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
		public bool Initialized => AnnotationStorage != null;

		/// <summary>Gets annotation storage for competition targets.</summary>
		/// <value>The annotation storage for competition targets.</value>
		[CanBeNull]
		public IAnnotationStorage AnnotationStorage { get; private set; }

		/// <summary>Gets number of targets in the competition.</summary>
		public int Count => _targets.Count;

		/// <summary>Gets a value indicating whether there's a target with unsaved changes.</summary>
		/// <value>
		/// <c>true</c> if there's a target with unsaved changes; otherwise, <c>false</c>.
		/// </value>
		public bool HasUnsavedChanges =>
			Initialized && _targets.Any(t => t.MetricValues.Any(m => m.HasUnsavedChanges));

		/// <summary>Gets <see cref="CompetitionTarget"/> for the specified target.</summary>
		/// <value>The <see cref="CompetitionTarget"/>.</value>
		/// <param name="target">The target.</param>
		/// <returns>Competition target.</returns>
		[CanBeNull]
		public CompetitionTarget this[Target target]
		{
			get
			{
				var key = target.Method.MethodHandle;
				return _targets.Contains(key) ? _targets[key] : null;
			}
		}

		/// <summary>Adds the specified competition target.</summary>
		/// <param name="competitionTarget">The competition target.</param>
		public void Add(CompetitionTarget competitionTarget)
		{
			Code.AssertState(!Initialized, "The targets is initialized already.");
			_targets.Add(competitionTarget);
		}

		/// <summary>Marks as initialized.</summary>
		/// <param name="annotationStorage">Annotation storage for the competition targets.</param>
		public void SetInitialized(IAnnotationStorage annotationStorage)
		{
			Code.AssertState(!Initialized, "The targets is initialized already.");
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