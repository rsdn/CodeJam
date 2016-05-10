using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CodeJam.Mapping
{
	using Metadata;

	class MappingSchemaInfo
	{
		public MappingSchemaInfo(string configuration)
		{
			Configuration = configuration;
		}

		public readonly string Configuration;
		public IMetadataReader MetadataReader;

		#region Default Values

		volatile ConcurrentDictionary<Type,object> _defaultValues;

		public Option<object> GetDefaultValue(Type type)
		{
			if (_defaultValues == null)
				return Option<object>.None;

			object o;
			return _defaultValues.TryGetValue(type, out o) ? Option.Create(o) : Option<object>.None;
		}

		public void SetDefaultValue(Type type, object value)
		{
			if (_defaultValues == null)
				lock (this)
					if (_defaultValues == null)
						_defaultValues = new ConcurrentDictionary<Type,object>();

			_defaultValues[type] = value;
		}

		#endregion

		#region GenericConvertProvider

		volatile ConcurrentDictionary<Type,List<Type[]>> _genericConvertProviders;

		public bool InitGenericConvertProvider(Type[] types, MappingSchema mappingSchema)
		{
			var changed = false;

			if (_genericConvertProviders != null)
			{
				lock (_genericConvertProviders)
				{
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
			}

			return changed;
		}

		public void SetGenericConvertProvider(Type type)
		{
			if (_genericConvertProviders == null)
				lock (this)
					if (_genericConvertProviders == null)
						_genericConvertProviders = new ConcurrentDictionary<Type,List<Type[]>>();

			if (!_genericConvertProviders.ContainsKey(type))
				_genericConvertProviders[type] = new List<Type[]>();
		}

		#endregion

		#region ConvertInfo

		ConvertInfo _convertInfo;

		public void SetConvertInfo(Type from, Type to, ConvertInfo.LambdaInfo expr)
		{
			if (_convertInfo == null)
				_convertInfo = new ConvertInfo();
			_convertInfo.Set(from, to, expr);
		}

		public ConvertInfo.LambdaInfo GetConvertInfo(Type from, Type to)
			=> _convertInfo?.Get(@from, to);

		private ConcurrentDictionary<object,Func<object,object>> _converters;
		public  ConcurrentDictionary<object,Func<object,object>>  Converters
			=> _converters ?? (_converters = new ConcurrentDictionary<object,Func<object,object>>());

		#endregion
	}
}
