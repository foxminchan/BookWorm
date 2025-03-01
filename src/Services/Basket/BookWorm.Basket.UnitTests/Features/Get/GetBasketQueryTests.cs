using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Get;
using BookWorm.Basket.Grpc.Services.Book;
using BookWorm.Basket.UnitTests.Fakers;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.ServiceDefaults.Keycloak;
using BookWorm.SharedKernel.Exceptions;

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
            _userId = Guid.NewGuid().ToString();
            _basket = new CustomerBasketFaker().Generate()[0];
            _claimsPrincipalMock = new();
            _repositoryMock = new();

            // Set up the claim using the KeycloakClaimTypes.Subject
            // and ensure GetClaimValue extension method will work
            var claim = new Claim(KeycloakClaimTypes.Subject, _userId);
            _claimsPrincipalMock.Setup(x => x.FindFirst(KeycloakClaimTypes.Subject)).Returns(claim);

            _handler = new(_repositoryMock.Object, _claimsPrincipalMock.Object);
        }

        [Test]
        public async Task GivenValidUserId_WhenHandling_ThenShouldReturnBasket()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetBasketAsync(_userId)).ReturnsAsync(_basket);

            // Act
            var result = await _handler.Handle(new(), CancellationToken.None);

            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(_basket.Id);
            result.Items.Count.ShouldBe(_basket.Items.Count);
            _repositoryMock.Verify(r => r.GetBasketAsync(_userId), Times.Once);
        }

        [Test]
        public async Task GivenMissingUserId_WhenHandling_ThenShouldThrowNotFoundException()
        {
            // Arrange
            _claimsPrincipalMock
                .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
                .Returns((Claim)null!);

            // Act
            var act = () => _handler.Handle(new(), CancellationToken.None);

            // Assert
            var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
            exception.Message.ShouldBe("User is not authenticated.");
            _repositoryMock.Verify(r => r.GetBasketAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GivenNonExistentBasket_WhenHandling_ThenShouldThrowNotFoundException()
        {
            // Arrange
            _repositoryMock
                .Setup(r => r.GetBasketAsync(_userId))
                .ReturnsAsync((CustomerBasket)null!);

            // Act
            var act = () => _handler.Handle(new(), CancellationToken.None);

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
            _bookIds = [Guid.NewGuid().ToString(), Guid.NewGuid().ToString()];

            _basketDto = new(
                Guid.NewGuid().ToString(),
                [new BasketItemDto(_bookIds[0], 2), new BasketItemDto(_bookIds[1], 1)]
            );

            _bookResponses =
            [
                new BookResponse
                {
                    Id = _bookIds[0],
                    Name = "Book 1",
                    Price = 19.99,
                    PriceSale = 15.99,
                },
                new BookResponse
                {
                    Id = _bookIds[1],
                    Name = "Book 2",
                    Price = 29.99,
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
            for (var i = 0; i < _bookIds.Count; i++)
            {
                var i1 = i;
                _bookServiceMock
                    .Setup(x => x.GetBookByIdAsync(_bookIds[i1], It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_bookResponses[i]);
            }

            // Act
            await _handler.Process(new(), _basketDto, CancellationToken.None);

            // Assert
            // Verify each book was looked up
            foreach (var bookId in _bookIds)
            {
                _bookServiceMock.Verify(
                    x => x.GetBookByIdAsync(bookId, It.IsAny<CancellationToken>()),
                    Times.Once
                );
            }
        }

        [Test]
        public async Task GivenBasketItemWithNonExistentBookId_WhenProcessing_ThenShouldThrowNotFoundException()
        {
            // Arrange
            // First book exists
            _bookServiceMock
                .Setup(x => x.GetBookByIdAsync(_bookIds[0], It.IsAny<CancellationToken>()))
                .ReturnsAsync(_bookResponses[0]);

            // Second book doesn't exist
            _bookServiceMock
                .Setup(x => x.GetBookByIdAsync(_bookIds[1], It.IsAny<CancellationToken>()))
                .ReturnsAsync((BookResponse)null!);

            // Act
            var act = async () => await _handler.Process(new(), _basketDto, CancellationToken.None);

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
