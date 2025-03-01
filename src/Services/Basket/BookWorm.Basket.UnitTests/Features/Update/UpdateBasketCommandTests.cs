using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Features.Update;
using BookWorm.Basket.UnitTests.Fakers;
using BookWorm.ServiceDefaults.Keycloak;
using BookWorm.SharedKernel.Exceptions;
using MediatR;

namespace BookWorm.Basket.UnitTests.Features.Update;

public sealed class UpdateBasketCommandTests
{
    private readonly CustomerBasket _customerBasket;
    private readonly UpdateBasketHandler _handler;
    private readonly Mock<IBasketRepository> _repositoryMock;
    private readonly string _userId;

    public UpdateBasketCommandTests()
    {
        _userId = Guid.NewGuid().ToString();
        _customerBasket = new CustomerBasketFaker().Generate().First();
        _repositoryMock = new();
        Mock<ClaimsPrincipal> claimsPrincipalMock = new();

        // Set up the claim using the KeycloakClaimTypes.Subject
        // and ensure GetClaimValue extension method will work
        var claim = new Claim(KeycloakClaimTypes.Subject, _userId);
        claimsPrincipalMock.Setup(x => x.FindFirst(KeycloakClaimTypes.Subject)).Returns(claim);

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
        _repositoryMock.Verify(x => x.UpdateBasketAsync(_customerBasket), Times.Once);
    }

    [Test]
    public async Task GivenNonExistingBasket_WhenHandlingUpdateCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateBasketCommandFaker().Generate();

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Basket with id {_userId} not found.");
        _repositoryMock.Verify(x => x.UpdateBasketAsync(It.IsAny<CustomerBasket>()), Times.Never);
    }

    [Test]
    public async Task GivenMissingUserId_WhenHandlingUpdateCommand_ThenShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new UpdateBasketCommandFaker().Generate();
        Mock<ClaimsPrincipal> claimsPrincipalMock = new();

        // Set up the claim using the KeycloakClaimTypes.Subject
        // and ensure GetClaimValue extension method will work
        claimsPrincipalMock
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
            .Returns((Claim)null!);

        var handler = new UpdateBasketHandler(_repositoryMock.Object, claimsPrincipalMock.Object);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User is not authenticated.");
        _repositoryMock.Verify(x => x.UpdateBasketAsync(It.IsAny<CustomerBasket>()), Times.Never);
    }
}
