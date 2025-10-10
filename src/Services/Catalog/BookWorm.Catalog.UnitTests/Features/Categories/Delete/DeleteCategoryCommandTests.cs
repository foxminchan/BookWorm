using BookWorm.Catalog.Domain.AggregatesModel.CategoryAggregate;
using BookWorm.Catalog.Features.Categories.Delete;
using BookWorm.Catalog.UnitTests.Fakers;
using BookWorm.Chassis.Exceptions;
using BookWorm.Chassis.Repository;
using Mediator;

namespace BookWorm.Catalog.UnitTests.Features.Categories.Delete;

public sealed class DeleteCategoryCommandTests
{
    private readonly CategoryFaker _categoryFaker;
    private readonly DeleteCategoryHandler _handler;
    private readonly Mock<ICategoryRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public DeleteCategoryCommandTests()
    {
        _categoryFaker = new();
        _repositoryMock = new();
        _unitOfWorkMock = new();
        _repositoryMock.Setup(x => x.UnitOfWork).Returns(_unitOfWorkMock.Object);
        _handler = new(_repositoryMock.Object);
    }

    [Test]
    public async Task GivenValidCategoryId_WhenHandlingDeleteCategory_ThenShouldCallDeleteAndSaveEntitiesAsync()
    {
        // Arrange
        var category = _categoryFaker.Generate(1)[0];
        var command = new DeleteCategoryCommand(category.Id);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _unitOfWorkMock
            .Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(
            r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.Delete(category), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenNonExistingCategoryId_WhenHandlingDeleteCategory_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var command = new DeleteCategoryCommand(categoryId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Category with id {categoryId} not found.");
        _repositoryMock.Verify(
            r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(
            u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenEmptyGuidCategoryId_WhenHandlingDeleteCategory_ThenShouldThrowNotFoundException()
    {
        // Arrange
        var categoryId = Guid.Empty;
        var command = new DeleteCategoryCommand(categoryId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category)null!);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await act.ShouldThrowAsync<NotFoundException>();
        exception.Message.ShouldBe($"Category with id {categoryId} not found.");
        _repositoryMock.Verify(
            r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(
            u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenHandlingDeleteCategory_ThenShouldPropagateException()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var command = new DeleteCategoryCommand(categoryId);
        var expectedException = new InvalidOperationException("Database connection failed");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _handler.Handle(command, CancellationToken.None)
        );

        exception.Message.ShouldBe("Database connection failed");
        _repositoryMock.Verify(
            r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(
            u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenSaveEntitiesAsyncThrowsException_WhenHandlingDeleteCategory_ThenShouldPropagateException()
    {
        // Arrange
        var category = _categoryFaker.Generate(1)[0];
        var command = new DeleteCategoryCommand(category.Id);
        var expectedException = new InvalidOperationException("Database save failed");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _unitOfWorkMock
            .Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _handler.Handle(command, CancellationToken.None)
        );

        exception.Message.ShouldBe("Database save failed");
        _repositoryMock.Verify(
            r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.Delete(category), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GivenCancellationToken_WhenHandlingDeleteCategory_ThenShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var category = _categoryFaker.Generate(1)[0];
        var command = new DeleteCategoryCommand(category.Id);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _repositoryMock
            .Setup(r => r.GetByIdAsync(category.Id, cancellationToken))
            .ReturnsAsync(category);
        _unitOfWorkMock.Setup(u => u.SaveEntitiesAsync(cancellationToken)).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(r => r.GetByIdAsync(category.Id, cancellationToken), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveEntitiesAsync(cancellationToken), Times.Once);
    }

    [Test]
    public async Task GivenAlreadyCancelledToken_WhenHandlingDeleteCategory_ThenShouldThrowOperationCanceledException()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var command = new DeleteCategoryCommand(categoryId);
        var cancelledToken = new CancellationToken(true);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _handler.Handle(command, cancelledToken)
        );

        _repositoryMock.Verify(
            r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Never);
        _unitOfWorkMock.Verify(
            u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task GivenValidCategoryWithSpecificProperties_WhenHandlingDeleteCategory_ThenShouldDeleteExactCategory()
    {
        // Arrange
        const string expectedCategoryName = "Technology Books";
        var category = new Category(expectedCategoryName);
        var command = new DeleteCategoryCommand(category.Id);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _unitOfWorkMock
            .Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(Unit.Value);
        _repositoryMock.Verify(
            r =>
                r.Delete(
                    It.Is<Category>(c => c.Id == category.Id && c.Name == expectedCategoryName)
                ),
            Times.Once
        );
    }

    [Test]
    public async Task GivenMultipleConcurrentRequests_WhenHandlingDeleteCategory_ThenShouldHandleEachRequestIndependently()
    {
        // Arrange
        var categories = _categoryFaker.Generate(3);
        var commands = categories.Select(c => new DeleteCategoryCommand(c.Id)).ToArray();

        foreach (var category in categories)
        {
            _repositoryMock
                .Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
        }

        _unitOfWorkMock
            .Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var tasks = commands.Select(command =>
            _handler.Handle(command, CancellationToken.None).AsTask()
        );
        var results = await Task.WhenAll(tasks);

        // Assert
        foreach (var result in results)
        {
            result.ShouldBe(Unit.Value);
        }

        foreach (var category in categories)
        {
            _repositoryMock.Verify(
                r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()),
                Times.Once
            );
            _repositoryMock.Verify(r => r.Delete(category), Times.Once);
        }

        _unitOfWorkMock.Verify(
            u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(3)
        );
    }
}
