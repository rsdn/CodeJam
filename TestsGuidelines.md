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