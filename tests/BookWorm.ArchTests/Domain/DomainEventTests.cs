using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class DomainEventTests : ArchUnitBaseTest
{
    [Test]
    public void GivenDomainEvents_WhenCheckingProperties_ThenShouldBeImmutable()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(DomainEvent))
            .And()
            .DoNotHaveName(nameof(DomainEvent))
            .Should()
            .BeSealed()
            .Because(
                "Domain events should be immutable to ensure they represent a fact that has occurred and cannot be changed."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenDomainEvents_WhenCheckingConstructors_ThenShouldHaveAllRequiredProperties()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(DomainEvent))
            .And()
            .DoNotHaveName(nameof(DomainEvent))
            .Should()
            .NotBeAbstract()
            .Because(
                "Domain events should not be abstract to allow them to be created when domain actions occur."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenDomainEvents_WhenCheckingNaming_ThenShouldEndWithEvent()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(DomainEvent))
            .Should()
            .HaveNameEndingWith("Event")
            .OrShould()
            .HaveNameEndingWith(nameof(DomainEvent))
            .Because(
                "Domain events should follow the naming convention to clearly identify their purpose."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenDomainEvents_WhenCheckingDependencies_ThenShouldNotDependOnInfrastructure()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(DomainEvent))
            .Should()
            .NotDependOnAny(
                Types().That().ResideInNamespaceMatching($"{nameof(BookWorm)}.*.Infrastructure")
            )
            .AndShould()
            .NotDependOnAny(
                Types().That().ResideInNamespaceMatching($"{nameof(BookWorm)}.*.Features")
            )
            .Because(
                "Domain events should only depend on domain concepts and not on infrastructure or application concerns."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenDomainEvents_WhenCheckingInheritance_ThenShouldInheritFromDomainEvent()
    {
        Classes()
            .That()
            .HaveNameEndingWith("Event")
            .And()
            .DoNotHaveName(nameof(DomainEvent))
            .And()
            .DoNotHaveNameEndingWith("IntegrationEvent")
            .And()
            .DoNotHaveName("IntegrationEvent")
            .Should()
            .BeAssignableTo(typeof(DomainEvent))
            .Because(
                "All domain events should inherit from the base DomainEvent class to ensure consistent behavior."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenDomainEvents_WhenCheckingModifiers_ThenShouldNotBeAbstract()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(DomainEvent))
            .And()
            .DoNotHaveName(nameof(DomainEvent))
            .Should()
            .NotBeAbstract()
            .Because(
                "Concrete domain events should not be abstract as they represent specific business events."
            )
            .Check(Architecture);
    }

    [Test]
    [Arguments($"{nameof(BookWorm)}.*.Infrastructure")]
    [Arguments($"{nameof(Microsoft)}.*")]
    [Arguments($"{nameof(System)}.*")]
    public void GivenDomainEvents_WhenCheckingLayerIsolation_ThenShouldNotDependOnHigherLayers(
        string namespacePattern
    )
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(DomainEvent))
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching(namespacePattern)
                    .And()
                    .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
            )
            .Because(
                $"Domain events should not depend on higher architectural layers like {namespacePattern}."
            )
            .Check(Architecture);
    }
}
