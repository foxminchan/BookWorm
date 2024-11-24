using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Get;
using BookWorm.Basket.UnitTests.Helpers;

namespace BookWorm.Basket.UnitTests.Services;

public sealed class BasketServiceTests
{
    private readonly BasketService _basketService;
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly Mock<ISender> _senderMock;

    public BasketServiceTests()
    {
        _senderMock = new();
        _bookServiceMock = new();
        Mock<ILogger<BasketService>> loggerMock = new();
        _basketService = new(_senderMock.Object, _bookServiceMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task GetBasket_ShouldReturnBasketResponse_WhenBasketExists()
    {
        // Arrange
        var basket = new BasketDto(
            Guid.NewGuid(),
            [new(Guid.NewGuid(), "Dummy Book", 10, 15m, 12m)],
            120m
        );

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetBasketQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);

        var book = new BookItem(Guid.NewGuid(), "Test Book", 10.0m, 8.0m);

        _bookServiceMock
            .Setup(x => x.GetBookAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var request = new Empty();
        var context = TestServerCallContext.Create();
        context.SetUserState("__HttpContext", new DefaultHttpContext());

        // Act
        var response = await _basketService.GetBasket(request, context);

        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBasket_ShouldReturnEmpty_WhenBasketNotFound()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<GetBasketQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BasketDto?)null!);

        var request = new Empty();
        var context = TestServerCallContext.Create();
        context.SetUserState("__HttpContext", new DefaultHttpContext());

        // Act
        var response = await _basketService.GetBasket(request, context);

        // Assert
        response
            .Should()
            .NotBeNull()
            .And.BeOfType<BasketResponse>()
            .Which.Books.Should()
            .BeEmpty();
    }
}
