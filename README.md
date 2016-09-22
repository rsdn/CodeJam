## CodeJam

CodeJam is a set of handy reusable .NET components that can simplify your daily work and
save your time when you copy and paste your favorite helper methods and classes from one project to another.

The CodeJam project includes two parts:

* Main - stable and feature complete code.
* Experimental - A workspace for code development. Contains incomplete and/or unstable parts.

### Main
CodeJam is dedicated to one principal goal: creating and maintaining reusable code. The CodeJam.Main is a place for
collaboration and sharing, where developers community can work together on code to be shared by the CodeJam.

CodeJam developers will make an effort to ensure that their components have no dependencies on other third-party
libraries, so that these components can be deployed easily. In addition, CodeJam will keep their interfaces as stable as
possible, so that users can use it without having to worry about changes in the future.

We welcome participation from all who are interested, at all skill levels. Coding, documenting, testing and
development process itself are all critical parts of the software development process. If you are interested in
contribute in any of these aspects, please join us!

### .NET Framework 3.5 support
.NET Framework 3.5 support is limited. Functionality not supported:

* Mapping - all mapping related classes not supported due to expression tree incompatibility
* ExpressionExtensions - all visitor related functionality excluded due to expression tree incompatibility
* MemberAccessor/TypeAccessor - excluded due to expression tree incompatibility
* ComparerBuilder - excluded due to absence of TypeAccessor and MemberAccessor classes

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
 - [Forum (russian)](http://rsdn.ru/forum/prj.codejam/)
 - [![Join the chat at https://gitter.im/rsdn/CodeJam](https://badges.gitter.im/rsdn/CodeJam.svg)](https://gitter.im/rsdn/CodeJam?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
