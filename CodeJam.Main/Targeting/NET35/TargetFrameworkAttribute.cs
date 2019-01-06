// Once Theraot v3 is be released, this file can be removed.

#if LESSTHAN_NET40
// ReSharper disable all
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE0044 // Make field readonly
// BASEDON: https://github.com/dotnet/corefx/blob/a10890f4ffe0fadf090c922578ba0e606ebdd16c/src/Common/src/CoreLib/System/Runtime/Versioning/TargetFrameworkAttribute.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: Identifies which SKU and version of the .NET
**   Framework that a particular library was compiled against.
**   Emitted by VS, and can help catch deployment problems.
**
===========================================================*/

using System;

namespace System.Runtime.Versioning
{
	[AttributeUsageAttribute(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class TargetFrameworkAttribute : Attribute
	{
		private string _frameworkName;  // A target framework moniker
		private string _frameworkDisplayName;

		// The frameworkName parameter is intended to be the string form of a FrameworkName instance.
		public TargetFrameworkAttribute(string frameworkName)
		{
			if (frameworkName == null)
				throw new ArgumentNullException(nameof(frameworkName));
			_frameworkName = frameworkName;
		}

		// The target framework moniker that this assembly was compiled against.
		// Use the FrameworkName class to interpret target framework monikers.
		public string FrameworkName
		{
			get { return _frameworkName; }
		}

		public string FrameworkDisplayName
		{
			get { return _frameworkDisplayName; }
			set { _frameworkDisplayName = value; }
		}
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#endif