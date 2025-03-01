using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Features.Delete;
using BookWorm.Basket.UnitTests.Fakers;
using BookWorm.ServiceDefaults.Keycloak;
using BookWorm.SharedKernel.Exceptions;
using MediatR;

namespace BookWorm.Basket.UnitTests.Features.Delete;

public sealed class DeleteBasketCommandTest
{
    private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private readonly CustomerBasket _customerBasket;
    private readonly DeleteBasketHandler _handler;
    private readonly Mock<IBasketRepository> _repositoryMock;
    private readonly string _userId;

    public DeleteBasketCommandTest()
    {
        _userId = Guid.NewGuid().ToString();
        _customerBasket = new CustomerBasketFaker().Generate().First();

        _repositoryMock = new();
        _claimsPrincipalMock = new();

        // Set up the claim using the KeycloakClaimTypes.Subject
        // and ensure GetClaimValue extension method will work
        var claim = new Claim(KeycloakClaimTypes.Subject, _userId);
        _claimsPrincipalMock.Setup(x => x.FindFirst(KeycloakClaimTypes.Subject)).Returns(claim);

        _handler = new(_repositoryMock.Object, _claimsPrincipalMock.Object);
    }

    [Test]
    public async Task GivenExistingBasket_WhenHandlingDeleteCommand_ThenShouldCallDeleteBasketAsync()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetBasketAsync(_userId)).ReturnsAsync(_customerBasket);

        _repositoryMock.Setup(x => x.DeleteBasketAsync(_userId)).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(new(), CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(x => x.DeleteBasketAsync(_userId), Times.Once);
    }

    [Test]
    public async Task GivenNonExistingBasket_WhenHandlingDeleteCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetBasketAsync(_userId)).ReturnsAsync((CustomerBasket)null!);

        // Act
        var act = async () => await _handler.Handle(new(), CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Basket with id {_userId} not found.");
        _repositoryMock.Verify(x => x.DeleteBasketAsync(_userId), Times.Never);
    }

    [Test]
    public async Task GivenEmptyUserId_WhenHandlingDeleteCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
            .Returns((Claim)null!);

        // Act
        var act = async () => await _handler.Handle(new(), CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User is not authenticated.");
        _repositoryMock.Verify(x => x.DeleteBasketAsync(It.IsAny<string>()), Times.Never);
    }
}
