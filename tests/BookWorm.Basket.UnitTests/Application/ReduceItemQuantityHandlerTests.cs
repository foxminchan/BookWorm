using BookWorm.Basket.Features.ReduceItemQuantity;
using BasketModel = BookWorm.Basket.Domain.Basket;

namespace BookWorm.Basket.UnitTests.Application;

public sealed class ReduceItemQuantityHandlerTests
{
    private readonly ReduceItemQuantityHandler _handler;
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly Mock<IRedisService> _mockRedisService;

    public ReduceItemQuantityHandlerTests()
    {
        _mockRedisService = new();
        _mockIdentityService = new();
        _handler = new(_mockRedisService.Object, _mockIdentityService.Object);
    }

    [Fact]
    public async Task GivenCommand_ShouldThrowArgumentNullException_WhenCustomerIdIsNullOrEmpty()
    {
        // Arrange
        var command = new ReduceItemQuantityCommand(Guid.NewGuid());
        _mockIdentityService.Setup(x => x.GetUserIdentity()).Returns((string?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenCommand_ShouldThrowNotFoundException_WhenBasketNotFound()
    {
        // Arrange
        var command = new ReduceItemQuantityCommand(Guid.NewGuid());
        var customerId = Guid.NewGuid().ToString();
        _mockIdentityService.Setup(x => x.GetUserIdentity()).Returns(customerId);
        _mockRedisService.Setup(x => x.HashGetAsync<BasketModel?>(nameof(Basket), customerId))
            .ReturnsAsync((BasketModel?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GivenCommand_ShouldReduceItemQuantity_WhenBasketExists()
    {
        // Arrange
        var command = new ReduceItemQuantityCommand(Guid.NewGuid());
        var customerId = Guid.NewGuid().ToString();
        var basketItems = new List<BasketItem> { new(command.BookId, 2) };
        var basket = new BasketModel(Guid.Parse(customerId), basketItems);
        _mockIdentityService.Setup(x => x.GetUserIdentity()).Returns(customerId);
        _mockRedisService.Setup(x => x.HashGetAsync<BasketModel?>(nameof(Basket), customerId))
            .ReturnsAsync(basket);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        basket.BasketItems.First().Quantity.Should().Be(1);
        _mockRedisService.Verify(x => x.HashSetAsync(nameof(Basket), customerId, basket), Times.Once);
    }

    [Fact]
    public async Task GivenCommand_ShouldRemoveItemFromBasket_WhenItemQuantityIsOne()
    {
        // Arrange
        var command = new ReduceItemQuantityCommand(Guid.NewGuid());
        var customerId = Guid.NewGuid().ToString();
        var basketItems = new List<BasketItem> { new(command.BookId, 1) };
        var basket = new BasketModel(Guid.Parse(customerId), basketItems);
        _mockIdentityService.Setup(x => x.GetUserIdentity()).Returns(customerId);
        _mockRedisService.Setup(x => x.HashGetAsync<BasketModel?>(nameof(Basket), customerId))
            .ReturnsAsync(basket);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        basket.BasketItems.Should().BeEmpty();
        _mockRedisService.Verify(x => x.HashSetAsync(nameof(Basket), customerId, basket), Times.Once);
    }
}
