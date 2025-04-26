using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Features.Authors.Create;

namespace BookWorm.Catalog.UnitTests.Features.Authors.Create;

public sealed class CreateAuthorCommandTests
{
    private readonly CreateAuthorCommandFaker _faker;
    private readonly CreateAuthorHandler _handler;
    private readonly Mock<IAuthorRepository> _repositoryMock;

    public CreateAuthorCommandTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateAuthor_ThenShouldCallSaveEntitiesAsync()
    {
        // Arrange
        var command = _faker.Generate();
        var author = new Author(command.Name);
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);
        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(author.Id);
        _repositoryMock.Verify(
            r =>
                r.AddAsync(
                    It.Is<Author>(a => a.Name == command.Name),
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
    public async Task GivenRepositoryThrowsException_WhenHandlingCreateAuthor_ThenShouldThrowException()
    {
        // Arrange
        var command = _faker.Generate();
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Repository error"));

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
            await _handler.Handle(command, CancellationToken.None)
        );
        _repositoryMock.Verify(
            r =>
                r.AddAsync(
                    It.Is<Author>(a => a.Name == command.Name),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
