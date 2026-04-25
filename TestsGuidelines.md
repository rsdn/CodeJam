# CodeJam Test Design Guidelines

This document provides baseline recommendations for writing tests in the CodeJam project.

> **Note:** These guidelines are not mandatory but serve as best practices for maintaining consistent test code.

## General Guidelines

- **DO** store tests in a separate project.

## Test Class Guidelines

- **DO** name test classes with the `Tests` suffix. Example: `EnumHelperTests`.

- **PREFER** using the name of the class being tested as the first part of the test class name. Otherwise, use a short description of the scope the test class covers.

- **DO** apply the `[TestFixture]` attribute.

- **CONSIDER** specifying a category in the `[TestFixture]` attribute if there's more than one test class for the same feature scope.

## Test Method Guidelines

- **DO** prefix test method names with `Test`. Example: `TestNotNull()`.

- **PREFER** adding a test number after the `Test` prefix. Example: `Test00IsDefined`.

- **PREFER** using the name of the method being tested as the first part of the test method name. Otherwise, use a short description of the scenario the test method covers.

- **DO NOT** mix "use case scenario" and "check all argument combinations" logic in the same test method.

  The test method should **EITHER** cover a specific use case **OR** test a single API point (different overloads or logically coupled methods are treated as the same API point).