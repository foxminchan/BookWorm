using BookWorm.Basket.UnitTests.Helpers;
using Microsoft.Extensions.Logging;

namespace BookWorm.Basket.UnitTests.Services;

public sealed class BasketServiceTests
{
    private readonly BasketService _basketService;
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly Mock<IRedisService> _redisServiceMock;

    public BasketServiceTests()
    {
        _redisServiceMock = new();
        _bookServiceMock = new();
        Mock<ILogger<BasketService>> loggerMock = new();
        _basketService = new(_redisServiceMock.Object, _bookServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetBasket_ShouldReturnBasketResponse_WhenBasketExists()
    {
        // Arrange
        var basketId = Guid.NewGuid().ToString();
        var basket = new Basket.Domain.Basket(Guid.NewGuid(), [new(Guid.NewGuid(), 1)]);

        _redisServiceMock
            .Setup(x => x.HashGetAsync<Basket.Domain.Basket?>(nameof(Basket.Domain.Basket), basketId))
            .ReturnsAsync(basket);

        var book = new BookItem(Guid.NewGuid(), "Test Book", 10.0m, 8.0m);

        _bookServiceMock
            .Setup(x => x.GetBookAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var request = new BasketRequest { BasketId = basketId };
        var context = new TestServerCallContext();

        // Act
        var response = await _basketService.GetBasket(request, context);

        // Assert
        response.Should().NotBeNull();
        response.BasketId.Should().Be(basket.AccountId.ToString());
        response.Books.Should().HaveCount(1);
        response.TotalPrice.Should().Be(8.0);
    }

    [Fact]
    public async Task GetBasket_ShouldThrowRpcException_WhenBasketNotFound()
    {
        // Arrange
        var basketId = Guid.NewGuid().ToString();

        _redisServiceMock
            .Setup(x => x.HashGetAsync<Basket.Domain.Basket?>(nameof(Basket.Domain.Basket), basketId))
            .ReturnsAsync((Basket.Domain.Basket?)null);

        var request = new BasketRequest { BasketId = basketId };
        var context = new TestServerCallContext();

        // Act
        Func<Task> act = async () => await _basketService.GetBasket(request, context);

        // Assert
        await act.Should().ThrowAsync<RpcException>();
    }
}
