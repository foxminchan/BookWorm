using BookWorm.Rating.Features.Summarize;
using BookWorm.Rating.Infrastructure.Summarizer;
using BookWorm.Rating.Infrastructure.Summarizer.Models;

namespace BookWorm.Rating.UnitTests.Features.Summarize;

public sealed class SummarizeFeedbackQueryTests
{
    [Test]
    public async Task GivenValidBookId_WhenHandlingQuery_ThenShouldPassTypedBookIdToSummarizer()
    {
        var bookId = Guid.CreateVersion7();
        var expected = new SummarizeResult("Summary");
        var summarizer = new Mock<ISummarizer>();
        summarizer
            .Setup(s => s.SummarizeAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new SummarizeFeedbackHandler(summarizer.Object);

        var result = await handler.Handle(new(bookId), CancellationToken.None);

        result.ShouldBe(expected);
        summarizer.Verify(s => s.SummarizeAsync(bookId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenCancellationToken_WhenHandlingQuery_ThenShouldPropagateIt()
    {
        var bookId = Guid.CreateVersion7();
        using var cancellation = new CancellationTokenSource();
        var cancellationToken = cancellation.Token;
        var summarizer = new Mock<ISummarizer>();
        summarizer
            .Setup(s => s.SummarizeAsync(bookId, cancellationToken))
            .ThrowsAsync(new OperationCanceledException(cancellationToken));
        var handler = new SummarizeFeedbackHandler(summarizer.Object);

        await Should.ThrowAsync<OperationCanceledException>(() =>
            handler.Handle(new(bookId), cancellationToken).AsTask()
        );
    }
}
