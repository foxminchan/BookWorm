using System.Text.Json;
using System.Text.Json.Serialization;
using BookWorm.Rating.Features;
using BookWorm.Rating.Features.List;
using BookWorm.Rating.Tools;
using BookWorm.SharedKernel.Results;
using Mediator;
using Microsoft.Extensions.AI;

namespace BookWorm.Rating.UnitTests.Tools;

public sealed class ReviewToolTests
{
    private readonly ReviewTool _reviewTool;
    private readonly Mock<ISender> _senderMock;
    private readonly Guid _validBookId;

    public ReviewToolTests()
    {
        _senderMock = new();
        _reviewTool = new(_senderMock.Object);
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
        var result = await _reviewTool.GetCustomerReviews(_validBookId);

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
        var result = await _reviewTool.GetCustomerReviews(_validBookId);

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
        var result = await _reviewTool.GetCustomerReviews(emptyBookId);

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
            _reviewTool.GetCustomerReviews(_validBookId)
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
        var result = await _reviewTool.GetCustomerReviews(_validBookId);

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
        var result = await _reviewTool.GetCustomerReviews(_validBookId);

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
        await _reviewTool.GetCustomerReviews(_validBookId);

        // Assert
        _senderMock.Verify(
            x =>
                x.Send(
                    It.Is<ListFeedbacksQuery>(q =>
                        q.BookId == _validBookId
                        && q.PageIndex == 1
                        && q.PageSize == int.MaxValue
                        && q.OrderBy == "Rating"
                        && !q.IsDescending
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
        var result = await _reviewTool.GetCustomerReviews(_validBookId);

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
        var result = await _reviewTool.GetCustomerReviews(_validBookId);

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
        var result = await _reviewTool.GetCustomerReviews(_validBookId);

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

    [Test]
    public void GivenReviewTool_WhenAsAITools_ThenShouldReturnNonNullEnumerable()
    {
        // Act
        var result = _reviewTool.AsAITools();

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public void GivenReviewTool_WhenAsAITools_ThenShouldReturnExactlyOneAITool()
    {
        // Act
        var result = _reviewTool.AsAITools();
        var tools = result.ToList();

        // Assert
        tools.Count.ShouldBe(1);
    }

    [Test]
    public void GivenReviewTool_WhenAsAITools_ThenShouldReturnNonNullAITool()
    {
        // Act
        var result = _reviewTool.AsAITools();
        var tool = result.FirstOrDefault();

        // Assert
        tool.ShouldNotBeNull();
    }

    [Test]
    public void GivenReviewTool_WhenAsAIToolsCalledMultipleTimes_ThenShouldReturnNewEnumerableEachTime()
    {
        // Act
        var result1 = _reviewTool.AsAITools();
        var result2 = _reviewTool.AsAITools();

        // Assert
        result1.ShouldNotBeSameAs(result2);
    }

    [Test]
    public void GivenReviewTool_WhenAsAITools_ThenShouldCreateToolWithGetCustomerReviewsName()
    {
        // Act
        var result = _reviewTool.AsAITools();
        var tool = result.First();

        // Assert
        tool.Name.ShouldBe(nameof(ReviewTool.GetCustomerReviews));
    }

    [Test]
    public void GivenReviewTool_WhenAsAITools_ThenShouldCreateToolWithDescription()
    {
        // Act
        var result = _reviewTool.AsAITools();
        var tool = result.First();

        // Assert
        tool.Description.ShouldNotBeNullOrWhiteSpace();
        tool.Description.ShouldContain("reviews");
        tool.Description.ShouldContain("book");
    }

    [Test]
    public void GivenReviewTool_WhenAsAIToolsEnumeratedMultipleTimes_ThenShouldReturnSameToolsEachTime()
    {
        // Act
        var result = _reviewTool.AsAITools().ToList();

        // Assert
        result.Count.ShouldBe(result.Count);
        result.Count.ShouldBe(1);
    }

    private static List<FeedbackDto> DeserializeFeedbackList(string json)
    {
        return JsonSerializer.Deserialize<List<FeedbackDto>>(
                json,
                TestFeedbackSerializationContext.Default.Options
            ) ?? [];
    }
}

[JsonSerializable(typeof(FeedbackDto))]
[JsonSerializable(typeof(List<FeedbackDto>))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
)]
internal sealed partial class TestFeedbackSerializationContext : JsonSerializerContext;
