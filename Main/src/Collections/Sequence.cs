using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	/// <summary>
	/// Contains methods for sequence creation.
	/// </summary>
	[PublicAPI]
	public class Sequence
	{
#pragma warning disable 1591
		public static IEnumerable<T> Create<T>(T start, [NotNull] Func<T, T> next)
			where T : class
		{
			Code.NotNull(next, nameof(next));

			var cur = start;
			while (true)
			{
				yield return cur;
				cur = next(cur);
			}
			// ReSharper disable once IteratorNeverReturns
		}

		public static IEnumerable<TResult> Create<T, TResult>(
			T start,
			[NotNull] Func<T, T> next,
			[NotNull] Func<T, TResult> resultSelector)
			where T : class
		{
			Code.NotNull(next, nameof(next));
			Code.NotNull(resultSelector, nameof(resultSelector));

			var cur = start;
			while (true)
			{
				yield return resultSelector(cur);
				cur = next(cur);
			}
			// ReSharper disable once IteratorNeverReturns
		}

		public static IEnumerable<T> Create<T>(T start, [NotNull] Func<T, bool> predicate, [NotNull] Func<T, T> next)
			where T : class
		{
			Code.NotNull(next, nameof(next));
			Code.NotNull(predicate, nameof (predicate));

			var cur = start;
			while (predicate(cur))
			{
				yield return cur;
				cur = next(cur);
			}
		}

		public static IEnumerable<TResult> Create<T, TResult>(
				T start,
				[NotNull] Func<T, bool> predicate,
				Func<T, T> next, [NotNull] Func<T, TResult> resultSelector)
			where T : class
		{
			Code.NotNull(next, nameof(next));
			Code.NotNull(predicate, nameof(predicate));
			Code.NotNull(resultSelector, nameof(resultSelector));

			var cur = start;
			while (predicate(cur))
			{
				yield return resultSelector(cur);
				cur = next(cur);
			}
		}

		public static IEnumerable<T> CreateWhileNotNull<T>(T start, [NotNull] Func<T, T> next)
			where T: class
		{
			Code.NotNull(next, nameof(next));

			var cur = start;
			while (cur != null)
			{
				yield return cur;
				cur = next(cur);
			}
		}

		public static IEnumerable<TResult> CreateWhileNotNull<T, TResult>(
			T start,
			[NotNull] Func<T, T> next,
			[NotNull] Func<T, TResult> resultSelector)
			where T : class
		{
			Code.NotNull(next, nameof(next));
			Code.NotNull(resultSelector, nameof(resultSelector));

			var cur = start;
			while (cur != null)
			{
				yield return resultSelector(cur);
				cur = next(cur);
			}
		}

		public IEnumerable<T> CreateSingle<T>(T element)
		{
			yield return element;
		}

		public IEnumerable<T> CreateSingle<T>(Func<T> elementFactory)
		{
			yield return elementFactory();
		}
	}
}