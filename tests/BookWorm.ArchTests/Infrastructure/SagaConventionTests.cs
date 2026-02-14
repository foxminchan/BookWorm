using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using MassTransit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Infrastructure;

public sealed class SagaConventionTests : ArchUnitBaseTest
{
    private const string SagaNamespace = $"{nameof(BookWorm)}.*.Saga";

    [Test]
    public void GivenSagaStates_WhenCheckingInterface_ThenShouldImplementSagaStateMachineInstance()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(SagaNamespace)
            .And()
            .HaveNameEndingWith("State")
            .Should()
            .BeAssignableTo(typeof(SagaStateMachineInstance))
            .Because(
                "Saga state classes should implement SagaStateMachineInstance to integrate with MassTransit state machines."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenSagaStates_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(SagaNamespace)
            .And()
            .HaveNameEndingWith("State")
            .Should()
            .BeSealed()
            .Because("Saga state classes should be sealed to prevent unintended inheritance.")
            .Check(Architecture);
    }

    [Test]
    public void GivenSagaStateMachines_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(SagaNamespace)
            .And()
            .HaveNameEndingWith("StateMachine")
            .Should()
            .BeSealed()
            .Because(
                "Saga state machine classes should be sealed to prevent unintended inheritance."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenSagaDefinitions_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(SagaNamespace)
            .And()
            .HaveNameEndingWith("Definition")
            .Should()
            .BeSealed()
            .Because("Saga definition classes should be sealed to prevent unintended inheritance.")
            .Check(Architecture);
    }
}
