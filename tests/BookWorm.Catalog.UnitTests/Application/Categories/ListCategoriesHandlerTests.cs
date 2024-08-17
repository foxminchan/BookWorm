using BookWorm.Catalog.Domain;
using BookWorm.Catalog.Features.Categories.List;
using BookWorm.Catalog.UnitTests.Builder;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.UnitTests.Application.Categories;

public sealed class ListCategoriesHandlerTests
{
    private readonly Mock<IReadRepository<Category>> _categoryRepositoryMock;
    private readonly ListCategoriesHandler _handler;

    public ListCategoriesHandlerTests()
    {
        _categoryRepositoryMock = new();
        _handler = new(_categoryRepositoryMock.Object);
    }

    [Fact]
    public async Task GivenRequest_ShouldReturnResult_WhenCategoriesIsNotEmpty()
    {
        // Arrange
        var categories = CategoryBuilder.WithDefaultValues();
        _categoryRepositoryMock.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _handler.Handle(new(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().HaveCount(categories.Count);
        _categoryRepositoryMock.Verify(x => x.ListAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
