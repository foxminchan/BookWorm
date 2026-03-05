using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class ValueObjectTests : ArchUnitBaseTest
{
    [Test]
    public async Task GivenArchitecture_WhenSearchingForValueObjects_ThenShouldFindConcreteImplementations()
    {
        var valueObjectClasses = Classes()
            .That()
            .AreAssignableTo(typeof(ValueObject))
            .And()
            .DoNotHaveName(nameof(ValueObject))
            .GetObjects(Architecture);

        await Assert
            .That(valueObjectClasses.Count())
            .IsGreaterThan(0)
            .Because(
                "Should find at least one concrete ValueObject implementation in the architecture"
            );
    }

    [Test]
    public void GivenValueObjects_WhenCheckingStructure_ThenShouldBeImmutable()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(ValueObject))
            .And()
            .DoNotHaveName(nameof(ValueObject))
            .Should()
            .BeSealed()
            .Because(
                "Concrete value objects should be immutable to ensure their value equality semantics are preserved."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenValueObjects_WhenCheckingInheritance_ThenShouldInheritFromValueObject()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(ValueObject))
            .And()
            .DoNotHaveName(nameof(ValueObject))
            .Should()
            .BeAssignableTo(typeof(ValueObject))
            .Because("All value objects must properly inherit from the ValueObject base class")
            .Check(Architecture);
    }

    [Test]
    public void GivenValueObjects_WhenCheckingNaming_ThenShouldFollowConventions()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(ValueObject))
            .And()
            .DoNotHaveName(nameof(ValueObject))
            .Should()
            .NotHaveNameEndingWith(nameof(Entity))
            .AndShould()
            .NotHaveNameEndingWith("Service")
            .Because(
                "Value objects should have names that clearly distinguish them from entities and services"
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenValueObjects_WhenCheckingDependencies_ThenShouldNotDependOnEntities()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(ValueObject))
            .And()
            .DoNotHaveName(nameof(ValueObject))
            .Should()
            .NotDependOnAny(Classes().That().AreAssignableTo(typeof(Entity)))
            .Because(
                "Value objects should not depend on entities to maintain proper domain boundaries."
            )
            .Check(Architecture);
    }
}
