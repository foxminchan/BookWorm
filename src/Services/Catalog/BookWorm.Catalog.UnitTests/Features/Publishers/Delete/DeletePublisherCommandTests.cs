using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Features.Publishers.Delete;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.CQRS.Command;
using BookWorm.Chassis.Exceptions;
using MediatR;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Delete;

public sealed class DeletePublisherCommandTests
{
    private readonly PublisherFaker _faker;
    private readonly DeletePublisherHandler _handler;
    private readonly Mock<IPublisherRepository> _repositoryMock;

    public DeletePublisherCommandTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public async Task GivenExistingPublisher_WhenHandlingDeletePublisherCommand_ThenShouldDeletePublisherAndSaveChanges()
    {
        // Arrange
        var publisher = _faker.Generate(1)[0];
        var publisherId = publisher.Id;
        var command = new DeletePublisherCommand(publisherId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publisher);

        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(
            r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.Delete(publisher), Times.Once);
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistingPublisher_WhenHandlingDeletePublisherCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var publisherId = Guid.CreateVersion7();
        var command = new DeletePublisherCommand(publisherId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Publisher?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });

        exception.Message.ShouldBe($"Publisher with id {publisherId} not found.");
        _repositoryMock.Verify(
            r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Publisher>()), Times.Never);
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingDeletePublisher_ThenShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var publisher = _faker.Generate(1)[0];
        var publisherId = publisher.Id;
        var command = new DeletePublisherCommand(publisherId);
        var cancellationToken = CancellationToken.None;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(publisherId, cancellationToken))
            .ReturnsAsync(publisher);

        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(cancellationToken))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _repositoryMock.Verify(r => r.GetByIdAsync(publisherId, cancellationToken), Times.Once);
        _repositoryMock.Verify(r => r.Delete(publisher), Times.Once);
        _repositoryMock.Verify(r => r.UnitOfWork.SaveEntitiesAsync(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingDeletePublisher_ThenShouldPropagateException()
    {
        // Arrange
        var publisherId = Guid.CreateVersion7();
        var command = new DeletePublisherCommand(publisherId);
        var expectedException = new InvalidOperationException("Database connection failed");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });

        exception.Message.ShouldBe("Database connection failed");
        _repositoryMock.Verify(
            r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Publisher>()), Times.Never);
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenSaveEntitiesThrowsException_WhenHandlingDeletePublisher_ThenShouldPropagateException()
    {
        // Arrange
        var publisher = _faker.Generate(1)[0];
        var publisherId = publisher.Id;
        var command = new DeletePublisherCommand(publisherId);
        var expectedException = new InvalidOperationException("Failed to save changes");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publisher);

        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });

        exception.Message.ShouldBe("Failed to save changes");
        _repositoryMock.Verify(
            r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.Delete(publisher), Times.Once);
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public void GivenDeletePublisherCommand_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Arrange
        var publisherId = Guid.CreateVersion7();

        // Act
        var command = new DeletePublisherCommand(publisherId);

        // Assert
        command.ShouldNotBeNull();
        command.ShouldBeOfType<DeletePublisherCommand>();
        command.ShouldBeAssignableTo<ICommand>();
        command.Id.ShouldBe(publisherId);
    }

    [Test]
    public void GivenTwoDeletePublisherCommandsWithSameId_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var publisherId = Guid.CreateVersion7();
        var command1 = new DeletePublisherCommand(publisherId);
        var command2 = new DeletePublisherCommand(publisherId);

        // Act & Assert
        command1.ShouldBe(command2);
        command1.Equals(command2).ShouldBeTrue();
        (command1 == command2).ShouldBeTrue();
        command1.GetHashCode().ShouldBe(command2.GetHashCode());
    }

    [Test]
    public void GivenTwoDeletePublisherCommandsWithDifferentIds_WhenComparing_ThenShouldNotBeEqual()
    {
        // Arrange
        var command1 = new DeletePublisherCommand(Guid.CreateVersion7());
        var command2 = new DeletePublisherCommand(Guid.CreateVersion7());

        // Act & Assert
        command1.ShouldNotBe(command2);
        command1.Equals(command2).ShouldBeFalse();
        (command1 == command2).ShouldBeFalse();
        command1.GetHashCode().ShouldNotBe(command2.GetHashCode());
    }

    [Test]
    public async Task GivenCancellationRequested_WhenHandlingDeletePublisher_ThenShouldRespectCancellation()
    {
        // Arrange
        var publisher = _faker.Generate(1)[0];
        var publisherId = publisher.Id;
        var command = new DeletePublisherCommand(publisherId);
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(publisher);

        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .Callback(cancellationTokenSource.Cancel)
            .ThrowsAsync(new OperationCanceledException());

        try
        {
            // Act & Assert
            await Should.ThrowAsync<OperationCanceledException>(async () =>
            {
                await _handler.Handle(command, cancellationToken);
            });

            _repositoryMock.Verify(
                r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()),
                Times.Once
            );
            _repositoryMock.Verify(r => r.Delete(publisher), Times.Once);
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }
    }
}
