# [DRAFT] CodeJam test design guidelines
The document is not finished yet. PLEASE IGNORE it until marked as [DRAFT].


## _Tests design guidelines_
**NB:** these are not mandatory and should be threated as a baseline recomendations.
However,

* **DO** store tests in a separate project.

### _Test classes design guidelines_
* **DO** name test classes with the suffix `Tests`. Example: `EnumHelperTests`.
* **PREFER** to use name of the class being tested as a first part of the test class name.
Otherwise, use short description of the scope the test class covers.
* **DO** apply the `[TestFixture]` attribute.
* **CONSIDER** specifying Category in the `[TestFixture]` attribute if there's more than one test class for the same feature scope.

### _Test methods design guidelines_
* **DO** prefix the name of the test method with the prefix `Test`. Example: `TestNotNull()`.
* **PREFER** to add a number of the test after the `Test` prefix. Example: `Test00IsDefined`.
* **PREFER** to use the name of the method being tested as a first part of the test method name.
Otherwise, use short description of the the scenario the test method covers.
* **DO NOT** place "use case scenario" and "check all args combinations" logic in the same test method.
The test method should **EITHER** cover the specific use case **OR** test single API point
(different overloads or logically coupled methods are treated as a same API point).


## Performance tests design guidelines_
**NB:** This section is based on 'Tests design guidelines' section. The same rules do apply unless otherwise stated.

### _Short intro_
The performance tests are based on [BenchmarkDotNet](https://github.com/PerfDotNet/BenchmarkDotNet) project and use some concepts from it.

__Competition test__. This kind of performance test allows to estimate __relative__ performance of the different implementations of the same behavior.
The metric used is the relative execution time of the competition test member. It's dimensionless value and it is calculated as a multiplier to the execution time of the reference (baseline) implementation.

Each competition test should be implemented as a separate class containing  
a __baseline competition method__ (should be marked with the `[CompetitionBaseline]` attribute)  
and one or more __competition methods__ (these should be marked with the `[CompetitionBenchmark]` attribute).

Each competition test should be run by its own __perftest runner method__. This one looks something alike
```
		[Test]
		public void RunCallCostPerfTests() => CompetitionBenchmarkRunner.Run(this, AssemblyWideConfig.RunConfig);
```
or
```
		[Test]
		public void RunIsDefinedCase() =>
			CompetitionBenchmarkRunner.Run<IsDefinedCase>(AssemblyWideConfig.RunConfig);
```
The first one covers the case when the test class contains only one perf test.
In that case all competition methods are placed direcly in the perftest class after the test runner method.

The second one should be used if the test class contains multiple perftests. In that case all competition methods
must be wrapped in the nested test case class (`IsDefinedCase`) and the test class sould be placed after the test runner method.

### _Performance test classes design guidelines_

* **DO** name performance test classes with the`PerfTests` suffix . Example: `EnumHelperPerfTests`.
* **PREFER** to use name of the class being tested as a first part of the performance test class name.
Otherwise, use short description of the scope the test class covers.
* **DO** apply the `[TestFixture]` attribute.
* **CONSIDER** specifying Category in the `[TestFixture]` attribute if there's more than one performance test class
for the same feature scope.

### _Performance test nested classes design guidelines_
* **DO** wrap competition methods into test case classes if there's more than one performance test in the test class. 
* **DO** create test case classes as nested classes of the performance test class.
* **DO** name test case classes with the `Case` suffix. Example: `EnumHelperPerfTests.IsDefinedCase`
* **DO** use short description of the the scenario the test case covers as a first part of the test case class name.
* **DO** create separate test case class for each performance test.
* **DO NOT** group unrelated methods in the same performance test.
All test case methods should represent different implementations of the same behavior.


---

NOT_FINISHED_YET:
### _Competition perf test classes design guidelines_
* DO name each perftest runner method as "Run" + TestCaseClassName. Examples: RunEnumHelperPerfTests or RunIsFlagSetCase.
* DO prefix the name of the test with Test prefix. Example: TestBinarySort().
* PREFER to add a 'number-of-test-in-class' prefix after the Test prefix. This makes it much easier to find the test by it's name. Example: Test01NaiveImplementation.
* DO suffix the name of the test case method with the description of the competition member. Example: Test02UsingEmittedCode 
* DO mark a baseline competition method as a [CompetitionBaseline]. There can be only one baseline method per each test case class
* CONSIDER to mark as a baseline the fastest method in the competition. Competition methods are rated as a relative to the baseline and if the competitor runs much faster than the baseline the ranks will be something alike 0.01 .. 0.02. 
 And if you use the fastest as a baseline you'll get something alike 54.22 .. 95.13, that's much more informative.