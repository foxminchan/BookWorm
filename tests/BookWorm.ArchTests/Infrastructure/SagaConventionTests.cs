using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Infrastructure;

public sealed class SagaConventionTests : ArchUnitBaseTest
{
    private const string SagaNamespace = $"{nameof(BookWorm)}.*.Saga";

    [Test]
    public void GivenSagas_WhenCheckingBaseClass_ThenShouldExtendWolverineSaga()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(SagaNamespace)
            .And()
            .HaveNameEndingWith("Saga")
            .Should()
            .BeAssignableTo(typeof(Wolverine.Saga))
            .Because(
                "Saga classes should extend Wolverine's Saga base class to integrate with Wolverine saga persistence."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenSagas_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(SagaNamespace)
            .And()
            .HaveNameEndingWith("Saga")
            .Should()
            .BeSealed()
            .Because("Saga classes should be sealed to prevent unintended inheritance.")
            .Check(Architecture);
    }
}
