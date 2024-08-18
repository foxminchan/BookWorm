using BookWorm.Basket.Features.Create;
using BookWorm.Basket.Infrastructure.Redis;
using BookWorm.Shared.Identity;

namespace BookWorm.Basket.UnitTests.Application;

public sealed class CreateBasketHandlerTests
{
    private readonly CreateBasketHandler _handler;
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly Mock<IRedisService> _mockRedisService;

    public CreateBasketHandlerTests()
    {
        _mockRedisService = new();
        _mockIdentityService = new();
        _handler = new(_mockRedisService.Object, _mockIdentityService.Object);
    }

    [Fact]
    public async Task GivenValidCommand_ShouldThrowArgumentNullException_WhenCustomerIdIsNullOrEmpty()
    {
        // Arrange
        var command = new CreateBasketCommand(Guid.NewGuid(), 1);
        _mockIdentityService.Setup(x => x.GetUserIdentity())
            .Returns((string?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenValidCommand_ShouldUpdateBasket_WhenBasketExists()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        var command = new CreateBasketCommand(Guid.NewGuid(), 1);
        var existingBasket = new Basket.Domain.Basket(Guid.Parse(customerId), [new(command.BookId, command.Quantity)]);

        _mockIdentityService.Setup(x => x.GetUserIdentity())
            .Returns(customerId);

        _mockRedisService.Setup(x => x.HashGetAsync<Basket.Domain.Basket?>(nameof(Basket), customerId))
            .ReturnsAsync(existingBasket);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().Be(existingBasket.AccountId);
        _mockRedisService.Verify(x => x.HashSetAsync(nameof(Basket), customerId, existingBasket), Times.Once);
    }

    [Fact]
    public async Task GivenValidCommand_ShouldCreateNewBasket_WhenBasketDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid().ToString();
        var command = new CreateBasketCommand(Guid.NewGuid(), 1);
        var newBasket = new Basket.Domain.Basket(Guid.Parse(customerId), [new(command.BookId, command.Quantity)]);

        _mockIdentityService.Setup(x => x.GetUserIdentity())
            .Returns(customerId);

        _mockRedisService.Setup(x => x.HashGetAsync<Basket.Domain.Basket?>(nameof(Basket), customerId))
            .ReturnsAsync((Basket.Domain.Basket?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().Be(newBasket.AccountId);
    }

    [Theory]
    [CombinatorialData]
    public async Task GivenInvalidCommand_ShouldThrowArgumentNullException(
        [CombinatorialValues(null)] Guid bookId,
        [CombinatorialValues(0, -1)] int quantity)
    {
        // Arrange
        var command = new CreateBasketCommand(bookId, quantity);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
