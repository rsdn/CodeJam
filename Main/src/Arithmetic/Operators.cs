using System;
using System.Threading;

using JetBrains.Annotations;

namespace CodeJam.Arithmetic
{
	/// <summary>
	/// Callbacks for common arithmetic actions.
	/// Look at OperatorsPerformanceTest to see why.
	/// </summary>
	/// <typeparam name="T">The type of the operands.</typeparam>
	// IMPORTANT: DO NOT declare static .ctor on the type. The class should be marked as beforefieldinit. 
	[PublicAPI]
	public static partial class Operators<T>
	{
		private const LazyThreadSafetyMode LazyMode = LazyThreadSafetyMode.PublicationOnly;

		private static readonly Lazy<Func<T, T, int>> _compare =
			new Lazy<Func<T, T, int>>(OperatorsFactory.Comparison<T>, LazyMode);

		/// <summary>
		/// Comparison callback
		/// </summary>
		[NotNull]
		public static Func<T, T, int> Compare => _compare.Value;
	}
}