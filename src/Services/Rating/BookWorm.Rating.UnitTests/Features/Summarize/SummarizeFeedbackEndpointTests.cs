using BookWorm.Rating.Features;
using BookWorm.Rating.Features.Summarize;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BookWorm.Rating.UnitTests.Features.Summarize;

public sealed class SummarizeFeedbackEndpointTests
{
    private readonly SummarizeFeedbackEndpoint _endpoint = new();
    private readonly Mock<ISender> _senderMock = new();
    private readonly Guid _validBookId = Guid.CreateVersion7();

    [Test]
    public async Task GivenValidBookId_WhenHandlingEndpointRequest_ThenShouldCallSenderWithQuery()
    {
        // Arrange
        var expectedSummary = new SummarizeResult(
            "This book has excellent ratings with an average of 4.5 stars."
        );
        _senderMock
            .Setup(s => s.Send(It.IsAny<SummarizeFeedbackQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        await _endpoint.HandleAsync(_validBookId, _senderMock.Object);

        // Assert
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<SummarizeFeedbackQuery>(q => q.BookId == _validBookId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidBookId_WhenHandlingEndpoint_ThenShouldReturnOkWithSummary()
    {
        // Arrange
        var expectedSummary = new SummarizeResult(
            "Excellent feedback summary with 5-star ratings."
        );
        _senderMock
            .Setup(s => s.Send(It.IsAny<SummarizeFeedbackQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _endpoint.HandleAsync(_validBookId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<SummarizeResult>>();
        result.Value.ShouldBe(expectedSummary);
        result.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GivenValidBookIdWithCancellationToken_WhenHandlingEndpoint_ThenShouldPassTokenToSender()
    {
        // Arrange
        var expectedSummary = new SummarizeResult("Summary with cancellation token handling.");
        var cancellationToken = CancellationToken.None;
        _senderMock
            .Setup(s => s.Send(It.IsAny<SummarizeFeedbackQuery>(), cancellationToken))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _endpoint.HandleAsync(
            _validBookId,
            _senderMock.Object,
            cancellationToken
        );

        // Assert
        result.ShouldBeOfType<Ok<SummarizeResult>>();
        result.Value.ShouldBe(expectedSummary);
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<SummarizeFeedbackQuery>(q => q.BookId == _validBookId),
                    cancellationToken
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyGuid_WhenHandlingEndpoint_ThenShouldStillCallSenderWithEmptyGuid()
    {
        // Arrange
        var emptyBookId = Guid.Empty;
        var expectedSummary = new SummarizeResult("No feedback found for this book.");
        _senderMock
            .Setup(s => s.Send(It.IsAny<SummarizeFeedbackQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _endpoint.HandleAsync(emptyBookId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<SummarizeResult>>();
        result.Value.ShouldBe(expectedSummary);
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<SummarizeFeedbackQuery>(q => q.BookId == emptyBookId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleCalls_WhenHandlingEndpointWithDifferentBookIds_ThenShouldCallSenderForEachId()
    {
        // Arrange
        var bookId1 = Guid.CreateVersion7();
        var bookId2 = Guid.CreateVersion7();
        var summary1 = new SummarizeResult("Summary for book 1");
        var summary2 = new SummarizeResult("Summary for book 2");

        _senderMock
            .Setup(s =>
                s.Send(
                    It.Is<SummarizeFeedbackQuery>(q => q.BookId == bookId1),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(summary1);

        _senderMock
            .Setup(s =>
                s.Send(
                    It.Is<SummarizeFeedbackQuery>(q => q.BookId == bookId2),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(summary2);

        // Act
        var result1 = await _endpoint.HandleAsync(bookId1, _senderMock.Object);
        var result2 = await _endpoint.HandleAsync(bookId2, _senderMock.Object);

        // Assert
        result1.ShouldBeOfType<Ok<SummarizeResult>>();
        result1.Value.ShouldBe(summary1);

        result2.ShouldBeOfType<Ok<SummarizeResult>>();
        result2.Value.ShouldBe(summary2);

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<SummarizeFeedbackQuery>(q => q.BookId == bookId1),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<SummarizeFeedbackQuery>(q => q.BookId == bookId2),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidBookId_WhenSenderReturnsEmptyString_ThenShouldReturnOkWithEmptyString()
    {
        // Arrange
        var emptySummary = new SummarizeResult("");
        _senderMock
            .Setup(s => s.Send(It.IsAny<SummarizeFeedbackQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptySummary);

        // Act
        var result = await _endpoint.HandleAsync(_validBookId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<SummarizeResult>>();
        result.Value.ShouldBe(emptySummary);
        result.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GivenValidBookId_WhenSenderReturnsLongSummary_ThenShouldReturnOkWithFullSummary()
    {
        // Arrange
        var longSummaryText = string.Join(
            " ",
            Enumerable.Repeat("This is a very detailed feedback summary.", 100)
        );
        var longSummary = new SummarizeResult(longSummaryText);
        _senderMock
            .Setup(s => s.Send(It.IsAny<SummarizeFeedbackQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(longSummary);

        // Act
        var result = await _endpoint.HandleAsync(_validBookId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<SummarizeResult>>();
        result.Value.ShouldBe(longSummary);
        result.Value.ShouldNotBeNull();
        result.Value.Summary.Length.ShouldBeGreaterThan(1000);
    }

    [Test]
    public async Task GivenNullSender_WhenHandlingEndpoint_ThenShouldThrowNullReferenceException()
    {
        // Arrange & Act & Assert
        await Should.ThrowAsync<NullReferenceException>(() =>
            _endpoint.HandleAsync(_validBookId, null!)
        );
    }

    [Test]
    public void GivenEndpoint_WhenCreating_ThenShouldInitializeCorrectly()
    {
        // Arrange & Act
        var endpoint = new SummarizeFeedbackEndpoint();

        // Assert
        endpoint.ShouldNotBeNull();
        endpoint.ShouldBeOfType<SummarizeFeedbackEndpoint>();
    }

    [Test]
    public async Task GivenValidBookId_WhenSenderThrowsException_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        _senderMock
            .Setup(s => s.Send(It.IsAny<SummarizeFeedbackQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Should.ThrowAsync<InvalidOperationException>(() =>
            _endpoint.HandleAsync(_validBookId, _senderMock.Object)
        );

        actualException.Message.ShouldBe("Test exception");
    }

    [Test]
    public async Task GivenValidBookId_WhenSenderReturnsNull_ThenShouldReturnOkWithNull()
    {
        // Arrange
        _senderMock
            .Setup(s => s.Send(It.IsAny<SummarizeFeedbackQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SummarizeResult)null!);

        // Act
        var result = await _endpoint.HandleAsync(_validBookId, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<SummarizeResult>>();
        result.Value.ShouldBeNull();
    }

    [Test]
    public async Task GivenDefaultGuid_WhenHandlingEndpoint_ThenShouldCallSenderWithDefaultGuid()
    {
        // Arrange
        var defaultGuid = Guid.Empty;
        var expectedSummary = new SummarizeResult("Summary for default guid.");
        _senderMock
            .Setup(s => s.Send(It.IsAny<SummarizeFeedbackQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _endpoint.HandleAsync(defaultGuid, _senderMock.Object);

        // Assert
        result.ShouldBeOfType<Ok<SummarizeResult>>();
        result.Value.ShouldBe(expectedSummary);
        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<SummarizeFeedbackQuery>(q => q.BookId == defaultGuid),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidBookId_WhenMultipleSimultaneousCalls_ThenShouldHandleAllCalls()
    {
        // Arrange
        var expectedSummary = new SummarizeResult("Concurrent summary response.");
        _senderMock
            .Setup(s => s.Send(It.IsAny<SummarizeFeedbackQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var tasks = Enumerable
            .Range(0, 5)
            .Select(_ => _endpoint.HandleAsync(_validBookId, _senderMock.Object))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Length.ShouldBe(5);
        foreach (var result in results)
        {
            result.ShouldBeOfType<Ok<SummarizeResult>>();
            result.Value.ShouldBe(expectedSummary);
        }

        _senderMock.Verify(
            s =>
                s.Send(
                    It.Is<SummarizeFeedbackQuery>(q => q.BookId == _validBookId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(5)
        );
    }
}
