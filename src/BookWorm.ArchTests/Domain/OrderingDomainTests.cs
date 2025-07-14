using BookWorm.ArchTests.Abstractions;
using BookWorm.ArchTests.TUnit;
using BookWorm.Notification.Domain.Models;
using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class OrderingDomainTests : ArchUnitBaseTest
{
    private const string DomainNamespace =
        $"{nameof(BookWorm)}.{nameof(Ordering)}.{nameof(Domain)}.*";

    [Test]
    public void GivenOrderingDomain_WhenCheckingClasses_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should()
            .BeSealed()
            .Because(
                "Classes in the domain layer should be sealed to prevent inheritance, ensuring that the domain model remains consistent and predictable."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenOrderingDomain_WhenCheckingClasses_ThenShouldBePublic()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should()
            .BePublic()
            .Because(
                "Classes in the domain layer should be public to allow access from other layers, such as application and infrastructure."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenOrderingRepository_WhenAnalyzingLocation_ThenShouldBeInDomainLayer()
    {
        Interfaces()
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should()
            .HaveNameStartingWith("I")
            .AndShould()
            .BePublic()
            .Because(
                "Repository interfaces should be public and follow the naming convention of starting with 'I' to indicate they are interfaces."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenOrderingDomainExceptions_WhenCheckingConventions_ThenShouldFollowStandardRules()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .HaveNameEndingWith(nameof(Exception))
            .Should()
            .BePublic()
            .AndShould()
            .BeSealed()
            .AndShould()
            .BeAssignableTo(typeof(Exception))
            .Because(
                "Domain exceptions should be public, sealed, and derive from System.Exception to ensure proper error handling and reporting."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenOrderingDomain_WhenCheckingAggregateRoots_ThenShouldHaveIdAndAggregateInterface()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .HaveName(nameof(Order))
            .Or()
            .HaveName(nameof(Buyer))
            .Should()
            .ImplementInterface(typeof(IAggregateRoot))
            .Because(
                "Aggregate root entities should implement IAggregateRoot to mark them as the root entity of an aggregate in the domain."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenOrderingDomain_WhenCheckingOrderEntity_ThenShouldHaveCollectionOfOrderItems()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .HaveName(nameof(Order))
            .Should()
            .HavePropertyMemberWithName("OrderItems")
            .Because(
                "The Order entity should have a collection of OrderItems as part of its aggregate."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenOrderingDomain_WhenCheckingValueObjects_ThenShouldBeImmutable()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .HaveName(nameof(Address))
            .Should()
            .BeSealed()
            .Because(
                "Value objects should be immutable and sealed to ensure they cannot be modified after creation."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenOrderingDomain_WhenCheckingDomainEvents_ThenShouldFollowNamingConventions()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching($"{DomainNamespace}.Events")
            .Should()
            .BeSealed()
            .AndShould()
            .HaveNameEndingWith("Event")
            .Because(
                "Domain events should be sealed and follow the naming convention of ending with 'Event'."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenOrderingDomain_WhenCheckingSpecifications_ThenShouldFollowPatterns()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching($"{DomainNamespace}.Specifications")
            .Should()
            .BePublic()
            .AndShould()
            .BeSealed()
            .OrShould()
            .BeAbstract()
            .Because(
                "Specifications should be public, sealed, and concrete to ensure they can be instantiated and used in the domain layer."
            )
            .Check(Architecture);
    }

    [Test]
    [Arguments($"{nameof(BookWorm)}.*.Api")]
    [Arguments($"{nameof(Microsoft)}.AspNetCore.*")]
    [Arguments($"{nameof(System)}.Web.*")]
    [Arguments($"{nameof(MediatR)}.*")]
    public void GivenOrderingDomain_WhenCheckingDependencies_ThenShouldBeIsolated(
        string namespacePattern
    )
    {
        Types()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespaceMatching(namespacePattern))
            .Because(
                $"The domain layer ({DomainNamespace}) should be isolated from application, infrastructure, UI concerns, and specific implementation frameworks not part of core domain logic."
            )
            .Check(Architecture);
    }
}
