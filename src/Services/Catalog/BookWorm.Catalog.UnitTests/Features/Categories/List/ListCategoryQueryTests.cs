using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Features.Categories;
using BookWorm.Catalog.Features.Categories.List;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.Query;

namespace BookWorm.Catalog.UnitTests.Features.Categories.List;

public sealed class ListCategoryQueryTests
{
    private readonly CategoryFaker _faker;
    private readonly ListCategoriesHandler _handler;
    private readonly Mock<ICategoryRepository> _repositoryMock;

    public ListCategoryQueryTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public void GivenListCategoriesQuery_WhenCreating_ThenShouldBeOfCorrectType()
    {
        // Act
        var query = new ListCategoriesQuery();

        // Assert
        query.ShouldNotBeNull();
        query.ShouldBeOfType<ListCategoriesQuery>();
        query.ShouldBeAssignableTo<IQuery<IReadOnlyList<CategoryDto>>>();
    }

    [Test]
    public async Task GivenValidQuery_WhenHandlingListCategories_ThenShouldReturnCategoryDtos()
    {
        // Arrange
        var categories = _faker.Generate();
        _repositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var query = new ListCategoriesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(categories.Length);
    }

    [Test]
    public async Task GivenEmptyCategories_WhenHandlingListCategories_ThenShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new ListCategoriesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingListCategories_ThenShouldThrowException()
    {
        // Arrange
        _repositoryMock
            .Setup(repo => repo.ListAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Database error"));

        var query = new ListCategoriesQuery();

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
            await _handler.Handle(query, CancellationToken.None)
        );
    }

    [Test]
    public void GivenTwoListCategoriesQueryInstances_WhenCompared_ThenShouldBeEqual()
    {
        // Arrange
        var query1 = new ListCategoriesQuery();
        var query2 = new ListCategoriesQuery();

        // Assert
        query1.ShouldBe(query2);
        query1.GetHashCode().ShouldBe(query2.GetHashCode());
        query1.ToString().ShouldBe(query2.ToString());
    }
}
