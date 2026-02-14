using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.Chassis.Endpoints;
using BookWorm.Chassis.Repository;
using Microsoft.EntityFrameworkCore;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Infrastructure;

public sealed class InfrastructureLayerTests : ArchUnitBaseTest
{
    private const string InfrastructureNamespace = $"{nameof(BookWorm)}.*.Infrastructure";
    private const string FeatureNamespace = $"{nameof(BookWorm)}.*.Features.*";

    [Test]
    public void GivenDbContexts_WhenCheckingNaming_ThenShouldEndWithDbContext()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(InfrastructureNamespace)
            .And()
            .AreAssignableTo(typeof(DbContext))
            .Should()
            .HaveNameEndingWith(nameof(DbContext))
            .OrShould()
            .HaveNameEndingWith("Context")
            .Because("DbContext classes should follow the naming convention for clarity.")
            .Check(Architecture);
    }

    [Test]
    public void GivenEntityConfigurations_WhenCheckingVisibility_ThenShouldBeInternal()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(InfrastructureNamespace)
            .And()
            .HaveNameEndingWith("Configuration")
            .And()
            .DoNotResideInNamespaceMatching($"{nameof(BookWorm)}.*.Infrastructure.Migrations")
            .And()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .BeInternal()
            .Because(
                "Entity configurations are infrastructure concerns and should be internal to prevent leaking persistence details."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenEntityConfigurations_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(InfrastructureNamespace)
            .And()
            .HaveNameEndingWith("Configuration")
            .And()
            .DoNotResideInNamespaceMatching($"{nameof(BookWorm)}.*.Infrastructure.Migrations")
            .And()
            .ImplementInterface(typeof(IEntityTypeConfiguration<>))
            .Should()
            .BeSealed()
            .Because("Entity configurations should be sealed to prevent unintended inheritance.")
            .Check(Architecture);
    }

    [Test]
    public void GivenRepositoryImplementations_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(InfrastructureNamespace)
            .And()
            .HaveNameEndingWith("Repository")
            .Should()
            .BeSealed()
            .Because(
                "Repository implementations should be sealed to prevent unintended inheritance and ensure consistent data access patterns."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenInfrastructure_WhenCheckingDependencies_ThenShouldNotDependOnFeatures()
    {
        Types()
            .That()
            .ResideInNamespaceMatching(InfrastructureNamespace)
            .And()
            .DoNotResideInNamespaceMatching($"{nameof(BookWorm)}.*.Infrastructure.Migrations")
            .Should()
            .NotDependOnAny(Types().That().ResideInNamespaceMatching(FeatureNamespace))
            .Because(
                "Infrastructure layer should not depend on Features to maintain proper layer separation."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenDbContexts_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(InfrastructureNamespace)
            .And()
            .AreAssignableTo(typeof(DbContext))
            .Should()
            .BeSealed()
            .Because("DbContext classes should be sealed to prevent unintended inheritance.")
            .Check(Architecture);
    }
}
