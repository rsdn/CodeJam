using System;
using System.ComponentModel;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Parus.Business
{
	/// <summary>
	/// Диапазон значений.
	/// </summary>
	/// <typeparam name="T">
	/// Тип значений диапазона. Должен реализовать <seealso cref="IComparable{T}"/> или <seealso cref="IComparable"/>.
	/// </typeparam>
	[Serializable]
	[DebuggerDisplay("{DebuggerDisplay,nq}")]
	public class Range<T> : IEquatable<Range<T>>, IFormattable
	{
		#region Static members
		#region Pre-defined values
		/// <summary>
		/// Пустой диапазон, ∅.
		/// </summary>
		[NotNull]
		public static readonly Range<T> Empty = new Range<T>(RangeBoundary<T>.Empty, RangeBoundary<T>.Empty);
		/// <summary>
		/// Бесконечный диапазон, (-∞..+∞).
		/// </summary>
		[NotNull]
		public static readonly Range<T> Infinity = new Range<T>(RangeBoundary<T>.NegativeInfinity, RangeBoundary<T>.PositiveInfinity);
		#endregion

		#region Operators
		/// <summary>
		/// Оператор "Равно".
		/// </summary>
		/// <param name="range1">Диапазон 1.</param>
		/// <param name="range2">Диапазон 2.</param>
		/// <returns><c>True</c>, если диапазоны равны.</returns>
		public static bool operator ==([NotNull] Range<T> range1, [NotNull] Range<T> range2)
		{
			return object.Equals(range1, range2);
		}

		/// <summary>
		/// Оператор "Не равно".
		/// </summary>
		/// <param name="range1">Диапазон 1.</param>
		/// <param name="range2">Диапазон 2.</param>
		/// <returns><c>True</c>, если диапазоны не равны.</returns>
		public static bool operator !=([NotNull] Range<T> range1, [NotNull] Range<T> range2)
		{
			return !(range1 == range2);
		}

		#region CLS-friendly operators
		/// <summary>
		/// Метод сравнения на равенство.
		/// </summary>
		/// <param name="range1">Диапазон 1.</param>
		/// <param name="range2">Диапазон 2.</param>
		/// <returns><c>True</c>, если границы равны.</returns>
		public static bool Equals([NotNull] Range<T> range1, [NotNull] Range<T> range2)
		{
			return range1 == range2;
		}
		#endregion
		#endregion
		#endregion

		#region Fields & .ctor()
		/// <summary>
		/// Конструктор диапазона.
		/// </summary>
		/// <param name="from">Значение левой границы. Если значение == <c>null</c>, создаётся бесконечная левая граница (-∞).</param>
		/// <param name="to">Значение правой границы. Если значение == <c>null</c>, создаётся бесконечная правая граница (+∞).</param>
		public Range(T from, T to)
			: this(Range.BoundaryFrom(from), Range.BoundaryTo(to), null)
		{
		}
		/// <summary>
		/// Конструктор диапазона.
		/// </summary>
		/// <param name="from">Значение левой границы. Если значение == <c>null</c>, создаётся бесконечная левая граница (-∞).</param>
		/// <param name="to">Значение правой границы. Если значение == <c>null</c>, создаётся бесконечная правая граница (+∞).</param>
		/// <param name="key">Ключ диапазона.</param>
		public Range(T from, T to, object key)
			: this(Range.BoundaryFrom(from), Range.BoundaryTo(to), key)
		{
		}

		/// <summary>
		/// Конструктор диапазона.
		/// </summary>
		/// <param name="from">Левая граница.</param>
		/// <param name="to">Правая граница.</param>
		public Range(RangeBoundary<T> from, RangeBoundary<T> to)
			: this(from, to, null)
		{
		}
		/// <summary>
		/// Конструктор диапазона.
		/// </summary>
		/// <param name="from">Левая граница.</param>
		/// <param name="to">Правая граница.</param>
		/// <param name="key">Ключ диапазона.</param>
		public Range(RangeBoundary<T> from, RangeBoundary<T> to, object key)
		{
			if (from.IsNotEmpty || to.IsNotEmpty)
			{
				Range.ValidateFrom(from);
				Range.ValidateTo(to);
				if (to < from)
				{
					throw new ArgumentOutOfRangeException("to", "Неверные границы диапазона.");
				}
			}

			From = from;
			To = to;
			Key = key;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Левая граница.
		/// </summary>
		public RangeBoundary<T> From { get; private set; }

		/// <summary>
		/// Правая граница.
		/// </summary>
		public RangeBoundary<T> To { get; private set; }

		/// <summary>
		/// Значение левой границы.
		/// </summary>
		public T FromValue
		{
			get
			{
				Debug.Assert(From.HasValue, "Попытка получить значение для границы без значения.");
				return From.Value;
			}
		}

		/// <summary>
		/// Значение правой границы.
		/// </summary>
		public T ToValue
		{
			get
			{
				Debug.Assert(To.HasValue, "Попытка получить значение для границы без значения.");
				return To.Value;
			}
		}

		/// <summary>
		/// Пустой диапазон, ∅.
		/// </summary>
		public bool IsEmpty
		{
			get { return From.IsEmpty; }
		}
		/// <summary>
		/// Непустой диапазон, != ∅.
		/// </summary>
		public bool IsNotEmpty
		{
			get { return From.IsNotEmpty; }
		}

		/// <summary>
		/// Диапазон нулевой длины (правая граница интервала равна левой).
		/// </summary>
		public bool IsSinglePoint
		{
			get { return From == To; }
		}

		/// <summary>
		/// Бесконечный диапазон (-∞..+∞).
		/// </summary>
		public bool IsInfinity
		{
			get { return From.IsNegativeInfinity && To.IsPositiveInfinity; }
		}

		/// <summary>
		/// Ключ диапазона.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public object Key { get; private set; }
		#endregion

		#region Create
		/// <summary>
		/// Пытается создать диапазон - результат операции над существующим диапазоном.
		/// Если левая граница больше правой - возвращает пустой диапазон.
		/// </summary>
		/// <param name="from">Левая граница диапазона.</param>
		/// <param name="to">Правая граница диапазона.</param>
		/// <returns>Новый диапазон или пустой диапазон, если левая граница больше правой.</returns>
		[NotNull]
		protected Range<T> TryCreateResult(RangeBoundary<T> from, RangeBoundary<T> to)
		{
			return (from.IsEmpty || from > to) ? CreateEmptyResult() : CreateResult(from, to);
		}


		/// <summary>
		/// Метод создания нового диапазона - результата операции над существующим диапазоном (пересечения, объединения и т.п.).
		/// Наследники должны переопределять данный метод,
		/// если требуется, чтобы операции над типом-наследником возвращали тип, отличный от <see cref="Range{T}"/>.
		/// </summary>
		/// <param name="from">Левая граница диапазона.</param>
		/// <param name="to">Правая граница диапазона.</param>
		/// <returns>Новый диапазон.</returns>
		[NotNull]
		protected virtual Range<T> CreateResult(RangeBoundary<T> from, RangeBoundary<T> to)
		{
			return new Range<T>(from, to, Key);
		}

		/// <summary>
		/// Метод создания нового пустого диапазона.
		/// Наследники должны переопределять данный метод,
		/// если требуется, чтобы операции над типом-наследником возвращали тип, отличный от <see cref="Range{T}"/>.
		/// </summary>
		[NotNull]
		protected virtual Range<T> CreateEmptyResult()
		{
			return Empty;
		}
		#endregion

		#region Self-operations
		/// <summary>
		/// Создаёт диапазон с закрытыми ( [a,b] ) границами.
		/// </summary>
		/// <param name="fromValueSelector">Callback для коррекции значения правой границы.</param>
		/// <param name="toValueSelector">Callback для коррекции значения левой границы.</param>
		/// <returns>
		/// Диапазон с закрытыми ( [a,b] ) границами,
		/// или пустой диапазон, если новая правая граница меньше левой.
		/// </returns>
		[NotNull]
		public Range<T> MakeInclusive(Func<T, T> fromValueSelector, Func<T, T> toValueSelector)
		{
			if (IsEmpty || (!From.IsExclusiveBoundary && !To.IsExclusiveBoundary))
			{
				return this;
			}

			var fromInclusive = From.IsExclusiveBoundary ?
				Range.BoundaryFrom(fromValueSelector(From.Value)) :
				From;

			var toInclusive = To.IsExclusiveBoundary ?
				Range.BoundaryTo(toValueSelector(To.Value)) :
				To;

			return TryCreateResult(fromInclusive, toInclusive);
		}

		/// <summary>
		/// Создаёт диапазон с открытыми ( (a,b) ) границами.
		/// </summary>
		/// <param name="fromValueSelector">Callback для коррекции значения правой границы.</param>
		/// <param name="toValueSelector">Callback для коррекции значения левой границы.</param>
		/// <returns>
		/// Диапазон с открытыми ( (a,b) ) границами,
		/// или пустой диапазон, если новая правая граница меньше левой.
		/// </returns>
		[NotNull]
		public Range<T> MakeExclusive(Func<T, T> fromValueSelector, Func<T, T> toValueSelector)
		{
			if (IsEmpty || (!From.IsInclusiveBoundary && !To.IsInclusiveBoundary))
			{
				return this;
			}

			var fromExclusive = From.IsInclusiveBoundary ?
				Range.ExclusiveBoundaryFrom(fromValueSelector(From.Value)) :
				From;

			var toExclusive = To.IsInclusiveBoundary ?
				Range.ExclusiveBoundaryTo(toValueSelector(To.Value)) :
				To;

			return TryCreateResult(fromExclusive, toExclusive);
		}

		/// <summary>
		/// Создаёт диапазон с изменёнными значениями границ.
		/// </summary>
		/// <param name="valueSelector">Callback для обновления значений границ.</param>
		/// <returns>
		/// Диапазон с изменёнными значениями границ,
		/// или пустой диапазон, если новая правая граница меньше левой.
		/// </returns>
		[NotNull]
		public Range<T> Update(Func<T, T> valueSelector)
		{
			return Update(valueSelector, valueSelector);
		}

		/// <summary>
		/// Создаёт диапазон с изменёнными значениями границ.
		/// </summary>
		/// <param name="fromValueSelector">Callback для обновления значения правой границы.</param>
		/// <param name="toValueSelector">Callback для обновления значения левой границы.</param>
		/// <returns>
		/// Диапазон с изменёнными значениями границ,
		/// или пустой диапазон, если новая правая граница меньше левой.
		/// </returns>
		[NotNull]
		public Range<T> Update(Func<T, T> fromValueSelector, Func<T, T> toValueSelector)
		{
			var from = From.UpdateValue(fromValueSelector);
			var to = To.UpdateValue(toValueSelector);
			return TryCreateResult(from, to);
		}

		/// <summary>
		/// Сбрасывает ключ (<seealso cref="Key"/>) диапазона.
		/// </summary>
		/// <returns>Диапазон с теми же границами, но без ключа.</returns>
		public Range<T> RemoveRangeKey()
		{
			return Range.Create(From, To);
		}

		/// <summary>
		/// Заменяет ключ (<seealso cref="Key"/>) диапазона.
		/// </summary>
		/// <returns>Диапазон с теми же границами, но с другим ключом.</returns>
		public Range<T> WithRangeKey(object key)
		{
			return Range.Create(From, To, key);
		}
		#endregion

		#region Contains & HasIntersection
		#region Contains
		/// <summary>
		/// Метод проверки на вхождение значения.
		/// </summary>
		/// <param name="value">Значение. Если значение == <c>null</c>, оно обрабатывается как -∞.</param>
		/// <returns><c>True</c>, если данный диапазон содержит значение.</returns>
		public bool Contains(T value)
		{
			return Contains(Range.BoundaryFrom(value));
		}

		/// <summary>
		/// Метод проверки на вхождение непустой границы.
		/// </summary>
		/// <param name="other">Граница другого диапазона.</param>
		/// <returns><c>True</c>, если данный диапазон содержит непустую границу другого диапазона.</returns>
		public bool Contains(RangeBoundary<T> other)
		{
			return other.IsNotEmpty && (From <= other && To >= other);
		}

		/// <summary>
		/// Метод проверки на вхождение поддиапазона.
		/// </summary>
		/// <param name="from">Левая граница поддиапазона.</param>
		/// <param name="to">Правая граница поддиапазона.</param>
		/// <returns><c>True</c>, если данный диапазон содержит поддиапазон.</returns>
		public bool Contains(T from, T to)
		{
			return Contains(Range.Create(from, to));
		}

		/// <summary>
		/// Метод проверки на вхождение непустого поддиапазона.
		/// </summary>
		/// <param name="other">Поддиапазон.</param>
		/// <returns><c>True</c>, если данный диапазон содержит поддиапазон и поддиапазон не пуст.</returns>
		public bool Contains([NotNull] Range<T> other)
		{
			if (IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && From <= other.From && To >= other.To;
		}
		#endregion

		#region HasIntersection
		/// <summary>
		/// Метод проверки на пересечение.
		/// </summary>
		/// <param name="from">Левая граница диапазона 2.</param>
		/// <param name="to">Правая граница диапазона 2.</param>
		/// <returns><c>True</c>, если данный диапазон пересекается с диапазоном 2.</returns>
		public bool HasIntersection(T from, T to)
		{
			return HasIntersection(Range.Create(from, to));
		}

		/// <summary>
		/// Метод проверки на пересечение непустых диапазонов.
		/// </summary>
		/// <param name="other">Диапазон 2.</param>
		/// <returns><c>True</c>, если данный диапазон пересекается с диапазоном 2 и оба не пусты.</returns>
		public bool HasIntersection([NotNull] Range<T> other)
		{
			if (IsEmpty)
			{
				return other.IsEmpty;
			}
			return other.IsNotEmpty && From <= other.To && To >= other.From;
		}
		#endregion

		#region Adjust
		/// <summary>
		/// Ограничивает значение текущим диапазоном.
		/// </summary>
		/// <param name="value">Ограничиваемое значение.</param>
		/// <returns><seealso cref="FromValue"/> (<seealso cref="ToValue"/>) если значение выходит за границы диапазона. Иначе <paramref name="value"/>.</returns>
		public T Adjust(T value)
		{
			if (From > value)
			{
				if (From.IsExclusiveBoundary)
				{
					throw new InvalidOperationException("From boundary is exclusive and has no value.");
				}
				return From.Value;
			}
			else if (To < value)
			{
				if (To.IsExclusiveBoundary)
				{
					throw new InvalidOperationException("To boundary is exclusive and has no value.");
				}
				return To.Value;
			}
			return value;
		}
		#endregion
		#endregion

		#region StartsAfter
		/// <summary>
		/// Проверяет, что диапазон начинается после переданного значения.
		/// </summary>
		/// <param name="value">Значение, с которым сравнивается начало диапазона. Если значение == <c>null</c>, оно обрабатывается как -∞.</param>
		/// <returns><c>True</c>, если диапазон начинается после переданного значения.</returns>
		public bool StartsAfter(T value)
		{
			return From > Range.BoundaryFrom(value); // DONTTOUCH: null обрабатывается как -∞
		}
		/// <summary>
		/// Проверяет, что диапазон начинается после переданной границы.
		/// </summary>
		/// <param name="other">Граница, с которой сравнивается начало диапазона.</param>
		/// <returns><c>True</c>, если диапазон начинается после переданной границы.</returns>
		public bool StartsAfter(RangeBoundary<T> other)
		{
			return other.IsNotEmpty && From > other;
		}
		/// <summary>
		/// Проверяет, что диапазон начинается после диапазона 2.
		/// </summary>
		/// <param name="other">Диапазон 2.</param>
		/// <returns><c>True</c>, если диапазон начинается после диапазона 2.</returns>
		public bool StartsAfter([NotNull] Range<T> other)
		{
			return other.IsNotEmpty && From > other.To;
		}
		#endregion

		#region EndsBefore
		/// <summary>
		/// Проверяет, что диапазон заканчивается перед переданным значением.
		/// </summary>
		/// <param name="value">Значение, с которым сравнивается начало диапазона. Если значение == <c>null</c>, оно обрабатывается как +∞.</param>
		/// <returns><c>True</c>, если диапазон заканчивается перед переданным значением.</returns>
		public bool EndsBefore(T value)
		{
			return To < Range.BoundaryTo(value); // DONTTOUCH: null обрабатывается как +∞
		}
		/// <summary>
		/// Проверяет, что диапазон заканчивается перед переданной границей.
		/// </summary>
		/// <param name="other">Граница, с которой сравнивается начало диапазона.</param>
		/// <returns><c>True</c>, если диапазон заканчивается перед переданной границей.</returns>
		public bool EndsBefore(RangeBoundary<T> other)
		{
			return To < other;
		}
		/// <summary>
		/// Проверяет, что диапазон заканчивается перед диапазоном 2.
		/// </summary>
		/// <param name="other">Диапазон 2.</param>
		/// <returns><c>True</c>, если диапазон заканчивается перед диапазоном 2.</returns>
		public bool EndsBefore([NotNull] Range<T> other)
		{
			return IsNotEmpty && To < other.From;
		}
		#endregion

		#region Union & Intersect
		#region Union/Extend
		/// <summary>
		/// Возвращает объединение диапазонов.
		/// </summary>
		/// <param name="from">Левая граница диапазона2.</param>
		/// <param name="to">Правая граница диапазона 2.</param>
		/// <returns>Объединение диапазонов.</returns>
		[NotNull]
		public Range<T> Union(T from, T to)
		{
			return Union(Range.Create(from, to));
		}

		/// <summary>
		/// Возвращает объединение диапазонов.
		/// </summary>
		/// <param name="other">Диапазон 2.</param>
		/// <returns>Объединение диапазонов.</returns>
		[NotNull]
		public Range<T> Union([NotNull] Range<T> other)
		{
			if (other.IsEmpty)
			{
				return this;
			}
			if (IsEmpty)
			{
				return CreateResult(other.From, other.To);
			}

			return CreateResult(
				Range.Min(From, other.From),
				Range.Max(To, other.To));
		}

		/// <summary>
		/// Расширяет границу справа.
		/// </summary>
		/// <param name="to">Новая правая граница.</param>
		/// <returns>Расширенный справа диапазон</returns>
		[NotNull]
		public Range<T> ExtendTo(T to)
		{
			return ExtendTo(Range.BoundaryTo(to));
		}
		/// <summary>
		/// Расширяет границу справа.
		/// </summary>
		/// <param name="toBoundary">Новая правая граница.</param>
		/// <returns>Расширенный справа диапазон</returns>
		[NotNull]
		public Range<T> ExtendTo(RangeBoundary<T> toBoundary)
		{
			Range.ValidateTo(toBoundary);

			return To >= toBoundary ? this : CreateResult(From, toBoundary);
		}

		/// <summary>
		/// Расширяет границу слева.
		/// </summary>
		/// <param name="from">Новая левая граница.</param>
		/// <returns>Расширенный справа диапазон</returns>
		[NotNull]
		public Range<T> ExtendFrom(T from)
		{
			return ExtendFrom(Range.BoundaryFrom(from));
		}
		/// <summary>
		/// Расширяет границу слева.
		/// </summary>
		/// <param name="fromBoundary">Новая левая граница.</param>
		/// <returns>Расширенный справа диапазон</returns>
		[NotNull]
		public Range<T> ExtendFrom(RangeBoundary<T> fromBoundary)
		{
			Range.ValidateFrom(fromBoundary);

			return From <= fromBoundary ? this : CreateResult(fromBoundary, To);
		}
		#endregion

		#region Trim/Intersect
		/// <summary>
		/// Возвращает пересечение диапазонов, или пустой диапазон, если пересечение не найдено.
		/// </summary>
		/// <param name="from">Левая граница диапазона2.</param>
		/// <param name="to">Правая граница диапазона 2.</param>
		/// <returns>Пересечение диапазонов, или пустой диапазон, если пересечение не найдено.</returns>
		[NotNull]
		public Range<T> Intersect(T from, T to)
		{
			return Intersect(Range.Create(from, to));
		}

		/// <summary>
		/// Возвращает пересечение диапазонов, или пустой диапазон, если пересечение не найдено.
		/// </summary>
		/// <param name="other">Диапазон 2.</param>
		/// <returns>Пересечение диапазонов, или пустой диапазон, если пересечение не найдено.</returns>
		[NotNull]
		public Range<T> Intersect([NotNull] Range<T> other)
		{
			if (IsEmpty)
			{
				return this;
			}
			if (other.IsEmpty)
			{
				return CreateEmptyResult();
			}

			return TryCreateResult(
				Range.Max(From, other.From),
				Range.Min(To, other.To));
		}

		/// <summary>
		/// Урезает границу справа.
		/// </summary>
		/// <param name="to">Новая правая граница.</param>
		/// <returns>Ужатый справа диапазон или пустой диапазон, если левая граница больше правой</returns>
		[NotNull]
		public Range<T> TrimTo(T to)
		{
			return TrimTo(Range.BoundaryTo(to));
		}
		/// <summary>
		/// Урезает границу справа
		/// </summary>
		/// <param name="toBoundary">Новая правая граница.</param>
		/// <returns>Ужатый справа диапазон или пустой диапазон, если левая граница больше правой</returns>
		[NotNull]
		public Range<T> TrimTo(RangeBoundary<T> toBoundary)
		{
			Range.ValidateTo(toBoundary);

			return To <= toBoundary ? this : TryCreateResult(From, toBoundary);
		}

		/// <summary>
		/// Урезает границу слева.
		/// </summary>
		/// <param name="from">Новая левая граница.</param>
		/// <returns>Ужатый слева диапазон или пустой диапазон, если левая граница больше правой</returns>
		[NotNull]
		public Range<T> TrimFrom(T from)
		{
			return TrimFrom(Range.BoundaryFrom(from));
		}
		/// <summary>
		/// Урезает границу слева.
		/// </summary>
		/// <param name="fromBoundary">Новая левая граница.</param>
		/// <returns>Ужатый слева диапазон или пустой диапазон, если левая граница больше правой</returns>
		[NotNull]
		public Range<T> TrimFrom(RangeBoundary<T> fromBoundary)
		{
			Range.ValidateFrom(fromBoundary);

			return From >= fromBoundary ? this : TryCreateResult(fromBoundary, To);
		}
		#endregion
		#endregion

		#region IEquatable<Range<T>>
		/// <summary>
		/// Сравнивает диапазоны на равенство
		/// </summary>
		/// <param name="other">Второй диапазон.</param>
		/// <returns><c>True</c>, если диапазоны равны.</returns>
		public bool Equals(Range<T> other)
		{
			return Equals(other, false);
		}

		/// <summary>
		/// Сравнивает диапазоны на равенство
		/// </summary>
		/// <param name="other">Второй диапазон.</param>
		/// <param name="ignoreRangeKey">Не учитывать ключи диапазонов при сравнении.</param>
		/// <returns><c>True</c>, если диапазоны равны.</returns>
		public bool Equals(Range<T> other, bool ignoreRangeKey)
		{
			Debug.Assert(!ReferenceEquals(other, null));

			if (IsEmpty)
			{
				return ReferenceEquals(other, null) || other.IsEmpty;
			}

			return !ReferenceEquals(other, null) &&
				From == other.From && To == other.To &&
				(ignoreRangeKey || object.Equals(Key, other.Key));
		}

		/// <summary>
		/// Сравнивает диапазоны на равенство
		/// </summary>
		/// <param name="obj">Второй диапазон.</param>
		/// <returns><c>True</c>, если диапазоны равны.</returns>
		public override sealed bool Equals(object obj)
		{
			return Equals(obj as Range<T>);
		}

		/// <summary>
		/// Возвращает хеш-код диапазона.
		/// </summary>
		/// <returns>Хеш-код диапазона.</returns>
		public override int GetHashCode()
		{
			var rangeHash = Range.CombineHashCodes(From.GetHashCode(), To.GetHashCode());
			return Key == null ?
				rangeHash :
				Range.CombineHashCodes(rangeHash, Key.GetHashCode());
		}
		#endregion

		#region ToString
		private string DebuggerDisplay
		{
			get { return ToString("g"); }
		}

		private string GetKeyString()
		{
			return Key == null ?
				null :
				Range.KeyPrefixChar +
				Key.ToString() +
				Range.KeySeparatorString;
		}

		/// <summary>
		/// Возвращает строковое представление диапазона.
		/// </summary>
		/// <returns>Строковое представление диапазона.</returns>
		public override string ToString()
		{
			if (IsEmpty)
			{
				return GetKeyString() + RangeBoundary.EmptyString;
			}

			return GetKeyString() +
				From.ToString() +
				Range.SeparatorString +
				To.ToString();
		}

		/// <summary>
		/// Возвращает строковое представление диапазона,
		/// используя заданный формат
		/// (если <typeparamref name="T"/> не реализует <seealso cref="IFormattable"/>, то параметры форматирования игнорируются)
		/// </summary>
		/// <param name="format">Формат значений границ диапазона</param>
		/// <returns>Строковое представление диапазона.</returns>
		[NotNull]
		public string ToString(string format)
		{
			return ToString(format, null);
		}

		/// <summary>
		/// Возвращает строковое представление диапазона,
		/// используя заданный формат и форматтер
		/// (если <typeparamref name="T"/> не реализует <seealso cref="IFormattable"/>, то параметры форматирования игнорируются)
		/// </summary>
		/// <param name="format">Формат значений границ диапазона</param>
		/// <param name="formatProvider">Провайдер настроек форматирования</param>
		/// <returns>Строковое представление диапазона.</returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (IsEmpty)
			{
				return GetKeyString() + RangeBoundary.EmptyString;
			}

			return GetKeyString() +
				From.ToString(format, formatProvider) +
				Range.SeparatorString +
				To.ToString(format, formatProvider);
		}
		#endregion
	}
}