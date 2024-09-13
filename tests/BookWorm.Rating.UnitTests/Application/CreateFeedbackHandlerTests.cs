using BookWorm.Rating.Features.Create;

namespace BookWorm.Rating.UnitTests.Application;

public sealed class CreateFeedbackHandlerTests
{
    private readonly Mock<IRatingRepository> _collectionMock;
    private readonly CreateFeedbackHandler _handler;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;

    public CreateFeedbackHandlerTests()
    {
        _collectionMock = new();
        _publishEndpointMock = new();
        _handler = new(_collectionMock.Object, _publishEndpointMock.Object);
    }

    [Fact]
    public async Task GivenValidCommand_ShouldInsertFeedbackAndPublishEvent()
    {
        // Arrange
        var command = new CreateFeedbackCommand(Guid.NewGuid(), 5, "Great book!", Guid.NewGuid());

        _collectionMock.Setup(x => x.AddAsync(
                It.IsAny<Feedback>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _publishEndpointMock.Setup(x =>
                x.Publish(It.IsAny<FeedbackCreatedIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _collectionMock.Verify(x => x.AddAsync(It.IsAny<Feedback>(), It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<FeedbackCreatedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [CombinatorialData]
    public async Task GivenMissingOrInvalidData_ShouldThrowArgumentException(
        [CombinatorialValues(-1, 6)] int rating,
        [CombinatorialValues(null, "", " ")] string? comment)
    {
        // Arrange
        var command = new CreateFeedbackCommand(Guid.NewGuid(), rating, comment, Guid.NewGuid());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _collectionMock.Verify(x => x.AddAsync(It.IsAny<Feedback>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
