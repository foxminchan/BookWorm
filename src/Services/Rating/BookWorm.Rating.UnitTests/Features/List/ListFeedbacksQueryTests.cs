using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.Domain.FeedbackAggregator.Specifications;
using BookWorm.Rating.Features;
using BookWorm.Rating.Features.List;
using BookWorm.Rating.UnitTests.Fakers;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace BookWorm.Rating.UnitTests.Features.List;

public sealed class ListFeedbacksQueryTests : IDisposable
{
    private readonly Guid _bookId;
    private readonly FusionCache _cache;
    private readonly Feedback[] _feedbacks;
    private readonly ListFeedbacksHandler _handler;
    private readonly Mock<IFeedbackRepository> _repositoryMock;

    public ListFeedbacksQueryTests()
    {
        _repositoryMock = new();
        _cache = new(new FusionCacheOptions());
        _handler = new(_repositoryMock.Object, _cache);
        _bookId = Guid.CreateVersion7();
        _feedbacks = new FeedbackFaker().Generate();
    }

    public void Dispose()
    {
        _cache.Dispose();
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
        result.Count.ShouldBe(_feedbacks.Length);
        result.PageIndex.ShouldBe(0);
        result.PageSize.ShouldBe(10);
        result.TotalItems.ShouldBe(_feedbacks.Length);
        result.TotalPages.ShouldBe(2);

        foreach (var feedback in _feedbacks)
        {
            result.ShouldContain(x => x.Id == feedback.Id);
            result.ShouldContain(x => x.FirstName == feedback.FirstName);
            result.ShouldContain(x => x.LastName == feedback.LastName);
            result.ShouldContain(x => x.Comment == feedback.Comment);
            result.ShouldContain(x => x.Rating == feedback.Rating);
            result.ShouldContain(x => x.BookId == feedback.BookId);
        }

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
    [MatrixDataSource]
    public async Task GivenValidQueryWithPagination_WhenHandlingListFeedbacks_ThenShouldApplyPagination(
        [Matrix<string>(
            nameof(Feedback.Rating),
            nameof(Feedback.CreatedAt),
            nameof(Feedback.RowVersion),
            null!,
            " ",
            ""
        )]
            string? orderBy,
        [Matrix<bool>(true, false)] bool isDescending
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
        result.Count.ShouldBe(feedbacks.Count);

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
            .ReturnsAsync([]);

        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeEmpty();
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

    [Test]
    public async Task GivenCachedFeedbacks_WhenHandlingListFeedbacksTwice_ThenShouldReturnFromCache()
    {
        // Arrange
        var query = new ListFeedbacksQuery(_bookId);
        _repositoryMock
            .Setup(x => x.ListAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_feedbacks);
        _repositoryMock
            .Setup(x => x.CountAsync(It.IsAny<FeedbackFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_feedbacks.Length);

        // Act
        var first = await _handler.Handle(query, CancellationToken.None);
        var second = await _handler.Handle(query, CancellationToken.None);

        // Assert
        first.Count.ShouldBe(_feedbacks.Length);
        second.Count.ShouldBe(_feedbacks.Length);
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
    public async Task GivenCachedFeedbackPage_WhenSerializedWithFusionCacheSerializer_ThenShouldRoundTripCleanly()
    {
        // Arrange
        var page = new CachedFeedbackPage(_feedbacks.ToFeedbackDtos(), _feedbacks.Length);
        var serializer = new FusionCacheSystemTextJsonSerializer();

        // Act
        var bytes = await serializer.SerializeAsync(page);
        var deserialized = await serializer.DeserializeAsync<CachedFeedbackPage>(bytes);

        // Assert
        deserialized.ShouldNotBeNull();
        deserialized.TotalItems.ShouldBe(_feedbacks.Length);
        deserialized.Items.Count.ShouldBe(_feedbacks.Length);
        deserialized.Items[0].Id.ShouldBe((Guid)_feedbacks[0].Id);
        deserialized.Items[0].Rating.ShouldBe(_feedbacks[0].Rating);
    }
}
