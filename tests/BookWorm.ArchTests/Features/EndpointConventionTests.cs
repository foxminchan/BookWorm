using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.Chassis.Endpoints;
using FluentValidation;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Features;

public sealed class EndpointConventionTests : ArchUnitBaseTest
{
    private const string FeatureNamespace = $"{nameof(BookWorm)}.*.Features.*";

    [Test]
    public void GivenEndpoints_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(FeatureNamespace)
            .And()
            .HaveNameEndingWith("Endpoint")
            .And()
            .ImplementInterface(typeof(IEndpoint))
            .Should()
            .BeSealed()
            .Because(
                "Endpoint classes should be sealed to prevent inheritance and ensure consistent API surface."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenValidators_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(FeatureNamespace)
            .And()
            .HaveNameEndingWith("Validator")
            .And()
            .ImplementInterface(typeof(IValidator<>))
            .Should()
            .BeSealed()
            .Because(
                "Validator classes should be sealed to prevent inheritance and ensure consistent validation behavior."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenEndpoints_WhenCheckingDependencies_ThenShouldNotDependOnInfrastructureRepositories()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(FeatureNamespace)
            .And()
            .HaveNameEndingWith("Endpoint")
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching($"{nameof(BookWorm)}.*.Infrastructure")
                    .And()
                    .HaveNameEndingWith("Repository")
            )
            .Because(
                "Endpoints should not directly depend on repository implementations; they should delegate to handlers via Mediator."
            )
            .Check(Architecture);
    }
}
