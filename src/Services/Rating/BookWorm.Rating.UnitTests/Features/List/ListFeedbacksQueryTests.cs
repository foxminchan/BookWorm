using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.Domain.FeedbackAggregator.Specifications;
using BookWorm.Rating.Features.List;
using BookWorm.Rating.UnitTests.Fakers;

namespace BookWorm.Rating.UnitTests.Features.List;

public sealed class ListFeedbacksQueryTests
{
    private readonly Guid _bookId;
    private readonly Feedback[] _feedbacks;
    private readonly ListFeedbacksHandler _handler;
    private readonly Mock<IFeedbackRepository> _repositoryMock;

    public ListFeedbacksQueryTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _bookId = Guid.CreateVersion7();
        _feedbacks = new FeedbackFaker().Generate();
    }

    [Test]
    public async Task GivenValidQuery_WhenHandlingListFeedbacks_ThenShouldReturnPagedResults()
    {
        // Arrange
        var query = new ListFeedbacksQuery(_bookId, 0, 10);

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_feedbacks);

        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_feedbacks.Length);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Count.ShouldBe(_feedbacks.Length);
        result.PageIndex.ShouldBe(0);
        result.PageSize.ShouldBe(10);
        result.TotalItems.ShouldBe(_feedbacks.Length);
        result.TotalPages.ShouldBe(2);

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            x => x.CountAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    [Arguments(nameof(Feedback.Rating), true)]
    [Arguments(nameof(Feedback.Rating), false)]
    [Arguments(nameof(Feedback.CreatedAt), true)]
    [Arguments(nameof(Feedback.CreatedAt), false)]
    public async Task GivenValidQueryWithPagination_WhenHandlingListFeedbacks_ThenShouldApplyPagination(
        string? orderBy,
        bool isDescending
    )
    {
        // Arrange
        const int pageIndex = 1;
        const int pageSize = 5;
        const int totalItems = 15;
        const int totalPages = 3;
        var query = new ListFeedbacksQuery(_bookId, pageIndex, pageSize, orderBy, isDescending);

        var feedbacks = new FeedbackFaker().Generate(pageSize);

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedbacks);

        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalItems);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PageIndex.ShouldBe(pageIndex);
        result.PageSize.ShouldBe(pageSize);
        result.TotalItems.ShouldBe(totalItems);
        result.TotalPages.ShouldBe(totalPages);
        result.Items.Count.ShouldBe(feedbacks.Count);

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            x => x.CountAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyResults_WhenHandlingListFeedbacks_ThenShouldReturnEmptyPagedResult()
    {
        // Arrange
        var query = new ListFeedbacksQuery(_bookId);

        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Feedback>());

        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.ShouldBeEmpty();
        result.TotalItems.ShouldBe(0);
        result.TotalPages.ShouldBe(0);

        _repositoryMock.Verify(
            x => x.ListAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            x => x.CountAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
