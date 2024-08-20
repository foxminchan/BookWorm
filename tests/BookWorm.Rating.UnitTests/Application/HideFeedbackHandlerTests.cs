using BookWorm.Rating.Domain;
using BookWorm.Rating.Features.Hide;

namespace BookWorm.Rating.UnitTests.Application;

public sealed class HideFeedbackHandlerTests
{
    private readonly HideFeedbackHandler _handler;
    private readonly Mock<IRatingRepository> _repositoryMock;

    public HideFeedbackHandlerTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task GivenValidRequest_ShouldHideFeedbackAndUpdateRepository_WhenFeedbackExists()
    {
        // Arrange
        var feedbackId = ObjectId.GenerateNewId();
        var feedback = new Feedback(Guid.NewGuid(), 5, "Great!", Guid.NewGuid());

        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedback);
        _repositoryMock.Setup(x =>
                x.UpdateAsync(It.IsAny<FilterDefinition<Feedback>>(), feedback, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new HideFeedbackCommand(feedbackId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        feedback.IsHidden.Should().BeTrue();
        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<FilterDefinition<Feedback>>(), feedback, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GivenValidRequest_ShouldThrowNotFoundException_WhenFeedbackNotFound()
    {
        // Arrange
        var feedbackId = ObjectId.GenerateNewId();

        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feedback?)null);

        var command = new HideFeedbackCommand(feedbackId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<Feedback>(),
                It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GivenValidRequest_ShouldNoAdditionalActions_WhenFeedbackAlreadyHidden()
    {
        // Arrange
        var feedbackId = ObjectId.GenerateNewId();
        var feedback = new Feedback(Guid.NewGuid(), 5, "Great!", Guid.NewGuid());

        _repositoryMock.Setup(x => x.GetAsync(It.IsAny<FilterDefinition<Feedback>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedback);

        var command = new HideFeedbackCommand(feedbackId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        feedback.IsHidden.Should().BeTrue();
        _repositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<FilterDefinition<Feedback>>(), feedback, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
