using System.ComponentModel.DataAnnotations;
using BookWorm.ArchTests.Abstractions;
using BookWorm.ArchTests.TUnit;
using BookWorm.Basket.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class BasketDomainTests : ArchUnitBaseTest
{
    private const string DomainNamespace = $"{nameof(BookWorm)}.{nameof(Basket)}.{nameof(Domain)}";

    [Test]
    public void GivenBasketDomain_WhenCheckingClasses_ThenShouldBeSealed()
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
    public void GivenBasketDomain_WhenCheckingClasses_ThenShouldBePublic()
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
    public void GivenBasketDomain_WhenCheckingClasses_ThenShouldNotBeAbstract()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should()
            .NotBeAbstract()
            .Check(Architecture);
    }

    [Test]
    public void GivenBasketRepository_WhenAnalyzingLocation_ThenShouldBeInDomainLayer()
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
    public void GivenBasketDomainExceptions_WhenCheckingConventions_ThenShouldFollowStandardRules()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(
                $"{nameof(BookWorm)}.{nameof(Basket)}.Infrastructure.Exceptions"
            )
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
    public void GivenBasketItems_WhenAnalyzingInterfaces_ThenShouldImplementIValidatableObject()
    {
        Classes()
            .That()
            .HaveName(nameof(BasketItem))
            .And()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should()
            .ImplementInterface(typeof(IValidatableObject))
            .Because(
                $"The BasketItem class should implement the {nameof(IValidatableObject)} interface to support validation logic."
            )
            .Check(Architecture);
    }

    [Test]
    [Arguments($"{nameof(Microsoft)}.*")]
    [Arguments($"{nameof(System)}.*")]
    [Arguments($"{nameof(MediatR)}")]
    public void GivenBasketDomain_WhenCheckingDependencies_ThenShouldBeIsolated(
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
}
