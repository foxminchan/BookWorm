using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Features.Authors.Update;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.Exceptions;

namespace BookWorm.Catalog.UnitTests.Features.Authors.Update;

public sealed class UpdateAuthorCommandTests
{
    private readonly AuthorFaker _faker;
    private readonly UpdateAuthorHandler _handler;
    private readonly Mock<IAuthorRepository> _repositoryMock;

    public UpdateAuthorCommandTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingUpdateAuthor_ThenShouldUpdateAuthorName()
    {
        // Arrange
        var author = _faker.Generate()[0];
        var command = new UpdateAuthorCommand(author.Id, "New Author Name");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(author.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        author.Name.ShouldBe("New Author Name");
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenInvalidAuthorId_WhenHandlingUpdateAuthor_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateAuthorCommand(Guid.CreateVersion7(), "New Author Name");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author)null!);

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(async () =>
            await _handler.Handle(command, CancellationToken.None)
        );
    }
}
