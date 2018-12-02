using System;
using System.Reflection;
using System.Threading;

// ReSharper disable once CheckNamespace

namespace BenchmarkDotNet.Helpers
{
	/// <summary>
	/// Helper type for value provider attributes such as <see cref="BenchmarkDotNet.Configs.IConfigSource"/>.
	/// </summary>
	/// <typeparam name="T">Type of value. It's re Use interface if possible.</typeparam>
	public sealed class AttributeValue<T> where T : class
	{
		private readonly Lazy<T> _valueLazy;

		/// <summary>Initializes a new instance of the <see cref="AttributeValue{T}"/> class.</summary>
		/// <param name="valueType">Type of the value. Should have a public parameterless constructor.</param>
		/// <param name="argName">Name of the argument. Passed to ArgumentException if arg validation failed.</param>
		/// <exception cref="ArgumentNullException"><paramref name="valueType"/> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="valueType"/> does not implement <typeparamref name="T"/>.</exception>
		public AttributeValue(Type valueType, string argName)
		{
			if (valueType == null)
				throw new ArgumentNullException(argName);
			if (!typeof(T).GetTypeInfo().IsAssignableFrom(valueType))
				throw new ArgumentNullException($"The {argName} is not derived from {typeof(T)}.");

			_valueLazy = new Lazy<T>(
				() => (T)Activator.CreateInstance(valueType),
				LazyThreadSafetyMode.ExecutionAndPublication);
		}

		/// <summary>Initializes a new instance of the <see cref="AttributeValue{T}"/> class.</summary>
		/// <param name="valueFactory">The value factory.</param>
		/// <exception cref="ArgumentNullException"><paramref name="valueFactory"/> is <c>null</c>.</exception>
		public AttributeValue(Func<T> valueFactory)
		{
			if (valueFactory == null)
				throw new ArgumentNullException(nameof(valueFactory));

			_valueLazy = new Lazy<T>(
				valueFactory,
				LazyThreadSafetyMode.ExecutionAndPublication);
		}

		/// <summary>The value provided by the attribute.</summary>
		/// <value>The value provided by the attribute.</value>
		public T Value => _valueLazy.Value;
	}
}