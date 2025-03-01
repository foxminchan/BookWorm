using BookWorm.Finance.Feature;
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
}
