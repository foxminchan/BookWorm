using BookWorm.ArchTests.Abstractions;
using BookWorm.ArchTests.TUnit;
using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class CatalogDomainTests : ArchUnitBaseTest
{
    private const string DomainNamespace =
        $"{nameof(BookWorm)}.{nameof(Catalog)}.{nameof(Domain)}.*";

    [Test]
    public void GivenCatalogDomain_WhenCheckingClasses_ThenShouldBeSealed()
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
    public void GivenCatalogDomain_WhenCheckingClasses_ThenShouldBePublic()
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
    public void GivenCatalogRepository_WhenAnalyzingLocation_ThenShouldBeInDomainLayer()
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
    public void GivenCatalogDomainExceptions_WhenCheckingConventions_ThenShouldFollowStandardRules()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching($"{nameof(BookWorm)}.{nameof(Catalog)}.Domain.Exceptions")
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
    [Arguments($"{nameof(BookWorm)}.*.Infrastructure")]
    [Arguments($"{nameof(Microsoft)}.*")]
    [Arguments($"{nameof(System)}.*")]
    [Arguments($"{nameof(MediatR)}")]
    public void GivenCatalogDomain_WhenCheckingDependencies_ThenShouldBeIsolated(
        string namespacePattern
    )
    {
        Types()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespaceMatching(namespacePattern))
            .Because(
                $"The domain layer ({DomainNamespace}) should be isolated from application, infrastructure, UI concerns, and specific implementation frameworks not part of core domain logic. It should primarily depend on itself, .NET base libraries, and approved shared kernels."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenCatalogDomain_WhenCheckingAggregateRoots_ThenShouldImplementIAggregateRoot()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .HaveName(nameof(Book))
            .Or()
            .HaveName(nameof(Author))
            .Or()
            .HaveName(nameof(Category))
            .Or()
            .HaveName(nameof(Publisher))
            .Should()
            .ImplementInterface(typeof(IAggregateRoot))
            .Because(
                "Aggregate root entities should implement IAggregateRoot to mark them as the root entity of an aggregate in the domain."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenCatalogDomain_WhenCheckingDomainEvents_ThenShouldFollowNamingConventionsAndBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching($"{nameof(BookWorm)}.{nameof(Catalog)}.Domain.Events")
            .Should()
            .BeSealed()
            .AndShould()
            .HaveNameEndingWith("Event")
            .AndShould()
            .BeAssignableTo(typeof(DomainEvent))
            .Because(
                "Domain events should be sealed, follow the naming convention of ending with 'Event', and inherit from DomainEvent."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenCatalogDomain_WhenCheckingSpecifications_ThenShouldBeInDomainLayer()
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
}
