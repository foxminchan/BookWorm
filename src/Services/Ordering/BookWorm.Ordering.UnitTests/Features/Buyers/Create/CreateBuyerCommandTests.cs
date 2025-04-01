using System.Security.Claims;
using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Features.Buyers.Create;
using BookWorm.ServiceDefaults.Keycloak;
using BookWorm.SharedKernel.Repository;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.Create;

public sealed class CreateBuyerCommandTests
{
    private readonly Mock<IBuyerRepository> _buyerRepositoryMock;
    private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private readonly CreateBuyerHandler _handler;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly string _userName;

    public CreateBuyerCommandTests()
    {
        var userId = Guid.CreateVersion7();
        _userName = "Test User";

        _buyerRepositoryMock = new();
        _claimsPrincipalMock = new();
        _unitOfWorkMock = new();

        // Setup claims principal
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
            .Returns(new Claim(KeycloakClaimTypes.Subject, userId.ToString()));
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Name))
            .Returns(new Claim(KeycloakClaimTypes.Name, _userName));

        // Setup repository
        _buyerRepositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _handler = new(_buyerRepositoryMock.Object, _claimsPrincipalMock.Object);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateBuyer_ThenShouldCreateBuyerAndReturnId()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "Seattle", "Washington");
        var buyerId = Guid.CreateVersion7();
        Buyer? buyerEntity = null;

        _buyerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Buyer>(), It.IsAny<CancellationToken>()))
            .Callback<Buyer, CancellationToken>((buyer, _) => buyerEntity = buyer)
            .ReturnsAsync(
                (Buyer buyer, CancellationToken _) =>
                {
                    buyer.GetType().GetProperty("Id")?.SetValue(buyer, buyerId);
                    return buyer;
                }
            );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(buyerId);
        buyerEntity.ShouldNotBeNull();
        buyerEntity.Name.ShouldBe(_userName);
        buyerEntity.Address!.Street.ShouldBe("123 Main St");
        buyerEntity.Address.City.ShouldBe("Seattle");
        buyerEntity.Address.Province.ShouldBe("Washington");
        _buyerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Buyer>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenInvalidUserId_WhenHandlingCreateBuyer_ThenShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "Seattle", "Washington");
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
            .Returns(new Claim(KeycloakClaimTypes.Subject, "invalid-guid"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<ArgumentException>();
        exception.Message.ShouldBe("Invalid Buyer ID format. (Parameter 'userId')");
        _buyerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Buyer>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenMissingUserId_WhenHandlingCreateBuyer_ThenThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "Seattle", "Washington");
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Subject))
            .Returns((Claim)default!);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User is not authenticated.");
        _buyerRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Buyer>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenMissingName_WhenHandlingCreateBuyer_ThenShouldCreateBuyerWithAnonymousName()
    {
        // Arrange
        var command = new CreateBuyerCommand("123 Main St", "Seattle", "Washington");
        _claimsPrincipalMock
            .Setup(x => x.FindFirst(KeycloakClaimTypes.Name))
            .Returns((Claim)default!);

        Buyer? buyerEntity = null;
        _buyerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Buyer>(), It.IsAny<CancellationToken>()))
            .Callback<Buyer, CancellationToken>((buyer, _) => buyerEntity = buyer)
            .ReturnsAsync((Buyer buyer, CancellationToken _) => buyer);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        buyerEntity.ShouldNotBeNull();
        buyerEntity.Name.ShouldBe("anonymous");
    }
}
