using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class CrossDomainDependencyTests : ArchUnitBaseTest
{
    [Test]
    [Arguments(nameof(Catalog), nameof(Ordering))]
    [Arguments(nameof(Catalog), nameof(Rating))]
    [Arguments(nameof(Catalog), nameof(Basket))]
    [Arguments(nameof(Catalog), nameof(Notification))]
    [Arguments(nameof(Ordering), nameof(Catalog))]
    [Arguments(nameof(Ordering), nameof(Rating))]
    [Arguments(nameof(Ordering), nameof(Basket))]
    [Arguments(nameof(Ordering), nameof(Notification))]
    [Arguments(nameof(Rating), nameof(Catalog))]
    [Arguments(nameof(Rating), nameof(Ordering))]
    [Arguments(nameof(Rating), nameof(Basket))]
    [Arguments(nameof(Rating), nameof(Notification))]
    [Arguments(nameof(Basket), nameof(Catalog))]
    [Arguments(nameof(Basket), nameof(Ordering))]
    [Arguments(nameof(Basket), nameof(Rating))]
    [Arguments(nameof(Basket), nameof(Notification))]
    [Arguments(nameof(Notification), nameof(Catalog))]
    [Arguments(nameof(Notification), nameof(Ordering))]
    [Arguments(nameof(Notification), nameof(Rating))]
    [Arguments(nameof(Notification), nameof(Basket))]
    public void GivenDomains_WhenCheckingDependencies_ThenShouldNotDependOnOtherDomains(
        string sourceDomain,
        string targetDomain
    )
    {
        Types()
            .That()
            .ResideInNamespaceMatching($"{nameof(BookWorm)}.{sourceDomain}.Domain")
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching($"{nameof(BookWorm)}.{targetDomain}.Domain")
            )
            .Because(
                $"Domain {sourceDomain} should not depend on domain {targetDomain} to maintain proper bounded context separation."
            )
            .Check(Architecture);
    }
}
