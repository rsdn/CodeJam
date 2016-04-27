using System;

using BenchmarkDotNet.Attributes;

using JetBrains.Annotations;

namespace CodeJam.Arithmetic
{
	/// <summary>
	/// Base class for all operator test cases;
	/// </summary>
	[PublicAPI]
	public abstract class OperatorsBaseCase<T>
	{
		/// <summary> Count of items </summary>
		protected int Count { get; set; } = 1000 * 1000;
		/// <summary> Repeat value A each </summary>
		protected int ValueARepeats { get; set; } = 5;
		/// <summary> Start offset for A index </summary>
		protected int ValueAOffset { get; set; }
		/// <summary> Repeat value B each </summary>
		protected int ValueBRepeats { get; set; } = int.MaxValue;
		/// <summary> Start offset for B index </summary>
		protected int ValueBOffset { get; set; } = 1;

		protected T[] ValuesA;
		protected T[] ValuesB;

		/// <summary> Get value A from index </summary>
		protected abstract T GetValueA(int i);

		/// <summary> Get value B from index </summary>
		protected abstract T GetValueB(int i);

		/// <summary>
		/// Called by unit test runner
		/// </summary>
		[Setup]
		[UsedImplicitly]
		public void Setup()
		{
			var count = Count;
			ValuesA = new T[count];
			ValuesB = new T[count];
			for (var i = 0; i < count; i++)
			{
				ValuesA[i] = GetValueA(i % ValueARepeats + ValueAOffset);
				ValuesB[i] = GetValueB(i % ValueBRepeats + ValueBOffset);
			}
		}
	}

	/// <summary> Base class for int perf tests </summary>
	public abstract class IntOperatorsBaseCase : OperatorsBaseCase<int>
	{
		protected override int GetValueA(int i) => i;
		protected override int GetValueB(int i) => i;
	}

	/// <summary> Base class for int? perf tests </summary>
	public abstract class NullableIntOperatorsBaseCase : OperatorsBaseCase<int?>
	{
		protected override int? GetValueA(int i) => i == 0 ? null : (int?)i;

		protected override int? GetValueB(int i) => i;
	}

	/// <summary> Base class for double? perf tests </summary>
	public abstract class NullableDoubleOperatorsBaseCase : OperatorsBaseCase<double?>
	{
		protected override double? GetValueA(int i) => i == 0 ? null : (int?)i;

		protected override double? GetValueB(int i) => i;
	}

	/// <summary> Base class for DateTime? perf tests </summary>
	public abstract class NullableDateTimeOperatorsBaseCase : OperatorsBaseCase<DateTime?>
	{
		protected override DateTime? GetValueA(int i) =>
			i == 0 ? (DateTime?)null : DateTime.UtcNow.AddDays(i);

		protected override DateTime? GetValueB(int i) => DateTime.UtcNow;
	}

	/// <summary> Base class for string perf tests </summary>
	public abstract class StringOperatorsBaseCase : OperatorsBaseCase<string>
	{
		/// <summary> Constructor </summary>
		protected StringOperatorsBaseCase()
		{
			Count /= 5;
		}

		protected override string GetValueA(int i) => i == 0 ? null : i.ToString();

		protected override string GetValueB(int i) => i.ToString();
	}
}