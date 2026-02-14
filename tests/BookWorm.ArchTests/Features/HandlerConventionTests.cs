using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using Mediator;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Features;

public sealed class HandlerConventionTests : ArchUnitBaseTest
{
    private const string FeatureNamespace = $"{nameof(BookWorm)}.*.Features.*";

    [Test]
    public void GivenCommandHandlers_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(FeatureNamespace)
            .And()
            .HaveNameEndingWith("Handler")
            .And()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .BeSealed()
            .Because(
                "Command handlers should be sealed to prevent inheritance and ensure single responsibility."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenQueryHandlers_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(FeatureNamespace)
            .And()
            .HaveNameEndingWith("Handler")
            .And()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .BeSealed()
            .Because(
                "Query handlers should be sealed to prevent inheritance and ensure single responsibility."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenCommandHandlers_WhenCheckingLocation_ThenShouldResideInFeaturesNamespace()
    {
        Classes()
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .And()
            .DoNotResideInNamespaceMatching($"{nameof(BookWorm)}.Chassis.*")
            .Should()
            .ResideInNamespaceMatching(FeatureNamespace)
            .Because(
                "Command handlers should reside in the Features namespace to follow vertical slice architecture."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenQueryHandlers_WhenCheckingLocation_ThenShouldResideInFeaturesNamespace()
    {
        Classes()
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .And()
            .DoNotResideInNamespaceMatching($"{nameof(BookWorm)}.Chassis.*")
            .Should()
            .ResideInNamespaceMatching(FeatureNamespace)
            .Because(
                "Query handlers should reside in the Features namespace to follow vertical slice architecture."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenHandlers_WhenCheckingDependencies_ThenShouldNotDependOnOtherHandlers()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(FeatureNamespace)
            .And()
            .HaveNameEndingWith("Handler")
            .Should()
            .NotDependOnAny(
                Classes()
                    .That()
                    .HaveNameEndingWith("Handler")
                    .And()
                    .ResideInNamespaceMatching(FeatureNamespace)
            )
            .Because(
                "Handlers should not depend on other handlers to maintain independence and single responsibility."
            )
            .Check(Architecture);
    }
}
