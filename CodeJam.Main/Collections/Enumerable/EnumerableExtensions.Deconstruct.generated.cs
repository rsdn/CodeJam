﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable

using System.Collections.Generic;

using JetBrains.Annotations;

namespace CodeJam.Collections
{
	partial class EnumerableExtensions
	{
		private const string _arrayTooShortMsg = "The array is too short.";
		private const string _listTooShortMsg = "The list is too short.";
		private const string _enumTooShortMsg = "The enumerable is too short.";

		/// <summary>
		/// Deconstructs 1 item of the <paramref name="array"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this T[] array,
			out T item1)
		{
			Code.NotNull(array, nameof (array));
			Code.AssertArgument(array.Length >= 1, nameof (array), _arrayTooShortMsg);

			item1 = array[0];
		}

		/// <summary>
		/// Deconstructs 1 items of the <paramref name="list"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IList<T> list,
			out T item1)
		{
			Code.NotNull(list, nameof (list));
			Code.AssertArgument(list.Count >= 1, nameof (list), _listTooShortMsg);

			item1 = list[0];
		}

		/// <summary>
		/// Deconstructs 1 items of the <paramref name="enumerable"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IEnumerable<T> enumerable,
			out T item1)
		{
			Code.NotNull(enumerable, nameof (enumerable));

			using var enumerator = enumerable.GetEnumerator();
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item1 = enumerator.Current;
		}

		/// <summary>
		/// Deconstructs 2 items of the <paramref name="array"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this T[] array,
			out T item1,
			out T item2)
		{
			Code.NotNull(array, nameof (array));
			Code.AssertArgument(array.Length >= 2, nameof (array), _arrayTooShortMsg);

			item1 = array[0];
			item2 = array[1];
		}

		/// <summary>
		/// Deconstructs 2 items of the <paramref name="list"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IList<T> list,
			out T item1,
			out T item2)
		{
			Code.NotNull(list, nameof (list));
			Code.AssertArgument(list.Count >= 2, nameof (list), _listTooShortMsg);

			item1 = list[0];
			item2 = list[1];
		}

		/// <summary>
		/// Deconstructs 2 items of the <paramref name="enumerable"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IEnumerable<T> enumerable,
			out T item1,
			out T item2)
		{
			Code.NotNull(enumerable, nameof (enumerable));

			using var enumerator = enumerable.GetEnumerator();
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item1 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item2 = enumerator.Current;
		}

		/// <summary>
		/// Deconstructs 3 items of the <paramref name="array"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this T[] array,
			out T item1,
			out T item2,
			out T item3)
		{
			Code.NotNull(array, nameof (array));
			Code.AssertArgument(array.Length >= 3, nameof (array), _arrayTooShortMsg);

			item1 = array[0];
			item2 = array[1];
			item3 = array[2];
		}

		/// <summary>
		/// Deconstructs 3 items of the <paramref name="list"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IList<T> list,
			out T item1,
			out T item2,
			out T item3)
		{
			Code.NotNull(list, nameof (list));
			Code.AssertArgument(list.Count >= 3, nameof (list), _listTooShortMsg);

			item1 = list[0];
			item2 = list[1];
			item3 = list[2];
		}

		/// <summary>
		/// Deconstructs 3 items of the <paramref name="enumerable"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IEnumerable<T> enumerable,
			out T item1,
			out T item2,
			out T item3)
		{
			Code.NotNull(enumerable, nameof (enumerable));

			using var enumerator = enumerable.GetEnumerator();
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item1 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item2 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item3 = enumerator.Current;
		}

		/// <summary>
		/// Deconstructs 4 items of the <paramref name="array"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this T[] array,
			out T item1,
			out T item2,
			out T item3,
			out T item4)
		{
			Code.NotNull(array, nameof (array));
			Code.AssertArgument(array.Length >= 4, nameof (array), _arrayTooShortMsg);

			item1 = array[0];
			item2 = array[1];
			item3 = array[2];
			item4 = array[3];
		}

		/// <summary>
		/// Deconstructs 4 items of the <paramref name="list"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IList<T> list,
			out T item1,
			out T item2,
			out T item3,
			out T item4)
		{
			Code.NotNull(list, nameof (list));
			Code.AssertArgument(list.Count >= 4, nameof (list), _listTooShortMsg);

			item1 = list[0];
			item2 = list[1];
			item3 = list[2];
			item4 = list[3];
		}

		/// <summary>
		/// Deconstructs 4 items of the <paramref name="enumerable"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IEnumerable<T> enumerable,
			out T item1,
			out T item2,
			out T item3,
			out T item4)
		{
			Code.NotNull(enumerable, nameof (enumerable));

			using var enumerator = enumerable.GetEnumerator();
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item1 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item2 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item3 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item4 = enumerator.Current;
		}

		/// <summary>
		/// Deconstructs 5 items of the <paramref name="array"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this T[] array,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5)
		{
			Code.NotNull(array, nameof (array));
			Code.AssertArgument(array.Length >= 5, nameof (array), _arrayTooShortMsg);

			item1 = array[0];
			item2 = array[1];
			item3 = array[2];
			item4 = array[3];
			item5 = array[4];
		}

		/// <summary>
		/// Deconstructs 5 items of the <paramref name="list"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IList<T> list,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5)
		{
			Code.NotNull(list, nameof (list));
			Code.AssertArgument(list.Count >= 5, nameof (list), _listTooShortMsg);

			item1 = list[0];
			item2 = list[1];
			item3 = list[2];
			item4 = list[3];
			item5 = list[4];
		}

		/// <summary>
		/// Deconstructs 5 items of the <paramref name="enumerable"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IEnumerable<T> enumerable,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5)
		{
			Code.NotNull(enumerable, nameof (enumerable));

			using var enumerator = enumerable.GetEnumerator();
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item1 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item2 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item3 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item4 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item5 = enumerator.Current;
		}

		/// <summary>
		/// Deconstructs 6 items of the <paramref name="array"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this T[] array,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6)
		{
			Code.NotNull(array, nameof (array));
			Code.AssertArgument(array.Length >= 6, nameof (array), _arrayTooShortMsg);

			item1 = array[0];
			item2 = array[1];
			item3 = array[2];
			item4 = array[3];
			item5 = array[4];
			item6 = array[5];
		}

		/// <summary>
		/// Deconstructs 6 items of the <paramref name="list"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IList<T> list,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6)
		{
			Code.NotNull(list, nameof (list));
			Code.AssertArgument(list.Count >= 6, nameof (list), _listTooShortMsg);

			item1 = list[0];
			item2 = list[1];
			item3 = list[2];
			item4 = list[3];
			item5 = list[4];
			item6 = list[5];
		}

		/// <summary>
		/// Deconstructs 6 items of the <paramref name="enumerable"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IEnumerable<T> enumerable,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6)
		{
			Code.NotNull(enumerable, nameof (enumerable));

			using var enumerator = enumerable.GetEnumerator();
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item1 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item2 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item3 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item4 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item5 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item6 = enumerator.Current;
		}

		/// <summary>
		/// Deconstructs 7 items of the <paramref name="array"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this T[] array,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7)
		{
			Code.NotNull(array, nameof (array));
			Code.AssertArgument(array.Length >= 7, nameof (array), _arrayTooShortMsg);

			item1 = array[0];
			item2 = array[1];
			item3 = array[2];
			item4 = array[3];
			item5 = array[4];
			item6 = array[5];
			item7 = array[6];
		}

		/// <summary>
		/// Deconstructs 7 items of the <paramref name="list"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IList<T> list,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7)
		{
			Code.NotNull(list, nameof (list));
			Code.AssertArgument(list.Count >= 7, nameof (list), _listTooShortMsg);

			item1 = list[0];
			item2 = list[1];
			item3 = list[2];
			item4 = list[3];
			item5 = list[4];
			item6 = list[5];
			item7 = list[6];
		}

		/// <summary>
		/// Deconstructs 7 items of the <paramref name="enumerable"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IEnumerable<T> enumerable,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7)
		{
			Code.NotNull(enumerable, nameof (enumerable));

			using var enumerator = enumerable.GetEnumerator();
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item1 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item2 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item3 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item4 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item5 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item6 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item7 = enumerator.Current;
		}

		/// <summary>
		/// Deconstructs 8 items of the <paramref name="array"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this T[] array,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7,
			out T item8)
		{
			Code.NotNull(array, nameof (array));
			Code.AssertArgument(array.Length >= 8, nameof (array), _arrayTooShortMsg);

			item1 = array[0];
			item2 = array[1];
			item3 = array[2];
			item4 = array[3];
			item5 = array[4];
			item6 = array[5];
			item7 = array[6];
			item8 = array[7];
		}

		/// <summary>
		/// Deconstructs 8 items of the <paramref name="list"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IList<T> list,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7,
			out T item8)
		{
			Code.NotNull(list, nameof (list));
			Code.AssertArgument(list.Count >= 8, nameof (list), _listTooShortMsg);

			item1 = list[0];
			item2 = list[1];
			item3 = list[2];
			item4 = list[3];
			item5 = list[4];
			item6 = list[5];
			item7 = list[6];
			item8 = list[7];
		}

		/// <summary>
		/// Deconstructs 8 items of the <paramref name="enumerable"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IEnumerable<T> enumerable,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7,
			out T item8)
		{
			Code.NotNull(enumerable, nameof (enumerable));

			using var enumerator = enumerable.GetEnumerator();
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item1 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item2 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item3 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item4 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item5 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item6 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item7 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item8 = enumerator.Current;
		}

		/// <summary>
		/// Deconstructs 9 items of the <paramref name="array"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this T[] array,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7,
			out T item8,
			out T item9)
		{
			Code.NotNull(array, nameof (array));
			Code.AssertArgument(array.Length >= 9, nameof (array), _arrayTooShortMsg);

			item1 = array[0];
			item2 = array[1];
			item3 = array[2];
			item4 = array[3];
			item5 = array[4];
			item6 = array[5];
			item7 = array[6];
			item8 = array[7];
			item9 = array[8];
		}

		/// <summary>
		/// Deconstructs 9 items of the <paramref name="list"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IList<T> list,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7,
			out T item8,
			out T item9)
		{
			Code.NotNull(list, nameof (list));
			Code.AssertArgument(list.Count >= 9, nameof (list), _listTooShortMsg);

			item1 = list[0];
			item2 = list[1];
			item3 = list[2];
			item4 = list[3];
			item5 = list[4];
			item6 = list[5];
			item7 = list[6];
			item8 = list[7];
			item9 = list[8];
		}

		/// <summary>
		/// Deconstructs 9 items of the <paramref name="enumerable"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IEnumerable<T> enumerable,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7,
			out T item8,
			out T item9)
		{
			Code.NotNull(enumerable, nameof (enumerable));

			using var enumerator = enumerable.GetEnumerator();
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item1 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item2 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item3 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item4 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item5 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item6 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item7 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item8 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item9 = enumerator.Current;
		}

		/// <summary>
		/// Deconstructs 10 items of the <paramref name="array"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this T[] array,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7,
			out T item8,
			out T item9,
			out T item10)
		{
			Code.NotNull(array, nameof (array));
			Code.AssertArgument(array.Length >= 10, nameof (array), _arrayTooShortMsg);

			item1 = array[0];
			item2 = array[1];
			item3 = array[2];
			item4 = array[3];
			item5 = array[4];
			item6 = array[5];
			item7 = array[6];
			item8 = array[7];
			item9 = array[8];
			item10 = array[9];
		}

		/// <summary>
		/// Deconstructs 10 items of the <paramref name="list"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IList<T> list,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7,
			out T item8,
			out T item9,
			out T item10)
		{
			Code.NotNull(list, nameof (list));
			Code.AssertArgument(list.Count >= 10, nameof (list), _listTooShortMsg);

			item1 = list[0];
			item2 = list[1];
			item3 = list[2];
			item4 = list[3];
			item5 = list[4];
			item6 = list[5];
			item7 = list[6];
			item8 = list[7];
			item9 = list[8];
			item10 = list[9];
		}

		/// <summary>
		/// Deconstructs 10 items of the <paramref name="enumerable"/>.
		/// </summary>
		[Pure, System.Diagnostics.Contracts.Pure]
		public static void Deconstruct<T>(
			this IEnumerable<T> enumerable,
			out T item1,
			out T item2,
			out T item3,
			out T item4,
			out T item5,
			out T item6,
			out T item7,
			out T item8,
			out T item9,
			out T item10)
		{
			Code.NotNull(enumerable, nameof (enumerable));

			using var enumerator = enumerable.GetEnumerator();
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item1 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item2 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item3 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item4 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item5 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item6 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item7 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item8 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item9 = enumerator.Current;
			Code.AssertArgument(enumerator.MoveNext(), nameof (enumerable), _enumTooShortMsg);
			item10 = enumerator.Current;
		}

	}
}

