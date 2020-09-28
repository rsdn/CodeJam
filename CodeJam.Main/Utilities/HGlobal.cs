using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// HGlobal wrapper.
	/// </summary>
	[PublicAPI]
	public static class HGlobal
	{
		/// <summary>
		/// Create a new HGlobal with given size.
		/// </summary>
		/// <param name="cb">The required number of bytes in memory.</param>
		/// <returns><see cref="HGlobalScope"/> instance</returns>
		[NotNull]
		public static HGlobalScope Create([NonNegativeValue] int cb) => new HGlobalScope(cb);

		/// <summary>
		/// Create a new HGlobal with sizeof(<typeparamref name="T"/>).
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <returns><see cref="HGlobalScope{T}"/> instance</returns>
		[NotNull]
		public static HGlobalScope<T> Create<T>() where T : struct => new HGlobalScope<T>();

		/// <summary>
		/// Create a new HGlobal with given size.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="cb">The required number of bytes in memory.</param>
		/// <returns><see cref="HGlobalScope{T}"/> instance</returns>
		[NotNull]
		public static HGlobalScope<T> Create<T>([NonNegativeValue] int cb) where T : struct => new HGlobalScope<T>(cb);
	}
}
