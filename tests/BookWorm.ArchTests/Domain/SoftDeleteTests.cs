using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Domain;

public sealed class SoftDeleteTests : ArchUnitBaseTest
{
    [Test]
    public void GivenSoftDeleteImplementations_WhenCheckingAggregateRoot_ThenShouldAlsoImplementIAggregateRoot()
    {
        Classes()
            .That()
            .ImplementInterface(typeof(ISoftDelete))
            .Should()
            .ImplementInterface(typeof(IAggregateRoot))
            .Because(
                "Only aggregate roots should support soft delete to maintain consistency boundaries."
            )
            .Check(Architecture);
    }

    [Test]
    public void GivenSoftDeleteImplementations_WhenCheckingModifiers_ThenShouldBeSealed()
    {
        Classes()
            .That()
            .ImplementInterface(typeof(ISoftDelete))
            .Should()
            .BeSealed()
            .Because(
                "Soft-deletable entities should be sealed to prevent inheritance and ensure predictable delete behavior."
            )
            .Check(Architecture);
    }
}
