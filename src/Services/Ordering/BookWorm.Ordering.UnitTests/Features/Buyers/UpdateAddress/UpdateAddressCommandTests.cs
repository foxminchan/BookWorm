using System.Security.Claims;
using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Features.Buyers.UpdateAddress;
using BookWorm.Ordering.UnitTests.Fakers;
using BookWorm.ServiceDefaults.Keycloak;
using BookWorm.SharedKernel.Exceptions;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.UpdateAddress;

public sealed class UpdateAddressCommandTests
{
    private readonly Buyer _buyer;
    private readonly Mock<IBuyerRepository> _buyerRepositoryMock;
    private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private readonly UpdateAddressCommand _command;
    private readonly UpdateAddressHandler _handler;
    private readonly Guid _userId;

    public UpdateAddressCommandTests()
    {
        _userId = Guid.CreateVersion7();
        _buyer = new BuyerFaker().Generate(1)[0];

        _buyerRepositoryMock = new();
        _claimsPrincipalMock = new();

        // Setup claim
        var claim = new Claim(KeycloakClaimTypes.Subject, _userId.ToString());
        _claimsPrincipalMock.Setup(x => x.FindFirst(KeycloakClaimTypes.Subject)).Returns(claim);

        _handler = new(_buyerRepositoryMock.Object, _claimsPrincipalMock.Object);

        _command = new("123 New Street", "New City", "New Province");
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdateAddress_ThenShouldUpdateBuyerAddress()
    {
        // Arrange
        _buyerRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_buyer);
        _buyerRepositoryMock
            .Setup(x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(_command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        _buyerRepositoryMock.Verify(
            x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
        _buyer.Address!.Street.ShouldBe(_command.Street);
        _buyer.Address.City.ShouldBe(_command.City);
        _buyer.Address.Province.ShouldBe(_command.Province);
    }

    [Test]
    public async Task GivenNonExistingBuyer_WhenHandlingUpdateAddress_ThenShouldThrowNotFoundException()
    {
        // Arrange
        _buyerRepositoryMock
            .Setup(x => x.GetByIdAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Buyer)null!);

        // Act
        var act = async () => await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Buyer with id {_userId} not found.");
        _buyerRepositoryMock.Verify(
            x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenInvalidUserId_WhenHandlingUpdateAddress_ThenShouldThrowArgumentException()
    {
        // Arrange
        var invalidClaim = new Claim(KeycloakClaimTypes.Subject, "not-a-guid");
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
            .Returns(invalidClaim);

        // Act
        var act = async () => await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<ArgumentException>();
        exception.Message.ShouldBe("Invalid Buyer ID format. (Parameter 'userId')");
        _buyerRepositoryMock.Verify(
            x => x.GetByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenNoSubjectClaim_WhenHandlingUpdateAddress_ThenShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
            .Returns((Claim)null!);

        // Act
        var act = async () => await _handler.Handle(_command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User is not authenticated");
        _buyerRepositoryMock.Verify(
            x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
