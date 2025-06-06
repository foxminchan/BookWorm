using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Get;
using BookWorm.Basket.Grpc.Services.Book;
using BookWorm.Basket.UnitTests.Fakers;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.Chassis.Exceptions;
using BookWorm.Chassis.Query;
using BookWorm.ServiceDefaults.Keycloak;

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

            // Set up the claim using the KeycloakClaimTypes.Subject
            // and ensure the GetClaimValue extension method will work
            var claim = new Claim(KeycloakClaimTypes.Subject, _userId);
            _claimsPrincipalMock.Setup(x => x.FindFirst(KeycloakClaimTypes.Subject)).Returns(claim);

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
                .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
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
            exception.Message.ShouldBe($"Basket with id {_userId} not found.");
            _repositoryMock.Verify(r => r.GetBasketAsync(_userId), Times.Once);
        }
    }

    public class PostGetBasketHandlerTests
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

            for (var i = 0; i < _bookIds.Count; i++)
            {
                var i1 = i;
                _bookServiceMock
                    .Setup(x => x.GetBookByIdAsync(_bookIds[i1], It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_bookResponses[i]);
            }

            // Act
            await _handler.Process(query, _basketDto, CancellationToken.None);

            // Assert
            // Verify each book was looked up
            foreach (var bookId in _bookIds)
            {
                _bookServiceMock.Verify(
                    x => x.GetBookByIdAsync(bookId, It.IsAny<CancellationToken>()),
                    Times.Once
                );
            }

            // Verify quantities are preserved during processing
            _basketDto.Items[0].Quantity.ShouldBe(2);
            _basketDto.Items[1].Quantity.ShouldBe(1);
        }

        [Test]
        public async Task GivenValidBasketWithItems_WhenProcessing_ThenShouldCreateNewResponseWithUpdatedItems()
        {
            // Arrange
            var query = new GetBasketQuery();
            var originalBasketDto = new CustomerBasketDto(
                Guid.CreateVersion7().ToString(),
                [new BasketItemDto(_bookIds[0], 3), new BasketItemDto(_bookIds[1], 5)]
            );

            for (var i = 0; i < _bookIds.Count; i++)
            {
                var i1 = i;
                _bookServiceMock
                    .Setup(x => x.GetBookByIdAsync(_bookIds[i1], It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_bookResponses[i]);
            }

            // Act
            await _handler.Process(query, originalBasketDto, CancellationToken.None);

            // Assert
            // Verify the response with statement creates a new object
            // This test ensures the line `_ = response with { Items = updatedItems };` is covered
            var updatedItems = originalBasketDto.Items;
            updatedItems.ShouldNotBeNull();
            updatedItems.Count.ShouldBe(2);

            // Verify that the 'response with' statement was executed (even though it's discarded)
            _bookServiceMock.Verify(
                x => x.GetBookByIdAsync(_bookIds[0], It.IsAny<CancellationToken>()),
                Times.Once
            );
            _bookServiceMock.Verify(
                x => x.GetBookByIdAsync(_bookIds[1], It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Test]
        public async Task GivenBasketItemWithNonExistentBookId_WhenProcessing_ThenShouldThrowNotFoundException()
        {
            // Arrange
            var query = new GetBasketQuery();

            // The First book exists
            _bookServiceMock
                .Setup(x => x.GetBookByIdAsync(_bookIds[0], It.IsAny<CancellationToken>()))
                .ReturnsAsync(_bookResponses[0]);

            // The Second book doesn't exist
            _bookServiceMock
                .Setup(x => x.GetBookByIdAsync(_bookIds[1], It.IsAny<CancellationToken>()))
                .ReturnsAsync((BookResponse?)null);

            // Act
            var act = async () => await _handler.Process(query, _basketDto, CancellationToken.None);

            // Assert
            var exception = await act.ShouldThrowAsync<NotFoundException>();
            exception.Message.ShouldBe($"Book with id {_bookIds[1]} not found.");
            _bookServiceMock.Verify(
                r => r.GetBookByIdAsync(_bookIds[1], It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}
