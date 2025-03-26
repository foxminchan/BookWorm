using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Features.Publishers.Update;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.SharedKernel.Exceptions;
using MediatR;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Update;

public sealed class UpdatePublisherCommandTests
{
    private readonly PublisherFaker _faker;
    private readonly UpdatePublisherHandler _handler;
    private readonly Mock<IPublisherRepository> _repositoryMock;

    public UpdatePublisherCommandTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public async Task GivenExistingPublisher_WhenHandlingUpdatePublisherCommand_ThenShouldUpdateNameAndSaveChanges()
    {
        // Arrange
        var publisher = _faker.Generate(1).First();
        var publisherId = publisher.Id;
        const string newName = "Updated Publisher Name";

        var command = new UpdatePublisherCommand(publisherId, newName);

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
        publisher.Name.ShouldBe(newName);
        _repositoryMock.Verify(
            r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenNonExistingPublisher_WhenHandlingUpdatePublisherCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var publisherId = Guid.CreateVersion7();
        var command = new UpdatePublisherCommand(publisherId, "Any Name");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(publisherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Publisher)null!);

        // Act & Assert
        var exception = await Should.ThrowAsync<NotFoundException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Message.ShouldContain(publisherId.ToString());
    }
}
