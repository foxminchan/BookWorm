using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.Chat.Domain.AggregatesModel.ConversationAggregate;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class ChatDomainTests : ArchUnitBaseTest
{
    private const string DomainNamespace = $"{nameof(BookWorm)}.{nameof(Chat)}.{nameof(Domain)}";

    [Test]
    public void GivenChatDomain_WhenCheckingClasses_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .AreNotNested()
            .Should()
            .BeSealed()
            .Because(
                "Classes in the domain layer should be sealed to prevent inheritance, ensuring that the domain model remains consistent and predictable."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenChatDomain_WhenCheckingClasses_ThenShouldBePublicOrInternal()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .AreNotNested()
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
    public void GivenChatDomainExceptions_WhenCheckingConventions_ThenShouldFollowStandardRules()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching($"{DomainNamespace}.{nameof(Exception)}s")
            .And()
            .HaveNameEndingWith(nameof(Exception))
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
    public void GivenChatDomain_WhenCheckingAggregateRoots_ThenShouldImplementIAggregateRoot()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching($"{DomainNamespace}.AggregatesModel.ConversationAggregate")
            .And()
            .HaveName(nameof(Conversation))
            .Should()
            .ImplementInterface(typeof(IAggregateRoot))
            .Because(
                "Aggregate root entities like Conversation should implement IAggregateRoot to mark them as the root entity of an aggregate in the domain."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenChatDomain_WhenCheckingDomainEvents_ThenShouldFollowNamingConventionsAndBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespaceMatching($"{DomainNamespace}.Events")
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
    [Arguments($"{nameof(Microsoft)}.*")]
    [Arguments($"{nameof(System)}.*")]
    [Arguments($"{nameof(Mediator)}.*")]
    public void GivenChatDomain_WhenCheckingDependencies_ThenShouldBeIsolated(
        string namespacePattern
    )
    {
        Types()
            .That()
            .ResideInNamespaceMatching(DomainNamespace)
            .And()
            .AreNotNested()
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching(namespacePattern)
                    .And()
                    .DoNotResideInNamespaceMatching("Microsoft.CodeCoverage.*")
            )
            .Because(
                $"The domain layer ({DomainNamespace}) should be isolated from application, infrastructure, UI concerns, and specific implementation frameworks not part of core domain logic."
            )
            .Check(Architecture);
    }
}
