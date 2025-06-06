using BookWorm.ArchTests.Abstractions;
using BookWorm.ArchTests.TUnit;
using Microsoft.EntityFrameworkCore;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Infrastructure;

public sealed class InfrastructureLayerTests : ArchUnitBaseTest
{
    private const string InfrastructureNamespace = $"{nameof(BookWorm)}.*.Infrastructure";

    [Test]
    public void GivenDbContexts_WhenCheckingNaming_ThenShouldEndWithDbContext()
    {
        Classes()
            .That()
            .ResideInNamespace(InfrastructureNamespace, true)
            .And()
            .AreAssignableTo(typeof(DbContext))
            .Should()
            .HaveNameEndingWith(nameof(DbContext))
            .OrShould()
            .HaveNameEndingWith("Context")
            .Because("DbContext classes should follow the naming convention for clarity.")
            .Check(Architecture);
    }
}
