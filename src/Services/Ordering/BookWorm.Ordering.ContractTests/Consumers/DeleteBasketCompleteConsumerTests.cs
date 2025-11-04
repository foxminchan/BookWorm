using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookWorm.Ordering.ContractTests.Consumers;

public sealed class DeleteBasketCompleteConsumerTests : SnapshotTestBase
{
    private const decimal TotalMoney = 125.99m;
    private readonly Mock<ILogger<DeleteBasketCompleteCommandHandler>> _loggerMock = new();
    private readonly Guid _orderId = Guid.CreateVersion7();

    [Test]
    public async Task GivenDeleteBasketCompleteCommand_WhenPublished_ThenConsumerShouldConsumeItAndLogInformation()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);

        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x => x.AddConsumer<DeleteBasketCompleteCommandHandler>())
            .AddScoped(_ => _loggerMock.Object)
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            // Act
            await harness.Bus.Publish(command);

            // Assert
            var consumer = harness.GetConsumerHarness<DeleteBasketCompleteCommandHandler>();

            await VerifySnapshot(new { harness, consumer });

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
        finally
        {
            await harness.Stop();
        }
    }
}
