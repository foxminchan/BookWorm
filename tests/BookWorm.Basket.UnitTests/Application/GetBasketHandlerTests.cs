using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Get;
using BasketModel = BookWorm.Basket.Domain.Basket;

namespace BookWorm.Basket.UnitTests.Application;

public sealed class GetBasketHandlerTests
{
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly GetBasketHandler _handler;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IRedisService> _redisServiceMock;

    public GetBasketHandlerTests()
    {
        _redisServiceMock = new();
        _identityServiceMock = new();
        _bookServiceMock = new();
        _handler = new(
            _redisServiceMock.Object,
            _identityServiceMock.Object,
            _bookServiceMock.Object
        );
    }

    [Fact]
    public async Task GivenValidCustomerId_ShouldReturnBasketDto_WhenBasketExists()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        var basketItems = new List<BasketItem> { new(Guid.NewGuid(), 2) };
        var basket = new BasketModel(Guid.Parse(customerId), basketItems);
        var book = new BookItem(basketItems.First().Id, "Test Book", 10.0m, 8.0m);
        var basketDto = new BasketDto(
            basket.AccountId,
            [new(book.Id, book.Name, basketItems.First().Quantity, book.Price, book.PriceSale)],
            16.0m
        );

        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(customerId);
        _redisServiceMock
            .Setup(x => x.HashGetAsync<BasketModel?>(nameof(Basket), customerId))
            .ReturnsAsync(basket);
        _bookServiceMock
            .Setup(x => x.GetBookAsync(basketItems.First().Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var result = await _handler.Handle(new(), CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(basketDto);
    }

    [Fact]
    public async Task GivenValidCustomerId_ShouldReturnNull_WhenBasketDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();

        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(customerId);
        _redisServiceMock
            .Setup(x => x.HashGetAsync<BasketModel?>(nameof(Basket), customerId))
            .ReturnsAsync((BasketModel?)null);

        // Act
        var result = await _handler.Handle(new(), CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GivenInvalidCustomerId_ShouldThrowArgumentNullException()
    {
        // Arrange
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns<string>(null!);

        // Act
        Func<Task> act = async () => await _handler.Handle(new(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenValidCustomerId_ShouldThrowException_WhenBookServiceFails()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        var basketItems = new List<BasketItem> { new(Guid.NewGuid(), 2) };
        var basket = new BasketModel(Guid.Parse(customerId), basketItems);

        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(customerId);
        _redisServiceMock
            .Setup(x => x.HashGetAsync<BasketModel?>(nameof(Basket), customerId))
            .ReturnsAsync(basket);
        _bookServiceMock
            .Setup(x => x.GetBookAsync(basketItems.First().Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Book service error"));

        // Act
        Func<Task> act = async () => await _handler.Handle(new(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task GivenValidCustomerId_ShouldReturnEmptyBasketDto_WhenNoBooksInBasket()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        var basket = new BasketModel(Guid.Parse(customerId), []);
        var basketDto = new BasketDto(basket.AccountId, [], 0.0m);

        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(customerId);
        _redisServiceMock
            .Setup(x => x.HashGetAsync<BasketModel?>(nameof(Basket), customerId))
            .ReturnsAsync(basket);

        // Act
        var result = await _handler.Handle(new(), CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(basketDto);
    }
}
