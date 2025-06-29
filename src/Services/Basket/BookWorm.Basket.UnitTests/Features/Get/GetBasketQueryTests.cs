using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Get;
using BookWorm.Basket.Grpc.Services.Book;
using BookWorm.Basket.UnitTests.Fakers;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Exceptions;
using BookWorm.Chassis.Query;

namespace BookWorm.Basket.UnitTests.Features.Get;

public static class GetBasketQueryTests
{
    public sealed class GetBasketHandlerTests
    {
        private readonly CustomerBasket _basket;
        private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
        private readonly GetBasketHandler _handler;
        private readonly Mock<IBasketRepository> _repositoryMock;
        private readonly string _userId;

        public GetBasketHandlerTests()
        {
            _userId = Guid.CreateVersion7().ToString();
            _basket = new CustomerBasketFaker().Generate()[0];
            _claimsPrincipalMock = new();
            _repositoryMock = new();

            // Set up the claim using the ClaimTypes.NameIdentifier
            // and ensure the GetClaimValue extension method will work
            var claim = new Claim(ClaimTypes.NameIdentifier, _userId);
            _claimsPrincipalMock.Setup(x => x.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);

            _handler = new(_repositoryMock.Object, _claimsPrincipalMock.Object);
        }

        [Test]
        public void GivenGetBasketQuery_WhenCreating_ThenShouldBeOfCorrectType()
        {
            // Act
            var query = new GetBasketQuery();

            // Assert
            query.ShouldNotBeNull();
            query.ShouldBeOfType<GetBasketQuery>();
            query.ShouldBeAssignableTo<IQuery<CustomerBasketDto>>();
        }

        [Test]
        public async Task GivenValidUserId_WhenHandling_ThenShouldReturnBasket()
        {
            // Arrange
            var query = new GetBasketQuery();

            _repositoryMock.Setup(r => r.GetBasketAsync(_userId)).ReturnsAsync(_basket);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(_basket.Id);
            result.Items.Count.ShouldBe(_basket.Items.Count);

            // Verify quantities are preserved by matching items
            var basketItemsArray = _basket.Items.ToArray();
            for (var i = 0; i < result.Items.Count; i++)
            {
                result.Items[i].Quantity.ShouldBe(basketItemsArray[i].Quantity);
            }

            _repositoryMock.Verify(r => r.GetBasketAsync(_userId), Times.Once);
        }

        [Test]
        public async Task GivenMissingUserId_WhenHandling_ThenShouldThrowNotFoundException()
        {
            // Arrange
            var query = new GetBasketQuery();

            _claimsPrincipalMock
                .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
                .Returns((Claim?)null);

            // Act
            var act = () => _handler.Handle(query, CancellationToken.None);

            // Assert
            var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
            exception.Message.ShouldBe("User is not authenticated.");
            _repositoryMock.Verify(r => r.GetBasketAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GivenNonExistentBasket_WhenHandling_ThenShouldThrowNotFoundException()
        {
            // Arrange
            var query = new GetBasketQuery();

            _repositoryMock
                .Setup(r => r.GetBasketAsync(_userId))
                .ReturnsAsync((CustomerBasket?)null);

            // Act
            var act = () => _handler.Handle(query, CancellationToken.None);

            // Assert
            var exception = await act.ShouldThrowAsync<NotFoundException>();
            exception.Message.ShouldBe($"CustomerBasket with id {_userId} not found.");
            _repositoryMock.Verify(r => r.GetBasketAsync(_userId), Times.Once);
        }

        [Test]
        public void GivenTwoGetBasketQueries_WhenComparing_ThenShouldBeEqual()
        {
            // Arrange
            var query1 = new GetBasketQuery();
            var query2 = new GetBasketQuery();

            // Act & Assert
            query1.ShouldBe(query2);
            query1.GetHashCode().ShouldBe(query2.GetHashCode());
        }

        [Test]
        public void GivenTwoGetBasketQueries_WhenCallingToString_ThenShouldReturnStringRepresentation()
        {
            // Arrange
            var query = new GetBasketQuery();

            // Act
            var result = query.ToString();

            // Assert
            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
            result.ShouldContain(nameof(GetBasketQuery));
        }

        [Test]
        public void GivenGetBasketQuery_WhenUsingWithExpression_ThenShouldCreateIdenticalCopy()
        {
            // Arrange
            var original = new GetBasketQuery();

            // Act
            var copy = original with
            { };

            // Assert
            copy.ShouldBe(original);
            copy.ShouldNotBeSameAs(original);
        }
    }

    public sealed class PostGetBasketHandlerTests
    {
        private readonly CustomerBasketDto _basketDto;
        private readonly List<string> _bookIds;
        private readonly List<BookResponse> _bookResponses;
        private readonly Mock<IBookService> _bookServiceMock;
        private readonly PostGetBasketHandler _handler;

        public PostGetBasketHandlerTests()
        {
            _bookIds = [Guid.CreateVersion7().ToString(), Guid.CreateVersion7().ToString()];

            _basketDto = new(
                Guid.CreateVersion7().ToString(),
                [new BasketItemDto(_bookIds[0], 2), new BasketItemDto(_bookIds[1], 1)]
            );

            _bookResponses =
            [
                new BookResponse
                {
                    Id = _bookIds[0],
                    Name = "Book 1",
                    Price = 19.99m,
                    PriceSale = 15.99m,
                },
                new BookResponse
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
            var booksResponse = new BooksResponse { Books = { _bookResponses } };
            _bookServiceMock
                .Setup(x => x.GetBooksByIdsAsync(_bookIds, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booksResponse);

            // Act
            await _handler.Process(query, _basketDto, CancellationToken.None);

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
                [new BasketItemDto(_bookIds[0], 3), new BasketItemDto(_bookIds[1], 5)]
            );
            var booksResponse = new BooksResponse { Books = { _bookResponses } };
            _bookServiceMock
                .Setup(x => x.GetBooksByIdsAsync(_bookIds, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booksResponse);

            // Act
            await _handler.Process(query, originalBasketDto, CancellationToken.None);

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
            var booksResponse = new BooksResponse();
            _bookServiceMock
                .Setup(x => x.GetBooksByIdsAsync(_bookIds, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booksResponse);

            // Act
            await _handler.Process(query, _basketDto, CancellationToken.None);

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
            await _handler.Process(query, emptyBasket, CancellationToken.None);

            // Assert
            _bookServiceMock.Verify(
                x => x.GetBooksByIdsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}
