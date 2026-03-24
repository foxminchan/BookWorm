---
name: csharp-tunit
description: "Generates TUnit test classes with data-driven tests, Shouldly assertions, and lifecycle hooks. Use when writing unit tests, creating test fixtures, migrating from xUnit, or adding data-driven test cases in .NET projects using TUnit."
---

# TUnit Best Practices

## Project Setup

- Use a separate test project named `[ProjectName].[TestType]` (e.g., `MyApp.UnitTests`)
- Reference TUnit package and TUnit.Assertions for fluent assertions
- TUnit requires .NET 8.0 or higher

## Test Structure

- Use `[Test]` attribute for test methods (no class-level attributes required)
- Follow Arrange-Act-Assert (AAA) pattern
- Name tests as `Given[Condition]_When[Action]_Then[ExpectedResult]`
- Lifecycle hooks: `[Before(Test)]` / `[After(Test)]` for setup/teardown
- Class-level: `[Before(Class)]` / `[After(Class)]` for shared context

## Example: Standard Test

```csharp
public class CalculatorTests
{
    private Calculator _sut;

    [Before(Test)]
    public void Setup() => _sut = new Calculator();

    [Test]
    public void GivenTwoNumbers_WhenAdding_ThenReturnSum()
    {
        var result = _sut.Add(2, 3);
        result.ShouldBe(5);
    }
}
```

## Example: Data-Driven Test

```csharp
public class PriceCalculatorTests
{
    [Test]
    [Arguments(100.0, 0.1, 90.0)]
    [Arguments(50.0, 0.25, 37.5)]
    [Arguments(200.0, 0.0, 200.0)]
    public void GivenPriceAndDiscount_WhenCalculating_ThenReturnDiscountedPrice(
        double price, double discount, double expected)
    {
        var result = PriceCalculator.ApplyDiscount(price, discount);
        result.ShouldBe(expected);
    }

    [Test]
    [MethodData(nameof(GetBulkPricingData))]
    public void GivenBulkOrder_WhenCalculating_ThenApplyTierDiscount(
        int quantity, double unitPrice, double expectedTotal)
    {
        var result = PriceCalculator.CalculateBulk(quantity, unitPrice);
        result.ShouldBe(expectedTotal);
    }

    public static IEnumerable<(int, double, double)> GetBulkPricingData()
    {
        yield return (10, 5.0, 45.0);
        yield return (100, 5.0, 400.0);
    }
}
```

## Assertions (Shouldly)

- `actual.ShouldBe(expected)` for value equality
- `actual.ShouldBeNull()` / `actual.ShouldNotBeNull()` for null checks
- `actual.ShouldContain(item)` for collections and strings
- `actual.ShouldBeOfType<T>()` for type assertions
- `actual.ShouldBeEmpty()` / `actual.ShouldNotBeEmpty()` for collections
- `Should.Throw<TException>(() => action)` for exception testing
- `Should.Throw<TException>(async () => await asyncAction)` for async exceptions

## Data-Driven Tests

- `[Arguments]` for inline test data (replaces xUnit `[InlineData]`)
- `[MethodData]` for method-based test data (replaces xUnit `[MemberData]`)
- `[ClassData]` for class-based test data sources
- Implement `ITestDataSource` for custom data sources

## Advanced Features

- `[Repeat(n)]` to repeat tests; `[Retry(n)]` for automatic retry
- `[ParallelLimit<T>]` to control concurrency; `[NotInParallel]` to disable it
- `[Skip("reason")]` to skip conditionally; `[Timeout(ms)]` for timeouts
- `[Category("Name")]` and `[DisplayName("Name")]` for organization

## Migration from xUnit

| xUnit | TUnit |
|-------|-------|
| `[Fact]` | `[Test]` |
| `[Theory]` + `[InlineData]` | `[Test]` + `[Arguments]` |
| `[MemberData]` | `[MethodData]` |
| `Assert.Equal` | `actual.ShouldBe(expected)` |
| `Assert.Throws<T>` | `Should.Throw<T>(() => action)` |
| Constructor / `IDisposable` | `[Before(Test)]` / `[After(Test)]` |
| `IClassFixture<T>` | `[Before(Class)]` / `[After(Class)]` |

## Validation Checkpoint

After generating tests, verify:
1. Every test method has the `[Test]` attribute
2. Test names follow `Given_When_Then` convention
3. Assertions use Shouldly syntax (not `Assert.*`)
4. Data-driven tests have meaningful parameter names
5. No test interdependencies unless explicitly using `[DependsOn]`
