using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class CrossDomainDependencyTests : ArchUnitBaseTest
{
    private static readonly string[] _domains =
    [
        nameof(Catalog),
        nameof(Ordering),
        nameof(Rating),
        nameof(Basket),
        nameof(Notification),
        nameof(Chat),
    ];

    public static IEnumerable<(string SourceDomain, string TargetDomain)> DomainPairs()
    {
        return from source in _domains
            from target in _domains
            where source != target
            select (source, target);
    }

    [Test]
    [MethodDataSource(nameof(DomainPairs))]
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
