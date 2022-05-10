using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;

using CodeJam.Reflection;

namespace CodeJam.ConnectionStrings
{
	[TypeConverter(typeof(ConnectionStringBaseTypeConverter))]
	public abstract partial class ConnectionStringBase
	{
		/// <inheritdoc />
		public class ConnectionStringBaseTypeConverter : TypeConverter
		{
			private static readonly ConcurrentDictionary<Type, Func<string, object>> _creators = new();

			private readonly Type _callingType;

			public ConnectionStringBaseTypeConverter(Type callingType)
			{
				Code.AssertArgument(
					callingType.IsAssignableTo<ConnectionStringBase>(), nameof(callingType),
					$"To be used with this converter, the type must be inherited from {nameof(ConnectionStringBase)}");
				_callingType = callingType;
			}

			private Func<string, object> GetCreator() => _creators.GetOrAdd(
				_callingType, ct =>
				{
					var ctor = ct.GetConstructor(new[] { typeof(string) });
					Code.AssertState(ctor != null, $"The type {ct} must have a constructor with a single argument of type string");
					var srcPrm = Expression.Parameter(typeof(string), "source");
					var lambda = Expression.Lambda<Func<string, object>>(Expression.New(ctor, srcPrm), srcPrm);
					return lambda.Compile();
				});

			#region Overrides of TypeConverter
			/// <inheritdoc />
			public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
				=> base.CanConvertFrom(context, sourceType) || sourceType == typeof(string);

			/// <inheritdoc />
			public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
				=> value is string str ? GetCreator()(str) : base.ConvertFrom(context, culture, value);
			#endregion
		}
	}
}