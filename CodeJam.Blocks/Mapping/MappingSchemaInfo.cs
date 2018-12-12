#if !LESSTHAN_NET40
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using CodeJam.Collections;

using JetBrains.Annotations;

namespace CodeJam.Mapping
{
	using Metadata;

	internal class MappingSchemaInfo
	{
		public MappingSchemaInfo(string configuration) => Configuration = configuration;

		public readonly string Configuration;
		public IMetadataReader MetadataReader;

		#region Default Values
		private volatile ConcurrentDictionary<Type,object> _defaultValues;

		[NotNull]
		public Option<object> GetDefaultValue([NotNull] Type type)
		{
			if (_defaultValues == null)
				return Option.None<object>();

			return _defaultValues.TryGetValue(type, out var o) ? Option.Some(o) : Option.None<object>();
		}

		public void SetDefaultValue([NotNull] Type type, object value)
		{
			if (_defaultValues == null)
				lock (this)
					if (_defaultValues == null)
						_defaultValues = new ConcurrentDictionary<Type,object>();

			_defaultValues[type] = value;
		}

		#endregion

		#region GenericConvertProvider
		private volatile ConcurrentDictionary<Type,List<Type[]>> _genericConvertProviders;

		public bool InitGenericConvertProvider([NotNull, ItemNotNull] Type[] types)
		{
			var changed = false;

			if (_genericConvertProviders != null)
			{
				lock (_genericConvertProviders)
					foreach (var type in _genericConvertProviders)
					{
						var args = type.Key.GetGenericArguments();

						if (args.Length == types.Length)
						{
							if (type.Value.Aggregate(false, (cur,ts) => cur || ts.SequenceEqual(types)))
								continue;

							var gtype    = type.Key.MakeGenericType(types);
							var provider = (IGenericInfoProvider)Activator.CreateInstance(gtype);

							provider.SetInfo(new MappingSchema(this));

							type.Value.Add(types);

							changed = true;
						}
					}
			}

			return changed;
		}

		public void SetGenericConvertProvider([NotNull] Type type)
		{
			if (_genericConvertProviders == null)
				lock (this)
					if (_genericConvertProviders == null)
						_genericConvertProviders = new ConcurrentDictionary<Type,List<Type[]>>();

			// ReSharper disable once InconsistentlySynchronizedField
			_genericConvertProviders.AddOrUpdate(type, t => new List<Type[]>());
		}
		#endregion

		#region ConvertInfo
		private ConvertInfo _convertInfo;

		public void SetConvertInfo([NotNull] Type from, [NotNull] Type to, [NotNull] ConvertInfo.LambdaInfo expr)
		{
			if (_convertInfo == null)
				_convertInfo = new ConvertInfo();
			_convertInfo.Set(from, to, expr);
		}

		[CanBeNull]
		public ConvertInfo.LambdaInfo GetConvertInfo([NotNull] Type from, [NotNull] Type to)
			=> _convertInfo?.Get(from, to);

		private ConcurrentDictionary<object,Func<object,object>> _converters;

		[NotNull]
		public ConcurrentDictionary<object,Func<object,object>> Converters
			=> _converters ?? (_converters = new ConcurrentDictionary<object,Func<object,object>>());

		#endregion

		#region Scalar Types
		private volatile ConcurrentDictionary<Type,bool> _scalarTypes;

		[NotNull]
		public Option<bool> GetScalarType([NotNull] Type type)
		{
			if (_scalarTypes != null)
			{
				if (_scalarTypes.TryGetValue(type, out var isScalarType))
					return Option.Some(isScalarType);
			}

			return Option.None<bool>();
		}

		public void SetScalarType([NotNull] Type type, bool isScalarType = true)
		{
			if (_scalarTypes == null)
				lock (this)
					if (_scalarTypes == null)
						_scalarTypes = new ConcurrentDictionary<Type,bool>();

			_scalarTypes[type] = isScalarType;
		}

		#endregion
	}
}
#endif