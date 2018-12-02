using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CodeJam.Collections
{
	public abstract class SuffixTreeEncoder : SuffixTreeBase
	{
		private static readonly MethodInfo _getRootMethod;
		private static readonly MethodInfo _getNodeMethod;
		private static readonly MethodInfo _getDataMethod;
		private static readonly Func<SuffixTreeBase, Node> _getRoot;
		private static readonly Func<SuffixTreeBase, int, Node> _getNode;
		private static readonly Func<SuffixTreeBase, string> _getData;

		static SuffixTreeEncoder()
		{
			var typeInfo = typeof(SuffixTreeBase);
			_getRootMethod = typeInfo.GetProperty("Root", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true);
			_getRoot = tree => (Node)_getRootMethod.Invoke(tree, null);
			_getNodeMethod = typeInfo.GetMethod("GetNode", BindingFlags.Instance | BindingFlags.NonPublic);
			_getNode = (tree, index) => (Node)_getNodeMethod.Invoke(tree, new object[] { index });
			_getDataMethod = typeInfo.GetProperty("InternalData", BindingFlags.Instance | BindingFlags.NonPublic).GetGetMethod(true);
			_getData = tree => (string)_getDataMethod.Invoke(tree, null);
		}

		public static string Encode<T>(T tree) where T : SuffixTreeBase
		{
			var root = _getRoot(tree);
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
			var data = _getData(tree);
			foreach (var v in children.Select(_ => _getNode(tree, _))
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
