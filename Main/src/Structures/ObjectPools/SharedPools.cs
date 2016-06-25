// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.

using System;

using JetBrains.Annotations;

namespace CodeJam.ObjectPools
{
	/// <summary>
	/// Shared object pool for roslyn
	/// 
	/// Use this shared pool if only concern is reducing object allocations.
	/// if perf of an object pool itself is also a concern, use ObjectPool directly.
	/// 
	/// For example, if you want to create a million of small objects within a second, 
	/// use the ObjectPool directly. it should have much less overhead than using this.
	/// </summary>
	// BASEDON: https://github.com/dotnet/roslyn/blob/56f605c41915317ccdb925f66974ee52282609e7/src/Workspaces/Core/Portable/Utilities/ObjectPools/SharedPools.cs
	[PublicAPI]
	public static class SharedPools
	{
		/// <summary>
		/// The pool that uses default constructor with 100 elements pooled
		/// </summary>
		/// <returns><see cref="ObjectPool{T}"/> instance.</returns>
		[NotNull]
		public static ObjectPool<T> BigDefault<T>() where T : class, new() => DefaultBigPool<T>.Instance;

		/// <summary>
		/// The pool that uses default constructor with 20 elements pooled
		/// </summary>
		/// <returns><see cref="ObjectPool{T}"/> instance.</returns>
		[NotNull]
		public static ObjectPool<T> Default<T>() where T : class, new() => DefaultNormalPool<T>.Instance;

		#region Inner type: DefaultBigPool

		private static class DefaultBigPool<T> where T : class, new()
		{
			public static readonly ObjectPool<T> Instance = new ObjectPool<T>(() => new T(), 100);
		}

		#endregion

		#region Inner type: DefaultNormalPool

		private static class DefaultNormalPool<T> where T : class, new()
		{
			public static readonly ObjectPool<T> Instance = new ObjectPool<T>(() => new T(), 20);
		}

		#endregion
	}
}
