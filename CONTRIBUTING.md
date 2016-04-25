# [PROPOSAL] CodeJam contributing guidelines
Please do not remove [PROPOSAL] modifier until the proposal is complete.
Until this these guidelines are not mandatory.

## Basics
### _Discussions_
Currently the entire CodeJam team are native Russian speakers so almost all discussions are located on [RSDN project forum](http://rsdn.ru/forum/prj.codejam/).

For English speakers: please, use the [GitHub issues](https://github.com/rsdn/CodeJam/issues). 

### _Project artefacts (member naming, documentation, code comments, issues, wiki etc.)_
All in English.

### _Correction of the guidelines_
Feel free to fix all typos, misphrases, obvious errors in text and so on.
However, please take time to open an issue before making more significant changes. Thanks!

### _What can you contribute?_
We are going to accept almost anything until it fits into description of the project:
>CodeJam is a set of handy reusable .NET components that can simplify your daily work and save your time when you copy and paste your favorite helper methods and classes from one project to another.

Some limitations do apply, however.

* The quality bar. The CodeJam project aims to provide the same level of good API design and same usabilty that the  .Net Framework does. This means that all new additions will be tuned to match common conventions. We trying to keep behavior in line with existing APIs, we often do change the names of the proposed APIs if there's better naming and we're trying to follow the framework design guidelines until there's no explicit reason to violate it. Be prepared:)
 All changes are disputable. So, if you want to keep the original design unmodified your arguments are welcome!
 
* No third-party dependencies if possible. We're trying to keep the library small and easy to plug in.

* Specialized and platform-dependent code should be added into separate packages such as [CodeJam.Extensibility](https://github.com/rsdn/CodeJam.Extensibility)  or [CodeJam.Web](https://github.com/rsdn/CodeJam.Web).

## Tooling support
**The project should support clone, restore packages and build routine.**
However, there are some third-party tools assumed to be used by the contributors.

### _Tooling support (ReSharper)_

We include ReSharper (8.0 and higher) settings file (`.\CodeJam.sln.DotSettings`) to make it easier to follow the guidelines.

1. If you have ReSharper installed, please try to fix all code issues with severity higher than **Hint** before pushing the changes.
1. If using the ReSharper is not suitable for you, consider to use the [ReSharper Command Line Tools](https://www.jetbrains.com/resharper/features/command-line.html).
1. If ReSharper recommendation is **obviously** wrong, please suppress it using the "Disable once with comment" command.
1. In all other cases please try to follow the ReSharper recommendations. This helps to maintain the same code style over all code files in the project. Thank you in advance for understanding.
1. If you think that some recommendation is obviously wrong and should be changed, please file an issue first. 

#### _ReSharper extensions_
Please, use the free [ReSpeller extension](https://resharper-plugins.jetbrains.com/packages/ReSpeller/) (installed via ReSharper's extension manager). All words that are not in team-shared dictionary should be added to it (Add custom word to user dictionary > Save To > Solution team-shared).

As an alternative there's [VS Spell Checker extension](https://visualstudiogallery.msdn.microsoft.com/a23de100-31a1-405c-b4b7-d6be40c3dfff) however it doesn't check typos in code and we do not maintain user dictionary for it. 

### _Tooling support (NUnit)_
TBD

### _Tooling support (Git)_
TBD

### _Tooling support (Markdown files)_
You can use any editor or extension (such as [Markdown Mode](https://visualstudiogallery.msdn.microsoft.com/0855e23e-4c4c-4c82-8b39-24ab5c5a7f79) or [Web Extensions](https://visualstudiogallery.msdn.microsoft.com/ee6e6d8c-c837-41fb-886a-6b50ae2d06a2)) to edit .md files.

However, please do not use features that are not supported by [GitHub Flavored Markdown](https://guides.github.com/features/mastering-markdown/) format.

## Coding conventions (baseline)
The coding conventions are based on the guidelines from a [.Net core project](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md).
With a few exceptions (most notable one - use **tabs**, not spaces) these should be compatible with each other.

### _Coding conventions (as in .Net core project)_
The rules that differs highlighted in bold.

 **TODO:** Valid authorship/copyright on this section? The following rules based on text from [here](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md) and we are not going to hide the origin:)

1. We use [Allman style](http://en.wikipedia.org/wiki/Indent_style#Allman_style) braces, where each brace begins on a new line. **Single line blocks should not use braces.**
2. **Use tabs** for indentation.
3. We use `_camelCase` for internal and private fields and use `readonly` where possible. **Prefix the fields with `_`**. When used on static fields, `readonly` should come after `static` (i.e. `static readonly` not `readonly static`).
4. We avoid `this.` unless absolutely necessary. 
5. We always specify the visibility, even if it's the default (i.e.
   `private string _foo` not `string _foo`). Visibility should be the first modifier (i.e. 
   `public abstract` not `abstract public`).
6. Namespace imports should be specified at the top of the file, *outside* of
   `namespace` declarations and should be sorted alphabetically.
7. Avoid more than one empty line at any time. For example, do not have two
   blank lines between members of a type.
8. Avoid spurious free spaces.
   For example avoid `if (someVar == 0)...`, where the dots mark the spurious free spaces.
   Consider enabling "View White Space (Ctrl+E, S)" if using Visual Studio, to aid detection.
9. **~~(leave existing style if no reasons to change)~~**. Does not apply.
10. **Use var when possible.**
11. We use language keywords instead of BCL types (i.e. `int, string, float` instead of `Int32, String, Single`, etc.) for both type references as well as method calls (i.e. `int.Parse` instead of `Int32.Parse`).
12. We use **PascalCasing to name constant fields, camelCasing to name constant local variables**. The only exception is for interop code where the constant value should exactly match the name and value of the code you are calling via interop.
13. We use ```nameof(...)``` instead of ```"..."``` whenever possible and relevant.

### _Coding conventions (additional rules)_
TBD
