using BookWorm.Finance.Features.Orders.Orchestration.Saga;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookWorm.Finance.UnitTests;

public sealed class OrderSagaCompensationsTests
{
    [Test]
    public async Task GivenNoCompensations_WhenCompensating_ThenShouldNotCallAnyActivity()
    {
        var contextMock = CreateContextMock();
        var compensations = new OrderSagaCompensations(contextMock.Object);

        await compensations.CompensateAsync();

        contextMock.Verify(
            c =>
                c.CallActivityAsync(
                    It.IsAny<TaskName>(),
                    It.IsAny<object?>(),
                    It.IsAny<TaskOptions?>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task GivenRegisteredCompensations_WhenCompensatingInParallel_ThenShouldExecuteAllAndClear()
    {
        var contextMock = CreateContextMock();
        var called = new List<(string Name, object? Payload)>();

        contextMock
            .Setup(c =>
                c.CallActivityAsync(
                    It.IsAny<TaskName>(),
                    It.IsAny<object?>(),
                    It.IsAny<TaskOptions?>()
                )
            )
            .Returns<TaskName, object?, TaskOptions?>(
                (name, payload, _) =>
                {
                    called.Add((name.Name, payload));
                    return Task.CompletedTask;
                }
            );

        var compensations = new OrderSagaCompensations(contextMock.Object);
        compensations.AddCompensation("FirstCompensation", "cmd-1");
        compensations.AddCompensation("SecondCompensation", "cmd-2");

        await compensations.CompensateAsync(true);

        called.Count.ShouldBe(2);
        called.ShouldContain(("FirstCompensation", "cmd-1"));
        called.ShouldContain(("SecondCompensation", "cmd-2"));

        await compensations.CompensateAsync(true);

        called.Count.ShouldBe(2);
    }

    [Test]
    public async Task GivenCompensationFailure_WhenCompensatingInParallel_ThenShouldIsolateFailureAndRunRemaining()
    {
        var contextMock = CreateContextMock();
        var called = new List<string>();

        contextMock
            .Setup(c =>
                c.CallActivityAsync(
                    It.IsAny<TaskName>(),
                    It.IsAny<object?>(),
                    It.IsAny<TaskOptions?>()
                )
            )
            .Returns<TaskName, object?, TaskOptions?>(
                (name, _, _) =>
                {
                    called.Add(name.Name);
                    return name.Name == "FailingCompensation"
                        ? Task.FromException(new InvalidOperationException("Boom"))
                        : Task.CompletedTask;
                }
            );

        var compensations = new OrderSagaCompensations(contextMock.Object);
        compensations.AddCompensation("FailingCompensation", "cmd-1");
        compensations.AddCompensation("HealthyCompensation", "cmd-2");

        await compensations.CompensateAsync(true);

        called.Count.ShouldBe(2);
        called.ShouldContain("FailingCompensation");
        called.ShouldContain("HealthyCompensation");
    }

    private static Mock<TaskOrchestrationContext> CreateContextMock()
    {
        var contextMock = new Mock<TaskOrchestrationContext>();
        contextMock
            .Setup(c => c.CreateReplaySafeLogger<OrderSagaCompensations>())
            .Returns(NullLogger<OrderSagaCompensations>.Instance);
        return contextMock;
    }
}
