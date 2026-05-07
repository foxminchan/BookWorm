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

    [Before(Test)]
    public void SetUp()
    {
        _orderId = Guid.CreateVersion7();
        _loggerMock = new();
    }

    [Test]
    public async Task GivenDeleteBasketCompleteCommand_WhenPublished_ThenConsumerShouldConsumeItAndLogInformation()
    {
        // Arrange
        var command = new DeleteBasketCompleteCommand(_orderId, TotalMoney);
        var handler = new DeleteBasketCompleteCommandHandler(_loggerMock.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        await SnapshotTestHelper.VerifyCloudEvent(command);
    }
}
