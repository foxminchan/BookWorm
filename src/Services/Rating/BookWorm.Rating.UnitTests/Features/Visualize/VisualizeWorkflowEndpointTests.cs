using BookWorm.Rating.Features.Visualize;
using BookWorm.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Rating.UnitTests.Features.Visualize;

public sealed class VisualizeWorkflowEndpointTests
{
    private readonly VisualizeWorkflowEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();

    [Test]
    public async Task GivenValidQueryWithMermaidType_WhenHandlingEndpoint_ThenShouldCallSenderAndReturnOkWithResult()
    {
        // Arrange
        const string expectedWorkflow = "graph TD\n    A --> B";
        var query = new VisualizeWorkflowQuery();

        _senderMock
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWorkflow);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<string>>();
        result.Value.ShouldBe(expectedWorkflow);
        result.StatusCode.ShouldBe(StatusCodes.Status200OK);
        _senderMock.Verify(s => s.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenValidQueryWithDotType_WhenHandlingEndpoint_ThenShouldCallSenderAndReturnOkWithResult()
    {
        // Arrange
        const string expectedWorkflow = "digraph G {\n    A -> B;\n}";
        var query = new VisualizeWorkflowQuery(VisualizationType.Dot);

        _senderMock
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWorkflow);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<string>>();
        result.Value.ShouldBe(expectedWorkflow);
        result.StatusCode.ShouldBe(StatusCodes.Status200OK);
        _senderMock.Verify(s => s.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenDefaultQuery_WhenHandlingEndpoint_ThenShouldCallSenderAndReturnOkWithMermaidResult()
    {
        // Arrange
        const string expectedWorkflow = "graph TD\n    Start --> End";
        var query = new VisualizeWorkflowQuery();

        _senderMock
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWorkflow);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<string>>();
        result.Value.ShouldBe(expectedWorkflow);
        result.StatusCode.ShouldBe(StatusCodes.Status200OK);
        query.Type.ShouldBe(VisualizationType.Mermaid);
    }

    [Test]
    public async Task GivenValidQueryWithCancellationToken_WhenHandlingEndpoint_ThenShouldPassTokenToSender()
    {
        // Arrange
        const string expectedWorkflow = "graph TD\n    A --> B";
        var query = new VisualizeWorkflowQuery();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _senderMock.Setup(s => s.Send(query, cancellationToken)).ReturnsAsync(expectedWorkflow);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object, cancellationToken);

        // Assert
        result.ShouldBeOfType<Ok<string>>();
        result.Value.ShouldBe(expectedWorkflow);
        _senderMock.Verify(s => s.Send(query, cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenHandlingEndpoint_ThenShouldPropagateException()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery();
        var expectedException = new InvalidOperationException("Workflow generation failed");

        _senderMock
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            _endpoint.HandleAsync(query, _senderMock.Object)
        );

        exception.ShouldBe(expectedException);
        exception.Message.ShouldBe("Workflow generation failed");
        _senderMock.Verify(s => s.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenEmptyResultFromSender_WhenHandlingEndpoint_ThenShouldReturnOkWithEmptyString()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery();

        _senderMock
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<string>>();
        result.Value.ShouldBeEmpty();
        result.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GivenInvalidEnumValue_WhenHandlingEndpoint_ThenShouldStillCallSender()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery((VisualizationType)99);

        _senderMock
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<string>>();
        result.Value.ShouldBeEmpty();
        _senderMock.Verify(s => s.Send(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenComplexWorkflowResult_WhenHandlingEndpoint_ThenShouldReturnOkWithFullContent()
    {
        // Arrange
        const string complexWorkflow = """
            graph TD
                A[Start] --> B{Decision}
                B -->|Yes| C[Process]
                B -->|No| D[Skip]
                C --> E[End]
                D --> E
            """;
        var query = new VisualizeWorkflowQuery();

        _senderMock
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(complexWorkflow);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<string>>();
        result.Value.ShouldBe(complexWorkflow);
        result.Value.ShouldNotBeNull();
        result.Value.ShouldContain("graph TD");
        result.Value.ShouldContain("Decision");
    }

    [Test]
    public async Task GivenMultipleConsecutiveCalls_WhenHandlingEndpoint_ThenShouldCallSenderForEachRequest()
    {
        // Arrange
        const string mermaidWorkflow = "graph TD\n    A --> B";
        const string dotWorkflow = "digraph G {\n    A -> B;\n}";
        var mermaidQuery = new VisualizeWorkflowQuery();
        var dotQuery = new VisualizeWorkflowQuery(VisualizationType.Dot);

        _senderMock
            .Setup(s => s.Send(mermaidQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mermaidWorkflow);

        _senderMock
            .Setup(s => s.Send(dotQuery, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dotWorkflow);

        // Act
        var result1 = await _endpoint.HandleAsync(mermaidQuery, _senderMock.Object);
        var result2 = await _endpoint.HandleAsync(dotQuery, _senderMock.Object);

        // Assert
        result1.ShouldBeOfType<Ok<string>>();
        result1.Value.ShouldBe(mermaidWorkflow);
        result2.ShouldBeOfType<Ok<string>>();
        result2.Value.ShouldBe(dotWorkflow);
        _senderMock.Verify(s => s.Send(mermaidQuery, It.IsAny<CancellationToken>()), Times.Once);
        _senderMock.Verify(s => s.Send(dotQuery, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenCancelledToken_WhenHandlingEndpoint_ThenShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new VisualizeWorkflowQuery();
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var cancellationToken = cancellationTokenSource.Token;

        _senderMock
            .Setup(s => s.Send(query, cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            _endpoint.HandleAsync(query, _senderMock.Object, cancellationToken)
        );

        _senderMock.Verify(s => s.Send(query, cancellationToken), Times.Once);
    }

    [Test]
    public void GivenEndpoint_WhenCreated_ThenShouldNotBeNull()
    {
        // Assert
        _endpoint.ShouldNotBeNull();
        _endpoint.ShouldBeOfType<VisualizeWorkflowEndpoint>();
    }

    [Test]
    public async Task GivenLargeWorkflowResult_WhenHandlingEndpoint_ThenShouldReturnOkWithCompleteResult()
    {
        // Arrange
        var largeWorkflow = new string('A', 10000);
        var query = new VisualizeWorkflowQuery();

        _senderMock
            .Setup(s => s.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(largeWorkflow);

        // Act
        var result = await _endpoint.HandleAsync(query, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<string>>();
        result.Value.ShouldBe(largeWorkflow);
        result.Value.ShouldNotBeNull();
        result.Value.Length.ShouldBe(10000);
    }
}
