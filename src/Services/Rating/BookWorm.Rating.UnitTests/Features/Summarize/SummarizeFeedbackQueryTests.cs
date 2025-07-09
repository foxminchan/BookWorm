using BookWorm.Chassis.Exceptions;
using BookWorm.Rating.Features;
using BookWorm.Rating.Features.Summarize;
using BookWorm.Rating.Infrastructure.Summarizer;

namespace BookWorm.Rating.UnitTests.Features.Summarize;

public sealed class SummarizeFeedbackQueryTests
{
    private readonly Mock<ISummarizer> _summarizerMock = new();
    private readonly SummarizeFeedbackHandler _handler;
    private readonly Guid _validBookId = Guid.CreateVersion7();

    public SummarizeFeedbackQueryTests()
    {
        _handler = new(_summarizerMock.Object);
    }

    [Test]
    public async Task GivenValidBookId_WhenHandlingQuery_ThenShouldCallSummarizerWithCorrectContent()
    {
        // Arrange
        var query = new SummarizeFeedbackQuery(_validBookId);
        var expectedContent = $"Get an overview of the ratings for book with ID  {_validBookId}";
        const string expectedSummary = "Excellent book with 4.5 star rating and positive reviews.";

        _summarizerMock
            .Setup(s => s.SummarizeAsync(expectedContent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Summary.ShouldBe(expectedSummary);
        _summarizerMock.Verify(
            s => s.SummarizeAsync(expectedContent, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidBookIdWithCancellationToken_WhenHandlingQuery_ThenShouldPassTokenToSummarizer()
    {
        // Arrange
        var query = new SummarizeFeedbackQuery(_validBookId);
        var cancellationToken = CancellationToken.None;
        const string expectedSummary = "Book summary with cancellation token.";

        _summarizerMock
            .Setup(s => s.SummarizeAsync(It.IsAny<string>(), cancellationToken))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Summary.ShouldBe(expectedSummary);
        _summarizerMock.Verify(
            s => s.SummarizeAsync(It.IsAny<string>(), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyGuid_WhenHandlingQuery_ThenShouldStillCallSummarizerWithEmptyGuid()
    {
        // Arrange
        var emptyBookId = Guid.Empty;
        var query = new SummarizeFeedbackQuery(emptyBookId);
        var expectedContent = $"Get an overview of the ratings for book with ID  {emptyBookId}";
        const string expectedSummary = "No ratings found for this book.";

        _summarizerMock
            .Setup(s => s.SummarizeAsync(expectedContent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Summary.ShouldBe(expectedSummary);
        _summarizerMock.Verify(
            s => s.SummarizeAsync(expectedContent, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSummarizerReturnsNull_WhenHandlingQuery_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var query = new SummarizeFeedbackQuery(_validBookId);

        _summarizerMock
            .Setup(s => s.SummarizeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None)
        );

        exception.Message.ShouldBe($"No ratings found for book with ID {_validBookId}");
    }

    [Test]
    public async Task GivenSummarizerReturnsEmptyString_WhenHandlingQuery_ThenShouldReturnEmptyResult()
    {
        // Arrange
        var query = new SummarizeFeedbackQuery(_validBookId);
        var emptySummary = string.Empty;

        _summarizerMock
            .Setup(s => s.SummarizeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptySummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Summary.ShouldBe(emptySummary);
    }

    [Test]
    public async Task GivenSummarizerReturnsWhitespace_WhenHandlingQuery_ThenShouldReturnWhitespaceResult()
    {
        // Arrange
        var query = new SummarizeFeedbackQuery(_validBookId);
        var whitespaceSummary = string.Empty.PadLeft(100, ' ');

        _summarizerMock
            .Setup(s => s.SummarizeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(whitespaceSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Summary.ShouldBe(whitespaceSummary);
    }

    [Test]
    public async Task GivenSummarizerReturnsLongSummary_WhenHandlingQuery_ThenShouldReturnFullSummary()
    {
        // Arrange
        var query = new SummarizeFeedbackQuery(_validBookId);
        var longSummary = string.Join(
            " ",
            Enumerable.Repeat("This is a very detailed rating summary.", 50)
        );

        _summarizerMock
            .Setup(s => s.SummarizeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(longSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Summary.ShouldBe(longSummary);
        result.Summary.Length.ShouldBeGreaterThan(1000);
    }

    [Test]
    public async Task GivenSummarizerThrowsException_WhenHandlingQuery_ThenShouldPropagateException()
    {
        // Arrange
        var query = new SummarizeFeedbackQuery(_validBookId);
        var expectedException = new InvalidOperationException("Summarizer service unavailable");

        _summarizerMock
            .Setup(s => s.SummarizeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Should.ThrowAsync<InvalidOperationException>(() =>
            _handler.Handle(query, CancellationToken.None)
        );

        actualException.Message.ShouldBe("Summarizer service unavailable");
    }

    [Test]
    public async Task GivenMultipleQueries_WhenHandlingQueriesWithDifferentBookIds_ThenShouldCallSummarizerForEach()
    {
        // Arrange
        var bookId1 = Guid.CreateVersion7();
        var bookId2 = Guid.CreateVersion7();
        var query1 = new SummarizeFeedbackQuery(bookId1);
        var query2 = new SummarizeFeedbackQuery(bookId2);

        const string summary1 = "Summary for book 1";
        const string summary2 = "Summary for book 2";

        _summarizerMock
            .Setup(s =>
                s.SummarizeAsync(
                    $"Get an overview of the ratings for book with ID  {bookId1}",
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(summary1);

        _summarizerMock
            .Setup(s =>
                s.SummarizeAsync(
                    $"Get an overview of the ratings for book with ID  {bookId2}",
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(summary2);

        // Act
        var result1 = await _handler.Handle(query1, CancellationToken.None);
        var result2 = await _handler.Handle(query2, CancellationToken.None);

        // Assert
        result1.Summary.ShouldBe(summary1);
        result2.Summary.ShouldBe(summary2);

        _summarizerMock.Verify(
            s =>
                s.SummarizeAsync(
                    $"Get an overview of the ratings for book with ID  {bookId1}",
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _summarizerMock.Verify(
            s =>
                s.SummarizeAsync(
                    $"Get an overview of the ratings for book with ID  {bookId2}",
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public void GivenQueryRecord_WhenCreatingInstance_ThenShouldInitializeCorrectly()
    {
        // Arrange & Act
        var query = new SummarizeFeedbackQuery(_validBookId);

        // Assert
        query.BookId.ShouldBe(_validBookId);
        query.ShouldBeOfType<SummarizeFeedbackQuery>();
    }

    [Test]
    public void GivenHandler_WhenCreatingInstance_ThenShouldInitializeCorrectly()
    {
        // Arrange & Act
        var handler = new SummarizeFeedbackHandler(_summarizerMock.Object);

        // Assert
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<SummarizeFeedbackHandler>();
    }

    [Test]
    public async Task GivenSpecialCharactersInBookId_WhenHandlingQuery_ThenShouldHandleCorrectly()
    {
        // Arrange
        var specialBookId = new Guid("12345678-1234-1234-1234-123456789012");
        var query = new SummarizeFeedbackQuery(specialBookId);
        var expectedContent = $"Get an overview of the ratings for book with ID  {specialBookId}";
        const string expectedSummary = "Book with special ID processed successfully.";

        _summarizerMock
            .Setup(s => s.SummarizeAsync(expectedContent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Summary.ShouldBe(expectedSummary);
    }

    [Test]
    public async Task GivenCancelledCancellationToken_WhenHandlingQuery_ThenShouldThrowOperationCancelledException()
    {
        // Arrange
        var query = new SummarizeFeedbackQuery(_validBookId);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _summarizerMock
            .Setup(s => s.SummarizeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(() =>
            _handler.Handle(query, cts.Token)
        );
    }

    [Test]
    public async Task GivenSummarizerReturnsResultWithImplicitConversion_WhenHandlingQuery_ThenShouldConvertCorrectly()
    {
        // Arrange
        var query = new SummarizeFeedbackQuery(_validBookId);
        const string summaryText = "Book rating summary via implicit conversion.";

        _summarizerMock
            .Setup(s => s.SummarizeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaryText);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Summary.ShouldBe(summaryText);

        // Test implicit conversion to string
        string implicitString = result;
        implicitString.ShouldBe(summaryText);

        // Test implicit conversion from string
        SummarizeResult implicitResult = summaryText;
        implicitResult.Summary.ShouldBe(summaryText);
    }
}
