using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Get;
using BookWorm.Basket.UnitTests.Fakers;
using BookWorm.Chassis.CQRS.Query;
using BookWorm.Chassis.Exceptions;

namespace BookWorm.Basket.UnitTests.Features.Get;

public sealed class GetBasketQueryTests
{
    private readonly CustomerBasket _basket;
    private readonly Mock<ClaimsPrincipal> _claimsPrincipalMock;
    private readonly GetBasketHandler _handler;
    private readonly Mock<IBasketRepository> _repositoryMock;
    private readonly string _userId;

    public GetBasketQueryTests()
    {
        _userId = Guid.CreateVersion7().ToString();
        _basket = new CustomerBasketFaker().Generate()[0];
        _claimsPrincipalMock = new();
        _repositoryMock = new();

        // Set up the claim using the ClaimTypes.NameIdentifier
        // and ensure the GetClaimValue extension method will work
        var claim = new Claim(ClaimTypes.NameIdentifier, _userId);
        _claimsPrincipalMock.Setup(x => x.FindFirst(ClaimTypes.NameIdentifier)).Returns(claim);

        _handler = new(_repositoryMock.Object, _claimsPrincipalMock.Object);
    }

    [Test]
    public void GivenGetBasketQuery_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Act
        var query = new GetBasketQuery();

        // Assert
        query.ShouldNotBeNull();
        query.ShouldBeOfType<GetBasketQuery>();
        query.ShouldBeAssignableTo<IQuery<CustomerBasketDto>>();
    }

    [Test]
    public async Task GivenValidUserId_WhenHandling_ThenShouldReturnBasket()
    {
        // Arrange
        var query = new GetBasketQuery();

        _repositoryMock.Setup(r => r.GetBasketAsync(_userId)).ReturnsAsync(_basket);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_basket.Id);
        result.Items.Count.ShouldBe(_basket.Items.Count);

        // Verify quantities are preserved by matching items
        var basketItemsArray = _basket.Items.ToArray();
        for (var i = 0; i < result.Items.Count; i++)
        {
            result.Items[i].Quantity.ShouldBe(basketItemsArray[i].Quantity);
        }

        _repositoryMock.Verify(r => r.GetBasketAsync(_userId), Times.Once);
    }

    [Test]
    public async Task GivenMissingUserId_WhenHandling_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetBasketQuery();

        _claimsPrincipalMock
            .Setup(x => x.FindFirst(ClaimTypes.NameIdentifier))
            .Returns((Claim?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<UnauthorizedAccessException>();
        exception.Message.ShouldBe("User is not authenticated.");
        _repositoryMock.Verify(r => r.GetBasketAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GivenNonExistentBasket_WhenHandling_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetBasketQuery();

        _repositoryMock.Setup(r => r.GetBasketAsync(_userId)).ReturnsAsync((CustomerBasket?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"CustomerBasket with id {_userId} not found.");
        _repositoryMock.Verify(r => r.GetBasketAsync(_userId), Times.Once);
    }

    [Test]
    public void GivenTwoGetBasketQueries_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var query1 = new GetBasketQuery();
        var query2 = new GetBasketQuery();

        // Act & Assert
        query1.ShouldBe(query2);
        query1.GetHashCode().ShouldBe(query2.GetHashCode());
    }

    [Test]
    public void GivenTwoGetBasketQueries_WhenCallingToString_ThenShouldReturnStringRepresentation()
    {
        // Arrange
        var query = new GetBasketQuery();

        // Act
        var result = query.ToString();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldContain(nameof(GetBasketQuery));
    }

    [Test]
    public void GivenGetBasketQuery_WhenUsingWithExpression_ThenShouldCreateIdenticalCopy()
    {
        // Arrange
        var original = new GetBasketQuery();

        // Act
        var copy = original with
        { };

        // Assert
        copy.ShouldBe(original);
        copy.ShouldNotBeSameAs(original);
    }
}
