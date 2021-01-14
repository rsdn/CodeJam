# Assertions

Methods are designed to check the arguments and state.

## Main assertions

Placed in the Code and DebugCode classes. The set of the class methods is identical, but the latter contains
conditional compilation directives that throw checks in the Release configuration.

Some methods have overloads for different types. This is done to improve performance.

```C#
public void Foo(string str, int i, int? ni, string[] collection)
{
    // Simple argument
    Code.NotNull(str, nameof (str));
    Code.NotNull(ni, nameof (ni));
    Code.AssertArgument(i > 5, nameof (i));

    // Collections
    Code.ItemNotNull(collection, nameof (collection));
    Code.NotNullAndItemNotNull(collection, nameof (collection));
    Code.NotNullNorEmpty(collection, nameof (collection));

    // Strings
    Code.NotNullNorEmpty(str, nameof (str));
    Code.NotNullNorWhitespace(str, nameof (str));

    // Fluent assertions. Useful for multiple checks of one argument.
    Code.Arg(str, nameof (str))
        .NotNull()
        .Assert(str.Length > 5);

    // Value checks
    Code.InRange(ni, nameof (ni), 5, 10);
    Code.ValidCount(collection.Count);
    Code.ValidCount(collection.Count, maxLen);
    Code.ValidIndex(i, nameof (i));
    Code.ValidIndex(i, nameof (i), collection.Count);
    Code.ValidIndex(startIndex: 0, count: 5, length: collection.Count);

    // State checks
    Code.AssertState(!_disposed, "Instance is disposed");
    // Format string overload. String interpolation evaluates  regardless of the condition, which degraded performance on the main execution path.
    Code.AssertState(!_disposed, "Instance {0} is disposed", _id);

    // Code consistency validation. Use reversed conditions to improve code readability.
    Code.BugIf(_id == null, "Invalid id value.");
    Code.BugIf(_id.Length != 10, "Invalid id value {0}.", id);
}
```
The factory of exceptions thrown by checks is available in the CodeExceptions class.

## Enum assertions

Placed in the EnumCode/DebugEnumCode.

```C#
public enum SimpleEnum { A = 1, B = 2, C = 4 }

[Flags]
public enum FlagsEnum { D = 1, E = 2, F = 4 }

public void Foo(SimpleEnum simple, FlagsEnum flags)
{
    // --- Argument validation, throws ArgumentException ---

    // Simple is defined enum value
    // Pass: simple = B.
    // Fail: simple = B|C.
    EnumCode.Defined(simple, nameof(simple));

    // All flags are defined enum values.
    // Pass: flags = D|E.
    // Fail: flags = 123.
    EnumCode.FlagsDefined(flags, nameof(flags));

    // Flags argument include specified value (D|E)
    // Pass: flags = D|E|F,
    // Fail: flags = D or D|F.
    EnumCode.FlagSet(flags, nameof(flags), FlagsEnum.D | FlagsEnum.E);

    // Flags argument include any of specified flags (D or E)
    // Pass: flags = D or D|E or D|F
    // Fail: flags = F.
    EnumCode.AnyFlagSet(flags, nameof(flags), FlagsEnum.D | FlagsEnum.E);

    // Flags argument DOES NOT include specified value (D|E)
    // Pass: flags = F,
    // Fail: flags = D or D|E or D|F.
    EnumCode.FlagUnset(flags, nameof(flags), FlagsEnum.D | FlagsEnum.E);

    // Flags argument DOES NOT include any of specified flags (D|E)
    // Pass: flags = D or D|F,
    // Fail: flags = D|E|F.
    EnumCode.AnyFlagUnset(flags, nameof(flags), FlagsEnum.D | FlagsEnum.E);

    // --- State validation, throws InvalidOperationException ---
    // Behavior matches to argument assertions.

    // Flags argument include specified flag (D)
    // Pass: flags = D or D|F,
    // Fail: flags = E|F.
    EnumCode.StateFlagSet(flags, FlagsEnum.D, "Flags ({0}) should include D flag.", flags);

    //EnumCode.AnyStateFlagSet(...);
    //EnumCode.FlagUnset(...);
    //EnumCode.AnyStateFlagSet(...);
}
```

## IO assertions

Placed in the IOCode/DebugIOCode.

```C#
public void Foo(string path)
{
    IoCode.IsWellFormedPath(path, nameof (path));
    IoCode.IsWellFormedAbsolutePath(path, nameof (path));
    IoCode.IsWellFormedRelativePath(path, nameof (path));
    IoCode.IsFileName(path, nameof (path));
}
```


## System.Uri assertions

Placed in the UriCode/DebugUriCode.

```C#
public void Foo(Uri uri)
{
    UriCode.IsWellFormedUri(uri, nameof (uri));
    UriCode.IsWellFormedAbsoluteUri(uri, nameof (uri));
    UriCode.IsWellFormedRelativeUri(uri, nameof (uri));
}
```

## DateTime assertions

Placed in the DateTimeCode/DebugDateTimeCode. All checks available for both DateTime and DateTimeOffset.

```C#
public void Foo(DateTime dt)
{
	Code.DateOnly(dt, nameof (dt));
	Code.IsUtc(dt, nameof (dt));
	Code.IsUtcAndDateOnly(dt, nameof (dt));
	Code.FirstDayOfMonth(dt, nameof (dt));
	Code.FirstDayOfYear(dt, nameof (dt));
	Code.LastDayOfMonth(dt, nameof (dt));
	Code.LastDayOfYear(dt, nameof (dt));
}
```