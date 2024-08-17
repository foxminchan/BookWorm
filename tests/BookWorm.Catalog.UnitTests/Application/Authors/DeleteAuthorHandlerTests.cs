using Ardalis.GuardClauses;
using BookWorm.Catalog.Domain;
using BookWorm.Catalog.Features.Authors.Delete;
using BookWorm.Catalog.UnitTests.Builder;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.UnitTests.Application.Authors;

public sealed class DeleteAuthorHandlerTests
{
    private readonly Mock<IRepository<Author>> _repositoryMock;
    private readonly DeleteAuthorHandler _handler;

    public DeleteAuthorHandlerTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task GivenValidAuthorId_ShouldReturnSuccess_WhenAuthorIsDeleted()
    {
        // Arrange
        var author = AuthorBuilder.WithDefaultValues()[0];
        var authorId = author.Id;
        var command = new DeleteAuthorCommand(authorId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _repositoryMock
            .Setup(r => r.DeleteAsync(author, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.GetByIdAsync(authorId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(author, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GivenInvalidAuthorId_ShouldThrowNotFoundException_WhenAuthorIsNotFound()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new DeleteAuthorCommand(authorId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.GetByIdAsync(authorId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
