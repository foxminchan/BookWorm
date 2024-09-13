using BookWorm.Ordering.Domain.BuyerAggregate;
using BookWorm.Ordering.Features.Buyers.Get;
using BookWorm.Ordering.UnitTests.Builder;

namespace BookWorm.Ordering.UnitTests.Application.Buyers;

public sealed class GetBuyerHandlerTests
{
    private readonly GetBuyerHandler _handler;
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IReadRepository<Buyer>> _repositoryMock = new();

    public GetBuyerHandlerTests()
    {
        _handler = new(_repositoryMock.Object, _identityServiceMock.Object);
    }

    [Fact]
    public async Task GivenValidBuyerIdAndUserIsNotAdmin_ShouldReturnBuyer_WhenHandleAsync()
    {
        // Arrange
        var buyer = BuyerBuilder.WithDefaultValues()[0];
        var buyerId = buyer.Id;

        _repositoryMock.Setup(x => x.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buyer);
        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(false);
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(buyerId.ToString());

        // Act
        var result = await _handler.Handle(new(buyerId), CancellationToken.None);

        // Assert
        result.Value.Should().Be(buyer);
        _repositoryMock.Verify(x => x.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()), Times.Once);
        _identityServiceMock.Verify(x => x.IsAdminRole(), Times.Once);
        _identityServiceMock.Verify(x => x.GetUserIdentity(), Times.Once);
    }

    [Fact]
    public async Task GivenValidBuyerIdAndUserIsAdmin_ShouldReturnBuyer_WhenHandleAsync()
    {
        // Arrange
        var buyer = BuyerBuilder.WithDefaultValues()[0];
        var buyerId = buyer.Id;

        _repositoryMock.Setup(x => x.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buyer);
        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(true);

        // Act
        var result = await _handler.Handle(new(buyerId), CancellationToken.None);

        // Assert
        result.Value.Should().Be(buyer);
        _repositoryMock.Verify(x => x.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()), Times.Once);
        _identityServiceMock.Verify(x => x.IsAdminRole(), Times.Once);
        _identityServiceMock.Verify(x => x.GetUserIdentity(), Times.Never);
    }

    [Fact]
    public async Task GivenInvalidBuyerIdAndUserIsNotAdmin_ShouldReturnNull_WhenHandleAsync()
    {
        // Arrange
        var buyerId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Buyer?)null);
        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(false);
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(buyerId.ToString());

        // Act
        var result = await _handler.Handle(new(buyerId), CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        _repositoryMock.Verify(x => x.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()), Times.Once);
        _identityServiceMock.Verify(x => x.IsAdminRole(), Times.Once);
        _identityServiceMock.Verify(x => x.GetUserIdentity(), Times.Once);
    }

    [Fact]
    public async Task GivenInvalidBuyerIdAndUserIsAdmin_ShouldReturnNull_WhenHandleAsync()
    {
        // Arrange
        var buyerId = Guid.NewGuid();

        _repositoryMock.Setup(x => x.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Buyer?)null);
        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(true);

        // Act
        var result = await _handler.Handle(new(buyerId), CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        _repositoryMock.Verify(x => x.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()), Times.Once);
        _identityServiceMock.Verify(x => x.IsAdminRole(), Times.Once);
        _identityServiceMock.Verify(x => x.GetUserIdentity(), Times.Never);
    }

    [Fact]
    public async Task GivenNullOrEmptyCustomerIdAndUserIsNotAdmin_ShouldReturnBadRequest_WhenHandleAsync()
    {
        // Arrange
        var buyerId = Guid.Empty;

        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(false);
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(string.Empty);

        // Act
        Func<Task> act = async () => await _handler.Handle(new(buyerId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _repositoryMock.Verify(x => x.GetByIdAsync(buyerId, It.IsAny<CancellationToken>()), Times.Never);
        _identityServiceMock.Verify(x => x.IsAdminRole(), Times.Once);
        _identityServiceMock.Verify(x => x.GetUserIdentity(), Times.Once);
    }
}
