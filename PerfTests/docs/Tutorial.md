# CodeJam.PerfTests: howto write performance tests

> **META-NOTE**
>
> Places to update are marked with *~…~*.

> **HERE BE PROOFS**
>
> Code in this document is taken from [CodeJam.Examples](https://github.com/ig-sinicyn/CodeJam.Examples) project. [![Build status](https://ci.appveyor.com/api/projects/status/ug5b3ldu48da15jv?svg=true)](https://ci.appveyor.com/project/ig-sinicyn/codejam-examples)

> **IMPORTANT**
>
> This document assumes you're familiar with [PerfTests basics](Overview.md) and [BenchmarkDotNet basics](http://benchmarkdotnet.org/Overview.htm). Please read the links before continue

[TOC]

## 0. Preparation

Let's start with a real-world scenario. 

We need to compare 1024-bit signed hashes stored as a byte arrays and we are searching the most effective implementation for it. Let's skip boring things like googling, coding, writing tests etc. Here, there are [implementations](https://github.com/ig-sinicyn/CodeJam.Examples/blob/master/CodeJam.Examples.PerfTests/Tutorial/0.%20Preparation/ByteArrayEquality.cs) that we're going to check.

And here are simple rules that help you to make your perftest even better:

* When possible, start with a bulletproof code that will work in all cases, has no third party dependencies and is simple to maintain. In our example it's the `EqualsForLoop` method:

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

  It's your backup as it represents the worst case possible. DO NOT improve it if it will make the code less readable or more complex. Add "improved" version as a separate implementation instead. 


* Consider to add a pretty naïve but seems-to-work solution:

  ```c#
  public static bool EqualsLinq(byte[] a, byte[] b) => a.SequenceEqual(b);
  ```

  Sometimes it turns out that simplest one is the best one. **Spolier:** not our case, though.


* Finally, add implementations that you hope to perform better. Logically speaking, there's no point to keep a code that perform _worse_;) 

* Add third-party implementations. It will allow you to compare your solution with real-world ones. And if they are really good  you can save a lot of time and just use them. As we've referenced [CodeJam](https://github.com/rsdn/CodeJam) library already, here it is:

  ```c#
  public static bool EqualsCodeJam(byte[] a, byte[] b) =>
  	CodeJam.Collections.ArrayExtensions.EqualsTo(a, b);
  ```

  Don't forget reference to origin in comments, if any. Like this:

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

* All candidate methods should have same public API and same behavior. All in all, you're interested in compatible and comparable implementations, right? Also, there should be green unit tests for all implementations before you're going to write any perftests. Remember, perftests are not about correctness, they are about performance.

* Think first. Do not hesitate to revise your scenario if you do believe this will allow you to simplify the code or to use better algorithms. As example: 

  > … to compare 1024-bit signed hashes stored as a byte arrays …

  wait, what if we'll use longs (`ulong` to be precise) instead of bytes? Let's add some:

  ```c#
  public static bool EqualsUInt64ForLoop(ulong[] a, ulong[] b)
  {
  	if (a.Length != b.Length)
  		return false;
  	for (int i = 0; i < a.Length; i++)
  		if (a[i] != b[i]) return false;
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

Ok, we have implementations to compare and tests for it, should we write perftests now?

## 1. Setup the environment

Not yet. We have some code ahead and it's a good point to stop and to check we have setup everything and all works fine. Let's start with simple perftest and then add CI service integration.

I will skip long explanations, check [Intro](Intro.md) and [Overview](Overview,md) docs for it. Long story short, here's the plan:

1. Add a reference to the one of [CodeJam.PerfTests.*](https://www.nuget.org/packages?q=CodeJam.PerfTests) packages. There are ones for NUnit, xUnit and MSTest; if we've missed your favorite unit-test framework - [create an issue](https://github.com/rsdn/CodeJam/issues) for it.

2. If you wish, add the `CodeJam.PerfTests` section into your `app.config`:

   ```xml
   <configuration>
   	<!-- <configSections> must be the first child element of the <configuration> element. -->
   	<configSections>
   		<section
   			name="CodeJam.PerfTests"
   			type="CodeJam.PerfTests.Configs.PerfTestsSection, CodeJam.PerfTests"/>
   	</configSections>

   	<!-- Rest of app.config content goes here -->

   	<CodeJam.PerfTests
   		Platform=""
   		AnnotateSources="false"
   		IgnoreExistingAnnotations="false"
   		PreviousRunLogUri=""
   		ReportWarningsAsErrors="false"
   		TroubleshootingMode="false" />
   </configuration>
   ```

   it will allow you to override defaults without changing source code. If you're interested in results on particular platform, set it in the app.config right now. For our tutorial [we will use x64](https://github.com/ig-sinicyn/CodeJam.Examples/blob/master/CodeJam.Examples.PerfTests/App.config#L12):

   ```xml
   <CodeJam.PerfTests Platform="X64" … />
   ```

3. Add a perftest that is known to work. I usually start with [this one](https://github.com/ig-sinicyn/CodeJam.Examples/blob/master/CodeJam.Examples.PerfTests/Tutorial/1.%20Setup%20the%20environment/SimplePerfTest.cs). It is simple and it is pretty sensitive to the machine clock accuracy. If it fails you can save your time and do not troubleshoot other perftests; there should be something else that interfere. It may be CPU throttling/power saving, anti-malware, windows update or ransomware encrypting your files right now. Whatever, you will need to fix it before continue.

   Do not forget to setup target platform for your perftest runner if you've specified the platform in `app.config`. If not, the perftest will wail with 

   ```
   Job CompetitionX64, EnvMode.Platform: run as X86 (X64 expected). Fix your test runner options.
   ```

   message.


4. Finally, setup the build and tests on CI service of your choice. Why to use CI at all? Think of it as a tool that checks correctness of your code for free without wasting your time. With perftests it's especially important as CI environment is much closer to production than developer PC sandbox. 

   In our example we will use AppVeyor CI integration but any CI service will go. Setup the build ([here's example](https://github.com/ig-sinicyn/CodeJam.Examples/blob/master/appveyor.yml) for appveyor.yml file) and do not forget to obtain URL to the last run log. AppVeyor's one will look like

   ```
   https://ci.appveyor.com/api/projects/ig-sinicyn/codejam-examples/artifacts/CodeJam.Examples.PerfTests.ImportantOnly.PerfTests.log?all=true 
   ```

   Check the ["Annotating source from previous run log file"](SourceAnnotations.md) section for more information.




## 2. Writing the PerfTest

### 2.1 Adding code for PerfTest

Ok, finally it's time to write a perftest. Start with an empty perftest class, containing only runner method:

```c#
	public class ByteArrayEqualityPerfTest
	{
		[Test]
		public void RunByteArrayEqualityPerfTest() => Competition.Run(this);
	}
```

(if the terminology is not familiar to you, check the [Overview](Overview.md) document). 

Next, add setup / cleanup methods if you do need them. In our case we will use `Setup()` method to fill arrays to compare.

```c#
	public class ByteArrayEqualityPerfTest
	{
		// Wrap all helpers into a region to improve readability
		// (Visual Studio collapses regions by default).
		#region Helpers
		private byte[] _arrayA;
		private byte[] _arrayB;

		private ulong[] _arrayA2;
		private ulong[] _arrayB2;

		[Setup]
		public void Setup()
		{
			// Constant rnd seed to get repeatable results
			var rnd = new Random(0);
			_arrayA = Enumerable.Range(0, 128).Select(i => (byte)rnd.Next()).ToArray();
			_arrayB = _arrayA.ToArray();
			_arrayA2 = ByteArrayEqualityTest.ToUInt64Array(_arrayA);
			_arrayB2 = _arrayA2.ToArray();
		} 
		#endregion

		// Perftest runner method. Naming pattern is $"Run{nameof(PerfTestClass)}".
		// You may use it to write additional assertions after the perftest is completed.
		[Test]
		public void RunByteArrayEqualityPerfTest() => Competition.Run(this);
	}
```

note that we use hardcoded random seed to provide same results for each run. You can replace it with random seed later, after the test will be completed. 

Finally, add implementations you want to check:

```c#
[CompetitionBaseline]
public bool EqualsForLoop() => ByteArrayEquality.EqualsForLoop(_arrayA, _arrayB);

[CompetitionBenchmark]
public bool EqualsLinq() => ByteArrayEquality.EqualsLinq(_arrayA, _arrayB);

[CompetitionBenchmark]
public bool EqualsCodeJam() => ByteArrayEquality.EqualsCodeJam(_arrayA, _arrayB);

// ...

[CompetitionBenchmark]
public bool EqualsUInt64ForLoop() => ByteArrayEquality.EqualsUInt64ForLoop(_arrayA2, _arrayB2);

[CompetitionBenchmark]
public bool EqualsUInt64Linq() => ByteArrayEquality.EqualsUInt64Linq(_arrayA2, _arrayB2);

[CompetitionBenchmark]
public bool EqualsUInt64CodeJam() => ByteArrayEquality.EqualsUInt64CodeJam(_arrayA2, _arrayB2);

// ...
```

Choose a reference implementation and mark it with `[CompetitionBaseline]` attribute. (see the [Choosing a baseline method](SourceAnnotations.md)  section for recommendations). Rest of competition methods should be annotated with `[CompetitionBenchmark]` attribute. 

The order of methods is irrelevant, however, it makes sense to place the benchmark method first and group rest of methods by cases they do check. In our example we have two sets of methods, for byte arrays and ulong arrays respectively. We prefer to keep same set methods together to ease comparison.



### 2.2 Annotating the PerfTest limits

For more details about options available here check the [Source Annotations](SourceAnnotations.md) document. For the example we will use attribute annotations. Apply `[CompetitionAnnotateSources]` attribute to the `ByteArrayEqualityPerfTest` class and run the `RunByteArrayEqualityPerfTest` perftest multiple times until the test will finish without warnings about annotation updates. It's usually takes two to four runs to adjust limits. 



### 2.3 Checking the results

Here's part of output for the perftest with actual timings

| Method                | Scaled    | Scaled-StdDev |
| --------------------- | --------- | ------------- |
| EqualsForLoop         | 1.00      | 0.00          |
| **EqualsLinq**        | **16.13** | **0.16**      |
| EqualsCodeJam         | 0.12      | 0.00          |
| EqualsVectors         | 0.20      | 0.00          |
| EqualsUnsafe          | 0.23      | 0.00          |
| EqualsInterop         | 0.18      | 0.00          |
| EqualsUInt64ForLoop   | 0.20      | 0.00          |
| **EqualsUInt64Linq**  | **2.33**  | **0.02**      |
| EqualsUInt64Hardcoded | 0.12      | 0.00          |
| EqualsUInt64CodeJam   | 0.11      | 0.00          |
| EqualsUInt64Vectors   | 0.18      | 0.00          |

What do we have here?

First, remember the "simplest one is the best one" linq-driven solution?

```c#
public static bool EqualsLinq(byte[] a, byte[] b) => a.SequenceEqual(b);
```

Turns out it is the worst one. Rest of competitors seems to look fine, but wait, what if we'll run the code as x86? 

> **HINT**
>
> If you have enabled target platform checking in app.config you will need to disable it by applying the `[CompetitionPlatform(Platform.AnyCpu)]` attribute to the perftest.

Change your test runner target platform and run the perftest again. Here's the summary:

| Method                  | Scaled    | Scaled-StdDev | Scaled (x86) | Scaled-StdDev (x86) |
| ----------------------- | --------- | ------------- | ------------ | ------------------- |
| EqualsForLoop           | 1.00      | 0.00          | 1.00         | 0.00                |
| **EqualsLinq**          | **16.13** | **0.16**      | **12.53**    | **0.15**            |
| EqualsCodeJam           | 0.12      | 0.00          | 0.14         | 0.00                |
| **EqualsVectors**       | 0.20      | 0.00          | **2.47**     | **0.04**            |
| EqualsUnsafe            | 0.23      | 0.00          | 0.23         | 0.00                |
| EqualsInterop           | 0.18      | 0.00          | 0.39         | 0.01                |
| EqualsUInt64ForLoop     | 0.20      | 0.00          | 0.20         | 0.01                |
| **EqualsUInt64Linq**    | **2.33**  | **0.02**      | **2.06**     | **0.05**            |
| EqualsUInt64Hardcoded   | 0.12      | 0.00          | 0.16         | 0.01                |
| EqualsUInt64CodeJam     | 0.11      | 0.00          | 0.13         | 0.00                |
| **EqualsUInt64Vectors** | 0.18      | 0.00          | **1.37**     | **0.02**            |

Well, we have another pair of outsiders now:

```c#
public static bool EqualsVectors(byte[] a, byte[] b)
{
	if (a.Length != b.Length)
		return false;

	int i;
	var max = a.Length - a.Length % Vector<byte>.Count;
	for (i = 0; i < max; i += Vector<byte>.Count)
		if (new Vector<byte>(a, i) != new Vector<byte>(b, i))
			return false;

	if (i < a.Length)
		for (; i < a.Length; i++)
			if (a[i] != b[i]) return false;
	return true;
}
```

Yep, it uses `System.Numerics.Vectors` nuget package and (as of .net 4.6) x86 version of the JIT does not provide hardware acceleration for it. Here's how to proof:

```c#
// Perftest runner method. Naming pattern is $"Run{nameof(PerfTestClass)}".
// You may use it to write additional assertions after the perftest is completed.
[Test]
public void RunByteArrayEqualityPerfTest()
{
	Competition.Run(this);

	// Fails on x86.
	Assert.IsTrue(Vector.IsHardwareAccelerated, "SIMD operations are not supported.");
}
```



### 2.4 Results from Continuous Integration build

Ok, we have some methods that looks like a good candidates for replacing the baseline implementation. Should we choose one right now? Nope! Remember, the results are taken on single machine only. The best thing we can do here is to run the test on different hardware.  We have one as we've setup CI build earlier. There are summary table with results including output [from CI build](https://ci.appveyor.com/project/ig-sinicyn/codejam-examples/build/0.0.1.4#L36):

| Method                  | Scaled    | Scaled (x86) | Scaled (CI) |
| ----------------------- | --------- | ------------ | ----------- |
| EqualsForLoop           | 1.00      | 1.00         | 1.00        |
| ~~EqualsLinq~~          | ~~16.13~~ | ~~12.53~~    | ~~20.67~~   |
| EqualsCodeJam           | 0.12      | 0.14         | 0.11        |
| ~~EqualsVectors~~       | ~~0.20~~  | ~~2.47~~     | ~~0.11~~    |
| EqualsUnsafe            | 0.23      | 0.23         | 0.21        |
| EqualsInterop           | 0.18      | 0.39         | 0.25        |
| EqualsUInt64ForLoop     | 0.20      | 0.20         | 0.18        |
| ~~EqualsUInt64Linq~~    | ~~2.33~~  | ~~2.06~~     | ~~3.30~~    |
| EqualsUInt64Hardcoded   | 0.12      | 0.16         | 0.10        |
| EqualsUInt64CodeJam     | 0.11      | 0.13         | 0.11        |
| ~~EqualsUInt64Vectors~~ | ~~0.18~~  | ~~1.37~~     | ~~0.12~~    |

Almost done!

Last one thing: we have collected the results from multiple runs into summary table, but it's hard to keep it in sync and it will not work at all if you have dozens or hundreds of perftests. Instead of doing this we'll update source annotations using previous run log. Add the URL to the CI last run log into `app.config`. Like this:

```xml
<CodeJam.PerfTests
	Platform="X64"
	AnnotateSources="false"
	IgnoreExistingAnnotations="false"
	PreviousRunLogUri="https://ci.appveyor.com/api/projects/ig-sinicyn/codejam-examples/artifacts/CodeJam.Examples.PerfTests.ImportantOnly.PerfTests.log?all=true"
	ReportWarningsAsErrors="false"
	TroubleshootingMode="false" />
```

then, run perftest one more time and check source annotations. Here they are:

| Method                  | Limit, min | Limit, max |
| ----------------------- | ---------- | ---------- |
| EqualsForLoop           | 1.00       | 1.00       |
| ~~EqualsLinq~~          | ~~12.17~~  | ~~21.48~~  |
| EqualsCodeJam           | 0.11       | 0.15       |
| ~~EqualsVectors~~       | ~~0.11~~   | ~~2.47~~   |
| EqualsUnsafe            | 0.16       | 0.27       |
| EqualsInterop           | 0.18       | 0.41       |
| EqualsUInt64ForLoop     | 0.17       | 0.24       |
| ~~EqualsUInt64Linq~~    | ~~1.97~~   | ~~3.40~~   |
| EqualsUInt64Hardcoded   | 0.10       | 0.17       |
| EqualsUInt64CodeJam     | 0.09       | 0.15       |
| ~~EqualsUInt64Vectors~~ | ~~0.12~~   | ~~1.45~~   |



### 2.5 Summary

Well, there are conclusions: 

* If you want to solve the task as it was formulated at start (store the hashes as a byte arrays), the best choice is to use array comparison implementation from the CodeJam library.
* If it's ok to use ulong arrays there are additional options: you may use CodeJam lib again, may prefer to use hardcoded version or (if the perfomance is not so critical) use baseline implementation adopted to `UInt64`.

We are done for now. In future chapters (*~will be added later~*) we will look for advanced scenarios such as using multiple case perftests, storing annotations outside of sources and customizing the competition config to make the test more accurate (in cost of additional execution time, of course). Stay tuned!