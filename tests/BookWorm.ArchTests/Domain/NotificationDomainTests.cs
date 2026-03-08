using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class NotificationDomainTests : ArchUnitBaseTest
{
    private const string DomainNamespace =
        $"{nameof(BookWorm)}.{nameof(Notification)}.{nameof(Domain)}.*";

    [Test]
    public void GivenNotificationDomain_WhenCheckingClasses_ThenShouldBeSealed()
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
    public void GivenNotificationDomain_WhenCheckingClasses_ThenShouldBePublicOrInternal()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .Should()
            .BePublic()
            .OrShould()
            .BeInternal()
            .Because(
                "Classes in the domain layer should be public or internal to control access from other layers."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenNotificationDomainExceptions_WhenCheckingConventions_ThenShouldFollowStandardRules()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .HaveNameEndingWith(nameof(Exception))
            .Should()
            .BeSealed()
            .AndShould()
            .BeAssignableTo(typeof(Exception))
            .Because("Domain exceptions should be sealed and derive from System.Exception.")
            .Check(Architecture);
    }
}
