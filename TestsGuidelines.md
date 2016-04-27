# [DRAFT] CodeJam test design guidelines
The document is not finished yet. PLEASE DO NOT THREAT it as a proposal.

IGNORE it until marked as [DRAFT]

## _Tests design guidelines_
NB: these are not mandatory and should be threated as a baseline recomendations. However,

* DO store benchmark tests in a separate project. 

### _NUnit test classes design guidelines_
* DO suffix the name of the test class with "Tests". Examlple: EnumHelperTests.
* PREFER to use name of the class being tested as a prefix of the test class name. Otherwise, the prefix should be the name of the scenario the test covers.
* DO apply [TestFixture] attribute.
* CONSIDER specifying Category in the attribute if there's more than one test class for the feature set.

### _NUnit test methods design guidelines_
* DO prefix the name of the test with TestXx prefix where Xx is the number of the test in the class. This makes it much easier to find the test by it's name. Example: Test00IsDefined (checks EnumHelper.IsDefined method)
* PREFER to use the name of the method being tested as a suffix of the test method name. Otherwise, the suffix should be the name of the scenario the test method covers.
* DO NOT place "check use case scenario" and "check all args combinations" logic in the same test method. The test method should EITHER cover the specific use case OR test single API point (different overloads or logically coupled methods are treated as a same API point).
 
## _Competition perf tests design guidelines_
NB: all recommendations from previos sections do apply.

### _Short perfTests description_
TBD
TODO: **define**
 - Competition test;
- Test case class;
- Competition methods;
- Test runner method.

### _Competition perf test classes design guidelines_
* DO suffix the name of the test class with "PerfTests". Example: EnumHelperPerfTests
* DO create a nested class for each perf test case. Place the competition methods in the test class itself only if there's only one test case.
* DO suffix test case class with "Case" suffix. Example: EnumHelperPerfTests.IsFlagSetCase
* DO create a new test case class for each test case scenario.
* DO NOT place unrelated methods in the same test case class. All test case methods should represent different implementations of the same case.

### _Competition perf test classes design guidelines_
* DO name each perftest runner method as "Run" + TestCaseClassName. Examples: RunEnumHelperPerfTests or RunIsFlagSetCase.
* DO prefix the name of the test case method with TestXx prefix where Xx is the number of the competition test in the test case class. This makes it much easier to find the test by it's name. Example: Test01NaiveImplementation 
* DO suffix the name of the test case method with the description of the competition member. Example: Test02UsingEmittedCode 
* DO mark a baseline competition method as a [CompetitionBaseline]. There can be only one baseline method per each test case class
* CONSIDER to mark as a baseline the fastest method in the competition. Competition methods are rated as a relative to the baseline and if the competitor runs much faster than the baseline the ranks will be something alike 0.01 .. 0.02. 
 And if you use the fastest as a baseline you'll get something alike 54.22 .. 95.13, that's much more informative.