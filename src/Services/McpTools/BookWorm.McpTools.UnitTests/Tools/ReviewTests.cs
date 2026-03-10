using System.Net;
using BookWorm.McpTools.Models;
using BookWorm.McpTools.Services;
using BookWorm.McpTools.Tools;
using Refit;

namespace BookWorm.McpTools.UnitTests.Tools;

public sealed class ReviewTests
{
    private readonly Mock<IRatingApi> _ratingApi = new();
    private readonly Review _sut;
    private readonly Guid _validBookId = Guid.CreateVersion7();

    public ReviewTests()
    {
        _sut = new(_ratingApi.Object);
    }

    [Test]
    public async Task GivenApiReturnsFailure_WhenGetBookReviews_ThenShouldReturnErrorMessage()
    {
        // Arrange
        var response = new ApiResponse<List<Feedback>>(
            new(HttpStatusCode.InternalServerError),
            null,
            new()
        );

        _ratingApi.Setup(x => x.ListFeedbacksAsync(It.IsAny<Guid>())).ReturnsAsync(response);

        // Act
        var result = await _sut.GetBookReviewsAsync(_validBookId);

        // Assert
        result.ShouldBe("There was an error retrieving reviews. Please try again later.");
    }

    [Test]
    public async Task GivenApiReturnsEmptyList_WhenGetBookReviews_ThenShouldReturnNotFoundMessage()
    {
        // Arrange
        var response = new ApiResponse<List<Feedback>>(new(HttpStatusCode.OK), [], new());

        _ratingApi.Setup(x => x.ListFeedbacksAsync(It.IsAny<Guid>())).ReturnsAsync(response);

        // Act
        var result = await _sut.GetBookReviewsAsync(_validBookId);

        // Assert
        result.ShouldBe($"No reviews found for book with ID {_validBookId}.");
    }

    [Test]
    public async Task GivenApiReturnsReviews_WhenGetBookReviews_ThenShouldReturnSerializedJson()
    {
        // Arrange
        var feedbacks = new List<Feedback>
        {
            new(Guid.CreateVersion7(), "John", "Doe", "Great book!", 5, _validBookId),
            new(Guid.CreateVersion7(), "Jane", "Smith", "Good read", 4, _validBookId),
        };

        var response = new ApiResponse<List<Feedback>>(new(HttpStatusCode.OK), feedbacks, new());

        _ratingApi.Setup(x => x.ListFeedbacksAsync(_validBookId)).ReturnsAsync(response);

        // Act
        var result = await _sut.GetBookReviewsAsync(_validBookId);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldContain("John");
        result.ShouldContain("Great book!");
        _ratingApi.Verify(x => x.ListFeedbacksAsync(_validBookId), Times.Once);
    }
}
