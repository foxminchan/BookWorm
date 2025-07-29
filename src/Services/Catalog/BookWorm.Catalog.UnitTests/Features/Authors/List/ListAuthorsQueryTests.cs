using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Features.Authors;
using BookWorm.Catalog.Features.Authors.List;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.CQRS.Query;

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
    public void GivenListAuthorsQuery_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Act
        var query = new ListAuthorsQuery();

        // Assert
        query.ShouldNotBeNull();
        query.ShouldBeOfType<ListAuthorsQuery>();
        query.ShouldBeAssignableTo<IQuery<IReadOnlyList<AuthorDto>>>();
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
    public async Task GivenEmptyAuthors_WhenHandlingListAuthorsQuery_ThenShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new ListAuthorsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
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

    [Test]
    public void GivenTwoListAuthorsQueries_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var query1 = new ListAuthorsQuery();
        var query2 = new ListAuthorsQuery();

        // Act & Assert
        query1.ShouldBe(query2);
        query1.Equals(query2).ShouldBeTrue();
        (query1 == query2).ShouldBeTrue();
        (query1 != query2).ShouldBeFalse();
    }

    [Test]
    public void GivenTwoListAuthorsQueries_WhenGettingHashCode_ThenShouldReturnSameHashCode()
    {
        // Arrange
        var query1 = new ListAuthorsQuery();
        var query2 = new ListAuthorsQuery();

        // Act
        var hashCode1 = query1.GetHashCode();
        var hashCode2 = query2.GetHashCode();

        // Assert
        hashCode1.ShouldBe(hashCode2);
    }

    [Test]
    public void GivenListAuthorsQuery_WhenCallingToString_ThenShouldReturnStringRepresentation()
    {
        // Arrange
        var query = new ListAuthorsQuery();

        // Act
        var result = query.ToString();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldContain(nameof(ListAuthorsQuery));
    }

    [Test]
    public void GivenListAuthorsQuery_WhenUsingWithExpression_ThenShouldCreateIdenticalCopy()
    {
        // Arrange
        var original = new ListAuthorsQuery();

        // Act
        var copy = original with
        { };

        // Assert
        copy.ShouldBe(original);
        copy.ShouldNotBeSameAs(original);
    }
}
