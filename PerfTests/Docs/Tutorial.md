# CodeJam.PerfTests: howto write performance tests

> **META-NOTE**
>
> Places to update are marked with *~…~*.

> **IMPORTANT**
>
> This document assumes you're familiar with [PerfTests basics](Overview.md) and [BenchmarkDotNet basics](http://benchmarkdotnet.org/Overview.htm). Please read the links before continue

[TOC]

## 0. Preparation

Let's start with a real-world scenario. 

We need to compare 1024-bit signed hashes stored as a byte arrays and we're searching most effective implementation for comparing small byte arrays. Let's skip boring things like googling, writing own implementation, writing tests etc. Here, there are [implementations](https://gist.github.com/ig-sinicyn/59b8f2abdb83a191b5215358babf6db3#file-codejam-examples-perftests-tutorial-code-cs) that we're going to check.

And here are simple rules that help you to make your perftest even better:

* When possible, start with a bulletproof code that will work in all cases, has no third party dependencies and is simple to maintain. In our case it's the `EqualsForLoop` method:

  ```c#
  public static bool EqualsForLoop(byte[] a, byte[] b)
  {
  	if (a.Length != b.Length)
  		return false;
  	for (int i = 0; i < a.Length; i++)
  		if (a[i] != b[i]) return false;  
  	return true;
  }
  ```

  It's your backup as it represents the worst case possible. DO NOT improve it if it'll make the code less readable or more complex. Add "improved" version as a separate implementation instead. 


* Then, add a pretty naïve but seems-to-be-simple solution. Like this:

  ```c#
  public static bool EqualsLinq(byte[] a, byte[] b) => a.SequenceEqual(b);
  ```

  Sometimes it turns out that simplest one is the best one. **Spolier:** not our case, though.


* Finally, add implementations that you hope to perform better. Strictly speaking, there's no point to keep code that perform _worse_;) Don't forget reference to origin in comments like this:

  ```c#
  // THANKSTO: plinth (http://stackoverflow.com/a/1445405/)
  public static bool EqualsInterop(byte[] b1, byte[] b2)
  {
  	// Validate buffers are the same length.
  	// This also ensures that the count does not exceed the length of either buffer.  
  	return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
  }
  [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
  private static extern int memcmp(byte[] b1, byte[] b2, long count);
  ```

  First, it's a good habit. Second, you always have someone to blame:)

* All candidate methods should have same public API and same behavior. All in all, you're interested in compatible and comparable implementations, right? This means there should be green unit tests for all implementations before you're going to write any perftests. Remember, perftests are not about correctness, they're about performance.

* Think first. Do not hesitate to tune your scenario if you do believe this will allow you to simplify the code or to use better algorithms. As example: 

  > … to compare 1024-bit signed hashes stored as a byte arrays …

  wait, what if we'll use longs (`ulong` to be precise) instead of bytes? Let's add some:

  ```c#
  public static bool EqualsUInt64ForLoop(ulong[] a, ulong[] b)
  {
  	if (a.Length != b.Length)
  		return false;
  	for (int i = 0; i < a.Length; i++)
  	{
  		if (a[i] != b[i])
  			return false;
  	}
  	return true;
  }
  ```

* Think twice. We have fixed-size arrays, can we use this knowledge?

  ```c#
  public static bool EqualsUInt64Hardcoded(ulong[] a, ulong[] b)
  {
  	if (a.Length != 16)
  		throw new ArgumentException("Length should be == 16", nameof(a));
  	if (b.Length != 16)
  		throw new ArgumentException("Length should be == 16", nameof(b));

  	return a[0] == b[0] && a[1] == b[1] && a[2] == b[2] && a[3] == b[3]
  		&& a[4] == b[4] && a[5] == b[5] && a[6] == b[6] && a[7] == b[7]
  		&& a[8] == b[8] && a[9] == b[9]
  		&& a[10] == b[10] && a[11] == b[11] && a[12] == b[12] && a[13] == b[13]
  		&& a[14] == b[14] && a[15] == b[15];
  }
  ```

Ok, we have some code and tests for it, should we write some perftests now?

