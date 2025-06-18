using BookWorm.Contracts;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.Ordering.UnitTests.Consumers;

public sealed class DeleteBasketCompleteConsumerTests
{
    private const decimal TotalMoney = 125.99m;
    private readonly Mock<ILogger<DeleteBasketCompleteCommandHandler>> _loggerMock = new();
    private readonly Guid _orderId = Guid.CreateVersion7();

    [Test]
    public async Task GivenValidCommand_WhenConsumed_ThenShouldLogInformation()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped(_ => _loggerMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketCompleteCommandHandler>();
        (await consumerHarness.Consumed.Any<DeleteBasketCompleteCommand>()).ShouldBeTrue();

        // No fault should be published
        (await harness.Published.Any<Fault<DeleteBasketCompleteCommand>>()).ShouldBeFalse();

        // Verify that information was logged
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Basket deletion completed")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );

        await harness.Stop();
    }

    [Test]
    public async Task GivenValidCommand_WhenConsumed_ThenShouldCompleteSuccessfully()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped(_ => _loggerMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketCompleteCommandHandler>();

        // Command should be consumed
        (await consumerHarness.Consumed.Any<DeleteBasketCompleteCommand>()).ShouldBeTrue();

        // No fault should be published (since the handler only logs and returns Task.CompletedTask)
        (await harness.Published.Any<Fault<DeleteBasketCompleteCommand>>()).ShouldBeFalse();

        await harness.Stop();
    }

    [Test]
    public async Task GivenMultipleCommands_WhenConsumed_ThenShouldProcessAllSuccessfully()
    {
        // Arrange
        var command1 = new DeleteBasketCompleteCommand(Guid.CreateVersion7(), TotalMoney);
        var command2 = new DeleteBasketCompleteCommand(Guid.CreateVersion7(), TotalMoney + 50);
        var command3 = new DeleteBasketCompleteCommand(Guid.CreateVersion7(), TotalMoney + 100);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped(_ => _loggerMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        // Act
        await harness.Bus.Publish(command1);
        await harness.Bus.Publish(command2);
        await harness.Bus.Publish(command3);

        // Assert
        var consumerHarness = harness.GetConsumerHarness<DeleteBasketCompleteCommandHandler>();

        // All commands should be consumed
        (
            await consumerHarness.Consumed.SelectAsync<DeleteBasketCompleteCommand>().Count()
        ).ShouldBe(3);

        // No faults should be published
        (await harness.Published.Any<Fault<DeleteBasketCompleteCommand>>()).ShouldBeFalse();

        // Verify that information was logged for each command
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Basket deletion completed")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Exactly(3)
        );

        await harness.Stop();
    }
}
