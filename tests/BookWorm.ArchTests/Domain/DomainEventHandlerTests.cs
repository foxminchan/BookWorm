using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.SharedKernel.SeedWork;
using Mediator;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class DomainEventHandlerTests : ArchUnitBaseTest
{
    [Test]
    public void GivenDomainEventHandlers_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .ResideInNamespaceMatching($"{nameof(BookWorm)}.*.Domain.EventHandlers")
            .Should()
            .BeSealed()
            .Because(
                "Domain event handlers should be sealed to prevent inheritance and ensure predictable behavior."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenDomainEventHandlers_WhenCheckingInterface_ThenShouldImplementINotificationHandler()
    {
        Classes()
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .ResideInNamespaceMatching($"{nameof(BookWorm)}.*.Domain.EventHandlers")
            .Should()
            .ImplementInterface(typeof(INotificationHandler<>))
            .Because(
                "Domain event handlers should implement INotificationHandler<T> from Mediator to handle domain events."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenDomainEventHandlers_WhenCheckingDependencies_ThenShouldNotDependOnFeatures()
    {
        Classes()
            .That()
            .HaveNameEndingWith("Handler")
            .And()
            .ResideInNamespaceMatching($"{nameof(BookWorm)}.*.Domain.EventHandlers")
            .Should()
            .NotDependOnAny(
                Types().That().ResideInNamespaceMatching($"{nameof(BookWorm)}.*.Features.*")
            )
            .Because(
                "Domain event handlers should not depend on Features to maintain proper layer separation."
            )
            .Check(Architecture);
    }
}
