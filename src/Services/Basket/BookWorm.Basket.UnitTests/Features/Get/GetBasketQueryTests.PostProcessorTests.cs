using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Get;
using BookWorm.Basket.Grpc.Services.Book;
using BookWorm.Catalog.Grpc.Services;

namespace BookWorm.Basket.UnitTests.Features.Get;

public sealed class GetBasketPostProcessorTests
{
    private readonly CustomerBasketDto _basketDto;
    private readonly List<string> _bookIds;
    private readonly List<GetBookResponse> _bookResponses;
    private readonly Mock<IBookService> _bookServiceMock;
    private readonly GetBasketPostProcessor _handler;

    public GetBasketPostProcessorTests()
    {
        _bookIds = [Guid.CreateVersion7().ToString(), Guid.CreateVersion7().ToString()];

        _basketDto = new(
            Guid.CreateVersion7().ToString(),
            [new(_bookIds[0], 2), new(_bookIds[1], 1)]
        );

        _bookResponses =
        [
            new()
            {
                Id = _bookIds[0],
                Name = "Book 1",
                Price = 19.99m,
                PriceSale = 15.99m,
            },
            new()
            {
                Id = _bookIds[1],
                Name = "Book 2",
                Price = 29.99m,
                PriceSale = 0,
            },
        ];

        _bookServiceMock = new();
        _handler = new(_bookServiceMock.Object);
    }

    [Test]
    public async Task GivenValidBasketWithItems_WhenProcessing_ThenShouldEnrichItemsWithBookDetails()
    {
        // Arrange
        var query = new GetBasketQuery();
        var booksResponse = new GetBooksResponse { Books = { _bookResponses } };
        _bookServiceMock
            .Setup(x => x.GetBooksByIdsAsync(_bookIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booksResponse);

        // Act
        await _handler.Handle(query, (_, _) => new(_basketDto), CancellationToken.None);

        // Assert
        _bookServiceMock.Verify(
            x => x.GetBooksByIdsAsync(_bookIds, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _basketDto.Items[0].Name.ShouldBe("Book 1");
        _basketDto.Items[0].Price.ShouldBe(19.99m);
        _basketDto.Items[0].PriceSale.ShouldBe(15.99m);
        _basketDto.Items[1].Name.ShouldBe("Book 2");
        _basketDto.Items[1].Price.ShouldBe(29.99m);
        _basketDto.Items[1].PriceSale.ShouldBe(0);
        _basketDto.Items[0].Quantity.ShouldBe(2);
        _basketDto.Items[1].Quantity.ShouldBe(1);
    }

    [Test]
    public async Task GivenValidBasketWithItems_WhenProcessing_ThenShouldUpdateItemsProperty()
    {
        // Arrange
        var query = new GetBasketQuery();
        var originalBasketDto = new CustomerBasketDto(
            Guid.CreateVersion7().ToString(),
            [new(_bookIds[0], 3), new(_bookIds[1], 5)]
        );
        var booksResponse = new GetBooksResponse { Books = { _bookResponses } };
        _bookServiceMock
            .Setup(x => x.GetBooksByIdsAsync(_bookIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booksResponse);

        // Act
        await _handler.Handle(query, (_, _) => new(originalBasketDto), CancellationToken.None);

        // Assert
        var updatedItems = originalBasketDto.Items;
        updatedItems.ShouldNotBeNull();
        updatedItems.Count.ShouldBe(2);
        updatedItems[0].Name.ShouldBe("Book 1");
        updatedItems[1].Name.ShouldBe("Book 2");
        updatedItems[0].Quantity.ShouldBe(3);
        updatedItems[1].Quantity.ShouldBe(5);
    }

    [Test]
    public async Task GivenBasketWithNoMatchingBooks_WhenProcessing_ThenShouldNotUpdateItems()
    {
        // Arrange
        var query = new GetBasketQuery();
        var booksResponse = new GetBooksResponse();
        _bookServiceMock
            .Setup(x => x.GetBooksByIdsAsync(_bookIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booksResponse);

        // Act
        await _handler.Handle(query, (_, _) => new(_basketDto), CancellationToken.None);

        // Assert
        _basketDto.Items[0].Name.ShouldBeNull();
        _basketDto.Items[1].Name.ShouldBeNull();
    }

    [Test]
    public async Task GivenEmptyBasket_WhenProcessing_ThenShouldNotCallBookService()
    {
        // Arrange
        var query = new GetBasketQuery();
        var emptyBasket = new CustomerBasketDto(Guid.CreateVersion7().ToString(), []);

        // Act
        await _handler.Handle(query, (_, _) => new(emptyBasket), CancellationToken.None);

        // Assert
        _bookServiceMock.Verify(
            x => x.GetBooksByIdsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
