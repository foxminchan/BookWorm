using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Contracts;

namespace BookWorm.Basket.UnitTests.IntegrationEvents;

public sealed class PlaceOrderCommandHandlerTests
{
    private readonly PlaceOrderCommandHandler _handler;
    private readonly Mock<IBasketRepository> _repositoryMock = new();

    public PlaceOrderCommandHandlerTests()
    {
        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenBasketDeletedSuccessfully_WhenConsuming_ThenShouldReturnCompleteEvent()
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
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var publishedEvent = result.ShouldBeOfType<BasketDeletedCompleteIntegrationEvent>();
        publishedEvent.OrderId.ShouldBe(orderId);
        publishedEvent.BasketId.ShouldBe(basketId);
        publishedEvent.TotalMoney.ShouldBe(totalMoney);
    }

    [Test]
    public async Task GivenBasketDeleteFailed_WhenConsuming_ThenShouldReturnFailedEvent()
    {
        // Arrange
        var basketId = Guid.CreateVersion7();
        var orderId = Guid.CreateVersion7();
        const string email = "john@example.com";
        const decimal totalMoney = 50.00m;

        var command = new PlaceOrderCommand(basketId, "John Doe", email, orderId, totalMoney);

        _repositoryMock.Setup(x => x.DeleteBasketAsync(basketId.ToString())).ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var publishedEvent = result.ShouldBeOfType<BasketDeletedFailedIntegrationEvent>();
        publishedEvent.OrderId.ShouldBe(orderId);
        publishedEvent.BasketId.ShouldBe(basketId);
        publishedEvent.Email.ShouldBe(email);
        publishedEvent.TotalMoney.ShouldBe(totalMoney);
    }
}
