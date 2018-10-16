#if TARGETS_NET && LESSTHAN_NET35
// BASEDON: https://github.com/dotnet/coreclr/blob/76c62b72ef2642c3ad91209acf02db6c8b42aff7/src/mscorlib/src/System/Runtime/CompilerServices/ExtensionAttribute.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Indicates that a method is an extension method, or that a class or assembly contains extension methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class ExtensionAttribute : Attribute { }
}
#endif