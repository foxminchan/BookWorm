using BookWorm.Rating.Features.List;

namespace BookWorm.Rating.UnitTests.Application;

public sealed class ListFeedbackHandlerTests
{
    private readonly ListFeedbackHandler _handler;
    private readonly Mock<IRatingRepository> _repositoryMock;

    public ListFeedbackHandlerTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task GivenValidRequest_ShouldReturnPagedResultWithFeedbacks_WhenFeedbacksExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var feedbacks = new List<Feedback>
        {
            new(bookId, 5, "Great!", Guid.NewGuid()),
            new(bookId, 4, "Good!", Guid.NewGuid()),
        };

        _repositoryMock
            .Setup(x =>
                x.ListAsync(
                    It.IsAny<FilterDefinition<Feedback>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(feedbacks);

        _repositoryMock
            .Setup(x =>
                x.CountAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(feedbacks.Count);

        var query = new ListFeedbackQuery(bookId, 0, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(feedbacks);
        result.PagedInfo.TotalRecords.Should().Be(feedbacks.Count);
        result.PagedInfo.TotalPages.Should().Be(1);
        _repositoryMock.Verify(
            x =>
                x.ListAsync(
                    It.IsAny<FilterDefinition<Feedback>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _repositoryMock.Verify(
            x =>
                x.CountAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task GivenValidRequest_ShouldReturnEmptyPagedResult_WhenNoFeedbacksExist()
    {
        // Arrange
        var bookId = Guid.NewGuid();

        _repositoryMock
            .Setup(x =>
                x.ListAsync(
                    It.IsAny<FilterDefinition<Feedback>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync([]);

        _repositoryMock
            .Setup(x =>
                x.CountAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(0);

        var query = new ListFeedbackQuery(bookId, 0, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeEmpty();
        result.PagedInfo.TotalRecords.Should().Be(0);
        result.PagedInfo.TotalPages.Should().Be(0);
        _repositoryMock.Verify(
            x =>
                x.ListAsync(
                    It.IsAny<FilterDefinition<Feedback>>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _repositoryMock.Verify(
            x =>
                x.CountAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Theory]
    [CombinatorialData]
    public async Task GivenValidRequest_ShouldCalculateTotalPagesCorrectly_WhenPageSizeChanges(
        [CombinatorialValues(1, 10, 25)] int pageSize,
        [CombinatorialValues(5, 10, 30)] int totalRecords
    )
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var feedbacks = new List<Feedback>();

        for (var i = 0; i < pageSize; i++)
        {
            feedbacks.Add(new(bookId, 5, "Great!", Guid.NewGuid()));
        }

        _repositoryMock
            .Setup(x =>
                x.ListAsync(
                    It.IsAny<FilterDefinition<Feedback>>(),
                    It.IsAny<int>(),
                    pageSize,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(feedbacks);

        _repositoryMock
            .Setup(x =>
                x.CountAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(totalRecords);

        var query = new ListFeedbackQuery(bookId, 0, pageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PagedInfo.TotalPages.Should().Be((int)Math.Ceiling(totalRecords / (double)pageSize));
        _repositoryMock.Verify(
            x =>
                x.ListAsync(
                    It.IsAny<FilterDefinition<Feedback>>(),
                    It.IsAny<int>(),
                    pageSize,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _repositoryMock.Verify(
            x =>
                x.CountAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
