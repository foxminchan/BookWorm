using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Infrastructure;

public sealed class SagaConventionTests : ArchUnitBaseTest
{
    private const string SagaNamespace = $"{nameof(BookWorm)}.*.Saga";

    [Test]
    public void GivenSagaClasses_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(SagaNamespace)
            .And()
            .HaveNameEndingWith("Saga")
            .Should()
            .BeSealed()
            .Because("Wolverine saga classes should be sealed to prevent unintended inheritance.")
            .Check(Architecture);
    }

    [Test]
    public void GivenSagaSettings_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(SagaNamespace)
            .And()
            .HaveNameEndingWith("Settings")
            .Should()
            .BeSealed()
            .Because("Saga settings classes should be sealed to prevent unintended inheritance.")
            .Check(Architecture);
    }
}
