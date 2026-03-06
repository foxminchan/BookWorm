using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.Chassis.Repository;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class DomainConventionTests : ArchUnitBaseTest
{
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
    public void GivenInfrastructureRepositories_WhenCheckingImplementation_ThenShouldImplementRepositoryInterface()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching($"{nameof(BookWorm)}.*.Infrastructure")
            .And()
            .HaveNameEndingWith("Repository")
            .And()
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
