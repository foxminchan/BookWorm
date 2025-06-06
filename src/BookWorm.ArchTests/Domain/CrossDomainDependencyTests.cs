using BookWorm.ArchTests.Abstractions;
using BookWorm.ArchTests.TUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class CrossDomainDependencyTests : ArchUnitBaseTest
{
    [Test]
    [Arguments(nameof(Catalog), nameof(Ordering))]
    [Arguments(nameof(Catalog), nameof(Rating))]
    [Arguments(nameof(Catalog), nameof(Basket))]
    [Arguments(nameof(Ordering), nameof(Catalog))]
    [Arguments(nameof(Ordering), nameof(Rating))]
    [Arguments(nameof(Rating), nameof(Ordering))]
    [Arguments(nameof(Rating), nameof(Basket))]
    [Arguments(nameof(Basket), nameof(Ordering))]
    [Arguments(nameof(Basket), nameof(Rating))]
    public void GivenDomains_WhenCheckingDependencies_ThenShouldNotDependOnOtherDomains(
        string sourceDomain,
        string targetDomain
    )
    {
        Types()
            .That()
            .ResideInNamespace($"{nameof(BookWorm)}.{sourceDomain}.Domain", true)
            .Should()
            .NotDependOnAny($"{nameof(BookWorm)}.{targetDomain}.Domain")
            .Because(
                $"Domain {sourceDomain} should not depend on domain {targetDomain} to maintain proper bounded context separation."
            )
            .Check(Architecture);
    }
}
