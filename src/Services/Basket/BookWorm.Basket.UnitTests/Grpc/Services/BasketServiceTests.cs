using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Grpc.Services.Basket;
using BookWorm.Basket.UnitTests.Grpc.Context;
using BookWorm.ServiceDefaults.Keycloak;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BookWorm.Basket.UnitTests.Grpc.Services;

public sealed class BasketServiceTests
{
    private readonly Mock<IBasketRepository> _basketRepositoryMock;
    private readonly BasketService _basketService;

    public BasketServiceTests()
    {
        _basketRepositoryMock = new();
        var loggerMock = new Mock<ILogger<BasketService>>();
        _basketService = new(_basketRepositoryMock.Object, loggerMock.Object);
    }

    [Test]
    public async Task GivenValidUserId_WhenGetBasketCalled_ThenShouldReturnBasketResponse()
    {
        // Arrange
        var request = new Empty();
        var basket = new CustomerBasket(
            Guid.NewGuid().ToString(),
            [
                new BasketItem(Guid.NewGuid().ToString(), 10),
                new BasketItem(Guid.NewGuid().ToString(), 20),
            ]
        );

        var context = TestServerCallContext.Create();
        var httpContext = new DefaultHttpContext
        {
            User = new(new ClaimsIdentity([new(KeycloakClaimTypes.Subject, basket.Id!)])),
        };
        context.SetUserState("__HttpContext", httpContext);

        _basketRepositoryMock
            .Setup(repo => repo.GetBasketAsync(It.IsAny<string>()))
            .ReturnsAsync(basket);

        // Act
        var response = await _basketService.GetBasket(request, context);

        // Assert
        response.ShouldNotBeNull();
        response.Items.Count.ShouldBe(basket.Items.Count);

        var basketItemsList = basket.Items.ToList();
        for (var i = 0; i < basket.Items.Count; i++)
        {
            response.Items[i].Id.ShouldBe(basketItemsList[i].Id);
            response.Items[i].Quantity.ShouldBe(basketItemsList[i].Quantity);
        }

        _basketRepositoryMock.Verify(repo => repo.GetBasketAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task GivenNoUser_WhenGetBasketCalled_ThenShouldReturnEmptyResponse()
    {
        // Arrange
        var request = new Empty();
        var context = TestServerCallContext.Create();
        context.SetUserState("__HttpContext", new DefaultHttpContext());

        // Act
        var response = await _basketService.GetBasket(request, context);

        // Assert
        response.ShouldNotBeNull();
        response.Items.Count.ShouldBe(0);
        _basketRepositoryMock.Verify(repo => repo.GetBasketAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GivenInvalidUserId_WhenGetBasketCalled_ThenShouldReturnEmptyResponse()
    {
        // Arrange
        var request = new Empty();

        var context = TestServerCallContext.Create();
        var httpContext = new DefaultHttpContext
        {
            User = new(
                new ClaimsIdentity(
                    [new(KeycloakClaimTypes.Subject, Guid.CreateVersion7().ToString())]
                )
            ),
        };
        context.SetUserState("__HttpContext", httpContext);

        // Act
        var response = await _basketService.GetBasket(request, context);

        // Assert
        response.ShouldNotBeNull();
        response.Items.Count.ShouldBe(0);
        _basketRepositoryMock.Verify(repo => repo.GetBasketAsync(It.IsAny<string>()), Times.Once);
    }
}
