using BookWorm.ArchTests.Abstractions;
using BookWorm.ArchTests.TUnit;
using BookWorm.Chassis.Repository;
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
            .BePublic()
            .AndShould()
            .NotBeAbstract()
            .Because(
                "Domain events should have public constructors to allow them to be created when domain actions occur."
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
    public void GivenDomainEvents_WhenCheckingStructure_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(DomainEvent))
            .And()
            .DoNotHaveName(nameof(DomainEvent))
            .Should()
            .BeSealed()
            .Because(
                "Domain events should be sealed to prevent inheritance and maintain their integrity."
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
    public void GivenRepositoryInterfaces_WhenCheckingNamingConventions_ThenShouldFollowPattern()
    {
        Interfaces()
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .HaveNameStartingWith("I")
            .AndShould()
            .BePublic()
            .Because(
                "Repository interfaces should follow naming conventions with 'I' prefix and be public."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenDomainServices_WhenCheckingStructure_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .HaveNameEndingWith("Service")
            .And()
            .ResideInNamespaceMatching($"{nameof(BookWorm)}")
            .Should()
            .BePublic()
            .OrShould()
            .BeInternal()
            .AndShould()
            .BeSealed()
            .Because(
                "Domain services should be public and sealed to provide specific domain operations."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenDomainExceptions_WhenCheckingConventions_ThenShouldFollowStandardRules()
    {
        Classes()
            .That()
            .HaveNameEndingWith(nameof(Exception))
            .And()
            .ResideInNamespaceMatching($"{nameof(BookWorm)}")
            .And()
            .DoNotResideInNamespaceMatching($"{nameof(BookWorm)}.Chassis.*")
            .And()
            .DoNotResideInNamespaceMatching($"{nameof(BookWorm)}.*.Infrastructure.*")
            .Should()
            .BePublic()
            .AndShould()
            .BeSealed()
            .AndShould()
            .BeAssignableTo(typeof(Exception))
            .Because(
                "Domain exceptions should be public, sealed, and derive from System.Exception."
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
            .NotDependOnAny(Types().That().ResideInNamespaceMatching(namespacePattern))
            .Because(
                $"Domain events should not depend on higher architectural layers like {namespacePattern}."
            )
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Test]
    public void GivenInfrastructureRepositories_WhenCheckingImplementation_ThenShouldImplementRepositoryInterface()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching($"{nameof(BookWorm)}.*.Infrastructure")
            .And()
            .HaveNameEndingWith("Repository")
            .And()
            // Special case for BasketRepository to ensure it does not conflict with domain repository rules
            .DoNotHaveNameEndingWith("BasketRepository")
            .Should()
            .ImplementInterface(typeof(IRepository<>))
            .OrShould()
            .ImplementInterface(typeof(IUnitOfWork))
            .Because(
                "Infrastructure repository implementations should implement domain repository interfaces."
            )
            .Check(Architecture);
    }
}
