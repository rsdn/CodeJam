namespace CodeJam
{
	/// <summary>
	/// Builder type for argument assertions.
	/// </summary>
	public struct ArgumentAssertion<T>
	{
		/// <summary>
		/// Initialize instance.
		/// </summary>
		/// <param name="arg">Argument value.</param>
		/// <param name="argName">Argument name.</param>
		public ArgumentAssertion(T arg, string argName)
		{
			Argument = arg;
			ArgumentName = argName;
		}

		/// <summary>
		/// Argument value.
		/// </summary>
		public T Argument { get; }

		/// <summary>
		/// Argument name.
		/// </summary>
		public string ArgumentName { get; }
	}
}