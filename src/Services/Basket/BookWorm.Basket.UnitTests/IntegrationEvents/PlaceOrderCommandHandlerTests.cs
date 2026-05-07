using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Contracts;
using Wolverine;

namespace BookWorm.Basket.UnitTests.IntegrationEvents;

public sealed class PlaceOrderCommandHandlerTests
{
    private readonly Mock<IMessageBus> _busMock = new();
    private readonly PlaceOrderCommandHandler _handler;
    private readonly Mock<IBasketRepository> _repositoryMock = new();

    public PlaceOrderCommandHandlerTests()
    {
        _handler = new(_repositoryMock.Object, _busMock.Object);
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

        _repositoryMock.Setup(x => x.DeleteBasketAsync(basketId.ToString())).ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _busMock.Verify(
            x =>
                x.PublishAsync(
                    It.Is<BasketDeletedCompleteIntegrationEvent>(e =>
                        e.OrderId == orderId && e.BasketId == basketId && e.TotalMoney == totalMoney
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
        _busMock.Verify(
            x =>
                x.PublishAsync(
                    It.IsAny<BasketDeletedFailedIntegrationEvent>(),
                    It.IsAny<DeliveryOptions>()
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

        _repositoryMock.Setup(x => x.DeleteBasketAsync(basketId.ToString())).ReturnsAsync(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _busMock.Verify(
            x =>
                x.PublishAsync(
                    It.Is<BasketDeletedFailedIntegrationEvent>(e =>
                        e.OrderId == orderId
                        && e.BasketId == basketId
                        && e.Email == email
                        && e.TotalMoney == totalMoney
                    ),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Once
        );
        _busMock.Verify(
            x =>
                x.PublishAsync(
                    It.IsAny<BasketDeletedCompleteIntegrationEvent>(),
                    It.IsAny<DeliveryOptions>()
                ),
            Times.Never
        );
    }
}
