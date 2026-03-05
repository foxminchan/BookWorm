using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.Ordering.ContractTests.Consumers;

public sealed class DeleteBasketCompleteConsumerTests
{
    private const decimal TotalMoney = 125.99m;
    private Guid _orderId;
    private ITestHarness _harness = null!;
    private ServiceProvider _provider = null!;
    private Mock<ILogger<DeleteBasketCompleteCommandHandler>> _loggerMock = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        _orderId = Guid.CreateVersion7();
        _loggerMock = new();

        _provider = new ServiceCollection()
            .AddTelemetryListener()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped(_ => _loggerMock.Object)
            .BuildServiceProvider(true);

        _harness = await _provider.StartTestHarness();
    }

    [After(Test)]
    public async Task TearDownAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Test]
    public async Task GivenDeleteBasketCompleteCommand_WhenPublished_ThenConsumerShouldConsumeItAndLogInformation()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);

        // Act
        await _harness.Bus.Publish(command);

        // Assert
        var consumer = _harness.GetConsumerHarness<DeleteBasketCompleteCommandHandler>();
        await consumer.Consumed.Any<DeleteBasketCompleteCommand>();

        await SnapshotTestHelper.Verify(new { harness = _harness, consumer });

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
    }
}
