﻿using BookWorm.ArchTests.Abstractions;
using BookWorm.ArchTests.TUnit;
using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.Domain.FeedbackAggregator.Specifications;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class RatingDomainTests : ArchUnitBaseTest
{
    private const string DomainNamespace = $"{nameof(BookWorm)}.{nameof(Rating)}.{nameof(Domain)}";

    [Test]
    public void GivenRatingDomain_WhenCheckingClasses_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespace(DomainNamespace, true)
            .And()
            .AreNot(typeof(FeedbackFilterSpec))
            .Should()
            .BeSealed()
            .Because(
                "Classes in the domain layer should be sealed to prevent inheritance, ensuring that the domain model remains consistent and predictable, unless they are designed for extension like specification base classes."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenRatingDomain_WhenCheckingClasses_ThenShouldBePublic()
    {
        Classes()
            .That()
            .ResideInNamespace(DomainNamespace, true)
            .Should()
            .BePublic()
            .Because(
                "Classes in the domain layer should be public to allow access from other layers, such as application and infrastructure."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenRatingRepository_WhenAnalyzingLocation_ThenShouldBeInDomainLayer()
    {
        Interfaces()
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .ResideInNamespace($"{DomainNamespace}.FeedbackAggregator", true)
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
    public void GivenRatingDomainExceptions_WhenCheckingConventions_ThenShouldFollowStandardRules()
    {
        Classes()
            .That()
            .ResideInNamespace($"{DomainNamespace}.{nameof(Exception)}s", true)
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
    public void GivenRatingDomain_WhenCheckingAggregateRoots_ThenShouldImplementIAggregateRoot()
    {
        Classes()
            .That()
            .ResideInNamespace($"{DomainNamespace}.FeedbackAggregator", true)
            .And()
            .HaveName(nameof(Feedback))
            .Should()
            .ImplementInterface(typeof(IAggregateRoot))
            .Because(
                "Aggregate root entities like Feedback should implement IAggregateRoot to mark them as the root entity of an aggregate in the domain."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenRatingDomain_WhenCheckingDomainEvents_ThenShouldFollowNamingConventionsAndBeSealed()
    {
        Classes()
            .That()
            .ResideInNamespace($"{DomainNamespace}.Events", true)
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
    public void GivenRatingDomain_WhenCheckingSpecifications_ThenShouldFollowPatterns()
    {
        Classes()
            .That()
            .ResideInNamespace($"{DomainNamespace}.FeedbackAggregator.Specifications", true)
            .Should()
            .BePublic()
            .AndShould()
            .BeSealed()
            .OrShould()
            .BeAbstract() // Allow for abstract base specification classes if any
            .Because(
                "Specifications should be public, and typically sealed or abstract if designed as a base, to ensure they can be instantiated and used correctly in the domain layer."
            )
            .Check(Architecture);
    }

    [Test]
    [Arguments($"{nameof(BookWorm)}.*.Infrastructure")]
    [Arguments($"{nameof(Microsoft)}.*")]
    [Arguments($"{nameof(System)}.*")]
    [Arguments($"{nameof(MediatR)}.*")]
    public void GivenRatingDomain_WhenCheckingDependencies_ThenShouldBeIsolated(
        string namespacePattern
    )
    {
        Types()
            .That()
            .ResideInNamespace(DomainNamespace, true)
            .Should()
            .NotDependOnAny(namespacePattern)
            .Because(
                $"The domain layer ({DomainNamespace}) should be isolated from application, infrastructure, UI concerns, and specific implementation frameworks not part of core domain logic."
            )
            .Check(Architecture);
    }
}
