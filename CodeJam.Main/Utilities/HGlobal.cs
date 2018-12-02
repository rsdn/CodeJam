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
		public static HGlobalScope Create(int cb) => new HGlobalScope(cb);

		/// <summary>
		/// Create a new HGlobal with sizeof(<typeparam name="T"/>).
		/// </summary>
		/// <returns><see cref="HGlobalScope{T}"/> instance</returns>
		public static HGlobalScope<T> Create<T>() where T : struct => new HGlobalScope<T>();

		/// <summary>
		/// Create a new HGlobal with given size.
		/// </summary>
		/// <param name="cb">The required number of bytes in memory.</param>
		/// <returns><see cref="HGlobalScope{T}"/> instance</returns>
		public static HGlobalScope<T> Create<T>(int cb) where T : struct => new HGlobalScope<T>(cb);
	}
}
