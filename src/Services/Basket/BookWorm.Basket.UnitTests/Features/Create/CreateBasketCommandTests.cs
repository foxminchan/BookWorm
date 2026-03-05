using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Features.Create;
using BookWorm.Basket.Infrastructure.Exceptions;
using BookWorm.Basket.UnitTests.Fakers;

namespace BookWorm.Basket.UnitTests.Features.Create;

public sealed class CreateBasketCommandTests
{
    private CreateBasketCommandFaker _faker = null!;
    private CreateBasketHandler _handler = null!;
    private Mock<IBasketRepository> _mockBasketRepository = null!;
    private string _userId = null!;

    [Before(Test)]
    public void Setup()
    {
        _mockBasketRepository = new();
        var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
        _userId = "user-123";
        _faker = new();

        mockClaimsPrincipal
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, _userId));

        _handler = new(_mockBasketRepository.Object, mockClaimsPrincipal.Object);
    }

    [Test]
    public async Task GivenValidCommand_WhenRepositoryReturnsNull_ThenShouldThrowException()
    {
        // Arrange
        var command = _faker.Generate();

        _mockBasketRepository
            .Setup(x => x.CreateOrUpdateBasketAsync(It.IsAny<CustomerBasket>()))
            .ReturnsAsync((CustomerBasket)null!);

        // Act & Assert
        var exception = await Should.ThrowAsync<BasketCreatedException>(async () =>
            await _handler.Handle(command, CancellationToken.None)
        );
        exception.Message.ShouldBe("An error occurred while creating the basket.");
        _mockBasketRepository.Verify(
            x =>
                x.CreateOrUpdateBasketAsync(
                    It.Is<CustomerBasket>(b =>
                        b.Id == _userId && b.Items.Count == command.Items.Count
                    )
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenUserIdIsEmpty_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var command = _faker.Generate();
        var mockEmptyClaimsPrincipal = new Mock<ClaimsPrincipal>();

        // Return null when looking for the claim
        mockEmptyClaimsPrincipal
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns((Claim?)null);

        var handler = new CreateBasketHandler(
            _mockBasketRepository.Object,
            mockEmptyClaimsPrincipal.Object
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
            await handler.Handle(command, CancellationToken.None)
        );
        exception.Message.ShouldBe("User is not authenticated.");
        _mockBasketRepository.Verify(
            x => x.CreateOrUpdateBasketAsync(It.IsAny<CustomerBasket>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenValidRequest_ThenShouldReturnBasketId()
    {
        // Arrange
        var command = _faker.Generate();
        var basket = new CustomerBasketFaker().Generate(1)[0];

        _mockBasketRepository
            .Setup(x => x.CreateOrUpdateBasketAsync(It.IsAny<CustomerBasket>()))
            .ReturnsAsync(basket);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(basket.Id);
        _mockBasketRepository.Verify(
            x =>
                x.CreateOrUpdateBasketAsync(
                    It.Is<CustomerBasket>(b =>
                        b.Id == _userId && b.Items.Count == command.Items.Count
                    )
                ),
            Times.Once
        );
    }
}
