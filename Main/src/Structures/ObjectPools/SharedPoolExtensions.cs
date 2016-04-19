// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Text;

using JetBrains.Annotations;

namespace CodeJam.ObjectPools
{
	/// <summary>
	/// The <see cref="ObjectPool{T}"/> extension.
	/// </summary>
	// BASEDON: https://github.com/dotnet/roslyn/blob/56f605c41915317ccdb925f66974ee52282609e7/src/Workspaces/Core/Portable/Utilities/ObjectPools/Extensions.cs
	[PublicAPI]
	public static class SharedPoolExtensions
	{
		private const int Threshold = 512;

		/// <summary>
		/// Gets an <see cref="PooledObject{T}"/> from the specified pool that can be released automatically.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="PooledObject{T}"/> instance.
		/// </returns>
		[Pure]
		public static PooledObject<StringBuilder> GetPooledObject([NotNull] this ObjectPool<StringBuilder> pool) =>
			new PooledObject<StringBuilder>(pool, p => p.AllocateAndClear(), (p, sb) => p.ClearAndFree(sb));

		/// <summary>
		/// Gets an <see cref="PooledObject{T}"/> from the specified pool that can be released automatically.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="PooledObject{T}"/> instance.
		/// </returns>
		[Pure]
		public static PooledObject<Stack<TItem>> GetPooledObject<TItem>([NotNull] this ObjectPool<Stack<TItem>> pool) =>
			new PooledObject<Stack<TItem>>(pool, p => p.AllocateAndClear(), (p, obj) => p.ClearAndFree(obj));

		/// <summary>
		/// Gets an <see cref="PooledObject{T}"/> from the specified pool that can be released automatically.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="PooledObject{T}"/> instance.
		/// </returns>
		[Pure]
		public static PooledObject<Queue<TItem>> GetPooledObject<TItem>([NotNull] this ObjectPool<Queue<TItem>> pool) =>
			new PooledObject<Queue<TItem>>(pool, p => p.AllocateAndClear(), (p, obj) => p.ClearAndFree(obj));

		/// <summary>
		/// Gets an <see cref="PooledObject{T}"/> from the specified pool that can be released automatically.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="PooledObject{T}"/> instance.
		/// </returns>
		[Pure]
		public static PooledObject<HashSet<TItem>> GetPooledObject<TItem>([NotNull] this ObjectPool<HashSet<TItem>> pool) =>
			new PooledObject<HashSet<TItem>>(pool, p => p.AllocateAndClear(), (p, obj) => p.ClearAndFree(obj));

		/// <summary>
		/// Gets an <see cref="PooledObject{T}"/> from the specified pool that can be released automatically.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="PooledObject{T}"/> instance.
		/// </returns>
		[Pure]
		public static PooledObject<Dictionary<TKey, TValue>> GetPooledObject<TKey, TValue>([NotNull] this ObjectPool<Dictionary<TKey, TValue>> pool) =>
			new PooledObject<Dictionary<TKey, TValue>>(pool, p => p.AllocateAndClear(), (p, obj) => p.ClearAndFree(obj));

		/// <summary>
		/// Gets an <see cref="PooledObject{T}"/> from the specified pool that can be released automatically.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="PooledObject{T}"/> instance.
		/// </returns>
		[Pure]
		public static PooledObject<List<TItem>> GetPooledObject<TItem>([NotNull] this ObjectPool<List<TItem>> pool) =>
			new PooledObject<List<TItem>>(pool, p => p.AllocateAndClear(), (p, obj) => p.ClearAndFree(obj));

		/// <summary>
		/// Gets an <see cref="PooledObject{T}"/> from the specified pool that can be released automatically.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="PooledObject{T}"/> instance.
		/// </returns>
		[Pure]
		public static PooledObject<T> GetPooledObject<T>([NotNull] this ObjectPool<T> pool) where T : class =>
			new PooledObject<T>(pool);

		/// <summary>
		/// Gets an <see cref="PooledObject{T}"/> from the specified pool that can be released automatically.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <param name="releaser">The function to release object.</param>
		/// <returns>
		/// The <see cref="PooledObject{T}"/> instance.
		/// </returns>
		[Pure]
		public static PooledObject<T> GetPooledObject<T>([NotNull] this ObjectPool<T> pool, [NotNull] Action<ObjectPool<T>, T> releaser) where T : class =>
			new PooledObject<T>(pool, releaser);

		/// <summary>
		/// Allocates a <see cref="StringBuilder"/> and clears a wrapped <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="StringBuilder"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static StringBuilder AllocateAndClear([NotNull] this ObjectPool<StringBuilder> pool)
		{
			var sb = pool.Allocate();
			sb.Clear();

			return sb;
		}

		/// <summary>
		/// Allocates a <see cref="Stack{T}"/> and clears a wrapped <see cref="Stack{T}"/> instance.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="Stack{T}"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static Stack<T> AllocateAndClear<T>([NotNull] this ObjectPool<Stack<T>> pool)
		{
			var stack = pool.Allocate();
			stack.Clear();

			return stack;
		}

		/// <summary>
		/// Allocates a <see cref="Queue{T}"/> and clears a wrapped <see cref="Queue{T}"/> instance.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="Queue{T}"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static Queue<T> AllocateAndClear<T>([NotNull] this ObjectPool<Queue<T>> pool)
		{
			var queue = pool.Allocate();
			queue.Clear();

			return queue;
		}

		/// <summary>
		/// Allocates a <see cref="HashSet{T}"/> and clears a wrapped <see cref="HashSet{T}"/> instance.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="HashSet{T}"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static HashSet<T> AllocateAndClear<T>([NotNull] this ObjectPool<HashSet<T>> pool)
		{
			var set = pool.Allocate();
			set.Clear();

			return set;
		}

		/// <summary>
		/// Allocates a <see cref="Dictionary{TKey,TValue}"/> and clears a wrapped <see cref="Dictionary{TKey,TValue}"/> instance.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="Dictionary{TKey,TValue}"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static Dictionary<TKey, TValue> AllocateAndClear<TKey, TValue>([NotNull] this ObjectPool<Dictionary<TKey, TValue>> pool)
		{
			var map = pool.Allocate();
			map.Clear();

			return map;
		}

		/// <summary>
		/// Allocates a <see cref="List{T}"/> and clears a wrapped <see cref="List{T}"/> instance.
		/// </summary>
		/// <param name="pool">The object pool to allocate from.</param>
		/// <returns>
		/// The <see cref="List{T}"/> instance.
		/// </returns>
		[NotNull, Pure]
		public static List<T> AllocateAndClear<T>([NotNull] this ObjectPool<List<T>> pool)
		{
			var list = pool.Allocate();
			list.Clear();

			return list;
		}

		/// <summary>
		/// Clears and returns objects to the pool.
		/// </summary>
		/// <param name="pool">The object pool to return to.</param>
		/// <param name="sb">The <see cref="StringBuilder"/> to release.</param>
		public static void ClearAndFree([NotNull] this ObjectPool<StringBuilder> pool, StringBuilder sb)
		{
			if (sb == null)
				return;

			sb.Clear();

			if (sb.Capacity > Threshold)
				sb.Capacity = Threshold;

			pool.Free(sb);
		}

		/// <summary>
		/// Clears and returns objects to the pool.
		/// </summary>
		/// <param name="pool">The object pool to return to.</param>
		/// <param name="set">The <see cref="HashSet{T}"/> to release.</param>
		public static void ClearAndFree<T>([NotNull] this ObjectPool<HashSet<T>> pool, HashSet<T> set)
		{
			if (set == null)
				return;

			var count = set.Count;
			set.Clear();

			if (count > Threshold)
				set.TrimExcess();

			pool.Free(set);
		}

		/// <summary>
		/// Clears and returns objects to the pool.
		/// </summary>
		/// <param name="pool">The object pool to return to.</param>
		/// <param name="stack">The <see cref="Stack{T}"/> to release.</param>
		public static void ClearAndFree<T>([NotNull] this ObjectPool<Stack<T>> pool, Stack<T> stack)
		{
			if (stack == null)
				return;

			var count = stack.Count;
			stack.Clear();

			if (count > Threshold)
				stack.TrimExcess();

			pool.Free(stack);
		}

		/// <summary>
		/// Clears and returns objects to the pool.
		/// </summary>
		/// <param name="pool">The object pool to return to.</param>
		/// <param name="queue">The <see cref="Queue{T}"/> to release.</param>
		public static void ClearAndFree<T>([NotNull] this ObjectPool<Queue<T>> pool, Queue<T> queue)
		{
			if (queue == null)
				return;

			var count = queue.Count;
			queue.Clear();

			if (count > Threshold)
				queue.TrimExcess();

			pool.Free(queue);
		}

		/// <summary>
		/// Clears and returns objects to the pool.
		/// </summary>
		/// <param name="pool">The object pool to return to.</param>
		/// <param name="map">The <see cref="Dictionary{TKey,TValue}"/> to release.</param>
		public static void ClearAndFree<TKey, TValue>([NotNull] this ObjectPool<Dictionary<TKey, TValue>> pool, Dictionary<TKey, TValue> map)
		{
			if (map == null)
				return;

			// if map grew too big, don't put it back to pool
			if (map.Count > Threshold)
			{
				pool.ForgetTrackedObject(map);
				return;
			}

			map.Clear();
			pool.Free(map);
		}

		/// <summary>
		/// Clears and returns objects to the pool.
		/// </summary>
		/// <param name="pool">The object pool to return to.</param>
		/// <param name="list">The <see cref="List{T}"/> to release.</param>
		public static void ClearAndFree<T>([NotNull] this ObjectPool<List<T>> pool, List<T> list)
		{
			if (list == null)
				return;

			list.Clear();

			if (list.Capacity > Threshold)
				list.Capacity = Threshold;

			pool.Free(list);
		}
	}
}
