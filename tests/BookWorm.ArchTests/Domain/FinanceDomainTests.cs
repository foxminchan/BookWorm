using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.Finance.Domain.AggregatesModel.OrderAggregate;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class FinanceDomainTests : ArchUnitBaseTest
{
    private const string DomainNamespace =
        $"{nameof(BookWorm)}.{nameof(Finance)}.{nameof(Domain)}.*";

    [Test]
    public void GivenFinanceDomain_WhenCheckingClasses_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .AreNotNested()
            .Should()
            .BeSealed()
            .Because(
                "Classes in the domain layer should be sealed to prevent inheritance, ensuring that the domain model remains consistent and predictable."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenFinanceDomain_WhenCheckingClasses_ThenShouldBePublic()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .AreNotNested()
            .Should()
            .BePublic()
            .Because(
                "Classes in the domain layer should be public to allow access from other layers, such as application and infrastructure."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenFinanceDomainExceptions_WhenCheckingConventions_ThenShouldFollowStandardRules()
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
                "Domain exceptions should be public, sealed, and derive from System.Exception."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenFinanceDomain_WhenCheckingAggregateRoots_ThenShouldImplementIAggregateRoot()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .HaveName(nameof(Order))
            .Should()
            .ImplementInterface(typeof(IAggregateRoot))
            .Because(
                "Aggregate root entities should implement IAggregateRoot to mark them as the root entity of an aggregate in the domain."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenFinanceDomain_WhenCheckingValueObjects_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .AreAssignableTo(typeof(ValueObject))
            .Should()
            .BeSealed()
            .Because(
                "Value objects should be immutable and sealed to ensure they cannot be modified after creation."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenFinanceDomain_WhenCheckingDomainEvents_ThenShouldFollowNamingConventions()
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
    [Arguments($"{nameof(BookWorm)}.*.Features.*")]
    [Arguments($"{nameof(BookWorm)}.Contracts")]
    [Arguments($"{nameof(Microsoft)}.DurableTask.*")]
    [Arguments($"{nameof(Microsoft)}.Azure.Functions.*")]
    [Arguments($"{nameof(Wolverine)}.*")]
    public void GivenFinanceDomain_WhenCheckingDependencies_ThenShouldBeIsolated(
        string namespacePattern
    )
    {
        Types()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .AreNotNested()
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespaceMatching(namespacePattern))
            .Because(
                $"The domain layer ({DomainNamespace}) should be isolated from application, orchestration, messaging, and integration contract concerns."
            )
            .Check(Architecture);
    }
}
