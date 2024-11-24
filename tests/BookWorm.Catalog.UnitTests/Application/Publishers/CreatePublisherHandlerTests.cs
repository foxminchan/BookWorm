using BookWorm.Catalog.Features.Publishers.Create;

namespace BookWorm.Catalog.UnitTests.Application.Publishers;

public sealed class CreatePublisherHandlerTests
{
    private readonly CreatePublisherHandler _handler;
    private readonly Mock<IRepository<Publisher>> _repositoryMock;

    public CreatePublisherHandlerTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task GivenPublisherName_ShouldReturnPublisherId_WhenPublisherIsCreated()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new CreatePublisherCommand("Test Publisher");
        var publisher = new Publisher(command.Name) { Id = publisherId };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Publisher>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(publisher);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().Be(publisherId);
        _repositoryMock.Verify(
            r =>
                r.AddAsync(
                    It.Is<Publisher>(p => p.Name == command.Name),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Theory]
    [CombinatorialData]
    public void GivenNullOrEmptyPublisherName_ShouldThrowException(
        [CombinatorialValues("", null)] string? name
    )
    {
        // Arrange
        var command = new CreatePublisherCommand(name!);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }
}
