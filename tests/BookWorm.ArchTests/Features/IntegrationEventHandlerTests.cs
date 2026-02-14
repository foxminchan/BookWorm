using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.Chassis.EventBus;
using MassTransit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Features;

public sealed class IntegrationEventHandlerTests : ArchUnitBaseTest
{
    private const string IntegrationEventHandlerNamespace =
        $"{nameof(BookWorm)}.*.IntegrationEvents.EventHandlers";

    private const string IntegrationEventNamespace = $"{nameof(BookWorm)}.Contracts";

    [Test]
    public void GivenIntegrationEventHandlers_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(IntegrationEventHandlerNamespace)
            .And()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .Because("Integration event handlers should be sealed to prevent inheritance.")
            .Check(Architecture);
    }

    [Test]
    public void GivenIntegrationEventHandlers_WhenCheckingNaming_ThenShouldEndWithHandler()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(IntegrationEventHandlerNamespace)
            .And()
            .DoNotHaveNameEndingWith("Definition")
            .Should()
            .HaveNameEndingWith("Handler")
            .Because(
                "Integration event handler classes should follow the naming convention of ending with 'Handler'."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenIntegrationEvents_WhenCheckingModifiers_ThenShouldBeRecords()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(IntegrationEventNamespace)
            .And()
            .HaveNameEndingWith("IntegrationEvent")
            .Should()
            .BeRecord()
            .Because(
                "Integration events should be records to ensure immutability and value-based equality."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenIntegrationEvents_WhenCheckingInheritance_ThenShouldExtendIntegrationEvent()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(IntegrationEventNamespace)
            .And()
            .HaveNameEndingWith("IntegrationEvent")
            .Should()
            .BeAssignableTo(typeof(IntegrationEvent))
            .Because(
                $"Integration events should extend the {nameof(IntegrationEvent)} class to ensure consistent event structure."
            )
            .Check(Architecture);
    }
}
