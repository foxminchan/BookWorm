using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Features.Publishers;
using BookWorm.Catalog.Features.Publishers.List;
using BookWorm.Catalog.UnitTests.Fakers;
using Mediator;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.List;

public sealed class ListPublishersQueryTests
{
    private readonly PublisherFaker _faker;
    private readonly ListPublishersHandler _handler;
    private readonly Mock<IPublisherRepository> _repositoryMock;

    public ListPublishersQueryTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public void GivenListPublishersQuery_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Act
        var query = new ListPublishersQuery();

        // Assert
        query.ShouldNotBeNull();
        query.ShouldBeOfType<ListPublishersQuery>();
        query.ShouldBeAssignableTo<IQuery<IReadOnlyList<PublisherDto>>>();
    }

    [Test]
    public async Task GivenValidQuery_WhenHandlingListPublishersQuery_ThenShouldReturnPublisherDtos()
    {
        // Arrange
        var publishers = _faker.Generate();
        _repositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(publishers);

        var query = new ListPublishersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(publishers.Length);
        _repositoryMock.Verify(repo => repo.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenEmptyPublishers_WhenHandlingListPublishersQuery_ThenShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new ListPublishersQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
        _repositoryMock.Verify(repo => repo.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingListPublishersQuery_ThenShouldThrowException()
    {
        // Arrange
        _repositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Repository error"));

        var query = new ListPublishersQuery();

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
        {
            await _handler.Handle(query, CancellationToken.None);
        });

        _repositoryMock.Verify(repo => repo.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void GivenTwoListPublishersQueries_WhenComparing_ThenShouldBeEqual()
    {
        // Arrange
        var query1 = new ListPublishersQuery();
        var query2 = new ListPublishersQuery();

        // Act & Assert
        query1.ShouldBe(query2);
        query1.Equals(query2).ShouldBeTrue();
        (query1 == query2).ShouldBeTrue();
        (query1 != query2).ShouldBeFalse();
    }

    [Test]
    public void GivenTwoListPublishersQueries_WhenGettingHashCode_ThenShouldReturnSameHashCode()
    {
        // Arrange
        var query1 = new ListPublishersQuery();
        var query2 = new ListPublishersQuery();

        // Act
        var hashCode1 = query1.GetHashCode();
        var hashCode2 = query2.GetHashCode();

        // Assert
        hashCode1.ShouldBe(hashCode2);
    }

    [Test]
    public void GivenListPublishersQuery_WhenCallingToString_ThenShouldReturnStringRepresentation()
    {
        // Arrange
        var query = new ListPublishersQuery();

        // Act
        var result = query.ToString();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.ShouldContain(nameof(ListPublishersQuery));
    }

    [Test]
    public void GivenListPublishersQuery_WhenUsingWithExpression_ThenShouldCreateIdenticalCopy()
    {
        // Arrange
        var original = new ListPublishersQuery();

        // Act
        var copy = original with
        { };

        // Assert
        copy.ShouldBe(original);
        copy.ShouldNotBeSameAs(original);
    }
}
