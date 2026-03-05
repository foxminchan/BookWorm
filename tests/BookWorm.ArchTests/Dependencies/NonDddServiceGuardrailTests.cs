using ArchUnitNET.TUnit;
using BookWorm.ArchTests.Abstractions;
using BookWorm.SharedKernel.SeedWork;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookWorm.ArchTests.Dependencies;

public sealed class NonDddServiceGuardrailTests : ArchUnitBaseTest
{
    [Test]
    [Arguments(nameof(Chat))]
    [Arguments(nameof(Finance))]
    [Arguments(nameof(Scheduler))]
    public void GivenNonDddService_WhenCheckingTypes_ThenShouldNotExtendEntity(string serviceName)
    {
        Types()
            .That()
            .Are(GetServiceTypes(serviceName))
            .Should()
            .NotBeAssignableTo(typeof(Entity))
            .Because(
                $"{serviceName} service does not follow DDD patterns and should not have types extending Entity."
            )
            .Check(Architecture);
    }

    [Test]
    [Arguments(nameof(Chat))]
    [Arguments(nameof(Finance))]
    [Arguments(nameof(Scheduler))]
    public void GivenNonDddService_WhenCheckingTypes_ThenShouldNotExtendValueObject(
        string serviceName
    )
    {
        Types()
            .That()
            .Are(GetServiceTypes(serviceName))
            .Should()
            .NotBeAssignableTo(typeof(ValueObject))
            .Because(
                $"{serviceName} service does not follow DDD patterns and should not have value objects."
            )
            .Check(Architecture);
    }

    [Test]
    [Arguments(nameof(Chat))]
    [Arguments(nameof(Finance))]
    [Arguments(nameof(Scheduler))]
    public void GivenNonDddService_WhenCheckingTypes_ThenShouldNotExtendDomainEvent(
        string serviceName
    )
    {
        Types()
            .That()
            .Are(GetServiceTypes(serviceName))
            .Should()
            .NotBeAssignableTo(typeof(DomainEvent))
            .Because(
                $"{serviceName} service does not follow DDD patterns and should not have domain events."
            )
            .Check(Architecture);
    }

    [Test]
    [Arguments(nameof(Chat))]
    [Arguments(nameof(Scheduler))]
    public void GivenNonDddService_WhenCheckingTypes_ThenShouldNotImplementIAggregateRoot(
        string serviceName
    )
    {
        Types()
            .That()
            .Are(GetServiceTypes(serviceName))
            .Should()
            .NotImplementInterface(typeof(IAggregateRoot))
            .Because(
                $"{serviceName} service does not follow DDD patterns and should not have aggregate roots."
            )
            .Check(Architecture);
    }
}
