using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Features.Categories.Update;
using BookWorm.Chassis.Exceptions;
using BookWorm.Chassis.Repository;
using MediatR;

namespace BookWorm.Catalog.UnitTests.Features.Categories.Update;

public sealed class UpdateCategoryCommandTests
{
    private readonly Category _category;
    private readonly Guid _categoryId;
    private readonly UpdateCategoryHandler _handler;
    private readonly Mock<ICategoryRepository> _repositoryMock;

    public UpdateCategoryCommandTests()
    {
        _categoryId = Guid.CreateVersion7();
        _category = new("Old Name");

        _repositoryMock = new();

        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenExistingCategory_WhenHandlingUpdateCommand_ThenShouldCallSaveEntitiesAsync()
    {
        // Arrange
        var command = new UpdateCategoryCommand(_categoryId, "New Name");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_category);

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(unitOfWorkMock.Object);
        unitOfWorkMock
            .Setup(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _category.Name.ShouldBe("New Name");
        unitOfWorkMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenNonExistingCategory_WhenHandlingUpdateCommand_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UpdateCategoryCommand(_categoryId, "New Name");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(_categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Category with id {_categoryId} not found.");
        _repositoryMock.Verify(x => x.UnitOfWork, Times.Never);
    }
}
