#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP // PUBLIC_API_CHANGES. TODO: update after fixes in Theraot.Core
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	/// <summary>
	/// Maps an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
	/// </summary>
	/// <typeparam name="TFrom">Type to map from.</typeparam>
	/// <typeparam name="TTo">Type to map to.</typeparam>
	/// <example>
	/// This example shows how to map one object to another.
	/// <code source="CodeJam.Blocks.Tests\Mapping\Examples\MapTests.cs" region="Example" lang="C#"/>
	/// </example>
	[PublicAPI]
	public class Mapper<TFrom, TTo>
	{
		[JetBrains.Annotations.NotNull] private MapperBuilder<TFrom, TTo> _mapperBuilder;

		[DisallowNull]
		private Expression<Func<TFrom?, TTo?, IDictionary<object, object>?, TTo>>? _mapperExpression;

		[DisallowNull]
		private Expression<Func<TFrom?, TTo?>>? _mapperExpressionEx = null!;

		[DisallowNull]
		private Func<TFrom?, TTo?, IDictionary<object, object>, TTo>? _mapper = null!;

		[DisallowNull]
		private Func<TFrom, TTo>? _mapperEx = null!;

		internal Mapper([JetBrains.Annotations.NotNull] MapperBuilder<TFrom, TTo> mapperBuilder) => _mapperBuilder = mapperBuilder;

		/// <summary>
		/// Returns a mapper expression to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// Returned expression is compatible to IQueryable.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure, JetBrains.Annotations.NotNull]
		public Expression<Func<TFrom?, TTo?>> GetMapperExpressionEx()
			=> _mapperExpressionEx ??= _mapperBuilder.GetMapperExpressionEx();

		/// <summary>
		/// Returns a mapper expression to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure, JetBrains.Annotations.NotNull]
		public Expression<Func<TFrom?, TTo?, IDictionary<object, object>?, TTo>> GetMapperExpression()
			=> _mapperExpression ??= _mapperBuilder.GetMapperExpression();

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure, JetBrains.Annotations.NotNull]
		public Func<TFrom, TTo> GetMapperEx()
			=> _mapperEx ??= GetMapperExpressionEx().Compile();

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure, JetBrains.Annotations.NotNull]
		public Func<TFrom?, TTo?, IDictionary<object, object>?, TTo> GetMapper()
			=> _mapper ??= GetMapperExpression().Compile();

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <param name="source">Object to map.</param>
		/// <returns>Destination object.</returns>
		[Pure]
		public TTo Map(TFrom? source)
			=> GetMapperEx()(source);

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <param name="source">Object to map.</param>
		/// <param name="destination">Destination object.</param>
		/// <returns>Destination object.</returns>
		public TTo Map(TFrom? source, TTo? destination)
			=> GetMapper()(source, destination, new Dictionary<object, object>());

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <param name="source">Object to map.</param>
		/// <param name="destination">Destination object.</param>
		/// <param name="crossReferenceDictionary">Storage for cress references if applied.</param>
		/// <returns>Destination object.</returns>
		[Pure]
		public TTo Map(TFrom? source, TTo? destination, IDictionary<object, object>? crossReferenceDictionary)
			=> GetMapper()(source, destination, crossReferenceDictionary);
	}
}
#endif