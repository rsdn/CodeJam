# Contributing to CodeJam

Thanks for your interest in contributing to CodeJam! This document provides guidelines and instructions for contributing.

## Discussions

The CodeJam team consists primarily of Russian speakers. Most discussions take place on the [RSDN project forum](http://rsdn.ru/forum/prj.codejam/).

For English speakers, please use [GitHub issues](https://github.com/rsdn/CodeJam/issues) for discussions.

## Project Artifacts

All project artifacts—including member naming, documentation, code comments, issues, and wiki content—should be in English.

## Improving the Guidelines

Feel free to fix typos, phrasing errors, or obvious mistakes in the documentation. For more significant changes, please open an issue first to discuss your proposed changes.

## What Can You Contribute?

We welcome contributions that align with CodeJam's mission:

> CodeJam is a collection of reusable .NET components that simplify everyday development tasks by eliminating the need to copy helper methods and utility classes between projects.

### Guidelines

- **Quality bar** — CodeJam aims to provide the same level of API design and usability as the .NET Framework. New additions will be reviewed for consistency with existing conventions. We may suggest naming changes or adjustments to follow framework design guidelines. Be prepared for discussion!

- **No third-party dependencies** — We keep the library lightweight and easy to integrate.

- **Specialized code** — Platform-dependent or specialized functionality should go into separate packages such as [CodeJam.Extensibility](https://github.com/rsdn/CodeJam.Extensibility) or [CodeJam.Web](https://github.com/rsdn/CodeJam.Web).

## Development Setup

### Prerequisites

The project supports standard clone, restore, and build workflows. Ensure you have the following:

- [.NET SDK](https://dotnet.microsoft.com/download) (latest version recommended)
- [Visual Studio](https://visualstudio.microsoft.com/) 2022 or later (optional but recommended)

### ReSharper Integration

We include ReSharper settings (`.CodeJam.sln.DotSettings`) to help maintain consistent code style.

1. If you have ReSharper installed, fix all code issues with severity higher than **Hint** before submitting changes.
2. If you don't use ReSharper, consider using [ReSharper Command Line Tools](https://www.jetbrains.com/resharper/features/command-line.html).
3. If a ReSharper recommendation is **obviously** incorrect, suppress it using "Disable once with comment."
4. For concerns about team-wide settings, please file an issue first.

#### ReSharper Extensions

We recommend the free [ReSpeller extension](https://resharper-plugins.jetbrains.com/packages/ReSpeller/) for spell checking. Add unrecognized words to the solution team-shared dictionary via **Add custom word to user dictionary > Save To > Solution team-shared**.

Alternatively, you can use the [VS Spell Checker extension](https://visualstudiogallery.msdn.microsoft.com/a23de100-31a1-405c-b4b7-d6be40c3dfff), though it doesn't check typos in code.

### Markdown Files

Use any editor or extension to edit `.md` files. Please ensure your changes are compatible with [GitHub Flavored Markdown](https://guides.github.com/features/mastering-markdown/).

## Coding Conventions

Our coding conventions are based on the [.NET Core project guidelines](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md) with a few exceptions.

### Key Rules

1. **Braces** — Use [Allman style](http://en.wikipedia.org/wiki/Indent_style#Allman_style) braces. Each brace begins on a new line. Single-line blocks should not use braces.

2. **Indentation** — Use **tabs**, not spaces.

3. **Fields** — Use `_camelCase` for internal and private fields. Prefix with `_`. Use `readonly` where possible. For static fields, `readonly` should come after `static` (e.g., `static readonly`).

4. **`this.` keyword** — Avoid unless absolutely necessary.

5. **Visibility** — Always specify visibility explicitly (e.g., `private string _foo`, not `string _foo`). Visibility should be the first modifier (e.g., `public abstract`, not `abstract public`).

6. **Using directives** — Place at the top of the file, outside of `namespace` declarations. Sort alphabetically.

7. **Blank lines** — Avoid more than one consecutive blank line.

8. **Trailing spaces** — Avoid spurious trailing spaces. Enable "View White Space" (Ctrl+E, S) in Visual Studio to detect them.

9. **`var` keyword** — Use `var` when the type is obvious.

10. **Language keywords** — Use language keywords instead of BCL types (e.g., `int`, `string`, `float` instead of `Int32`, `String`, `Single`).

11. **Constants** — Use **PascalCasing** for constant fields and **camelCasing** for local constant variables. The only exception is interop code where the name must match exactly.

12. **`nameof()`** — Use `nameof(...)` instead of string literals whenever possible.

## Questions?

If you have questions, feel free to:

- Open a [GitHub issue](https://github.com/rsdn/CodeJam/issues)
- Visit the [RSDN forum](http://rsdn.ru/forum/prj.codejam/) (Russian)