# String helpers

CodeJam contains a number of small string manipulation methods.

In addition, there are also some standard static methods of the string and char classes in infix form.
This form is more convenient for writing code and for reading it.

```c#
if (str.IsNullOrEmpty()) ...;
if (str.IsNullOrWhitespace()) ...;
if (str.NotNullNorEmpty()) ...;
if (str.NotNullNorWhitespace()) ...;
s = str.EmptyIfNull();
s = str.NullIfEmpty();
 
 
s = "Data: {0}".FormatWith();
 
 
s = strCollection.Join(separator: ", ");
 
 
l = str.Length; // returns 0 if str is null
 
 
s = str.ToBase64();
 
 
bs = str.ToBytes();
bs = str.ToBytes(Encoding.Utf16);
 
 
if (str.StartsWith('X') && str.EndsWith('Y')) ...; // Char overload with better performance
 
 
s = str.Substring(StringOrigin.End, 4);
 
 
s = str.Prefix(4);
s = str.Suffix(4);
 
 
s = str.TrimPrefix("Xxx");
s = str.TrimSuffix("Xxx");
 
 
s = 1048576.ToByteSizeString(); // Returns 1Mb
 
 
ss = str.SplitWithTrim(','); // Splits string and trims all parts
 
 
s = 123456.ToHexString(); // Fast hex string formatting
 
 
s = str.Unquote(); // removes quotation marks
 
 
s = str.Remove("A", "Bb"); // Removes all specified substrings
 
 
s = 12.ToInvariantString(); // Converts to string with invariant culture formatter
if (str.StartsWih\thInvariant("X") || str.EndsWithInvariant("Y") || str.IndexOfInvariant("Z") == 0 || str.LastIndexOfInvariant("A") == 0) ...;
 
 
dt = str.ToDateTime();
dt = str.ToDateTimeInvariant();
dt2 = str.ToDateTimeOffset();
dt2 = str.ToDateTimeOffsetInvariant();
 
 
n = str.ToByte(); // String to numeric conversions. Supports byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal.
 
 
Array.Sort(strCollection, NaturalOrderStringComparer.Comparer);
```