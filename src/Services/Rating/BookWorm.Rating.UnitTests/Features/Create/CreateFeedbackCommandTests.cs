using BookWorm.Rating.Domain.FeedbackAggregator;
using BookWorm.Rating.Features.Create;

namespace BookWorm.Rating.UnitTests.Features.Create;

public sealed class CreateFeedbackCommandTests
{
    private readonly CreateFeedbackCommandFaker _faker;
    private readonly CreateFeedbackHandler _handler;
    private readonly Mock<IFeedbackRepository> _repositoryMock;

    public CreateFeedbackCommandTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();

        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateFeedback_ThenShouldAddToRepositoryAndSaveChanges()
    {
        // Arrange
        var command = _faker.Generate();

        var feedbackWithId = new Feedback(
            command.BookId,
            command.FirstName,
            command.LastName,
            command.Comment,
            command.Rating
        );

        // Set a known ID for verification
        var expectedId = Guid.CreateVersion7();

        // Use reflection to set the ID since it's likely not directly settable
        typeof(Feedback).GetProperty("Id")?.SetValue(feedbackWithId, expectedId);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Feedback>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedbackWithId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedId);
        _repositoryMock.Verify(
            r =>
                r.AddAsync(
                    It.Is<Feedback>(f =>
                        f.BookId == command.BookId
                        && f.FirstName == command.FirstName
                        && f.LastName == command.LastName
                        && f.Comment == command.Comment
                        && f.Rating == command.Rating
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateFeedback_ThenShouldReturnCorrectId()
    {
        // Arrange
        var command = _faker.Generate();
        var expectedId = Guid.CreateVersion7();
        var feedback = new Feedback(
            command.BookId,
            command.FirstName,
            command.LastName,
            command.Comment,
            command.Rating
        );

        // Use reflection to set the ID since it's likely not directly settable
        typeof(Feedback).GetProperty("Id")?.SetValue(feedback, expectedId);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Feedback>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedback);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedId);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Feedback>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateFeedback_ThenShouldCreateCorrectFeedbackEntity()
    {
        // Arrange
        var command = _faker.Generate();
        Feedback? capturedFeedback = null;

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Feedback>(), It.IsAny<CancellationToken>()))
            .Callback<Feedback, CancellationToken>((f, _) => capturedFeedback = f)
            .ReturnsAsync(
                new Feedback(
                    command.BookId,
                    command.FirstName,
                    command.LastName,
                    command.Comment,
                    command.Rating
                )
            );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedFeedback.ShouldNotBeNull();
        capturedFeedback.BookId.ShouldBe(command.BookId);
        capturedFeedback.FirstName.ShouldBe(command.FirstName);
        capturedFeedback.LastName.ShouldBe(command.LastName);
        capturedFeedback.Comment.ShouldBe(command.Comment);
        capturedFeedback.Rating.ShouldBe(command.Rating);

        _repositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Feedback>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
