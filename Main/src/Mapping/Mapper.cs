#if !FW35
using System;
using System.Collections.Generic;
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
	/// <code source="Main\tests\Mapping\Examples\MapTests.cs" lang="C#"/>
	/// </example>
	[PublicAPI]
	public class Mapper<TFrom,TTo>
	{
		private Expression<Func<TFrom,TTo,IDictionary<object,object>,TTo>> _mapperExpression;

		private Expression<Func<TFrom,TTo>> _mapperExpressionEx;

		private Func<TFrom,TTo,IDictionary<object,object>,TTo> _mapper;

		private Func<TFrom,TTo> _mapperEx;

		internal Mapper(MapperBuilder<TFrom,TTo> mapperBuilder)
		{
			_mapperExpression   = mapperBuilder.GetMapperExpression();
			_mapperExpressionEx = mapperBuilder.GetMapperExpressionEx();

			_mapper   = _mapperExpression.  Compile();
			_mapperEx = _mapperExpressionEx.Compile();
		}

		/// <summary>
		/// Returns a mapper expression to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// Returned expression is compatible to IQueriable.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure, NotNull]
		public Expression<Func<TFrom,TTo>> GetMapperExpressionEx()
			=> _mapperExpressionEx;

		/// <summary>
		/// Returns a mapper expression to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure, NotNull]
		public Expression<Func<TFrom,TTo,IDictionary<object,object>,TTo>> GetMapperExpression()
			=> _mapperExpression;

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure, NotNull]
		public Func<TFrom,TTo> GetMapperEx()
			=> _mapperEx;

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure, NotNull]
		public Func<TFrom,TTo,IDictionary<object,object>,TTo> GetMapper()
			=> _mapper;

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <param name="source">Object to map.</param>
		/// <returns>Destination object.</returns>
		[Pure]
		public TTo Map(TFrom source)
			=> _mapperEx(source);

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <param name="source">Object to map.</param>
		/// <param name="destination">Destination object.</param>
		/// <returns>Destination object.</returns>
		public TTo Map(TFrom source, TTo destination)
			=> _mapper(source, destination, new Dictionary<object, object>());

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <param name="source">Object to map.</param>
		/// <param name="destination">Destination object.</param>
		/// <param name="crossReferenceDictionary">Storage for cress references if applied.</param>
		/// <returns>Destination object.</returns>
		[Pure]
		public TTo Map(TFrom source, TTo destination, IDictionary<object,object> crossReferenceDictionary)
			=> _mapper(source, destination, crossReferenceDictionary);
	}
}
#endif