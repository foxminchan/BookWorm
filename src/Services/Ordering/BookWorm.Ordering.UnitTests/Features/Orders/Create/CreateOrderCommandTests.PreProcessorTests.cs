using BookWorm.Basket.Grpc.Services;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Exceptions;
using BookWorm.Ordering.Features.Orders.Create;
using BookWorm.Ordering.Grpc.Services.Basket;
using BookWorm.Ordering.Grpc.Services.Book;
using BookWorm.Ordering.Infrastructure.Services;

namespace BookWorm.Ordering.UnitTests.Features.Orders.Create;

public sealed class CreateOrderPreProcessorTests
{
    private readonly Mock<IBasketService> _basketServiceMock;
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly CreateOrderCommand _command;
    private readonly CreateOrderPreProcessor _handler;

    public CreateOrderPreProcessorTests()
    {
        _basketServiceMock = new();
        _bookServiceMock = new();

        var basketMetadata = new BasketMetadata(_basketServiceMock.Object, _bookServiceMock.Object);

        _handler = new(basketMetadata);
        _command = new();
    }

    [Test]
    public async Task GivenValidBasketItems_WhenProcessing_ThenShouldPopulateOrderItems()
    {
        // Arrange
        var basketResponse = new BasketResponse
        {
            Id = Guid.CreateVersion7().ToString(),
            Items =
            {
                new Item { Id = Guid.CreateVersion7().ToString(), Quantity = 2 },
                new Item { Id = Guid.CreateVersion7().ToString(), Quantity = 1 },
            },
        };

        var booksResponse = new BooksResponse
        {
            Books =
            {
                new BookResponse
                {
                    Id = basketResponse.Items[0].Id,
                    Name = "Book 1",
                    Price = 10.99m,
                },
                new BookResponse
                {
                    Id = basketResponse.Items[1].Id,
                    Name = "Book 2",
                    Price = 12.99m,
                    PriceSale = 10.99m,
                },
            },
        };

        _basketServiceMock
            .Setup(x => x.GetBasket(It.IsAny<CancellationToken>()))
            .ReturnsAsync(basketResponse);

        _bookServiceMock
            .Setup(x =>
                x.GetBooksByIdsAsync(
                    It.Is<IEnumerable<string>>(ids =>
                        new HashSet<string>(ids).SetEquals(
                            new[] { basketResponse.Items[0].Id, basketResponse.Items[1].Id }
                        )
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(booksResponse);

        // Act
        await _handler.Handle(_command, (_, _) => new(Guid.Empty), CancellationToken.None);

        // Assert
        _command.Items.Count.ShouldBe(2);

        var book1Item = _command.Items.First(i =>
            i.BookId == Guid.Parse(booksResponse.Books[0].Id)
        );
        book1Item.Quantity.ShouldBe(2);
        book1Item.Price.ShouldBe(10.99m);

        var book2Item = _command.Items.First(i =>
            i.BookId == Guid.Parse(booksResponse.Books[1].Id)
        );
        book2Item.Quantity.ShouldBe(1);
        book2Item.Price.ShouldBe(10.99m);
    }

    [Test]
    public async Task GivenMissingBook_WhenProcessing_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var basketResponse = new BasketResponse
        {
            Id = "user123",
            Items =
            {
                new Item { Id = "book1", Quantity = 2 },
            },
        };

        var booksResponse = new BooksResponse(); // Empty response, no books found

        _basketServiceMock
            .Setup(x => x.GetBasket(It.IsAny<CancellationToken>()))
            .ReturnsAsync(basketResponse);

        _bookServiceMock
            .Setup(x =>
                x.GetBooksByIdsAsync(
                    It.Is<IEnumerable<string>>(ids => ids.Contains("book1")),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(booksResponse);

        // Act
        var act = async () =>
            await _handler.Handle(_command, (_, _) => new(Guid.Empty), CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe("BookResponse with id book1 not found.");
    }
}
