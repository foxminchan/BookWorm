using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Features.Authors.List;
using BookWorm.Catalog.UnitTests.Fakers;

namespace BookWorm.Catalog.UnitTests.Features.Authors.List;

public sealed class ListAuthorsQueryTests
{
    private readonly AuthorFaker _faker;
    private readonly ListAuthorsHandler _handler;
    private readonly Mock<IAuthorRepository> _repositoryMock;

    public ListAuthorsQueryTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public async Task GivenValidQuery_WhenHandlingListAuthorsQuery_ThenShouldReturnAuthors()
    {
        // Arrange
        var authors = _faker.Generate();
        _repositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(authors);

        var query = new ListAuthorsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(authors.Length);
        _repositoryMock.Verify(repo => repo.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingListAuthorsQuery_ThenShouldThrowException()
    {
        // Arrange
        _repositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Database error"));

        var query = new ListAuthorsQuery();

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.ShouldThrowAsync<Exception>();
        _repositoryMock.Verify(repo => repo.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
