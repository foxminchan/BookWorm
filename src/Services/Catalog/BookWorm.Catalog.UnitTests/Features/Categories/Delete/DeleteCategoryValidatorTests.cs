using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using BookWorm.Catalog.Features.Categories.Delete;
using BookWorm.Catalog.UnitTests.Fakers;

namespace BookWorm.Catalog.UnitTests.Features.Categories.Delete;

public sealed class DeleteCategoryValidatorTests
{
    private readonly Guid _categoryId = Guid.CreateVersion7();
    private readonly DeleteCategoryValidator _deleteCategoryValidator;
    private readonly Mock<IBookRepository> _mockRepository;

    public DeleteCategoryValidatorTests()
    {
        _mockRepository = new();
        var categoryValidator = new CategoryValidator(_mockRepository.Object);
        _deleteCategoryValidator = new(categoryValidator);
    }

    [Test]
    public async Task GivenCategoryWithNoBooks_WhenValidating_ThenShouldReturnValid()
    {
        // Arrange
        var command = new DeleteCategoryCommand(_categoryId);

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        // Act
        var result = await _deleteCategoryValidator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCategoryWithBooks_WhenValidating_ThenShouldReturnInvalid()
    {
        // Arrange
        var command = new DeleteCategoryCommand(_categoryId);
        var bookFaker = new BookFaker();
        var booksWithCategory = bookFaker.Generate(2);

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(booksWithCategory);

        // Act
        var result = await _deleteCategoryValidator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].ErrorMessage.ShouldBe("Category has books and cannot be deleted");
        result.Errors[0].PropertyName.ShouldBe("Id");

        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenEmptyGuid_WhenValidating_ThenShouldCallRepository()
    {
        // Arrange
        var command = new DeleteCategoryCommand(Guid.Empty);

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        // Act
        var result = await _deleteCategoryValidator.ValidateAsync(command, CancellationToken.None);

        // Assert
        result.IsValid.ShouldBeTrue();

        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenValidating_ThenExceptionShouldPropagate()
    {
        // Arrange
        var command = new DeleteCategoryCommand(_categoryId);
        var expectedException = new InvalidOperationException("Database connection failed");

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _deleteCategoryValidator.ValidateAsync(command, CancellationToken.None)
        );

        exception.Message.ShouldBe("Database connection failed");

        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenCancellationToken_WhenValidating_ThenShouldPassCancellationTokenToRepository()
    {
        // Arrange
        var command = new DeleteCategoryCommand(_categoryId);
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        // Act
        var result = await _deleteCategoryValidator.ValidateAsync(command, cancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();

        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), cancellationToken),
            Times.Once
        );
    }

    [Test]
    public async Task GivenAlreadyCancelledToken_WhenValidating_ThenShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = new DeleteCategoryCommand(_categoryId);
        var cancelledToken = new CancellationToken(true);

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _deleteCategoryValidator.ValidateAsync(command, cancelledToken)
        );
    }

    [Test]
    public async Task GivenCategoryId_WhenValidating_ThenShouldPassCorrectParametersToBookFilterSpec()
    {
        // Arrange
        var command = new DeleteCategoryCommand(_categoryId);

        _mockRepository
            .Setup(r =>
                r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        // Act
        await _deleteCategoryValidator.ValidateAsync(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            r => r.ListAsync(It.Is<BookFilterSpec>(spec => true), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
