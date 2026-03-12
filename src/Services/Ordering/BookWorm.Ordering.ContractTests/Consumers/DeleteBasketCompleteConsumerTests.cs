using BookWorm.Common;
using BookWorm.Contracts;
using BookWorm.Ordering.IntegrationEvents.EventHandlers;
using Microsoft.Extensions.Logging;

namespace BookWorm.Ordering.ContractTests.Consumers;

public sealed class DeleteBasketCompleteConsumerTests
{
    private const decimal TotalMoney = 125.99m;
    private Mock<ILogger<DeleteBasketCompleteCommandHandler>> _loggerMock = null!;
    private Guid _orderId;
    private DeleteBasketCompleteCommandHandler _handler = null!;

    [Before(Test)]
    public void SetUp()
    {
        _orderId = Guid.CreateVersion7();
        _loggerMock = new();
        _handler = new(_loggerMock.Object);
    }

    [Test]
    public async Task GivenDeleteBasketCompleteCommand_WhenPublished_ThenConsumerShouldConsumeItAndLogInformation()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);

        // Act
        await _handler.Handle(command);

        // Assert
        await SnapshotTestHelper.Verify(command);

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
