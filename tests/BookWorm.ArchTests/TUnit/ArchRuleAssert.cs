using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Fluent.Extensions;

namespace BookWorm.ArchTests.TUnit;

public static class ArchRuleAssert
{
    /// <summary>
    ///     Verifies that the architecture meets the criteria of the arch rule.
    /// </summary>
    /// <param name="architecture">The architecture to be tested</param>
    /// <param name="archRule">The rule to test the architecture with</param>
    public static void FulfilsRule(Architecture architecture, IArchRule archRule)
    {
        if (!archRule.HasNoViolations(architecture))
        {
            Assert.Fail(archRule.Evaluate(architecture).ToErrorMessage());
        }
    }
}
