using BookWorm.Ordering.Domain.BuyerAggregate;
using BookWorm.Ordering.Features.Buyers.Create;

namespace BookWorm.Ordering.UnitTests.Application.Buyers;

public sealed class CreateBuyerHandlerTests
{
    private readonly CreateBuyerHandler _handler;
    private readonly Mock<IIdentityService> _identityServiceMock = new();
    private readonly Mock<IRepository<Buyer>> _repositoryMock = new();

    public CreateBuyerHandlerTests()
    {
        _handler = new(_repositoryMock.Object, _identityServiceMock.Object);
    }

    [Fact]
    public async Task GivenValidRequest_ShouldReturnResultWithBuyerId()
    {
        // Arrange
        var buyerId = Guid.NewGuid().ToString();
        const string fullName = "John Doe";
        var address = new Address("123 Main St", "City", "Province");
        var command = new CreateBuyerCommand(address.Street, address.City, address.Province);
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(buyerId);
        _identityServiceMock.Setup(x => x.GetFullName()).Returns(fullName);
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Buyer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Buyer(Guid.NewGuid(), fullName, address));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().NotBeEmpty();
    }

    [Theory]
    [CombinatorialData]
    public async Task GivenInvalidRequest_ShouldThrowArgumentNullException(
        [CombinatorialValues(null, "", " ")] string street,
        [CombinatorialValues(null, "", " ")] string city,
        [CombinatorialValues(null, "", " ")] string province)
    {
        // Arrange
        var command = new CreateBuyerCommand(street, city, province);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenInvalidBuyerId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "City", "Province");
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns((string?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GivenInvalidFullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "City", "Province");
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(Guid.NewGuid().ToString());
        _identityServiceMock.Setup(x => x.GetFullName()).Returns((string?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
