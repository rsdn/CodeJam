using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	using Expressions;
	using Reflection;

	/// <summary>
	/// Maps an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
	/// </summary>
	/// <typeparam name="TFrom">Type to map from.</typeparam>
	/// <typeparam name="TTo">Type to map to.</typeparam>
	[PublicAPI]
	public class Mapper<TFrom,TTo>
	{
		MappingSchema                                  _mappingSchema;
		List<Tuple<LambdaExpression,LambdaExpression>> _memberMappers;
		Func<MemberAccessor,bool>                      _memberFilter = _ => true;
		bool?                                          _processCrossReferences;
		Dictionary<Type,Dictionary<string,string>>     _fromMapping;
		Dictionary<Type,Dictionary<string,string>>     _toMapping;

		/// <summary>
		/// Sets mapping schema.
		/// </summary>
		/// <param name="schema">Mapping schema to set.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> SetMappingSchema([NotNull] MappingSchema schema)
		{
			Code.NotNull(schema, nameof(schema));

			_mappingSchema = schema;
			return this;
		}

		/// <summary>
		/// Adds a predicate to filter target members to map.
		/// </summary>
		/// <param name="predicate">Predicate to filter members to map.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> MemberFilter([NotNull] Func<MemberAccessor,bool> predicate)
		{
			Code.NotNull(predicate, nameof(predicate));

			_memberFilter = predicate;
			return this;
		}

		/// <summary>
		/// Defines member name mapping for source types.
		/// </summary>
		/// <param name="type">Type to map.</param>
		/// <param name="memberName">Type member name.</param>
		/// <param name="mapName">Mapping name.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> FromMapping([NotNull] Type type, [NotNull] string memberName, [NotNull] string mapName)
		{
			Code.NotNull(type,       nameof(type));
			Code.NotNull(memberName, nameof(memberName));
			Code.NotNull(mapName,    nameof(mapName));

			if (_fromMapping == null)
				_fromMapping = new Dictionary<Type,Dictionary<string,string>>();

			Dictionary<string,string> dic;

			if (!_fromMapping.TryGetValue(type, out dic))
				_fromMapping[type] = dic = new Dictionary<string,string>();

			dic[memberName] = mapName;

			return this;
		}

		/// <summary>
		/// Defines member name mapping for source types.
		/// </summary>
		/// <typeparam name="T">Type to map.</typeparam>
		/// <param name="memberName">Type member name.</param>
		/// <param name="mapName">Mapping name.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> FromMapping<T>([NotNull] string memberName, [NotNull] string mapName)
			=> FromMapping(typeof(T), memberName, mapName);

		/// <summary>
		/// Defines member name mapping for source types.
		/// </summary>
		/// <param name="memberName">Type member name.</param>
		/// <param name="mapName">Mapping name.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> FromMapping([NotNull] string memberName, [NotNull] string mapName)
			=> FromMapping(typeof(TFrom), memberName, mapName);

		/// <summary>
		/// Defines member name mapping for source types.
		/// </summary>
		/// <param name="type">Type to map.</param>
		/// <param name="mapping">Mapping parameters.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> FromMapping([NotNull] Type type, [NotNull] IReadOnlyDictionary<string,string> mapping)
		{
			Code.NotNull(type,    nameof(type));
			Code.NotNull(mapping, nameof(mapping));

			foreach (var item in mapping)
				FromMapping(type, item.Key, item.Value);
			return this;
		}

		/// <summary>
		/// Defines member name mapping for source types.
		/// </summary>
		/// <param name="mapping">Mapping parameters.</param>
		/// <typeparam name="T">Type to map.</typeparam>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> FromMapping<T>(IReadOnlyDictionary<string,string> mapping)
			=> FromMapping(typeof(T), mapping);

		/// <summary>
		/// Defines member name mapping for source types.
		/// </summary>
		/// <param name="mapping">Mapping parameters.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> FromMapping(IReadOnlyDictionary<string,string> mapping)
			=> FromMapping(typeof(TFrom), mapping);

		/// <summary>
		/// Defines member name mapping for destination types.
		/// </summary>
		/// <param name="type">Type to map.</param>
		/// <param name="memberName">Type member name.</param>
		/// <param name="mapName">Mapping name.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> ToMapping(Type type, string memberName, string mapName)
		{
			if (_toMapping == null)
				_toMapping = new Dictionary<Type,Dictionary<string,string>>();

			Dictionary<string,string> dic;

			if (!_toMapping.TryGetValue(type, out dic))
				_toMapping[type] = dic = new Dictionary<string,string>();

			dic[memberName] = mapName;

			return this;
		}

		/// <summary>
		/// Defines member name mapping for destination types.
		/// </summary>
		/// <typeparam name="T">Type to map.</typeparam>
		/// <param name="memberName">Type member name.</param>
		/// <param name="mapName">Mapping name.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> ToMapping<T>(string memberName, string mapName)
			=> ToMapping(typeof(T), memberName, mapName);

		/// <summary>
		/// Defines member name mapping for destination types.
		/// </summary>
		/// <param name="memberName">Type member name.</param>
		/// <param name="mapName">Mapping name.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> ToMapping(string memberName, string mapName)
			=> ToMapping(typeof(TTo), memberName, mapName);

		/// <summary>
		/// Defines member name mapping for destination types.
		/// </summary>
		/// <param name="type">Type to map.</param>
		/// <param name="mapping">Mapping parameters.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> ToMapping([NotNull] Type type, [NotNull] IReadOnlyDictionary<string,string> mapping)
		{
			Code.NotNull(type,    nameof(type));
			Code.NotNull(mapping, nameof(mapping));

			foreach (var item in mapping)
				ToMapping(type, item.Key, item.Value);
			return this;
		}

		/// <summary>
		/// Defines member name mapping for destination types.
		/// </summary>
		/// <param name="mapping">Mapping parameters.</param>
		/// <typeparam name="T">Type to map.</typeparam>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> ToMapping<T>(IReadOnlyDictionary<string,string> mapping)
			=> ToMapping(typeof(T), mapping);

		/// <summary>
		/// Defines member name mapping for destination types.
		/// </summary>
		/// <param name="mapping">Mapping parameters.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> ToMapping(IReadOnlyDictionary<string,string> mapping)
			=> ToMapping(typeof(TTo), mapping);

		/// <summary>
		/// Defines member name mapping for source and destination types.
		/// </summary>
		/// <param name="type">Type to map.</param>
		/// <param name="memberName">Type member name.</param>
		/// <param name="mapName">Mapping name.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> Mapping(Type type, string memberName, string mapName)
			=> FromMapping(type, memberName, mapName).ToMapping (type, memberName, mapName);

		/// <summary>
		/// Defines member name mapping for source and destination types.
		/// </summary>
		/// <typeparam name="T">Type to map.</typeparam>
		/// <param name="memberName">Type member name.</param>
		/// <param name="mapName">Mapping name.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> Mapping<T>(string memberName, string mapName)
			=> Mapping(typeof(T), memberName, mapName);

		/// <summary>
		/// Defines member name mapping for source and destination types.
		/// </summary>
		/// <param name="memberName">Type member name.</param>
		/// <param name="mapName">Mapping name.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> Mapping(string memberName, string mapName)
			=> Mapping(typeof(TFrom), memberName, mapName).Mapping(typeof(TTo), memberName, mapName);

		/// <summary>
		/// Defines member name mapping for source and destination types.
		/// </summary>
		/// <param name="type">Type to map.</param>
		/// <param name="mapping">Mapping parameters.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> Mapping([NotNull] Type type, [NotNull] IReadOnlyDictionary<string,string> mapping)
		{
			Code.NotNull(type,    nameof(type));
			Code.NotNull(mapping, nameof(mapping));

			foreach (var item in mapping)
				Mapping(type, item.Key, item.Value);
			return this;
		}

		/// <summary>
		/// Defines member name mapping for source and destination types.
		/// </summary>
		/// <param name="mapping">Mapping parameters.</param>
		/// <typeparam name="T">Type to map.</typeparam>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> Mapping<T>(IReadOnlyDictionary<string,string> mapping)
			=> Mapping(typeof(T), mapping);

		/// <summary>
		/// Defines member name mapping for source and destination types.
		/// </summary>
		/// <param name="mapping">Mapping parameters.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> Mapping(IReadOnlyDictionary<string,string> mapping)
			=> Mapping(typeof(TFrom), mapping).Mapping(typeof(TFrom), mapping);

		/// <summary>
		/// Returns a mapper expression to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure]
		public Expression<Func<TFrom,TTo>> GetMapperExpression()
			=> GetExpressionMapper().GetExpression();

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure]
		public Func<TFrom,TTo> GetMapper() => GetMapperExpression().Compile();

		/// <summary>
		/// Returns a mapper expression to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure]
		public Expression<Func<TFrom,TTo,TTo>> GetActionMapperExpression()
			=> GetExpressionMapper().GetActionExpression();

		/// <summary>
		/// Returns a mapper to map an object of <i>TFrom</i> type to an object of <i>TTo</i> type.
		/// </summary>
		/// <returns>Mapping expression.</returns>
		[Pure]
		public Func<TFrom,TTo,TTo> GetActionMapper() => GetActionMapperExpression().Compile();

		/// <summary>
		/// Adds member mapper.
		/// </summary>
		/// <typeparam name="T">Type of the member to map.</typeparam>
		/// <param name="toMember">Expression that returns a member to map.</param>
		/// <param name="setter">Expression to set the member.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> MapMember<T>(Expression<Func<TTo,T>> toMember, Expression<Func<TFrom,T>> setter)
		{
			if (_memberMappers == null)
				_memberMappers = new List<Tuple<LambdaExpression,LambdaExpression>>();

			_memberMappers.Add(Tuple.Create((LambdaExpression)toMember, (LambdaExpression)setter));

			return this;
		}

		/// <summary>
		/// If true, processes object cross references.
		/// </summary>
		/// <param name="doProcess">If true, processes object cross references.</param>
		/// <returns>Returns this mapper.</returns>
		public Mapper<TFrom,TTo> ProcessCrossReferences(bool? doProcess)
		{
			_processCrossReferences = doProcess;
			return this;
		}

		ExpressionMapper<TFrom,TTo> GetExpressionMapper()
			=> new ExpressionMapper<TFrom,TTo>
			{
				MappingSchema          = _mappingSchema ?? MappingSchema.Default,
				MemberMappers          = _memberMappers?.Select(mm => Tuple.Create(mm.Item1.GetMembersInfo(), mm.Item2)).ToArray(),
				MemberFilter           = _memberFilter,
				ProcessCrossReferences = _processCrossReferences,
				ToMapping              = _toMapping,
				FromMapping            = _fromMapping,
			};
	}
}
