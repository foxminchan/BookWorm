using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Features.Categories.Create;

namespace BookWorm.Catalog.UnitTests.Features.Categories.Create;

public sealed class CreateCategoryCommandTests
{
    private readonly CreateCategoryCommandFaker _faker;
    private readonly CreateCategoryHandler _handler;
    private readonly Mock<ICategoryRepository> _repositoryMock;

    public CreateCategoryCommandTests()
    {
        _repositoryMock = new();
        _handler = new(_repositoryMock.Object);
        _faker = new();
    }

    [Test]
    public async Task GivenValidCommand_WhenHandlingCreateCategory_ThenShouldCallSaveEntitiesAsync()
    {
        // Arrange
        var command = _faker.Generate();
        var categoryId = Guid.CreateVersion7();
        var category = new Category(command.Name) { Id = categoryId };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _repositoryMock
            .Setup(r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(categoryId);
        _repositoryMock.Verify(
            r =>
                r.AddAsync(
                    It.Is<Category>(c => c.Name == command.Name),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _repositoryMock.Verify(
            r => r.UnitOfWork.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingCreateCategory_ThenShouldThrowException()
    {
        // Arrange
        var command = _faker.Generate();
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new("Database error"));

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
            await _handler.Handle(command, CancellationToken.None)
        );
    }
}
