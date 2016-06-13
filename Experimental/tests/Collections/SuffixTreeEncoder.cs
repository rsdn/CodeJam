using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CodeJam.Collections
{
	public abstract class SuffixTreeEncoder : SuffixTreeBase
	{
		private static readonly MethodInfo getRootMethod_;
		private static readonly MethodInfo getNodeMethod_;
		private static readonly MethodInfo getDataMethod_;
		private static readonly Func<SuffixTreeBase, Node> getRoot_;
		private static readonly Func<SuffixTreeBase, int, Node> getNode_;
		private static readonly Func<SuffixTreeBase, string> getData_;

		static SuffixTreeEncoder()
		{
			getRootMethod_ = typeof(SuffixTreeBase).GetProperty("Root", BindingFlags.Instance | BindingFlags.NonPublic).GetMethod;
			getRoot_ = tree => (Node)getRootMethod_.Invoke(tree, null);
			getNodeMethod_ = typeof(SuffixTreeBase).GetMethod("GetNode", BindingFlags.Instance | BindingFlags.NonPublic);
			getNode_ = (tree, index) => (Node)getNodeMethod_.Invoke(tree, new object[] { index });
			getDataMethod_ = typeof(SuffixTreeBase).GetProperty("InternalData", BindingFlags.Instance | BindingFlags.NonPublic).GetMethod;
			getData_ = tree => (string)getDataMethod_.Invoke(tree, null);
		}

		public static string Encode<T>(T tree) where T : SuffixTreeBase
		{
			var root = getRoot_(tree);
			var children = root.Children;
			var sb = new StringBuilder();
			sb.Append('[');
			if (children != null)
			{
				AppendChildren(sb, tree, children);
			}
			sb.Append(']');
			return sb.ToString();
		}

		private static void AppendChildren<T>(StringBuilder sb, T tree, IEnumerable<int> children) where T : SuffixTreeBase
		{
			var first = true;
			var data = getData_(tree);
			foreach (var v in children.Select(_ => getNode_(tree, _))
				.Select(_ => new { value = data.Substring(_.Begin, _.End - _.Begin), children = _.Children })
				.OrderBy(_ => _.value))
			{
				if (!first)
				{
					sb.Append(',');
				}
				sb.Append("{" + v.value + "}");
				if (v.children != null)
				{
					sb.Append('[');
					AppendChildren(sb, tree, v.children);
					sb.Append(']');
				}
				first = false;
			}
		}

		private SuffixTreeEncoder() { }
	}
}
