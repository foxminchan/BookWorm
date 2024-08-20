using BookWorm.Catalog.Domain;
using BookWorm.Catalog.Features.Categories.Create;

namespace BookWorm.Catalog.UnitTests.Application.Categories;

public sealed class CreateCategoryHandlerTests
{
    private readonly CreateCategoryHandler _handler;
    private readonly Mock<IRepository<Category>> _repositoryMock;

    public CreateCategoryHandlerTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task GivenCategoryName_ShouldReturnCategoryId_WhenCategoryIsCreated()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var command = new CreateCategoryCommand("Test Category");
        var category = new Category(command.Name) { Id = categoryId };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().Be(categoryId);
        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<Category>(c => c.Name == command.Name), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [CombinatorialData]
    public void GivenNullOrEmptyCategoryName_ShouldThrowException([CombinatorialValues("", null)] string? name)
    {
        // Arrange
        var command = new CreateCategoryCommand(name!);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentException>();
    }
}
