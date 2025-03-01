using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Features.Authors.Delete;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.SharedKernel.Exceptions;
using MediatR;

namespace BookWorm.Catalog.UnitTests.Features.Authors.Delete;

public sealed class DeleteAuthorCommandTests
{
    private readonly AuthorFaker _faker;
    private readonly DeleteAuthorHandler _handler;
    private readonly Mock<IAuthorRepository> _repositoryMock;

    public DeleteAuthorCommandTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingDeleteAuthor_ThenShouldCallDeleteAndSaveEntitiesAsync()
    {
        // Arrange
        var author = _faker.Generate()[0];
        var command = new DeleteAuthorCommand(author.Id);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(author.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _repositoryMock.Setup(r => r.Delete(author));

        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(r => r.Delete(author), Times.Once);
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenInvalidCommand_WhenHandlingDeleteAuthor_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var command = new DeleteAuthorCommand(Guid.NewGuid());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author)null!);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Author>()), Times.Never);
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
