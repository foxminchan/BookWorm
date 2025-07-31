using System.Text.Json;
using BookWorm.Rating.Features;
using BookWorm.Rating.Features.List;
using BookWorm.Rating.Plugins;
using BookWorm.SharedKernel.Results;
using MediatR;

namespace BookWorm.Rating.UnitTests.Plugins;

public sealed class ReviewPluginTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly ReviewPlugin _reviewPlugin;
    private readonly Mock<ISender> _senderMock;
    private readonly Guid _validBookId;

    public ReviewPluginTests()
    {
        _senderMock = new();
        _reviewPlugin = new(_senderMock.Object);
        _validBookId = Guid.CreateVersion7();
    }

    [Test]
    public async Task GivenValidBookIdWithExistingReviews_WhenGetCustomerReviews_ThenShouldReturnJsonSerializedReviews()
    {
        // Arrange
        var feedbackDtos = new List<FeedbackDto>
        {
            new(Guid.CreateVersion7(), "John", "Doe", "Great book!", 5, _validBookId),
            new(Guid.CreateVersion7(), "Jane", "Smith", "Good read", 4, _validBookId),
        };

        var pagedResult = new PagedResult<FeedbackDto>(feedbackDtos, 1, int.MaxValue, 2);

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _reviewPlugin.GetCustomerReviews(_validBookId);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();

        // Verify JSON structure by deserializing with proper options
        var deserializedResult = DeserializeFeedbackList(result);
        deserializedResult.ShouldNotBeNull();
        deserializedResult.Count.ShouldBe(2);
        deserializedResult.ShouldContain(x => x.FirstName == "John");
        deserializedResult.ShouldContain(x => x.FirstName == "Jane");

        // Verify correct query was sent
        _senderMock.Verify(
            x =>
                x.Send(
                    It.Is<ListFeedbacksQuery>(q =>
                        q.BookId == _validBookId && q.PageIndex == 1 && q.PageSize == int.MaxValue
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidBookIdWithNoReviews_WhenGetCustomerReviews_ThenShouldReturnNoReviewsMessage()
    {
        // Arrange
        var emptyResult = new PagedResult<FeedbackDto>([], 1, int.MaxValue, 0);

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _reviewPlugin.GetCustomerReviews(_validBookId);

        // Assert
        result.ShouldBe("No reviews found for this book");

        _senderMock.Verify(
            x =>
                x.Send(
                    It.Is<ListFeedbacksQuery>(q => q.BookId == _validBookId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyGuidBookId_WhenGetCustomerReviews_ThenShouldCallSenderWithEmptyGuid()
    {
        // Arrange
        var emptyBookId = Guid.Empty;
        var emptyResult = new PagedResult<FeedbackDto>([], 1, int.MaxValue, 0);

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await _reviewPlugin.GetCustomerReviews(emptyBookId);

        // Assert
        result.ShouldBe("No reviews found for this book");

        _senderMock.Verify(
            x =>
                x.Send(
                    It.Is<ListFeedbacksQuery>(q => q.BookId == emptyBookId),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenSenderThrowsException_WhenGetCustomerReviews_ThenShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database connection failed");

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Should.ThrowAsync<InvalidOperationException>(() =>
            _reviewPlugin.GetCustomerReviews(_validBookId)
        );

        actualException.Message.ShouldBe("Database connection failed");

        _senderMock.Verify(
            x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidBookIdWithMultipleReviews_WhenGetCustomerReviews_ThenShouldSerializeAllReviews()
    {
        // Arrange
        var feedbackDtos = new List<FeedbackDto>();
        for (var i = 1; i <= 5; i++)
        {
            feedbackDtos.Add(
                new(
                    Guid.CreateVersion7(),
                    $"FirstName{i}",
                    $"LastName{i}",
                    $"Comment {i}",
                    i,
                    _validBookId
                )
            );
        }

        var pagedResult = new PagedResult<FeedbackDto>(feedbackDtos, 1, int.MaxValue, 5);

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _reviewPlugin.GetCustomerReviews(_validBookId);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();

        var deserializedResult = DeserializeFeedbackList(result);
        deserializedResult.ShouldNotBeNull();
        deserializedResult.Count.ShouldBe(5);

        // Verify all reviews are serialized correctly
        for (var i = 1; i <= 5; i++)
        {
            deserializedResult.ShouldContain(x => x.FirstName == $"FirstName{i}");
            deserializedResult.ShouldContain(x => x.Rating == i);
        }
    }

    [Test]
    public async Task GivenValidBookIdWithNullAndEmptyComments_WhenGetCustomerReviews_ThenShouldHandleNullValues()
    {
        // Arrange
        var feedbackDtos = new List<FeedbackDto>
        {
            new(Guid.CreateVersion7(), "John", "Doe", null, 5, _validBookId),
            new(Guid.CreateVersion7(), null, null, "Great book!", 4, _validBookId),
            new(Guid.CreateVersion7(), "", "", "", 3, _validBookId),
        };

        var pagedResult = new PagedResult<FeedbackDto>(feedbackDtos, 1, int.MaxValue, 3);

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _reviewPlugin.GetCustomerReviews(_validBookId);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();

        var deserializedResult = DeserializeFeedbackList(result);
        deserializedResult.ShouldNotBeNull();
        deserializedResult.Count.ShouldBe(3);

        // Verify null values are handled correctly
        deserializedResult.ShouldContain(x => x.Comment == null);
        deserializedResult.ShouldContain(x => x.FirstName == null);
        deserializedResult.ShouldContain(x => x.FirstName == "");
    }

    [Test]
    public async Task GivenValidBookId_WhenGetCustomerReviews_ThenShouldUseCorrectQueryParameters()
    {
        // Arrange
        var pagedResult = new PagedResult<FeedbackDto>([], 1, int.MaxValue, 0);

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        await _reviewPlugin.GetCustomerReviews(_validBookId);

        // Assert
        _senderMock.Verify(
            x =>
                x.Send(
                    It.Is<ListFeedbacksQuery>(q =>
                        q.BookId == _validBookId
                        && q.PageIndex == 1
                        && q.PageSize == int.MaxValue
                        && q.OrderBy == "Rating"
                        && q.IsDescending == false
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidBookIdWithExistingReviews_WhenGetCustomerReviews_ThenShouldSerializeOnlyItemsNotPaginationMetadata()
    {
        // Arrange
        var feedbackDtos = new List<FeedbackDto>
        {
            new(Guid.CreateVersion7(), "John", "Doe", "Great book!", 5, _validBookId),
        };

        var pagedResult = new PagedResult<FeedbackDto>(feedbackDtos, 1, int.MaxValue, 1);

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _reviewPlugin.GetCustomerReviews(_validBookId);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();

        // Verify JSON is an array, not an object with pagination metadata
        result.ShouldStartWith("[");
        result.ShouldEndWith("]");

        // Verify it doesn't contain pagination properties
        result.ShouldNotContain("pageIndex");
        result.ShouldNotContain("pageSize");
        result.ShouldNotContain("totalItems");
        result.ShouldNotContain("totalPages");

        // Verify it contains the actual feedback data
        result.ShouldContain("John");
        result.ShouldContain("Doe");
        result.ShouldContain("Great book!");
    }

    [Test]
    public async Task GivenValidBookIdWithExistingReviews_WhenGetCustomerReviews_ThenShouldReturnValidJsonWithCorrectPropertyCasing()
    {
        // Arrange
        var feedbackDtos = new List<FeedbackDto>
        {
            new(Guid.CreateVersion7(), "John", "Doe", "Great book!", 5, _validBookId),
        };

        var pagedResult = new PagedResult<FeedbackDto>(feedbackDtos, 1, int.MaxValue, 1);

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _reviewPlugin.GetCustomerReviews(_validBookId);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();

        // Verify JSON uses camelCase property names as expected by the serialization context
        result.ShouldContain("\"firstName\":");
        result.ShouldContain("\"lastName\":");
        result.ShouldContain("\"comment\":");
        result.ShouldContain("\"rating\":");
        result.ShouldContain("\"bookId\":");

        // Verify JSON doesn't use PascalCase - removed to avoid case-insensitive matching issues
        // Instead, we'll rely on the positive assertions above and deserialization test below

        // Verify deserialization works correctly
        var deserializedResult = DeserializeFeedbackList(result);
        deserializedResult.Count.ShouldBe(1);
        deserializedResult[0].FirstName.ShouldBe("John");
        deserializedResult[0].LastName.ShouldBe("Doe");
        deserializedResult[0].Comment.ShouldBe("Great book!");
        deserializedResult[0].Rating.ShouldBe(5);
    }

    [Test]
    public async Task GivenValidBookIdWithExistingReviews_WhenGetCustomerReviews_ThenShouldUseCorrectJsonSerialization()
    {
        // Arrange
        var feedbackId = Guid.CreateVersion7();
        var feedbackDtos = new List<FeedbackDto>
        {
            new(feedbackId, "John", "Doe", "Great book!", 5, _validBookId),
        };

        var pagedResult = new PagedResult<FeedbackDto>(feedbackDtos, 1, int.MaxValue, 1);

        _senderMock
            .Setup(x => x.Send(It.IsAny<ListFeedbacksQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _reviewPlugin.GetCustomerReviews(_validBookId);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();

        // Verify it's a JSON array
        result.ShouldStartWith("[");
        result.ShouldEndWith("]");

        // Verify the JSON can be properly deserialized with our helper method
        var deserializedResult = DeserializeFeedbackList(result);
        deserializedResult.Count.ShouldBe(1);

        var feedback = deserializedResult[0];
        feedback.Id.ShouldBe(feedbackId);
        feedback.FirstName.ShouldBe("John");
        feedback.LastName.ShouldBe("Doe");
        feedback.Comment.ShouldBe("Great book!");
        feedback.Rating.ShouldBe(5);
        feedback.BookId.ShouldBe(_validBookId);

        // Verify that raw deserialization without proper options would fail
        // This indirectly confirms that camelCase is being used
        var rawDeserialized = JsonSerializer.Deserialize<List<FeedbackDto>>(result);
        rawDeserialized.ShouldNotBeNull();
        // With camelCase JSON and default deserializer, the properties should be null/default
        rawDeserialized[0].FirstName.ShouldBeNull();
        rawDeserialized[0].LastName.ShouldBeNull();
    }

    private static List<FeedbackDto> DeserializeFeedbackList(string json)
    {
        return JsonSerializer.Deserialize<List<FeedbackDto>>(json, _jsonOptions) ?? [];
    }
}
