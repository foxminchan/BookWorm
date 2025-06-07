using System.Security.Claims;
using BookWorm.Chassis.Exceptions;
using BookWorm.Constants.Core;
using BookWorm.Ordering.Domain.AggregatesModel.BuyerAggregate;
using BookWorm.Ordering.Features.Buyers.Get;
using BookWorm.Ordering.UnitTests.Fakers;

namespace BookWorm.Ordering.UnitTests.Features.Buyers.Get;

public sealed class GetBuyerQueryTests
{
    private readonly Mock<IBuyerRepository> _buyerRepositoryMock = new();
    private readonly Guid _otherBuyerId = Guid.CreateVersion7();
    private readonly Buyer _testBuyer = new BuyerFaker().Generate()[0];
    private readonly Guid _testBuyerId = Guid.CreateVersion7();

    [Test]
    public async Task GivenAdminUser_WhenGettingAnotherUsersBuyer_ThenShouldReturnBuyer()
    {
        // Arrange
        var claimsPrincipal = CreateClaimsPrincipal(true, _testBuyerId);

        _buyerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(_otherBuyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testBuyer);

        var handler = new GetBuyerHandler(_buyerRepositoryMock.Object, claimsPrincipal);
        var command = new GetBuyerQuery(_otherBuyerId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        _buyerRepositoryMock.Verify(
            repo => repo.GetByIdAsync(_otherBuyerId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenRegularUser_WhenGettingOwnBuyer_ThenShouldReturnBuyer()
    {
        // Arrange
        var claimsPrincipal = CreateClaimsPrincipal(false, _testBuyerId);

        _buyerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(_testBuyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testBuyer);

        var handler = new GetBuyerHandler(_buyerRepositoryMock.Object, claimsPrincipal);
        var command = new GetBuyerQuery(_otherBuyerId); // Request ID is ignored for regular users

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        _buyerRepositoryMock.Verify(
            repo => repo.GetByIdAsync(_testBuyerId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenRegularUser_WhenGettingAnotherUsersBuyer_ThenShouldUseOwnIdInstead()
    {
        // Arrange
        var claimsPrincipal = CreateClaimsPrincipal(false, _testBuyerId);

        _buyerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(_testBuyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testBuyer);

        var handler = new GetBuyerHandler(_buyerRepositoryMock.Object, claimsPrincipal);
        var command = new GetBuyerQuery(_otherBuyerId); // This ID will be ignored

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        _buyerRepositoryMock.Verify(
            repo => repo.GetByIdAsync(_testBuyerId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _buyerRepositoryMock.Verify(
            repo => repo.GetByIdAsync(_otherBuyerId, It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenMissingBuyer_WhenHandlingGetBuyerCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var claimsPrincipal = CreateClaimsPrincipal(false, _testBuyerId);

        _buyerRepositoryMock
            .Setup(repo => repo.GetByIdAsync(_testBuyerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Buyer)null!);

        var handler = new GetBuyerHandler(_buyerRepositoryMock.Object, claimsPrincipal);
        var command = new GetBuyerQuery(Guid.CreateVersion7());

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Buyer with id {_testBuyerId} not found.");
    }

    [Test]
    public async Task GivenMissingUserIdClaim_WhenHandlingGetBuyerCommand_ThenShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock
            .Setup(cp => cp.FindFirst(ClaimTypes.NameIdentifier))
            .Returns((Claim)null!);

        var handler = new GetBuyerHandler(_buyerRepositoryMock.Object, claimsPrincipalMock.Object);
        var command = new GetBuyerQuery(Guid.CreateVersion7());

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User is not authenticated.");
        _buyerRepositoryMock.Verify(
            repo => repo.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(bool isAdmin, Guid userId)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };

        if (isAdmin)
        {
            claims.Add(new(ClaimTypes.Role, Authorization.Roles.Admin));
        }

        var identity = new ClaimsIdentity(claims);
        return new(identity);
    }
}
