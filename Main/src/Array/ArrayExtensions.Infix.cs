using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using JetBrains.Annotations;

namespace CodeJam
{
	/// <summary>
	/// <see cref="Array" /> class extensions.
	/// </summary>
	[PublicAPI]
	public static class ArrayExtensions
	{
		/// <summary>Returns a read-only wrapper for the specified array.</summary>
		/// <returns>A read-only <see cref="ReadOnlyCollection{T}" /> wrapper for the specified array.</returns>
		/// <param name="array">The one-dimensional, zero-based array to wrap in a read-only <see cref="ReadOnlyCollection{T}" />  wrapper. </param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array) => Array.AsReadOnly(array);

		#region BinarySearch

		/// <summary>Searches an entire one-dimensional sorted <see cref="Array" /> for a specific element, using the <see cref="IComparable{T}" /> generic interface implemented by each element of the <see cref="Array" /> and by the specified object.</summary>
		/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
		/// <param name="array">The sorted one-dimensional, zero-based <see cref="Array" /> to search. </param>
		/// <param name="value">The object to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="value" /> does not implement the <see cref="IComparable{T}" /> generic interface, and the search encounters an element that does not implement the <see cref="IComparable{T}" /> generic interface.</exception>
		public static int BinarySearch<T>([NotNull] this T[] array, T value) => Array.BinarySearch(array, value);

		/// <summary>Searches an entire one-dimensional sorted <see cref="Array" /> for a value using the specified <see cref="IComparer{T}" /> generic interface.</summary>
		/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
		/// <param name="array">The sorted one-dimensional, zero-based <see cref="Array" /> to search.  </param>
		/// <param name="value">The object to search for.</param>
		/// <param name="comparer">The <see cref="IComparer{T}" /> implementation to use when comparing elements.-or- null to use the <see cref="IComparable{T}" /> implementation of each element.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="comparer" /> is null, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="comparer" /> is null, <paramref name="value" /> does not implement the <see cref="IComparable{T}" /> generic interface, and the search encounters an element that does not implement the <see cref="IComparable{T}" /> generic interface.</exception>
		public static int BinarySearch<T>([NotNull] this T[] array, T value, IComparer<T> comparer) => Array.BinarySearch(array, value, comparer);

		/// <summary>Searches a range of elements in a one-dimensional sorted <see cref="Array" /> for a value, using the <see cref="IComparable{T}" /> generic interface implemented by each element of the <see cref="Array" /> and by the specified value.</summary>
		/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
		/// <param name="array">The sorted one-dimensional, zero-based <see cref="Array" /> to search. </param>
		/// <param name="index">The starting index of the range to search.</param>
		/// <param name="length">The length of the range to search.</param>
		/// <param name="value">The object to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.-or-<paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="value" /> does not implement the <see cref="IComparable{T}" /> generic interface, and the search encounters an element that does not implement the <see cref="IComparable{T}" /> generic interface.</exception>
		public static int BinarySearch<T>([NotNull] this T[] array, int index, int length, T value) => Array.BinarySearch(array, index, length, value);

		/// <summary>Searches a range of elements in a one-dimensional sorted <see cref="Array" /> for a value, using the specified <see cref="IComparer{T}" /> generic interface.</summary>
		/// <returns>The index of the specified <paramref name="value" /> in the specified <paramref name="array" />, if <paramref name="value" /> is found. If <paramref name="value" /> is not found and <paramref name="value" /> is less than one or more elements in <paramref name="array" />, a negative number which is the bitwise complement of the index of the first element that is larger than <paramref name="value" />. If <paramref name="value" /> is not found and <paramref name="value" /> is greater than any of the elements in <paramref name="array" />, a negative number which is the bitwise complement of (the index of the last element plus 1).</returns>
		/// <param name="array">The sorted one-dimensional, zero-based <see cref="Array" /> to search. </param>
		/// <param name="index">The starting index of the range to search.</param>
		/// <param name="length">The length of the range to search.</param>
		/// <param name="value">The object to search for.</param>
		/// <param name="comparer">The <see cref="IComparer{T}" /> implementation to use when comparing elements.-or- null to use the <see cref="IComparable{T}" /> implementation of each element.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.-or-<paramref name="comparer" /> is null, and <paramref name="value" /> is of a type that is not compatible with the elements of <paramref name="array" />.</exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="comparer" /> is null, <paramref name="value" /> does not implement the <see cref="IComparable{T}" /> generic interface, and the search encounters an element that does not implement the <see cref="IComparable{T}" /> generic interface.</exception>
		public static int BinarySearch<T>([NotNull] this T[] array, int index, int length, T value, IComparer<T> comparer) => Array.BinarySearch(array, index, length, value, comparer);

		#endregion BinarySearch

		/// <summary>Sets a range of elements in the <see cref="Array" /> to zero, to false, or to null, depending on the element type.</summary>
		/// <param name="array">The <see cref="Array" /> whose elements need to be cleared.</param>
		/// <param name="index">The starting index of the range of elements to clear.</param>
		/// <param name="length">The number of elements to clear.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="IndexOutOfRangeException">
		/// <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.-or-The sum of <paramref name="index" /> and <paramref name="length" /> is greater than the size of the <see cref="Array" />.</exception>
		/// <filterpriority>1</filterpriority>
		public static void Clear([NotNull] this Array array, int index, int length) => Array.Clear(array, index, length);

		/// <summary>Copies a range of elements from an <see cref="Array" /> starting at the specified source index and pastes them to another <see cref="Array" /> starting at the specified destination index.  Guarantees that all changes are undone if the copy does not succeed completely.</summary>
		/// <param name="sourceArray">The <see cref="Array" /> that contains the data to copy.</param>
		/// <param name="sourceIndex">A 32-bit integer that represents the index in the <paramref name="sourceArray" /> at which copying begins.</param>
		/// <param name="destinationArray">The <see cref="Array" /> that receives the data.</param>
		/// <param name="destinationIndex">A 32-bit integer that represents the index in the <paramref name="destinationArray" /> at which storing begins.</param>
		/// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sourceArray" /> is null.-or-<paramref name="destinationArray" /> is null.</exception>
		/// <exception cref="RankException">
		/// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
		/// <exception cref="ArrayTypeMismatchException">The <paramref name="sourceArray" /> type is neither the same as nor derived from the <paramref name="destinationArray" /> type.</exception>
		/// <exception cref="InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="sourceIndex" /> is less than the lower bound of the first dimension of <paramref name="sourceArray" />.-or-<paramref name="destinationIndex" /> is less than the lower bound of the first dimension of <paramref name="destinationArray" />.-or-<paramref name="length" /> is less than zero.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="length" /> is greater than the number of elements from <paramref name="sourceIndex" /> to the end of <paramref name="sourceArray" />.-or-<paramref name="length" /> is greater than the number of elements from <paramref name="destinationIndex" /> to the end of <paramref name="destinationArray" />.</exception>
		public static void ConstrainedCopy([NotNull] this Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length) => Array.ConstrainedCopy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

		/// <summary>Converts an array of one type to an array of another type.</summary>
		/// <returns>An array of the target type containing the converted elements from the source array.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to convert to a target type.</param>
		/// <param name="converter">A <see cref="Converter{TInput, TOutput}" /> that converts each element from one type to another type.</param>
		/// <typeparam name="TInput">The type of the elements of the source array.</typeparam>
		/// <typeparam name="TOutput">The type of the elements of the target array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="converter" /> is null.</exception>
		public static TOutput[] ConvertAll<TInput, TOutput>([NotNull] this TInput[] array, [NotNull] Converter<TInput, TOutput> converter) => Array.ConvertAll(array, converter);

		#region Copy

		/// <summary>Copies a range of elements from an <see cref="Array" /> starting at the first element and pastes them into another <see cref="Array" /> starting at the first element. The length is specified as a 32-bit integer.</summary>
		/// <param name="sourceArray">The <see cref="Array" /> that contains the data to copy.</param>
		/// <param name="destinationArray">The <see cref="Array" /> that receives the data.</param>
		/// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sourceArray" /> is null.-or-<paramref name="destinationArray" /> is null.</exception>
		/// <exception cref="RankException">
		/// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
		/// <exception cref="ArrayTypeMismatchException">
		/// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
		/// <exception cref="InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="length" /> is less than zero.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="length" /> is greater than the number of elements in <paramref name="sourceArray" />.-or-<paramref name="length" /> is greater than the number of elements in <paramref name="destinationArray" />.</exception>
		/// <filterpriority>1</filterpriority>
		public static void Copy([NotNull] this Array sourceArray, Array destinationArray, int length) => Array.Copy(sourceArray, destinationArray, length);

		/// <summary>Copies a range of elements from an <see cref="Array" /> starting at the first element and pastes them into another <see cref="Array" /> starting at the first element. The length is specified as a 64-bit integer.</summary>
		/// <param name="sourceArray">The <see cref="Array" /> that contains the data to copy.</param>
		/// <param name="destinationArray">The <see cref="Array" /> that receives the data.</param>
		/// <param name="length">A 64-bit integer that represents the number of elements to copy. The integer must be between zero and <see cref="F:System.Int32.MaxValue" />, inclusive.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sourceArray" /> is null.-or-<paramref name="destinationArray" /> is null.</exception>
		/// <exception cref="RankException">
		/// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
		/// <exception cref="ArrayTypeMismatchException">
		/// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
		/// <exception cref="InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="length" /> is less than 0 or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="length" /> is greater than the number of elements in <paramref name="sourceArray" />.-or-<paramref name="length" /> is greater than the number of elements in <paramref name="destinationArray" />.</exception>
		/// <filterpriority>1</filterpriority>
		public static void Copy([NotNull] this Array sourceArray, Array destinationArray, long length) => Array.Copy(sourceArray, destinationArray, length);

		/// <summary>Copies a range of elements from an <see cref="Array" /> starting at the specified source index and pastes them to another <see cref="Array" /> starting at the specified destination index. The length and the indexes are specified as 32-bit integers.</summary>
		/// <param name="sourceArray">The <see cref="Array" /> that contains the data to copy.</param>
		/// <param name="sourceIndex">A 32-bit integer that represents the index in the <paramref name="sourceArray" /> at which copying begins.</param>
		/// <param name="destinationArray">The <see cref="Array" /> that receives the data.</param>
		/// <param name="destinationIndex">A 32-bit integer that represents the index in the <paramref name="destinationArray" /> at which storing begins.</param>
		/// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sourceArray" /> is null.-or-<paramref name="destinationArray" /> is null.</exception>
		/// <exception cref="RankException">
		/// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
		/// <exception cref="ArrayTypeMismatchException">
		/// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
		/// <exception cref="InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="sourceIndex" /> is less than the lower bound of the first dimension of <paramref name="sourceArray" />.-or-<paramref name="destinationIndex" /> is less than the lower bound of the first dimension of <paramref name="destinationArray" />.-or-<paramref name="length" /> is less than zero.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="length" /> is greater than the number of elements from <paramref name="sourceIndex" /> to the end of <paramref name="sourceArray" />.-or-<paramref name="length" /> is greater than the number of elements from <paramref name="destinationIndex" /> to the end of <paramref name="destinationArray" />.</exception>
		/// <filterpriority>1</filterpriority>
		public static void Copy([NotNull] this Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length) => Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

		/// <summary>Copies a range of elements from an <see cref="Array" /> starting at the specified source index and pastes them to another <see cref="Array" /> starting at the specified destination index. The length and the indexes are specified as 64-bit integers.</summary>
		/// <param name="sourceArray">The <see cref="Array" /> that contains the data to copy.</param>
		/// <param name="sourceIndex">A 64-bit integer that represents the index in the <paramref name="sourceArray" /> at which copying begins.</param>
		/// <param name="destinationArray">The <see cref="Array" /> that receives the data.</param>
		/// <param name="destinationIndex">A 64-bit integer that represents the index in the <paramref name="destinationArray" /> at which storing begins.</param>
		/// <param name="length">A 64-bit integer that represents the number of elements to copy. The integer must be between zero and <see cref="F:System.Int32.MaxValue" />, inclusive.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="sourceArray" /> is null.-or-<paramref name="destinationArray" /> is null.</exception>
		/// <exception cref="RankException">
		/// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> have different ranks.</exception>
		/// <exception cref="ArrayTypeMismatchException">
		/// <paramref name="sourceArray" /> and <paramref name="destinationArray" /> are of incompatible types.</exception>
		/// <exception cref="InvalidCastException">At least one element in <paramref name="sourceArray" /> cannot be cast to the type of <paramref name="destinationArray" />.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="sourceIndex" /> is outside the range of valid indexes for the <paramref name="sourceArray" />.-or-<paramref name="destinationIndex" /> is outside the range of valid indexes for the <paramref name="destinationArray" />.-or-<paramref name="length" /> is less than 0 or greater than <see cref="F:System.Int32.MaxValue" />.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="length" /> is greater than the number of elements from <paramref name="sourceIndex" /> to the end of <paramref name="sourceArray" />.-or-<paramref name="length" /> is greater than the number of elements from <paramref name="destinationIndex" /> to the end of <paramref name="destinationArray" />.</exception>
		/// <filterpriority>1</filterpriority>
		public static void Copy([NotNull] this Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length) => Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);

		#endregion Copy

		/// <summary>Determines whether the specified array contains elements that match the conditions defined by the specified predicate.</summary>
		/// <returns>true if <paramref name="array" /> contains one or more elements that match the conditions defined by the specified predicate; otherwise, false.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions of the elements to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		public static bool Exists<T>([NotNull] this T[] array, [NotNull] Predicate<T> match) => Array.Exists(array, match);

		/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the first occurrence within the entire <see cref="Array" />.</summary>
		/// <returns>The first element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions of the element to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		public static T Find<T>([NotNull] this T[] array, [NotNull] Predicate<T> match) => Array.Find(array, match);

		/// <summary>Retrieves all the elements that match the conditions defined by the specified predicate.</summary>
		/// <returns>An <see cref="Array" /> containing all the elements that match the conditions defined by the specified predicate, if found; otherwise, an empty <see cref="Array" />.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions of the elements to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		public static T[] FindAll<T>([NotNull] this T[] array, [NotNull] Predicate<T> match) => Array.FindAll(array, match);

		#region Find[Last]Index

		/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire <see cref="Array" />.</summary>
		/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions of the element to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		public static int FindIndex<T>([NotNull] this T[] array, [NotNull] Predicate<T> match) => Array.FindIndex(array, match);

		/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="Array" /> that extends from the specified index to the last element.</summary>
		/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="startIndex">The zero-based starting index of the search.</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions of the element to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
		public static int FindIndex<T>([NotNull] this T[] array, int startIndex, [NotNull] Predicate<T> match) => Array.FindIndex(array, startIndex, match);

		/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the <see cref="Array" /> that starts at the specified index and contains the specified number of elements.</summary>
		/// <returns>The zero-based index of the first occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="startIndex">The zero-based starting index of the search.</param>
		/// <param name="count">The number of elements in the section to search.</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions of the element to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.-or-<paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
		public static int FindIndex<T>([NotNull] this T[] array, int startIndex, int count, [NotNull] Predicate<T> match) => Array.FindIndex(array, startIndex, count, match);

		/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire <see cref="Array" />.</summary>
		/// <returns>The last element that matches the conditions defined by the specified predicate, if found; otherwise, the default value for type <paramref name="T" />.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions of the element to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		public static T FindLast<T>([NotNull] this T[] array, [NotNull] Predicate<T> match) => Array.FindLast(array, match);

		/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire <see cref="Array" />.</summary>
		/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions of the element to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		public static int FindLastIndex<T>([NotNull] this T[] array, [NotNull] Predicate<T> match) => Array.FindLastIndex(array, match);

		/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="Array" /> that extends from the first element to the specified index.</summary>
		/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="startIndex">The zero-based starting index of the backward search.</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions of the element to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
		public static int FindLastIndex<T>([NotNull] this T[] array, int startIndex, [NotNull] Predicate<T> match) => Array.FindLastIndex(array, startIndex, match);

		/// <summary>Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the <see cref="Array" /> that contains the specified number of elements and ends at the specified index.</summary>
		/// <returns>The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="match" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="startIndex">The zero-based starting index of the backward search.</param>
		/// <param name="count">The number of elements in the section to search.</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions of the element to search for.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.-or-<paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
		public static int FindLastIndex<T>([NotNull] this T[] array, int startIndex, int count, [NotNull] Predicate<T> match) => Array.FindLastIndex(array, startIndex, count, match);

		#endregion Find[Last]Index

		/// <summary>Performs the specified action on each element of the specified array.</summary>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> on whose elements the action is to be performed.</param>
		/// <param name="action">The <see cref="Action{T}" /> to perform on each element of <paramref name="array" />.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="action" /> is null.</exception>
		public static void ForEach<T>([NotNull] this T[] array, [NotNull] Action<T> action) => Array.ForEach(array, action);

		#region [Last]IndexOf

		/// <summary>Searches for the specified object and returns the index of the first occurrence within the entire <see cref="Array" />.</summary>
		/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the entire <paramref name="array" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="value">The object to locate in <paramref name="array" />.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		public static int IndexOf<T>([NotNull] this T[] array, T value) => Array.IndexOf(array, value);

		/// <summary>Searches for the specified object and returns the index of the first occurrence within the range of elements in the <see cref="Array" /> that extends from the specified index to the last element.</summary>
		/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that extends from <paramref name="startIndex" /> to the last element, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="value">The object to locate in <paramref name="array" />.</param>
		/// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty array.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
		public static int IndexOf<T>([NotNull] this T[] array, T value, int startIndex) => Array.IndexOf(array, value, startIndex);

		/// <summary>Searches for the specified object and returns the index of the first occurrence within the range of elements in the <see cref="Array" /> that starts at the specified index and contains the specified number of elements.</summary>
		/// <returns>The zero-based index of the first occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that starts at <paramref name="startIndex" /> and contains the number of elements specified in <paramref name="count" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="value">The object to locate in <paramref name="array" />.</param>
		/// <param name="startIndex">The zero-based starting index of the search. 0 (zero) is valid in an empty array.</param>
		/// <param name="count">The number of elements in the section to search.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.-or-<paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
		public static int IndexOf<T>([NotNull] this T[] array, T value, int startIndex, int count) => Array.IndexOf(array, value, startIndex, count);

		/// <summary>Searches for the specified object and returns the index of the last occurrence within the entire <see cref="Array" />.</summary>
		/// <returns>The zero-based index of the last occurrence of <paramref name="value" /> within the entire <paramref name="array" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="value">The object to locate in <paramref name="array" />.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		public static int LastIndexOf<T>([NotNull] this T[] array, T value) => Array.LastIndexOf(array, value);

		/// <summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="Array" /> that extends from the first element to the specified index.</summary>
		/// <returns>The zero-based index of the last occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that extends from the first element to <paramref name="startIndex" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="value">The object to locate in <paramref name="array" />.</param>
		/// <param name="startIndex">The zero-based starting index of the backward search.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.</exception>
		public static int LastIndexOf<T>([NotNull] this T[] array, T value, int startIndex) => Array.LastIndexOf(array, value, startIndex);

		/// <summary>Searches for the specified object and returns the index of the last occurrence within the range of elements in the <see cref="Array" /> that contains the specified number of elements and ends at the specified index.</summary>
		/// <returns>The zero-based index of the last occurrence of <paramref name="value" /> within the range of elements in <paramref name="array" /> that contains the number of elements specified in <paramref name="count" /> and ends at <paramref name="startIndex" />, if found; otherwise, –1.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to search.</param>
		/// <param name="value">The object to locate in <paramref name="array" />.</param>
		/// <param name="startIndex">The zero-based starting index of the backward search.</param>
		/// <param name="count">The number of elements in the section to search.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="startIndex" /> is outside the range of valid indexes for <paramref name="array" />.-or-<paramref name="count" /> is less than zero.-or-<paramref name="startIndex" /> and <paramref name="count" /> do not specify a valid section in <paramref name="array" />.</exception>
		public static int LastIndexOf<T>([NotNull] this T[] array, T value, int startIndex, int count) => Array.LastIndexOf(array, value, startIndex, count);

		#endregion [Last]IndexOf

		/// <summary>Reverses the sequence of the elements in the entire one-dimensional <see cref="Array" />.</summary>
		/// <param name="array">The one-dimensional <see cref="Array" /> to reverse.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null. </exception>
		/// <exception cref="RankException">
		/// <paramref name="array" /> is multidimensional. </exception>
		/// <filterpriority>1</filterpriority>
		public static void Reverse([NotNull] this Array array) => Array.Reverse(array);

		/// <summary>Reverses the sequence of the elements in a range of elements in the one-dimensional <see cref="Array" />.</summary>
		/// <param name="array">The one-dimensional <see cref="Array" /> to reverse.</param>
		/// <param name="index">The starting index of the section to reverse.</param>
		/// <param name="length">The number of elements in the section to reverse.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="RankException">
		/// <paramref name="array" /> is multidimensional.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.</exception>
		/// <filterpriority>1</filterpriority>
		public static void Reverse([NotNull] this Array array, int index, int length) => Array.Reverse(array, index, length);

		#region Sort

		/// <summary>Sorts the elements in an entire <see cref="Array" /> using the <see cref="IComparable{T}" /> generic interface implementation of each element of the <see cref="Array" />.</summary>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to sort.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="InvalidOperationException">One or more elements in <paramref name="array" /> do not implement the <see cref="IComparable{T}" /> generic interface.</exception>
		public static void Sort<T>([NotNull] this T[] array) => Array.Sort(array);

		/// <summary>Sorts the elements in an <see cref="Array" /> using the specified <see cref="IComparer{T}" /> generic interface.</summary>
		/// <param name="array">The one-dimensional, zero-base <see cref="Array" /> to sort</param>
		/// <param name="comparer">The <see cref="IComparer{T}" /> generic interface implementation to use when comparing elements, or null to use the <see cref="IComparable{T}" /> generic interface implementation of each element.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="comparer" /> is null, and one or more elements in <paramref name="array" /> do not implement the <see cref="IComparable{T}" /> generic interface.</exception>
		/// <exception cref="ArgumentException">The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
		public static void Sort<T>([NotNull] this T[] array, IComparer<T> comparer) => Array.Sort(array, comparer);

		/// <summary>Sorts the elements in an <see cref="Array" /> using the specified <see cref="Comparison{T}" />.</summary>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to sort</param>
		/// <param name="comparison">The <see cref="Comparison{T}" /> to use when comparing elements.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="comparison" /> is null.</exception>
		/// <exception cref="ArgumentException">The implementation of <paramref name="comparison" /> caused an error during the sort. For example, <paramref name="comparison" /> might not return 0 when comparing an item with itself.</exception>
		public static void Sort<T>([NotNull] this T[] array, [NotNull] Comparison<T> comparison) => Array.Sort(array, comparison);

		/// <summary>Sorts the elements in a range of elements in an <see cref="Array" /> using the <see cref="IComparable{T}" /> generic interface implementation of each element of the <see cref="Array" />.</summary>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to sort</param>
		/// <param name="index">The starting index of the range to sort.</param>
		/// <param name="length">The number of elements in the range to sort.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />.</exception>
		/// <exception cref="InvalidOperationException">One or more elements in <paramref name="array" /> do not implement the <see cref="IComparable{T}" /> generic interface.</exception>
		public static void Sort<T>([NotNull] this T[] array, int index, int length) => Array.Sort(array, index, length);

		/// <summary>Sorts the elements in a range of elements in an <see cref="Array" /> using the specified <see cref="IComparer{T}" /> generic interface.</summary>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to sort.</param>
		/// <param name="index">The starting index of the range to sort.</param>
		/// <param name="length">The number of elements in the range to sort.</param>
		/// <param name="comparer">The <see cref="IComparer{T}" /> generic interface implementation to use when comparing elements, or null to use the <see cref="IComparable{T}" /> generic interface implementation of each element.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index" /> is less than the lower bound of <paramref name="array" />.-or-<paramref name="length" /> is less than zero.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="index" /> and <paramref name="length" /> do not specify a valid range in <paramref name="array" />. -or-The implementation of <paramref name="comparer" /> caused an error during the sort. For example, <paramref name="comparer" /> might not return 0 when comparing an item with itself.</exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="comparer" /> is null, and one or more elements in <paramref name="array" /> do not implement the <see cref="IComparable{T}" /> generic interface.</exception>
		public static void Sort<T>([NotNull] this T[] array, int index, int length, IComparer<T> comparer) => Array.Sort(array, index, length, comparer);

		#endregion Sort

		/// <summary>Determines whether every element in the array matches the conditions defined by the specified predicate.</summary>
		/// <returns>true if every element in <paramref name="array" /> matches the conditions defined by the specified predicate; otherwise, false. If there are no elements in the array, the return value is true.</returns>
		/// <param name="array">The one-dimensional, zero-based <see cref="Array" /> to check against the conditions</param>
		/// <param name="match">The <see cref="Predicate{T}" /> that defines the conditions to check against the elements.</param>
		/// <typeparam name="T">The type of the elements of the array.</typeparam>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array" /> is null.-or-<paramref name="match" /> is null.</exception>
		public static bool TrueForAll<T>([NotNull] this T[] array, [NotNull] Predicate<T> match) => Array.TrueForAll(array, match);
	}
}
