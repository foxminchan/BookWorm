using BookWorm.Basket.Domain;
using BookWorm.Basket.IntegrationEvents.EventHandlers;
using BookWorm.Chassis.EventBus.Idempotency;
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

    [Test]
    public async Task GivenDuplicatePlaceOrderCommand_WhenConsuming_ThenShouldReturnCompleteEventWithoutDeletingAgain()
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

        var guardMock = new Mock<IIntegrationEventIdempotencyGuard>();
        guardMock
            .Setup(x =>
                x.TryAcquireAsync(
                    orderId,
                    nameof(PlaceOrderCommandHandler),
                    null,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);

        var handler = new PlaceOrderCommandHandler(_repositoryMock.Object, guardMock.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var publishedEvent = result.ShouldBeOfType<BasketDeletedCompleteIntegrationEvent>();
        publishedEvent.OrderId.ShouldBe(orderId);
        publishedEvent.BasketId.ShouldBe(basketId);
        publishedEvent.TotalMoney.ShouldBe(totalMoney);
        _repositoryMock.Verify(x => x.DeleteBasketAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GivenRepositoryThrowsWithIdempotencyGuard_WhenConsuming_ThenShouldReleaseGuardAndRethrow()
    {
        // Arrange
        var basketId = Guid.CreateVersion7();
        var orderId = Guid.CreateVersion7();
        var command = new PlaceOrderCommand(
            basketId,
            "John Doe",
            "john@example.com",
            orderId,
            99.99m
        );

        var guardMock = new Mock<IIntegrationEventIdempotencyGuard>();
        guardMock
            .Setup(x =>
                x.TryAcquireAsync(
                    orderId,
                    nameof(PlaceOrderCommandHandler),
                    null,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.DeleteBasketAsync(basketId.ToString()))
            .ThrowsAsync(new InvalidOperationException("Redis failure"));

        var handler = new PlaceOrderCommandHandler(_repositoryMock.Object, guardMock.Object);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
        guardMock.Verify(
            x => x.ReleaseAsync(orderId, nameof(PlaceOrderCommandHandler), CancellationToken.None),
            Times.Once
        );
    }

    [Test]
    public async Task GivenRepositoryThrowsWithoutIdempotencyGuard_WhenConsuming_ThenShouldRethrow()
    {
        // Arrange
        var basketId = Guid.CreateVersion7();
        var orderId = Guid.CreateVersion7();
        var command = new PlaceOrderCommand(
            basketId,
            "John Doe",
            "john@example.com",
            orderId,
            99.99m
        );

        _repositoryMock
            .Setup(x => x.DeleteBasketAsync(basketId.ToString()))
            .ThrowsAsync(new InvalidOperationException("Redis failure"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<InvalidOperationException>();
    }
}
