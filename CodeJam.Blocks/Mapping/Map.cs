﻿#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// Mapper helper class.
	/// </summary>
	/// <example>
	/// This example shows how to map one object to another.
	/// <code source="CodeJam.Blocks.Tests\Mapping\Examples\MapTests.cs" region="Example" lang="C#"/>
	/// </example>
	[PublicAPI]
	public static class Map
	{
		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <typeparam name="TFrom">Type to map from.</typeparam>
		/// <typeparam name="TTo">Type to map to.</typeparam>
		/// <returns>Mapping expression.</returns>
		[Pure]
		public static Mapper<TFrom, TTo> GetMapper<TFrom, TTo>()
			=> new Mapper<TFrom, TTo>(new MapperBuilder<TFrom, TTo>());

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <typeparam name="TFrom">Type to map from.</typeparam>
		/// <typeparam name="TTo">Type to map to.</typeparam>
		/// <param name="setter">MapperBuilder parameter setter.</param>
		/// <returns>Mapping expression.</returns>
		[Pure]
		[NotNull]
		public static Mapper<TFrom, TTo> GetMapper<TFrom, TTo>(
			[NotNull] Func<MapperBuilder<TFrom, TTo>, MapperBuilder<TFrom, TTo>> setter)
		{
			Code.NotNull(setter, nameof(setter));
			return new Mapper<TFrom, TTo>(setter(new MapperBuilder<TFrom, TTo>()));
		}

		private static class MapHolder<T>
		{
			[NotNull]
			public static readonly Mapper<T, T> Mapper =
				GetMapper<T, T>(m => m
					 .SetProcessCrossReferences(true)
					 .SetDeepCopy(true));
		}

		/// <summary>
		/// Performs deep copy.
		/// </summary>
		/// <param name="obj">An object to copy.</param>
		/// <typeparam name="T">Type of object.</typeparam>
		/// <returns>Created object.</returns>
		[Pure]
		public static T DeepCopy<T>(this T obj) => MapHolder<T>.Mapper.Map(obj);
	}
}
#endif
