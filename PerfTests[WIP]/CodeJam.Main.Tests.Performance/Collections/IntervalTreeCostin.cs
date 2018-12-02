/////////////////////////////////////////////////////////////////////
// File Name               : IntervalTree.cs
//      Created            : 24 7 2012   23:20
//      Author             : Costin S
//
/////////////////////////////////////////////////////////////////////
#define TREE_WITH_PARENT_POINTERS
// BASEDOON: https://code.google.com/archive/p/intervaltree/
namespace IntervalTree
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;

	public interface IInterval<T> where T : IComparable<T>
	{
		#region Properties

		T Start { get; }
		T End { get; }

		#endregion

		#region Methods

		bool OverlapsWith(IInterval<T> other);

		#endregion
	}

	/// <summary>
	/// Interval structure
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct Interval<T> : IInterval<T> where T : IComparable<T>
	{
		#region C'tor

		public Interval(T start, T end)
			: this()
		{
			if (start.CompareTo(end) >= 0)
			{
				throw new ArgumentException("the start value of the interval must be smaller than the end value. null interval are not allowed");
			}

			this.Start = start;
			this.End = end;
		}

		#endregion

		#region IInterval<T> implementation

		public T Start { get; private set; }
		public T End { get; private set; }

		/// <summary>
		/// Determines if two intervals overlap (i.e. if this interval starts before the other ends and it finishes after the other starts)
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns>
		///   <c>true</c> if the specified other is overlapping; otherwise, <c>false</c>.
		/// </returns>
		public bool OverlapsWith(IInterval<T> other)
		{
			return (this.Start.CompareTo(other.End) < 0 && this.End.CompareTo(other.Start) > 0);
		}

		#endregion

		public override string ToString()
		{
			return "[" + Start.ToString() + "," + End.ToString() + "]";
		}
	}

	/// <summary>
	/// Interval Tree implementation
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class IntervalTreeCostin<T, TypeValue> where T : IComparable<T>
	{
		#region Properties

		private IntervalNode Root { get; set; }

		#endregion

		#region Ctor

		/// <summary>
		/// Initializes a new instance of the <see cref="IntervalTreeCostin&lt;T, TypeValue&gt;"/> class.
		/// </summary>
		public IntervalTreeCostin()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IntervalTreeCostin&lt;T, TypeValue&gt;"/> class.
		/// </summary>
		/// <param name="elems">The elems.</param>
		public IntervalTreeCostin(IEnumerable<KeyValuePair<IInterval<T>, TypeValue>> elems)
		{
			if (elems != null)
			{
				foreach (var elem in elems)
				{
					Add(elem);
				}
			}
		}

		#endregion

		#region Delegates

		/// <summary>
		/// visitor delegate
		/// </summary>
		/// <typeparam name="TNode">The type of the node.</typeparam>
		/// <param name="node">The node.</param>
		/// <param name="level">The level.</param>
		private delegate void VisitNodeHandler<TNode>(TNode node, int level);

		#endregion

		#region Nested Classes

		/// <summary>
		/// node class
		/// </summary>
		/// <typeparam name="TElem">The type of the elem.</typeparam>
		private class IntervalNode
		{
			#region Fields

			private T _MaxValue;

			#endregion

			#region Properties

			private int Balance { get; set; }
			private IntervalNode Left { get; set; }
			private IntervalNode Right { get; set; }

			public KeyValuePair<IInterval<T>, TypeValue> Data { get; private set; }
			private List<KeyValuePair<T, TypeValue>> Range { get; set; }

			/// <summary>
			/// Gets the max range. 
			/// </summary>
			private T MaxRange
			{
				get
				{
					return this.Data.Key.End;
				}
			}

			public T Max
			{
				get
				{
					return Max(_MaxValue, MaxRange);
				}
				set
				{
					_MaxValue = value;
				}
			}

#if TREE_WITH_PARENT_POINTERS
			public IntervalNode Parent { get; set; }
#endif

			#endregion

			#region Methods

			/// <summary>
			/// Adds the specified elem.
			/// </summary>
			/// <param name="elem">The elem.</param>
			/// <param name="data">The data.</param>
			/// <returns></returns>
			public static IntervalNode Add(
				IntervalNode elem,
				KeyValuePair<IInterval<T>, TypeValue> data,
				ref bool wasAdded,
				ref bool wasSuccessful)
			{
				if (elem == null)
				{
					elem = new IntervalNode { Data = data, Left = null, Right = null, Balance = 0, Max = data.Key.End };
					wasAdded = true;
					wasSuccessful = true;
				}
				else
				{
					if (data.Key.Start.CompareTo(elem.Data.Key.Start) < 0)
					{
						elem.Left = Add(elem.Left, data, ref wasAdded, ref wasSuccessful);
						if (wasAdded)
						{
							elem.Balance--;

							if (elem.Balance == 0)
							{
								wasAdded = false;
							}
						}

#if TREE_WITH_PARENT_POINTERS
						elem.Left.Parent = elem;
#endif

						if (elem.Balance == -2)
						{
							if (elem.Left.Balance == 1)
							{
								int elemLeftRightBalance = elem.Left.Right.Balance;

								elem.Left = elem.Left.RotateLeft();
								elem = elem.RotateRight();

								elem.Balance = 0;
								elem.Left.Balance = elemLeftRightBalance == 1 ? -1 : 0;
								elem.Right.Balance = elemLeftRightBalance == -1 ? 1 : 0;
							}

							else if (elem.Left.Balance == -1)
							{
								elem = elem.RotateRight();
								elem.Balance = 0;
								elem.Right.Balance = 0;
							}
							wasAdded = false;
						}
					}
					else if (data.Key.Start.CompareTo(elem.Data.Key.Start) > 0)
					{
						elem.Right = Add(elem.Right, data, ref wasAdded, ref wasSuccessful);
						if (wasAdded)
						{
							elem.Balance++;
							if (elem.Balance == 0)
							{
								wasAdded = false;
							}
						}

#if TREE_WITH_PARENT_POINTERS
						elem.Right.Parent = elem;
#endif

						if (elem.Balance == 2)
						{
							if (elem.Right.Balance == -1)
							{
								int elemRightLeftBalance = elem.Right.Left.Balance;

								elem.Right = elem.Right.RotateRight();
								elem = elem.RotateLeft();

								elem.Balance = 0;
								elem.Left.Balance = elemRightLeftBalance == 1 ? -1 : 0;
								elem.Right.Balance = elemRightLeftBalance == -1 ? 1 : 0;
							}

							else if (elem.Right.Balance == 1)
							{
								elem = elem.RotateLeft();

								elem.Balance = 0;
								elem.Left.Balance = 0;
							}
							wasAdded = false;
						}
					}

					else
					{
						// we allow multiple values per key
						if (elem.Range == null)
						{
							elem.Range = new List<KeyValuePair<T, TypeValue>>();
						}

						//always store the max Y value in the node.Data itself .. store the Range list in decreasing order
						if (data.Key.End.CompareTo(elem.Data.Key.End) > 0)
						{
							elem.Range.Insert(0, new KeyValuePair<T, TypeValue>(elem.Data.Key.End, elem.Data.Value));
							elem.Data = data;
						}
						else
						{
							for (int i = 0; i < elem.Range.Count; i++)
							{
								if (data.Key.End.CompareTo(elem.Range[i].Key) >= 0)
								{
									elem.Range.Insert(i, new KeyValuePair<T, TypeValue>(data.Key.End, data.Value));

									break;
								}
							}
						}

						wasSuccessful = true;
					}

				}
				ComputeMax(elem);
				return elem;
			}

			/// <summary>
			/// Searches the specified subtree.
			/// </summary>
			/// <param name="subtree">The subtree.</param>
			/// <param name="data">The data.</param>
			/// <returns></returns>
			public static IntervalNode Search(IntervalNode subtree, IInterval<T> data)
			{
				if (subtree != null)
				{
					if (data.Start.CompareTo(subtree.Data.Key.Start) < 0)
					{
						return Search(subtree.Left, data);
					}
					else if (data.Start.CompareTo(subtree.Data.Key.Start) > 0)
					{
						return Search(subtree.Right, data);
					}
					else
					{
						return subtree;
					}
				}
				else
					return null;
			}

			/// <summary>
			/// Computes the max.
			/// </summary>
			/// <param name="node">The node.</param>
			public static void ComputeMax(IntervalNode node)
			{
				if (node.Left == null && node.Right == null)
				{
					node.Max = node.MaxRange;
				}
				else if (node.Left == null)
				{
					node.Max = Max(node.MaxRange, node.Right.Max);
				}
				else if (node.Right == null)
				{
					node.Max = Max(node.MaxRange, node.Left.Max);
				}
				else
				{
					node.Max = Max(node.MaxRange, Max(node.Left.Max, node.Right.Max));
				}
			}

			/// <summary>
			/// Rotates lefts this instance.
			/// Assumes that this.Right != null
			/// </summary>
			/// <returns></returns>
			private IntervalNode RotateLeft()
			{
				var right = this.Right;
				Debug.Assert(this.Right != null);

				this.Right = right.Left;
				ComputeMax(this);

#if TREE_WITH_PARENT_POINTERS
				var parent = this.Parent;
				if (right.Left != null)
				{
					right.Left.Parent = this;
				}
#endif
				right.Left = this;
				ComputeMax(right);

#if TREE_WITH_PARENT_POINTERS
				this.Parent = right;
				if (parent != null)
				{
					if (parent.Left == this)
						parent.Left = right;
					else
						parent.Right = right;

				}
				right.Parent = parent;
#endif

				return right;

			}

			/// <summary>
			/// Rotates right this instance.
			/// Assumes that (this.Left != null)
			/// </summary>
			/// <returns></returns>
			private IntervalNode RotateRight()
			{
				var left = this.Left;
				Debug.Assert(this.Left != null);

				this.Left = left.Right;
				ComputeMax(this);

#if TREE_WITH_PARENT_POINTERS
				var parent = this.Parent;
				if (left.Right != null)
				{
					left.Right.Parent = this;
				}
#endif
				left.Right = this;
				ComputeMax(left);

#if TREE_WITH_PARENT_POINTERS
				this.Parent = left;
				if (parent != null)
				{
					if (parent.Left == this)
						parent.Left = left;
					else
						parent.Right = left;

				}
				left.Parent = parent;
#endif
				return left;
			}

			/// <summary>
			/// Finds the min.
			/// </summary>
			/// <param name="node">The node.</param>
			/// <returns></returns>
			public static IntervalNode FindMin(IntervalNode node)
			{
				while (node != null && node.Left != null)
				{
					node = node.Left;
				}
				return node;
			}

			/// <summary>
			/// Finds the max.
			/// </summary>
			/// <param name="node">The node.</param>
			/// <returns></returns>
			public static IntervalNode FindMax(IntervalNode node)
			{
				while (node != null && node.Right != null)
				{
					node = node.Right;
				}
				return node;
			}

			public IEnumerable<KeyValuePair<IInterval<T>, TypeValue>> GetRange()
			{
				if (this.Range != null)
				{
					foreach (var value in this.Range)
					{
						var kInterval = new Interval<T>(this.Data.Key.Start, value.Key);
						yield return new KeyValuePair<IInterval<T>, TypeValue>(kInterval, value.Value);
					}
				}
				else
				{
					yield break;
				}
			}

#if TREE_WITH_PARENT_POINTERS

			/// <summary>
			/// Succeeds this instance.
			/// </summary>
			/// <returns></returns>
			public IntervalNode Successor()
			{
				if (this.Right != null)
					return FindMin(this.Right);
				else
				{
					var p = this;
					while (p.Parent != null && p.Parent.Right == p)
						p = p.Parent;
					return p.Parent;
				}
			}

			/// <summary>
			/// Precedes this instance.
			/// </summary>
			/// <returns></returns>
			public IntervalNode Predecesor()
			{
				if (this.Left != null)
					return FindMax(this.Left);
				else
				{
					var p = this;
					while (p.Parent != null && p.Parent.Left == p)
						p = p.Parent;
					return p.Parent;
				}
			}
#endif

			/// <summary>
			/// Deletes the specified node.
			/// </summary>
			/// <param name="node">The node.</param>
			/// <param name="arg">The arg.</param>
			/// <returns></returns>
			public static IntervalNode Delete(IntervalNode node, IInterval<T> arg, ref bool wasDeleted, ref bool wasSuccessful)
			{
				int cmp = arg.Start.CompareTo(node.Data.Key.Start);
				if (cmp < 0)
				{
					if (node.Left != null)
					{
						node.Left = Delete(node.Left, arg, ref wasDeleted, ref wasSuccessful);

						if (wasDeleted)
						{
							node.Balance++;
						}
					}
				}
				else if (cmp == 0)
				{
					int position = -1;

					// find the exact interval to delete based on the Y value.. consider changing this code
					if (arg.End.CompareTo(node.Data.Key.End) == 0)
					{
						position = 0;
					}
					else
					{
						if (node.Range != null && node.Range.Count > 0)
						{
							for (int k = 0; k < node.Range.Count; k++)
							{
								if (arg.End.CompareTo(node.Range[k].Key) == 0)
								{
									position = k + 1;
								}
							}
						}
					}

					// couldn't find the interval in the tree, throw an exception
					if (position == -1)
					{
						throw new ArgumentOutOfRangeException("arg", "cannot delete the specified interval. invalid argument.");
					}

					if (position > 0)
					{
						// we're counting the value stored in the node.Data.Value as position 0, all values stored in Range represent position + 1, position + 2, ...etc
						if (node.Range != null && position - 1 < node.Range.Count)
						{
							node.Range.RemoveAt(position - 1);

							if (node.Range.Count == 0)
							{
								node.Range = null;
							}

							wasSuccessful = true;
						}
					}
					else if (position == 0 && node.Range != null && node.Range.Count > 0)
					{
						node.Data = new KeyValuePair<IInterval<T>, TypeValue>(
																		new Interval<T>(node.Data.Key.Start, node.Range[0].Key),
																		node.Range[0].Value);
						node.Range.RemoveAt(0);
						if (node.Range.Count == 0)
						{
							node.Range = null;
						}

						wasSuccessful = true;
					}
					else
					{
						if (node.Left != null && node.Right != null)
						{
							var min = FindMin(node.Right);

							var data = node.Data;
							var range = node.Range;

							node.Data = min.Data;
							node.Range = min.Range;

							min.Data = data;
							min.Range = range;

							wasDeleted = false;
							node.Right = Delete(node.Right, data.Key, ref wasDeleted, ref wasSuccessful);

							if (wasDeleted)
							{
								node.Balance--;
							}
						}
						else if (node.Left == null)
						{
							wasDeleted = true;
							wasSuccessful = true;
							return node.Right;
						}
						else
						{
							wasDeleted = true;
							wasSuccessful = true;
							return node.Left;
						}
					}
				}
				else
				{
					if (node.Right != null)
					{
						node.Right = Delete(node.Right, arg, ref wasDeleted, ref wasSuccessful);
						if (wasDeleted)
						{
							node.Balance--;
						}
					}
				}
				ComputeMax(node);

				if (wasDeleted)
				{
					if (node.Balance == 1 || node.Balance == -1)
					{
						wasDeleted = false;
						return node;
					}

					else if (node.Balance == -2)
					{
						if (node.Left.Balance == 1)
						{
							int leftRightBalance = node.Left.Right.Balance;

							node.Left = node.Left.RotateLeft();
							node = node.RotateRight();

							node.Balance = 0;
							node.Left.Balance = (leftRightBalance == 1) ? -1 : 0;
							node.Right.Balance = (leftRightBalance == -1) ? 1 : 0;
						}
						else if (node.Left.Balance == -1)
						{
							node = node.RotateRight();
							node.Balance = 0;
							node.Right.Balance = 0;
						}
						else if (node.Left.Balance == 0)
						{
							node = node.RotateRight();
							node.Balance = 1;
							node.Right.Balance = -1;

							wasDeleted = false;
						}
					}

					else if (node.Balance == 2)
					{
						if (node.Right.Balance == -1)
						{
							int rightLeftBalance = node.Right.Left.Balance;

							node.Right = node.Right.RotateRight();
							node = node.RotateLeft();

							node.Balance = 0;
							node.Left.Balance = (rightLeftBalance == 1) ? -1 : 0;
							node.Right.Balance = (rightLeftBalance == -1) ? 1 : 0;
						}
						else if (node.Right.Balance == 1)
						{
							node = node.RotateLeft();
							node.Balance = 0;
							node.Left.Balance = 0;
						}
						else if (node.Right.Balance == 0)
						{
							node = node.RotateLeft();
							node.Balance = -1;
							node.Left.Balance = 1;

							wasDeleted = false;
						}
					}
				}
				return node;
			}

			/// <summary>
			/// Returns all intervals beginning at the specified start value
			/// </summary>
			/// <param name="subtree">The subtree.</param>
			/// <param name="data">The data.</param>
			/// <returns></returns>
			public static List<KeyValuePair<IInterval<T>, TypeValue>> GetIntervalsStartingAt(IntervalNode subtree, T start)
			{
				if (subtree != null)
				{
					if (start.CompareTo(subtree.Data.Key.Start) < 0)
					{
						return GetIntervalsStartingAt(subtree.Left, start);
					}
					else if (start.CompareTo(subtree.Data.Key.Start) > 0)
					{
						return GetIntervalsStartingAt(subtree.Right, start);
					}
					else
					{
						var result = new List<KeyValuePair<IInterval<T>, TypeValue>>();
						result.Add(subtree.Data);

						if (subtree.Range != null)
						{
							foreach (var value in subtree.Range)
							{
								var kInterval = new Interval<T>(start, value.Key);
								result.Add(new KeyValuePair<IInterval<T>, TypeValue>(kInterval, value.Value));
							}
						}

						return result;
					}
				}
				else
				{
					return null;
				}
			}

			/// <summary>
			/// Searches for all intervals in this subtree that are overlapping the argument interval.
			/// </summary>
			/// <param name="toFind">To find.</param>
			/// <param name="list">The list.</param>
			public void GetIntervalsOverlappingWith(IInterval<T> toFind, ref List<KeyValuePair<IInterval<T>, TypeValue>> list)
			{
				if (toFind.End.CompareTo(this.Data.Key.Start) <= 0)
				{
					////toFind ends before subtree.Data begins, prune the right subtree
					if (this.Left != null)
					{
						this.Left.GetIntervalsOverlappingWith(toFind, ref list);
					}
				}
				else if (toFind.Start.CompareTo(this.Max) >= 0)
				{
					////toFind begins after the subtree.Max ends, prune the left subtree
					if (this.Right != null)
					{
						this.Right.GetIntervalsOverlappingWith(toFind, ref list);
					}
				}
				else
				{
					//// search the left subtree
					if (this.Left != null)
					{
						this.Left.GetIntervalsOverlappingWith(toFind, ref list);
					}

					if (this.Data.Key.OverlapsWith(toFind))
					{
						if (list == null)
						{
							list = new List<KeyValuePair<IInterval<T>, TypeValue>>();
						}
						list.Add(this.Data);
					}

					if (this.Range != null && this.Range.Count > 0)
					{
						for (int k = 0; k < this.Range.Count; k++)
						{
							var kInterval = new Interval<T>(this.Data.Key.Start, this.Range[k].Key);
							if (kInterval.OverlapsWith(toFind))
							{
								if (list == null)
								{
									list = new List<KeyValuePair<IInterval<T>, TypeValue>>();
								}
								list.Add(new KeyValuePair<IInterval<T>, TypeValue>(kInterval, this.Range[k].Value));
							}
						}
					}

					//// search the right subtree
					if (this.Right != null)
					{
						this.Right.GetIntervalsOverlappingWith(toFind, ref list);
					}
				}
			}

			/// <summary>
			/// Gets all intervals in this subtree that are overlapping the argument interval.
			/// </summary>
			/// <param name="toFind">To find.</param>
			/// <returns></returns>
			public IEnumerable<KeyValuePair<IInterval<T>, TypeValue>> GetIntervalsOverlappingWith(IInterval<T> toFind)
			{
				if (toFind.End.CompareTo(this.Data.Key.Start) <= 0)
				{
					//toFind ends before subtree.Data begins, prune the right subtree
					if (this.Left != null)
					{
						foreach (var value in this.Left.GetIntervalsOverlappingWith(toFind))
						{
							yield return value;
						}
					}
				}
				else if (toFind.Start.CompareTo(this.Max) >= 0)
				{
					//toFind begins after the subtree.Max ends, prune the left subtree
					if (this.Right != null)
					{
						foreach (var value in this.Right.GetIntervalsOverlappingWith(toFind))
						{
							yield return value;
						}
					}
				}
				else
				{
					if (this.Left != null)
					{
						foreach (var value in this.Left.GetIntervalsOverlappingWith(toFind))
						{
							yield return value;
						}
					}

					if (this.Data.Key.OverlapsWith(toFind))
					{
						yield return this.Data;
					}

					if (this.Range != null && this.Range.Count > 0)
					{
						foreach (var value in this.Range)
						{
							var kInterval = new Interval<T>(this.Data.Key.Start, value.Key);

							if (kInterval.OverlapsWith(toFind))
							{
								yield return new KeyValuePair<IInterval<T>, TypeValue>(kInterval, value.Value);
							}
						}
					}


					if (this.Right != null)
					{
						foreach (var value in this.Right.GetIntervalsOverlappingWith(toFind))
						{
							yield return value;
						}
					}
				}
			}

			public void Visit(VisitNodeHandler<IntervalNode> visitor, int level)
			{
				if (this.Left != null)
				{
					this.Left.Visit(visitor, level + 1);
				}

				visitor(this, level);

				if (this.Right != null)
				{
					this.Right.Visit(visitor, level + 1);
				}
			}

			#endregion
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds the specified interval.
		/// </summary>
		/// <param name="arg">The arg.</param>
		public void Add(T x, T y, TypeValue value)
		{
			Add(new KeyValuePair<IInterval<T>, TypeValue>(new Interval<T>(x, y), value));
		}

		/// <summary>
		/// Adds the specified arg.
		/// </summary>
		/// <param name="arg">The arg.</param>
		public void Add(Interval<T> arg, TypeValue value)
		{
			Add(new KeyValuePair<IInterval<T>, TypeValue>(arg, value));
		}

		/// <summary>
		/// Adds the specified arg.
		/// </summary>
		/// <param name="arg">The arg.</param>
		public void Add(KeyValuePair<IInterval<T>, TypeValue> arg)
		{
			bool wasAdded = false;
			bool wasSuccessful = false;
			this.Root = IntervalNode.Add(this.Root, arg, ref wasAdded, ref wasSuccessful);
			IntervalNode.ComputeMax(this.Root);
		}

		/// <summary>
		/// Deletes the interval starting at x.
		/// </summary>
		/// <param name="arg">The arg.</param>
		public void Delete(IInterval<T> arg)
		{
			if (Root != null)
			{
				bool wasDeleted = false;
				bool wasSuccessful = false;

				Root = IntervalNode.Delete(Root, arg, ref wasDeleted, ref wasSuccessful);
				if (this.Root != null)
				{
					IntervalNode.ComputeMax(this.Root);
				}
			}
		}

		/// <summary>
		/// Searches for all intervals overlapping the one specified as an argument.
		/// </summary>
		/// <param name="toFind">To find.</param>
		/// <param name="list">The list.</param>
		public void GetIntervalsOverlappingWith(IInterval<T> toFind, ref List<KeyValuePair<IInterval<T>, TypeValue>> list)
		{
			if (this.Root != null)
			{
				this.Root.GetIntervalsOverlappingWith(toFind, ref list);
			}
		}

		/// <summary>
		/// Searches for all intervals overlapping the one specified as an argument.
		/// </summary>
		/// <param name="toFind">To find.</param>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<IInterval<T>, TypeValue>> GetIntervalsOverlappingWith(IInterval<T> toFind)
		{
			return (Root != null) ? Root.GetIntervalsOverlappingWith(toFind) : null;
		}

		/// <summary>
		/// Searches the specified arg.
		/// </summary>
		/// <param name="arg">The arg.</param>
		/// <returns></returns>
		public List<KeyValuePair<IInterval<T>, TypeValue>> GetIntervalsStartingAt(T arg)
		{
			return IntervalNode.GetIntervalsStartingAt(Root, arg);
		}

#if TREE_WITH_PARENT_POINTERS
		/// <summary>
		/// Gets the collection of keys (ascending order)
		/// </summary>
		public IEnumerable<IInterval<T>> Keys
		{
			get
			{
				if (this.Root == null)
					yield break;

				var p = IntervalNode.FindMin(this.Root);
				while (p != null)
				{
					yield return p.Data.Key;

					foreach (var rangeNode in p.GetRange())
					{
						yield return rangeNode.Key;
					}

					p = p.Successor();
				}
			}
		}

		/// <summary>
		/// Gets the collection of values (ascending order)
		/// </summary>
		public IEnumerable<TypeValue> Values
		{
			get
			{
				if (this.Root == null)
					yield break;

				var p = IntervalNode.FindMin(this.Root);
				while (p != null)
				{
					yield return p.Data.Value;

					foreach (var rangeNode in p.GetRange())
					{
						yield return rangeNode.Value;
					}

					p = p.Successor();
				}
			}
		}

#endif

		/// <summary>
		/// Clears this instance.
		/// </summary>
		public void Clear()
		{
			this.Root = null;
		}

		public void Print()
		{
			this.Visit((node, level) =>
			{
				Console.Write(new string(' ', 2 * level));
				Console.WriteLine("{0}", "[" + node.Data.Key.Start.ToString() + "," + node.Data.Key.End.ToString() + "]");
			});
		}

		/// <summary>
		/// Visit_inorders the specified visitor. Defined for debugging purposes only
		/// </summary>
		/// <param name="visitor">The visitor.</param>
		private void Visit(VisitNodeHandler<IntervalNode> visitor)
		{
			if (Root != null)
			{
				Root.Visit(visitor, 0);
			}
		}

		internal static T Max(T first, T second)
		{
			return first.CompareTo(second) >= 0 ? first : second;
		}

		#endregion
	}
}
