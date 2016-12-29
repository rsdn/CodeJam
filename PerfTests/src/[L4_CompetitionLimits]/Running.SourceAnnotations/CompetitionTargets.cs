using System;
using System.Collections;
using System.Collections.Generic;

using BenchmarkDotNet.Running;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam.PerfTests.Running.SourceAnnotations
{
	/// <summary>Storage class for competition targets.</summary>
	internal sealed class CompetitionTargets : IReadOnlyCollection<CompetitionTarget>
	{
		private readonly Dictionary<TargetCacheKey, CompetitionTarget> _targets = new Dictionary<TargetCacheKey, CompetitionTarget>();

		/// <summary>Gets the <see cref="CompetitionTarget"/> for the specified target.</summary>
		/// <value>The <see cref="CompetitionTarget"/>.</value>
		/// <param name="target">The target.</param>
		/// <returns>Competition target</returns>
		[CanBeNull]
		public CompetitionTarget this[Target target] => _targets.GetValueOrDefault(
			new TargetCacheKey(target.Type.TypeHandle, target.Method.MethodHandle));

		/// <summary>Gets a value indicating whether the collection was filled with targets.</summary>
		/// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
		public bool Initialized { get; private set; }

		/// <summary>Gets number of targets in the competition.</summary>
		public int Count => _targets.Count;

		/// <summary>Adds the specified competition target.</summary>
		/// <param name="competitionTarget">The competition target.</param>
		public void Add(CompetitionTarget competitionTarget)
		{
			Code.AssertState(!Initialized, "The targets is initialized already.");
			_targets.Add(competitionTarget.TargetKey, competitionTarget);
		}

		/// <summary>Marks as initialized.</summary>
		public void SetInitialized()
		{
			Code.AssertState(!Initialized, "The targets is initialized already.");
			Initialized = true;
		}

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<CompetitionTarget> GetEnumerator() => _targets.Values.GetEnumerator();

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}