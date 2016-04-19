// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.

using System;

using JetBrains.Annotations;

namespace CodeJam.ObjectPools
{
	/// <summary>
	/// The RAII object to automatically release pooled object when its owning pool.
	/// </summary>
	// BASEDON: https://github.com/dotnet/roslyn/blob/56f605c41915317ccdb925f66974ee52282609e7/src/Workspaces/Core/Portable/Utilities/ObjectPools/PooledObject.cs
	[PublicAPI]
	public struct PooledObject<T> : IDisposable where T : class
	{
		private readonly Action<ObjectPool<T>, T> _releaser;
		private readonly ObjectPool<T> _pool;
		private T _pooledObject;

		/// <summary>
		/// Gets the object instance.
		/// </summary>
		[NotNull]
		// ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
		public T Object => _pooledObject;

		/// <summary>
		/// Initializes a new instance of the <see cref="PooledObject{T}"/> class.
		/// </summary>
		/// <param name="pool">The object pool.</param>
		public PooledObject([NotNull] ObjectPool<T> pool) : this()
		{
			_pool = pool;
			_pooledObject = pool.Allocate();
			_releaser = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PooledObject{T}"/> class.
		/// </summary>
		/// <param name="pool">The object pool.</param>
		/// <param name="releaser">The function to release object.</param>
		public PooledObject([NotNull] ObjectPool<T> pool, [NotNull] Action<ObjectPool<T>, T> releaser)
		{
			_pool = pool;
			_pooledObject = pool.Allocate();
			_releaser = releaser;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PooledObject{T}"/> class.
		/// </summary>
		/// <param name="pool">The object pool.</param>
		/// <param name="allocator">Th function to allocate object from the specified pool.</param>
		/// <param name="releaser">The function to release object.</param>
		public PooledObject(
			[NotNull] ObjectPool<T> pool,
			[NotNull] Func<ObjectPool<T>, T> allocator,
			[NotNull] Action<ObjectPool<T>, T> releaser)
		{
			_pool = pool;
			_pooledObject = allocator(pool);
			_releaser = releaser;
		}

		/// <summary>
		/// Returns object to the pool.
		/// </summary>
		public void Dispose()
		{
			if (_pooledObject == null)
				return;

			if (_releaser != null)
			{
				_releaser(_pool, _pooledObject);
			}
			else
			{
				_pool.Free(_pooledObject);
			}

			_pooledObject = null;
		}
	}
}
