![CodeJam.Main.Icon](/Main/nuget/CodeJam.Main.Icon.png)

##  CodeJam

CodeJam is a set of handy reusable .NET components that can simplify your daily work and
save your time when you copy and paste your favorite helper methods and classes from one project to another.

The CodeJam project includes three parts:

* Main - stable and feature complete code.
* Blocks - app building blocks, more specific and feature oriented functionality.
* Experimental - A workspace for code development. Contains incomplete and/or unstable parts.
* [CodeJam.PerfTests](https://github.com/rsdn/CodeJam/tree/master/PerfTests) - a performance testing framework for .Net projects.

### Main
CodeJam is dedicated to one principal goal: creating and maintaining reusable code. The CodeJam main package is a place for
collaboration and sharing, where developers community can work together on code to be shared by the CodeJam.

CodeJam developers will make an effort to ensure that their components have no dependencies on other third-party
libraries, so that these components can be deployed easily. In addition, CodeJam will keep their interfaces as stable as
possible, so that users can use it without having to worry about changes in the future.

We welcome participation from all who are interested, at all skill levels. Coding, documenting, testing and
development process itself are all critical parts of the software development process. If you are interested in
contribute in any of these aspects, please join us!

### Blocks

### PerfTests
CodeJam.PerfTests is performance testing framework for .Net projects.

It allows to compare multiple implementations by execution time (*~memory limits coming soon~*), to annotate test methods
with timing limits and to check the limits each time the test is run.

### .NET Framework 3.5 support
.NET Framework 3.5 support is limited. Functionality not supported:

* Mapping - all mapping related classes not supported due to expression tree incompatibility
* ExpressionExtensions - all visitor related functionality excluded due to expression tree incompatibility
* MemberAccessor/TypeAccessor - excluded due to expression tree incompatibility
* ComparerBuilder - excluded due to absence of TypeAccessor and MemberAccessor classes
* Enumerable.ToDiagnosticString - excluded due to absence of TypeAccessor class
* PerfTests package not support FW 3.5 at all.

### Contribute!
Some of the best ways to contribute are to try things out, report bugs, and join in design conversations.

* [How to Contribute](https://github.com/rsdn/CodeJam/blob/master/CONTRIBUTING.md)

Looking for something to work on? The list of
[up for grabs issues](https://github.com/rsdn/CodeJam/issues?q=is%3Aopen+is%3Aissue) is a great place to start.

### Download
Just install CodeJam nuget package via Visual Studio Package Manager and use it!

To install the latest release without Visual Studio, run [nuget](https://dist.nuget.org/index.html)
command line:


```
nuget install CodeJam
```

To get the latest "preview" drop, add the `-pre` switch to the nuget commands

### Links
- [Class library documentation](https://github.com/rsdn/CodeJam/wiki/DocHome)
- Continious integration build [![Build status](https://ci.appveyor.com/api/projects/status/oxdyxkgwotiv64r1/branch/master?svg=true)](https://ci.appveyor.com/project/andrewvk/codejam)
- Docs continious integration build [![Docs build status](https://ci.appveyor.com/api/projects/status/bucrjn2eceptbqwl?svg=true)](https://ci.appveyor.com/project/andrewvk/codejam-jlvna)
- [Continiuos Integration builds Nuget feed](https://ci.appveyor.com/nuget/codejam)
- [Forum (russian)](https://rsdn.org/forum/prj.codejam/)
- [![Join the chat at https://gitter.im/rsdn/CodeJam](https://badges.gitter.im/rsdn/CodeJam.svg)](https://gitter.im/rsdn/CodeJam?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### Licensing & notices

The CodeJam project is free software.
There is no warranty; not even for merchantability or fitness for a particular purpose.

You may use, copy, modify and redistribute all files included in this
distribution, individually or in aggregate, subject to the terms and conditions
of the MIT license.  See the file `LICENSE` for details.


The project includes code parts taken from (mostly to ease targeting to previous versions of .Net)
or inspired by third-party implementations.

All such places are marked with `// BASEDON: ` comment. Here they are:

- [`CodeExceptions`](https://github.com/rsdn/CodeJam/tree/master/Main/src/Assertions/CodeExceptions.cs#L55): trace source design follows style introduced by the `System.Diagnostics.PresentationTraceSources`.
- [`NaturalOrderStringComparer`](https://github.com/rsdn/CodeJam/tree/master/Main/src/Strings/NaturalOrderStringComparer.cs): based on [the C version by Martin Pool](http://sourcefrog.net/projects/natsort/)
- Types that enables targeting to [.Net 3.5](https://github.com/rsdn/CodeJam/tree/master/Main/src/Targeting/FW35) and [.Net 4.0](https://github.com/rsdn/CodeJam/tree/master/Main/src/Targeting/FW40), all taken from [CoreFx ](https://github.com/dotnet/corefx)and [CoreClr](https://github.com/dotnet/coreclr/) projects.
- [`InterlockedOperations`](https://github.com/rsdn/CodeJam/tree/master/Main/src/Threading/InterlockedOperations.tt) uses CAS loop undercover, reference to the Roslyn implementation given as a proof the code is correct.


In addition, you may -- at your option -- use, copy, modify and redistribute
all images included in this distribution under the directory named `images`
according to the terms and conditions of the Creative Commons Attribution-ShareAlike 4.0 International License.
Use following text as a template for attribution:

```
CodeJam logo (c) by Arthur Kozyrev

CodeJam logo is licensed under a
Creative Commons Attribution-ShareAlike 4.0 International License.

You should have received a copy of the license along with this
work. If not, see <http://creativecommons.org/licenses/by-sa/4.0/>
```

See the file `LICENSE-CC-BY-SA` for details.