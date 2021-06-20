﻿#if TARGETS_NET || LESSTHAN_NETSTANDARD21
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using CodeJam;

using JetBrains.Annotations;

using MethodImplOptionsEx = CodeJam.Targeting.MethodImplOptionsEx;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>Represent a type can be used to index a collection either from the start or the end.</summary>
    /// <remarks>
    /// Index is used by the C# compiler to support the new index syntax
    /// <code>
    /// int[] someArray = new int[5] { 1, 2, 3, 4, 5 } ;
    /// int lastElement = someArray[^1]; // lastElement = 5
    /// </code>
    /// </remarks>
    [PublicAPI]
    internal readonly struct Index : IEquatable<Index>
    {
        private readonly int _value;

        /// <summary>Construct an Index using a value and indicating if the index is from the start or from the end.</summary>
        /// <param name="value">The index value. it has to be zero or positive number.</param>
        /// <param name="fromEnd">Indicating if the index is from the start or from the end.</param>
        /// <remarks>
        /// If the Index constructed from the end, index value 1 means pointing at the last element and index value 0 means pointing at beyond last element.
        /// </remarks>
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public Index(int value, bool fromEnd = false)
        {
            if (value < 0)
            {
	            throw CodeExceptions.ArgumentNegative(nameof(value));
            }

            if (fromEnd)
                _value = ~value;
            else
                _value = value;
        }

        // The following private constructors mainly created for perf reason to avoid the checks
        private Index(int value) => _value = value;

        /// <summary>Create an Index pointing at first element.</summary>
        public static Index Start => new(0);

        /// <summary>Create an Index pointing at beyond last element.</summary>
        public static Index End => new(~0);

        /// <summary>Create an Index from the start at the position indicated by the value.</summary>
        /// <param name="value">The index value from the start.</param>
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Index FromStart(int value)
        {
            if (value < 0)
            {
	            throw CodeExceptions.ArgumentNegative(nameof(value));
            }

            return new Index(value);
        }

        /// <summary>Create an Index from the end at the position indicated by the value.</summary>
        /// <param name="value">The index value from the end.</param>
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static Index FromEnd(int value)
        {
            if (value < 0)
            {
	            throw CodeExceptions.ArgumentNegative(nameof(value));
            }

            return new Index(~value);
        }

        /// <summary>Returns the index value.</summary>
        public int Value => _value < 0 ? ~_value : _value;

	    /// <summary>Indicates whether the index is from the start or the end.</summary>
        public bool IsFromEnd => _value < 0;

        /// <summary>Calculate the offset from the start using the giving collection length.</summary>
        /// <param name="length">The length of the collection that the Index will be used with. length has to be a positive value</param>
        /// <remarks>
        /// For performance reason, we don't validate the input length parameter and the returned offset value against negative values.
        /// we don't validate either the returned offset is greater than the input length.
        /// It is expected Index will be used with collections which always have non negative length/count. If the returned offset is negative and
        /// then used to index a collection will get out of range exception which will be same affect as the validation.
        /// </remarks>
        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public int GetOffset(int length)
        {
            var offset = _value;
            if (IsFromEnd)
            {
                // offset = length - (~value)
                // offset = length + (~(~value) + 1)
                // offset = length + value + 1

                offset += length + 1;
            }
            return offset;
        }

        /// <summary>Indicates whether the current Index object is equal to another object of the same type.</summary>
        /// <param name="obj">An object to compare with this object</param>
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is Index index && _value == index._value;

        /// <summary>Indicates whether the current Index object is equal to another Index object.</summary>
        /// <param name="other">An object to compare with this object</param>
        public bool Equals(Index other) => _value == other._value;

        /// <summary>Returns the hash code for this instance.</summary>
        public override int GetHashCode() => _value;

        /// <summary>Converts integer number to an Index.</summary>
        public static implicit operator Index(int value) => FromStart(value);

        /// <summary>Converts the value of the current Index object to its equivalent string representation.</summary>
        public override string ToString()
        {
            if (IsFromEnd)
                return ToStringFromEnd();

            return ((uint)Value).ToString();
        }

        private string ToStringFromEnd()
        {
#if (!NETSTANDARD2_0 && !TARGETS_NET)
            Span<char> span = stackalloc char[11]; // 1 for ^ and 10 for longest possible uint value
	          // ReSharper disable once RedundantAssignment
            bool formatted = ((uint)Value).TryFormat(span[1..], out int charsWritten);
            Debug.Assert(formatted);
            span[0] = '^';
            return new string(span[..(charsWritten + 1)]);
#else
            return '^' + Value.ToString();
#endif
        }
    }
}
#endif