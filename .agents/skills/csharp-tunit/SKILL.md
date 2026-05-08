---
name: csharp-tunit
description: "Get best practices for TUnit unit testing, including data-driven tests"
---

# TUnit Best Practices

Your goal is to help me write effective unit tests with TUnit, covering both standard and data-driven testing approaches.

## Project Setup

- Use a separate test project with naming convention `[ProjectName].[TestType]` (e.g., `MyApp.UnitTests`)
- Reference TUnit package and TUnit.Assertions for fluent assertions
- Create test classes that match the classes being tested (e.g., `CalculatorTests` for `Calculator`)
- Use .NET SDK test commands: `dotnet test` for running tests
- TUnit requires .NET 8.0 or higher

## Test Structure

- No test class attributes required (like xUnit/NUnit)
- Use `[Test]` attribute for test methods (not `[Fact]` like xUnit)
- Follow the Arrange-Act-Assert (AAA) pattern
- Name tests using the pattern `Given[Condition]_When[Action]_Then[ExpectedResult]` for clarity
- Use lifecycle hooks: `[Before(Test)]` for setup and `[After(Test)]` for teardown
- Use `[Before(Class)]` and `[After(Class)]` for shared context between tests in a class
- Use `[Before(Assembly)]` and `[After(Assembly)]` for shared context across test classes
- TUnit supports advanced lifecycle hooks like `[Before(TestSession)]` and `[After(TestSession)]`

## Standard Tests

- Keep tests focused on a single behavior
- Avoid testing multiple behaviors in one test method
- Use Shouldly's fluent assertion syntax (e.g., `actual.ShouldBe(expected)`)
- Include only the assertions needed to verify the test case
- Make tests independent and idempotent (can run in any order)
- Avoid test interdependencies (use `[DependsOn]` attribute if needed)

## Data-Driven Tests

- Use `[Arguments]` attribute for inline test data (equivalent to xUnit's `[InlineData]`)
- Use `[MethodData]` for method-based test data (equivalent to xUnit's `[MemberData]`)
- Use `[ClassData]` for class-based test data
- Create custom data sources by implementing `ITestDataSource`
- Use meaningful parameter names in data-driven tests
- Multiple `[Arguments]` attributes can be applied to the same test method

## Assertions (Shouldly)

- Use `actual.ShouldBe(expected)` for value equality
- Use `actual.ShouldBeSameAs(expected)` for reference equality
- Use `actual.ShouldBeTrue()` or `actual.ShouldBeFalse()` for boolean conditions
- Use `actual.ShouldBeNull()` or `actual.ShouldNotBeNull()` for null checks
- Use `actual.ShouldContain(item)` or `actual.ShouldNotContain(item)` for collections
- Use `actual.ShouldContain("substring")` for string containment
- Use `actual.ShouldMatch(pattern)` for regex pattern matching
- Use `actual.ShouldBeGreaterThan(value)`, `actual.ShouldBeLessThan(value)` for comparisons
- Use `actual.ShouldBeOfType<T>()` for type assertions
- Use `actual.ShouldBeEmpty()` or `actual.ShouldNotBeEmpty()` for collections and strings
- Use `Should.Throw<TException>(() => action)` for sync exception testing
- Use `Should.Throw<TException>(async () => await asyncAction)` for async exception testing
- Use `Should.NotThrow(() => action)` to verify no exception is thrown
- All Shouldly assertions are synchronous (no `await` needed except for async exception testing)

## Advanced Features

- Use `[Repeat(n)]` to repeat tests multiple times
- Use `[Retry(n)]` for automatic retry on failure
- Use `[ParallelLimit<T>]` to control parallel execution limits
- Use `[Skip("reason")]` to skip tests conditionally
- Use `[DependsOn(nameof(OtherTest))]` to create test dependencies
- Use `[Timeout(milliseconds)]` to set test timeouts
- Create custom attributes by extending TUnit's base attributes

## Test Organization

- Group tests by feature or component
- Use `[Category("CategoryName")]` for test categorization
- Use `[DisplayName("Custom Test Name")]` for custom test names
- Consider using `TestContext` for test diagnostics and information
- Use conditional attributes like custom `[WindowsOnly]` for platform-specific tests

## Performance and Parallel Execution

- TUnit runs tests in parallel by default (unlike xUnit which requires explicit configuration)
- Use `[NotInParallel]` to disable parallel execution for specific tests
- Use `[ParallelLimit<T>]` with custom limit classes to control concurrency
- Tests within the same class run sequentially by default
- Use `[Repeat(n)]` with `[ParallelLimit<T>]` for load testing scenarios

## Migration from xUnit

- Replace `[Fact]` with `[Test]`
- Replace `[Theory]` with `[Test]` and use `[Arguments]` for data
- Replace `[InlineData]` with `[Arguments]`
- Replace `[MemberData]` with `[MethodData]`
- Replace `Assert.Equal` with `actual.ShouldBe(expected)`
- Replace `Assert.True` with `condition.ShouldBeTrue()`
- Replace `Assert.Throws<T>` with `Should.Throw<T>(() => action)`
- Replace constructor/IDisposable with `[Before(Test)]`/`[After(Test)]`
- Replace `IClassFixture<T>` with `[Before(Class)]`/`[After(Class)]`

**Why TUnit over xUnit?**

TUnit offers a modern, fast, and flexible testing experience with advanced features not present in xUnit, such as asynchronous assertions, more refined lifecycle hooks, and improved data-driven testing capabilities. TUnit's fluent assertions provide clearer and more expressive test validation, making it especially suitable for complex .NET projects.
