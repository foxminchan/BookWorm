using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Features.Update;
using BookWorm.Basket.UnitTests.Fakers;
using BookWorm.Chassis.Exceptions;
using Mediator;

namespace BookWorm.Basket.UnitTests.Features.Update;

public sealed class UpdateBasketCommandTests
{
    private CustomerBasket _customerBasket = null!;
    private UpdateBasketHandler _handler = null!;
    private Mock<IBasketRepository> _repositoryMock = null!;
    private string _userId = null!;

    [Before(Test)]
    public void Setup()
    {
        _userId = Guid.CreateVersion7().ToString();
        _customerBasket = new CustomerBasketFaker().Generate()[0];
        _repositoryMock = new();
        Mock<ClaimsPrincipal> claimsPrincipalMock = new();

        var claim = new Claim(ClaimTypes.NameIdentifier, _userId);
        claimsPrincipalMock.Setup(x => x.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);

        _handler = new(_repositoryMock.Object, claimsPrincipalMock.Object);
    }

    [Test]
    public async Task GivenExistingBasket_WhenHandlingUpdateCommand_ThenShouldCallUpdateBasketAsync()
    {
        // Arrange
        var command = new UpdateBasketCommandFaker().Generate();

        _repositoryMock.Setup(x => x.GetBasketAsync(_userId)).ReturnsAsync(_customerBasket);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(x => x.CreateOrUpdateBasketAsync(_customerBasket), Times.Once);
    }

    [Test]
    public async Task GivenNonExistingBasket_WhenHandlingUpdateCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateBasketCommandFaker().Generate();

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None)
        );
        exception.Message.ShouldBe($"CustomerBasket with id {_userId} not found.");
        _repositoryMock.Verify(
            x => x.CreateOrUpdateBasketAsync(It.IsAny<CustomerBasket>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenMissingUserId_WhenHandlingUpdateCommand_ThenShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateBasketCommandFaker().Generate();
        Mock<ClaimsPrincipal> claimsPrincipalMock = new();

        // Set up the claim using the ClaimTypes.NameIdentifier
        // and ensure GetClaimValue extension method will work
        claimsPrincipalMock
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns((Claim?)null);

        var handler = new UpdateBasketHandler(_repositoryMock.Object, claimsPrincipalMock.Object);

        // Act & Assert
        var exception = await Should.ThrowAsync<UnauthorizedAccessException>(async () =>
            await handler.Handle(command, CancellationToken.None)
        );
        exception.Message.ShouldBe("User is not authenticated.");
        _repositoryMock.Verify(
            x => x.CreateOrUpdateBasketAsync(It.IsAny<CustomerBasket>()),
            Times.Never
        );
    }
}
