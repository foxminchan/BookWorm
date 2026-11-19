using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Infrastructure;

public sealed class SagaConventionTests : ArchUnitBaseTest
{
    private const string OrchestrationNamespace = $"{nameof(BookWorm)}.*.Orchestration";

    [Test]
    public void GivenOrchestratorClasses_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(OrchestrationNamespace)
            .And()
            .HaveNameEndingWith("Orchestrator")
            .Should()
            .BeSealed()
            .Because(
                "Durable Functions orchestrator classes should be sealed to prevent unintended inheritance."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenOrchestrationSettings_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(OrchestrationNamespace)
            .And()
            .HaveNameEndingWith("Settings")
            .Should()
            .BeSealed()
            .Because(
                "Orchestration settings classes should be sealed to prevent unintended inheritance."
            )
            .Check(Architecture);
    }
}
