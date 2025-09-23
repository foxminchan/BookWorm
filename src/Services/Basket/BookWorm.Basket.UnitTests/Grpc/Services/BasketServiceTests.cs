using System.Security.Claims;
using BookWorm.Basket.Domain;
using BookWorm.Basket.Grpc.Services.Basket;
using BookWorm.Basket.UnitTests.Grpc.Context;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
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
            Guid.CreateVersion7().ToString(),
            [new(Guid.CreateVersion7().ToString(), 10), new(Guid.CreateVersion7().ToString(), 20)]
        );

        var context = TestServerCallContext.Create();
        var httpContext = new DefaultHttpContext
        {
            User = new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier, basket.Id)])),
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
    public async Task GivenNoUser_WhenGetBasketCalled_ThenShouldThrowRpcException()
    {
        // Arrange
        var request = new Empty();
        var context = TestServerCallContext.Create();
        context.SetUserState("__HttpContext", new DefaultHttpContext());

        // Act & Assert
        var exception = await Should.ThrowAsync<RpcException>(() =>
            _basketService.GetBasket(request, context)
        );

        exception.StatusCode.ShouldBe(StatusCode.Unauthenticated);
        exception.Status.Detail.ShouldBe("User is not authenticated.");
        _basketRepositoryMock.Verify(repo => repo.GetBasketAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GivenEmptyUserId_WhenGetBasketCalled_ThenShouldThrowRpcException()
    {
        // Arrange
        var request = new Empty();

        var context = TestServerCallContext.Create();
        var httpContext = new DefaultHttpContext
        {
            User = new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier, string.Empty)])),
        };
        context.SetUserState("__HttpContext", httpContext);

        // Act & Assert
        var exception = await Should.ThrowAsync<RpcException>(() =>
            _basketService.GetBasket(request, context)
        );

        exception.StatusCode.ShouldBe(StatusCode.Unauthenticated);
        exception.Status.Detail.ShouldBe("User is not authenticated.");
        _basketRepositoryMock.Verify(repo => repo.GetBasketAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GivenDebugLoggingEnabled_WhenGetBasketCalled_ThenShouldLogDebugMessage()
    {
        // Arrange
        var request = new Empty();
        var userId = Guid.CreateVersion7().ToString();
        var basket = new CustomerBasket(userId, [new(Guid.CreateVersion7().ToString(), 10)]);

        var context = TestServerCallContext.Create();
        var httpContext = new DefaultHttpContext
        {
            User = new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier, userId)])),
        };
        context.SetUserState("__HttpContext", httpContext);

        var loggerMock = new Mock<ILogger<BasketService>>();
        // Setup logger to return true for IsEnabled(LogLevel.Debug)
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        var basketService = new BasketService(_basketRepositoryMock.Object, loggerMock.Object);

        _basketRepositoryMock.Setup(repo => repo.GetBasketAsync(userId)).ReturnsAsync(basket);

        // Act
        var response = await basketService.GetBasket(request, context);

        // Assert
        // Verify logger.Log was called with Debug level
        loggerMock.Verify(
            l =>
                l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!
                ),
            Times.Once
        );

        response.ShouldNotBeNull();
        response.Items.Count.ShouldBe(basket.Items.Count);
    }

    [Test]
    public async Task GivenDebugLoggingDisabled_WhenGetBasketCalled_ThenShouldNotLogDebugMessage()
    {
        // Arrange
        var request = new Empty();
        var userId = Guid.CreateVersion7().ToString();
        var basket = new CustomerBasket(userId, [new(Guid.CreateVersion7().ToString(), 10)]);

        var context = TestServerCallContext.Create();
        var httpContext = new DefaultHttpContext
        {
            User = new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier, userId)])),
        };
        context.SetUserState("__HttpContext", httpContext);

        var loggerMock = new Mock<ILogger<BasketService>>();
        // Setup logger to return false for IsEnabled(LogLevel.Debug)
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);

        var basketService = new BasketService(_basketRepositoryMock.Object, loggerMock.Object);

        _basketRepositoryMock.Setup(repo => repo.GetBasketAsync(userId)).ReturnsAsync(basket);

        // Act
        var response = await basketService.GetBasket(request, context);

        // Assert
        // Verify logger.Log was NOT called with Debug level
        loggerMock.Verify(
            l =>
                l.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!
                ),
            Times.Never
        );

        response.ShouldNotBeNull();
        response.Items.Count.ShouldBe(basket.Items.Count);
    }

    [Test]
    public async Task GivenValidUserIdWithNoBasket_WhenGetBasketCalled_ThenShouldReturnEmptyResponse()
    {
        // Arrange
        var request = new Empty();
        var userId = Guid.CreateVersion7().ToString();

        var context = TestServerCallContext.Create();
        var httpContext = new DefaultHttpContext
        {
            User = new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier, userId)])),
        };
        context.SetUserState("__HttpContext", httpContext);

        _basketRepositoryMock
            .Setup(repo => repo.GetBasketAsync(userId))
            .ReturnsAsync((CustomerBasket?)null);

        // Act
        var response = await _basketService.GetBasket(request, context);

        // Assert
        response.ShouldNotBeNull();
        response.Items.Count.ShouldBe(0);
        _basketRepositoryMock.Verify(repo => repo.GetBasketAsync(userId), Times.Once);
    }

    [Test]
    public async Task GivenNoUser_WhenGetBasketCalled_ThenShouldLogWarning()
    {
        // Arrange
        var request = new Empty();
        var context = TestServerCallContext.Create();
        context.SetUserState("__HttpContext", new DefaultHttpContext());

        var loggerMock = new Mock<ILogger<BasketService>>();
        var basketService = new BasketService(_basketRepositoryMock.Object, loggerMock.Object);

        // Act & Assert
        var exception = await Should.ThrowAsync<RpcException>(() =>
            basketService.GetBasket(request, context)
        );

        // Verify warning log was called
        loggerMock.Verify(
            l =>
                l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!
                ),
            Times.Once
        );

        exception.StatusCode.ShouldBe(StatusCode.Unauthenticated);
    }
}
