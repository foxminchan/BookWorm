using BookWorm.Rating.Features.Delete;

namespace BookWorm.Rating.UnitTests.Application;

public sealed class DeleteFeedbackHandlerTests
{
    private readonly DeleteFeedbackHandler _handler;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IRatingRepository> _repositoryMock;

    public DeleteFeedbackHandlerTests()
    {
        _repositoryMock = new();
        _publishEndpointMock = new();
        _identityServiceMock = new();
        _handler = new(_repositoryMock.Object, _publishEndpointMock.Object, _identityServiceMock.Object);
    }

    [Fact]
    public async Task GivenValidRequest_ShouldDeleteFeedbackAndPublishEvent_WhenUserIsAdmin()
    {
        // Arrange
        var feedbackId = ObjectId.GenerateNewId();
        var feedback = new Feedback(Guid.NewGuid(), 5, "Great!", Guid.NewGuid());

        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(true);
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedback);
        _repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publishEndpointMock.Setup(x =>
                x.Publish(It.IsAny<FeedbackDeletedIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteFeedbackCommand(feedbackId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<FeedbackDeletedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenValidRequest_ShouldDeleteFeedbackAndPublishEvent_WhenUserIsNotAdminAndOwnsFeedback()
    {
        // Arrange
        var feedbackId = ObjectId.GenerateNewId();
        var userId = Guid.NewGuid();
        var feedback = new Feedback(Guid.NewGuid(), 5, "Great!", userId);

        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(false);
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(userId.ToString());
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedback);
        _repositoryMock.Setup(x => x.DeleteAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publishEndpointMock.Setup(x =>
                x.Publish(It.IsAny<FeedbackDeletedIntegrationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteFeedbackCommand(feedbackId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<FeedbackDeletedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenValidRequest_ShouldThrowNotFoundException_WhenFeedbackNotFound()
    {
        // Arrange
        var feedbackId = ObjectId.GenerateNewId();

        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(true);
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feedback?)null);

        var command = new DeleteFeedbackCommand(feedbackId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<FeedbackDeletedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [CombinatorialData]
    public async Task GivenValidRequest_ShouldThrowNotFoundException_WhenUserIsNotAdminAndDoesNotOwnFeedback(
        [CombinatorialValues(true, false)] bool isAdmin)
    {
        // Arrange
        var feedbackId = ObjectId.GenerateNewId();
        var userId = Guid.NewGuid();

        _identityServiceMock.Setup(x => x.IsAdminRole()).Returns(isAdmin);
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns(userId.ToString());
        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feedback?)null);

        var command = new DeleteFeedbackCommand(feedbackId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()), Times.Never);
        _publishEndpointMock.Verify(
            x => x.Publish(It.IsAny<FeedbackDeletedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
