# CodeJam

![CodeJam.Main.Icon](/images/nuget/CodeJam.Main.Icon.png)

[![NuGet](https://img.shields.io/nuget/v/CodeJam.svg)](https://www.nuget.org/packages/CodeJam/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/CodeJam.svg)](https://www.nuget.org/packages/CodeJam/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/oxdyxkgwotiv64r1/branch/master?svg=true)](https://ci.appveyor.com/project/andrewvk/codejam/branch/master)

**A collection of reusable .NET components that simplify everyday development tasks.**

CodeJam eliminates the need to copy helper methods and utility classes between projects. It provides battle-tested, production-ready code that saves time and ensures consistency across your codebase.

## Features

- **Main** — Stable, feature-complete utilities for production use
- **Blocks** — Application building blocks with specialized functionality
- **Experimental** — A sandbox for developing new features (may be unstable)

## Installation

Install via NuGet:

```bash
dotnet add package CodeJam
```

Or via the NuGet Package Manager Console:

```powershell
Install-Package CodeJam
```

For preview releases:

```bash
dotnet add package CodeJam --prerelease
```

## Documentation

- **[Class Library Documentation](https://github.com/rsdn/CodeJam/wiki/DocHome)** — Full API reference and guides
- **[Main Library README](CodeJam.Main/Readme.md)** — Details on the main package components

## Supported Platforms

CodeJam targets modern .NET platforms:

| Platform | Status |
|----------|--------|
| .NET 6.0+ | ✅ Full support |
| .NET Framework 4.6.1+ | ✅ Full support |
| .NET Standard 2.0+ | ✅ Full support |

> **Note:** Support for .NET Framework 2.0–3.5, .NET Core 1.x, and .NET Standard 1.x was discontinued in version 4.0.

## Contributing

We welcome contributions! Here's how to get started:

1. Check out the [Contributing Guide](CONTRIBUTING.md)
2. Browse [up-for-grabs issues](https://github.com/rsdn/CodeJam/issues?q=is%3Aopen+is%3Aissue) for good first tasks
3. Join the discussion in the [Forum (Russian)](https://rsdn.org/forum/prj.codejam/)

## Links

- [CI NuGet Feed](https://ci.appveyor.com/nuget/codejam) — Latest CI builds
- [Docs CI Build](https://ci.appveyor.com/project/andrewvk/codejam-jlvna/branch/master) — Documentation build status

## License

CodeJam is released under the [MIT License](LICENSE).

The project includes code adapted from third-party sources, marked with `// BASEDON:` comments. Notable attributions:

- **CodeExceptions** — Trace source design inspired by `System.Diagnostics.PresentationTraceSources`
- **NaturalOrderStringComparer** — Based on [Martin Pool's C implementation](http://sourcefrog.net/projects/natsort/)
- **Targeting types** — Adapted from [CoreFx](https://github.com/dotnet/corefx) and [CoreClr](https://github.com/dotnet/coreclr/)

### Logo License

The CodeJam logo by Arthur Kozyrev is licensed under [Creative Commons Attribution-ShareAlike 4.0 International](http://creativecommons.org/licenses/by-sa/4.0/). See `LOGO-CC-BY-SA` for details.