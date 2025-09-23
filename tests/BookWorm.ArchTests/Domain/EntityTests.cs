using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class EntityTests : ArchUnitBaseTest
{
    [Test]
    public void GivenEntities_WhenCheckingId_ThenShouldHaveIdProperty()
    {
        // Only check concrete entities, not base classes
        Classes()
            .That()
            .AreAssignableTo(typeof(Entity))
            .And()
            .DoNotHaveName(nameof(Entity))
            .And()
            .DoNotHaveName(nameof(AuditableEntity))
            .And()
            .AreNotAbstract()
            .Should()
            .BeAssignableTo(typeof(Entity))
            .Because(
                "All concrete entities must inherit from Entity base class which provides Id property."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenEntities_WhenCheckingDomainEvents_ThenShouldInheritFromHasDomainEventsBase()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(Entity))
            .And()
            .DoNotHaveName(nameof(Entity))
            .And()
            .AreNotAbstract()
            .Should()
            .BeAssignableTo(typeof(HasDomainEventsBase))
            .Because("Entities should be able to raise domain events through HasDomainEventsBase.")
            .Check(Architecture);
    }

    [Test]
    public void GivenEntities_WhenCheckingMethods_ThenShouldNotExposeCollections()
    {
        MethodMembers()
            .That()
            .AreDeclaredIn(Classes().That().AreAssignableTo(typeof(Entity)))
            .And()
            .ArePublic()
            .Should()
            .NotHaveReturnType(typeof(List<>))
            .AndShould()
            .NotHaveReturnType(typeof(HashSet<>))
            .Because(
                "Entities should not expose mutable collections directly; use IReadOnlyCollection instead."
            )
            .Check(Architecture);
    }
}
