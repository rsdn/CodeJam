using JetBrains.Annotations;

namespace CodeJam.CmdLine
{
	/// <summary>
	/// Node for command line option.
	/// </summary>
	[PublicAPI]
	public class OptionNode : CmdLineNodeBase
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		private OptionNode(string text, int position, int length, OptionType type)
			: base(text, position, length)
		{
			Type = type;
		}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public OptionNode(
			string text,
			int position,
			int length)
			: this(text, position, length, OptionType.Valueless)
		{}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public OptionNode(
			string text,
			int position,
			int length,
			bool boolValue)
			: this(text, position, length, OptionType.Bool)
		{
			BoolValue = boolValue;
		}

		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public OptionNode(
			string text,
			int position,
			int length,
			QuotedOrNonquotedValueNode value)
			: this(text, position, length, OptionType.Value)
		{
			Value = value;
		}

		/// <summary>
		/// Type of node.
		/// </summary>
		public OptionType Type { get; }

		/// <summary>
		/// Boolean value.
		/// </summary>
		public bool BoolValue { get; }

		/// <summary>
		/// Option value.
		/// </summary>
		public QuotedOrNonquotedValueNode Value { get; }
	}
}