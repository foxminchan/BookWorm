using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Features.Publishers.Create;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Create;

public sealed class CreatePublisherCommandTests
{
    private readonly CreatePublisherCommandFaker _faker;
    private readonly CreatePublisherHandler _handler;
    private readonly Mock<IPublisherRepository> _repositoryMock;

    public CreatePublisherCommandTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreatePublisher_ThenShouldCallAddAsync()
    {
        // Arrange
        var command = _faker.Generate();
        var expectedId = Guid.CreateVersion7();
        var publisher = new Publisher(command.Name) { Id = expectedId };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Publisher>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(publisher);

        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedId);
        _repositoryMock.Verify(
            r =>
                r.AddAsync(
                    It.Is<Publisher>(p => p.Name == command.Name),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingCreatePublisher_ThenShouldThrowException()
    {
        // Arrange
        var command = _faker.Generate();

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Publisher>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Database error"));

        // Act & Assert
        await Should.ThrowAsync<Exception>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );
    }
}
