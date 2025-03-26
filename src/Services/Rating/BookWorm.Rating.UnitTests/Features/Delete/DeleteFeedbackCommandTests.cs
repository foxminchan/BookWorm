using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.Features.Delete;
using BookWorm.Rating.UnitTests.Fakers;
using BookWorm.SharedKernel.Exceptions;
using MediatR;

namespace BookWorm.Rating.UnitTests.Features.Delete;

public sealed class DeleteFeedbackCommandTests
{
    private readonly Feedback _feedback;
    private readonly DeleteFeedbackHandler _handler;
    private readonly Mock<IFeedbackRepository> _repositoryMock;

    public DeleteFeedbackCommandTests()
    {
        _feedback = new FeedbackFaker().Generate().First();
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenExistingFeedback_WhenHandlingDeleteCommand_ThenShouldDeleteAndSaveChanges()
    {
        // Arrange
        var command = new DeleteFeedbackCommand(_feedback.Id);
        _repositoryMock
            .Setup(x => x.GetByIdAsync(_feedback.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_feedback);
        _repositoryMock
            .Setup(x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(x => x.Delete(_feedback), Times.Once);
        _repositoryMock.Verify(
            x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistingFeedback_WhenHandlingDeleteCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var feedbackId = Guid.CreateVersion7();
        var command = new DeleteFeedbackCommand(feedbackId);
        _repositoryMock
            .Setup(x => x.GetByIdAsync(feedbackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feedback)default!);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Feedback with ID {feedbackId} not found.");
        _repositoryMock.Verify(x => x.Delete(It.IsAny<Feedback>()), Times.Never);
        _repositoryMock.Verify(
            x => x.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
