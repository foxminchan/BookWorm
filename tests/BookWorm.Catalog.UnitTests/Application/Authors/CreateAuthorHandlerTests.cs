using BookWorm.Catalog.Domain;
using BookWorm.Catalog.Features.Authors.Create;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.UnitTests.Application.Authors;

public sealed class CreateAuthorHandlerTests
{
    private readonly Mock<IRepository<Author>> _repositoryMock;
    private readonly CreateAuthorHandler _handler;

    public CreateAuthorHandlerTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task GivenAuthorName_ShouldReturnAuthorId_WhenAuthorIsCreated()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new CreateAuthorCommand("Test Author");
        var author = new Author(command.Name) { Id = authorId };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().Be(authorId);
        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<Author>(a => a.Name == command.Name), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory, CombinatorialData]
    public void GivenNullOrEmptyAuthorName_ShouldThrowException([CombinatorialValues("", null)] string? name)
    {
        // Arrange
        var command = new CreateAuthorCommand(name!);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }
}
