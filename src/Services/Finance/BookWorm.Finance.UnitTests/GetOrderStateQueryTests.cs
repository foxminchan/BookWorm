﻿using BookWorm.Chassis.Query;
using BookWorm.Finance.Feature;
using BookWorm.Finance.Saga;
using Microsoft.Extensions.Logging;

namespace BookWorm.Finance.UnitTests;

public sealed class GetOrderStateQueryTests
{
    private readonly GetOrderStateHandler _handler;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;

    public GetOrderStateQueryTests()
    {
        _loggerFactoryMock = new();
        _handler = new(_loggerFactoryMock.Object);
    }

    [Test]
    public void GivenGetOrderStateQuery_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Act
        var query = new GetOrderStateQuery();

        // Assert
        query.ShouldNotBeNull();
        query.ShouldBeOfType<GetOrderStateQuery>();
        query.ShouldBeAssignableTo<IQuery<string>>();
    }

    [Test]
    public async Task GivenGetOrderStateQuery_WhenHandled_ThenShouldReturnNonEmptyString()
    {
        // Arrange
        var query = new GetOrderStateQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
    }

    [Test]
    public async Task GivenGetOrderStateQuery_WhenHandled_ThenShouldContainGraphvizContent()
    {
        // Arrange
        var query = new GetOrderStateQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldContain("digraph"); // GraphViz dot files start with "digraph"
        result.ShouldContain("->"); // Should contain transition arrows
    }

    [Test]
    public async Task GivenGetOrderStateQuery_WhenHandled_ThenShouldUseProvidedLoggerFactory()
    {
        // Arrange
        var query = new GetOrderStateQuery();
        var mockLogger = new Mock<ILogger>();
        _loggerFactoryMock
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(mockLogger.Object);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _loggerFactoryMock.Verify(f => f.CreateLogger(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Test]
    public async Task GivenGetOrderStateQuery_WhenHandled_ThenShouldCreateOrderStateMachineWithProvidedLoggerFactory()
    {
        // Arrange
        var query = new GetOrderStateQuery();
        var mockLogger = new Mock<ILogger>();

        // Setup the logger factory to return our mock logger
        _loggerFactoryMock
            .Setup(f => f.CreateLogger(nameof(OrderStateMachine)))
            .Returns(mockLogger.Object)
            .Verifiable();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();

        // Verify that the OrderStateMachine constructor was called with our LoggerFactory
        // This is verified by checking that CreateLogger was called with the specific type name
        _loggerFactoryMock.Verify(
            f => f.CreateLogger(nameof(OrderStateMachine)),
            Times.Once,
            "OrderStateMachine constructor should be called with the provided ILoggerFactory"
        );
    }
}
