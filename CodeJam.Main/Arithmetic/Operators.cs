using System;
using System.Linq.Expressions;
using System.Threading;

using JetBrains.Annotations;

using SuppressMessageAttribute = System.Diagnostics.CodeAnalysis.SuppressMessageAttribute;

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
		private const LazyThreadSafetyMode _lazyMode = LazyThreadSafetyMode.PublicationOnly;

		#region Compare
		/// <summary>
		/// Gets a comparison function.
		/// </summary>
		[NotNull]
		public static Func<T?, T?, int> Compare => CompareHelper.Value;

		/// <summary>
		/// The helper class.
		/// </summary>
		private static class CompareHelper
		{
			[NotNull, ItemNotNull]
			private static readonly Lazy<Func<T?, T?, int>> _value = new Lazy<Func<T?, T?, int>>(OperatorsFactory.Comparison<T>, _lazyMode);

			/// <summary>
			/// Gets a comparison function.
			/// </summary>
			[NotNull]
			public static Func<T?, T?, int> Value => _value.Value;
		}
		#endregion

		#region HasNaN
		/// <summary>
		/// Determines whether the type has NaN value.
		/// </summary>
		public static bool HasNaN => NaNHelper.HasNaN;

		/// <summary>
		/// Gets a value that is not a number (NaN).
		/// </summary>
		public static T NaN => NaNHelper.NaN;

		/// <summary>
		/// The helper class.
		/// </summary>
		[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
		private static class NaNHelper
		{
			[NotNull, ItemNotNull]
			private static readonly Lazy<T> _value = new Lazy<T>(OperatorsFactory.GetNaN<T>, _lazyMode);

			/// <summary>
			/// Determines whether the type has NaN value.
			/// </summary>
			public static bool HasNaN { get; } = OperatorsFactory.HasNaN<T>();

			/// <summary>
			/// Gets a value that is not a number (NaN).
			/// </summary>
			[NotNull]
			public static T NaN => _value.Value;
		}
		#endregion

		#region NegativeInfinity
		/// <summary>
		/// Gets a value that determines whether the type has negative infinity value.
		/// </summary>
		public static bool HasNegativeInfinity => NegativeInfinityHelper.HasNegativeInfinity;

		/// <summary>
		/// Gets negative infinity.
		/// </summary>
		public static T NegativeInfinity => NegativeInfinityHelper.NegativeInfinity;

		/// <summary>
		/// The helper class.
		/// </summary>
		[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
		private static class NegativeInfinityHelper
		{
			[NotNull, ItemNotNull]
			private static readonly Lazy<T> _value = new Lazy<T>(OperatorsFactory.GetNegativeInfinity<T>, _lazyMode);

			/// <summary>
			/// Gets a value that determines whether the type has negative infinity value.
			/// </summary>
			public static bool HasNegativeInfinity { get; } = OperatorsFactory.HasNegativeInfinity<T>();

			/// <summary>
			/// Gets negative infinity.
			/// </summary>
			[NotNull] public static T NegativeInfinity => _value.Value;
		}
		#endregion

		#region PositiveInfinity
		/// <summary>
		/// Gets a value that determines whether the type has positive infinity value.
		/// </summary>
		public static bool HasPositiveInfinity => PositiveInfinityHelper.HasPositiveInfinity;

		/// <summary>
		/// Gets positive infinity.
		/// </summary>
		public static T PositiveInfinity => PositiveInfinityHelper.PositiveInfinity;

		/// <summary>
		/// The helper class.
		/// </summary>
		[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
		private static class PositiveInfinityHelper
		{
			[NotNull, ItemNotNull]
			private static readonly Lazy<T> _value = new Lazy<T>(OperatorsFactory.GetPositiveInfinity<T>, _lazyMode);

			/// <summary>
			/// Gets a value that determines whether the type has positive infinity value.
			/// </summary>
			public static readonly bool HasPositiveInfinity = OperatorsFactory.HasPositiveInfinity<T>();

			/// <summary>
			/// Gets positive infinity.
			/// </summary>
			[NotNull] public static T PositiveInfinity => _value.Value;
		}
		#endregion

		#region Custom impl for _onesComplement (FW 3.5 targeting)
		/// <summary>Gets a ones complement operation function, such as (~a) in C#.</summary>
		[NotNull]
		public static Func<T?, T?> OnesComplement => OnesComplementHelper.LazyValue.Value;

		/// <summary>
		/// The helper class.
		/// </summary>
		private static class OnesComplementHelper
		{
			/// <summary>
			/// The operator factory.
			/// </summary>
			[NotNull, ItemNotNull]
			public static readonly Lazy<Func<T?, T?>> LazyValue = new Lazy<Func<T?, T?>>(CreateValue, _lazyMode);

			/// <summary>
			/// Returns the operator function.
			/// </summary>
			/// <returns>
			/// The operator function.
			/// </returns>
			[NotNull]
			private static Func<T?, T?> CreateValue()
			{
#if NET40_OR_GREATER || TARGETS_NETSTANDARD || TARGETS_NETCOREAPP
				return OperatorsFactory.UnaryOperator<T>(ExpressionType.OnesComplement);
#else
				return OperatorsFactory.UnaryOperator<T>(ExpressionType.Not);
#endif
			}
		}
		#endregion
	}
}
