using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	///<summary>
	/// Base class for command line AST node
	///</summary>
	[PublicAPI]
	public abstract class CmdLineNodeBase
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		protected CmdLineNodeBase([NotNull] string text, [NonNegativeValue] int position, [NonNegativeValue] int length)
		{
			Text = text;
			Position = position;
			Length = length;
		}

		/// <summary>
		/// Node text.
		/// </summary>
		[NotNull]
		public string Text { get; }

		/// <summary>
		/// Node position.
		/// </summary>
		public int Position { get; }

		/// <summary>
		/// Node length.
		/// </summary>
		public int Length { get; }
	}
}