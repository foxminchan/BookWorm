using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class AuditableEntityTests : ArchUnitBaseTest
{
    [Test]
    public void GivenAuditableEntities_WhenCheckingInheritance_ThenShouldInheritFromEntity()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(AuditableEntity))
            .And()
            .DoNotHaveName(nameof(AuditableEntity))
            .And()
            .AreNotAbstract()
            .Should()
            .BeAssignableTo(typeof(Entity))
            .Because(
                "Auditable entities must inherit from Entity base class which provides Id and domain event support."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenAuditableEntities_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(AuditableEntity))
            .And()
            .DoNotHaveName(nameof(AuditableEntity))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .Because(
                "Concrete auditable entities should be sealed to prevent unintended inheritance."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenAuditableEntities_WhenCheckingVisibility_ThenShouldBePublic()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(AuditableEntity))
            .And()
            .DoNotHaveName(nameof(AuditableEntity))
            .And()
            .AreNotAbstract()
            .Should()
            .BePublic()
            .Because("Auditable entities should be public to allow access from other layers.")
            .Check(Architecture);
    }
}
