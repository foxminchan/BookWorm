using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Contracts;
using MassTransit;

namespace BookWorm.Basket.UnitTests.IntegrationEvents;

public sealed class PlaceOrderCommandHandlerTests
{
    private readonly Mock<ConsumeContext<PlaceOrderCommand>> _contextMock = new();
    private readonly PlaceOrderCommandHandler _handler;
    private readonly Mock<IBasketRepository> _repositoryMock = new();

    public PlaceOrderCommandHandlerTests()
    {
        _contextMock.Setup(x => x.CancellationToken).Returns(CancellationToken.None);

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenBasketDeletedSuccessfully_WhenConsuming_ThenShouldPublishCompleteEvent()
    {
        // Arrange
        var basketId = Guid.CreateVersion7();
        var orderId = Guid.CreateVersion7();
        const decimal totalMoney = 99.99m;

        var command = new PlaceOrderCommand(
            basketId,
            "John Doe",
            "john@example.com",
            orderId,
            totalMoney
        );

        _contextMock.Setup(x => x.Message).Returns(command);

        _repositoryMock.Setup(x => x.DeleteBasketAsync(basketId.ToString())).ReturnsAsync(true);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        _contextMock.Verify(
            x =>
                x.Publish(
                    It.Is<BasketDeletedCompleteIntegrationEvent>(e =>
                        e.OrderId == orderId && e.BasketId == basketId && e.TotalMoney == totalMoney
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _contextMock.Verify(
            x =>
                x.Publish(
                    It.IsAny<BasketDeletedFailedIntegrationEvent>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task GivenBasketDeleteFailed_WhenConsuming_ThenShouldPublishFailedEvent()
    {
        // Arrange
        var basketId = Guid.CreateVersion7();
        var orderId = Guid.CreateVersion7();
        const string email = "john@example.com";
        const decimal totalMoney = 50.00m;

        var command = new PlaceOrderCommand(basketId, "John Doe", email, orderId, totalMoney);

        _contextMock.Setup(x => x.Message).Returns(command);

        _repositoryMock.Setup(x => x.DeleteBasketAsync(basketId.ToString())).ReturnsAsync(false);

        // Act
        await _handler.Consume(_contextMock.Object);

        // Assert
        _contextMock.Verify(
            x =>
                x.Publish(
                    It.Is<BasketDeletedFailedIntegrationEvent>(e =>
                        e.OrderId == orderId
                        && e.BasketId == basketId
                        && e.Email == email
                        && e.TotalMoney == totalMoney
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _contextMock.Verify(
            x =>
                x.Publish(
                    It.IsAny<BasketDeletedCompleteIntegrationEvent>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}
