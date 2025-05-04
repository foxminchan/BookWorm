using BookWorm.Catalog.Domain.AggregatesModel.PublisherAggregate;
using BookWorm.Catalog.Features.Publishers;
using BookWorm.Catalog.Features.Publishers.List;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.Query;

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
}
