using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// Common interface for all tuples.
	/// </summary>
	[PublicAPI]
	public interface IValueTuple
	{
		/// <summary>
		/// Tuple rank (count of values).
		/// </summary>
		int Rank { get; }

		/// <summary>
		/// Returns value by it index.
		/// </summary>
		/// <param name="index">Index of value, zero based.</param>
		object this[int index] { get; }
	}
}