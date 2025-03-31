using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Exceptions;
using BookWorm.Basket.Features.Create;
using BookWorm.Basket.UnitTests.Fakers;
using BookWorm.ServiceDefaults.Keycloak;

namespace BookWorm.Basket.UnitTests.Features.Create;

public sealed class CreateBasketCommandTests
{
    private readonly CreateBasketCommandFaker _faker;
    private readonly CreateBasketHandler _handler;
    private readonly Mock<IBasketRepository> _mockBasketRepository;
    private readonly string _userId;

    public CreateBasketCommandTests()
    {
        _mockBasketRepository = new();
        var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
        _userId = "user-123";
        _faker = new();

        mockClaimsPrincipal
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
            .Returns(new Claim(KeycloakClaimTypes.Subject, _userId));

        _handler = new(_mockBasketRepository.Object, mockClaimsPrincipal.Object);
    }

    [Test]
    public async Task GivenValidCommand_WhenRepositoryReturnsNull_ThenShouldThrowException()
    {
        // Arrange
        var command = _faker.Generate();

        _mockBasketRepository
            .Setup(x => x.UpdateBasketAsync(It.IsAny<CustomerBasket>()))
            .ReturnsAsync((CustomerBasket)null!);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<BasketCreatedException>();
        exception.Message.ShouldBe("An error occurred while creating the basket.");
        _mockBasketRepository.Verify(
            x =>
                x.UpdateBasketAsync(
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
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
            .Returns((Claim)default!);

        var handler = new CreateBasketHandler(
            _mockBasketRepository.Object,
            mockEmptyClaimsPrincipal.Object
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User is not authenticated.");
        _mockBasketRepository.Verify(
            x => x.UpdateBasketAsync(It.IsAny<CustomerBasket>()),
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
            .Setup(x => x.UpdateBasketAsync(It.IsAny<CustomerBasket>()))
            .ReturnsAsync(basket);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(basket.Id);
        _mockBasketRepository.Verify(
            x =>
                x.UpdateBasketAsync(
                    It.Is<CustomerBasket>(b =>
                        b.Id == _userId && b.Items.Count == command.Items.Count
                    )
                ),
            Times.Once
        );
    }
}
