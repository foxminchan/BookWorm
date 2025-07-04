using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using BookWorm.Catalog.Features.Publishers.Delete;
using BookWorm.Catalog.UnitTests.Fakers;

namespace BookWorm.Catalog.UnitTests.Features.Publishers.Delete;

public sealed class DeletePublisherValidatorTests
{
    private readonly Mock<IBookRepository> _mockRepository = new();
    private readonly DeletePublisherValidator _validator;
    private readonly PublisherValidator _publisherValidator;

    public DeletePublisherValidatorTests()
    {
        _publisherValidator = new(_mockRepository.Object);
        _validator = new(_publisherValidator);
    }

    [Test]
    public async Task GivenValidCommandWithNoBooks_WhenValidating_ThenShouldBeValid()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DeletePublisherCommand(publisherId);

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task GivenValidCommandWithExistingBooks_WhenValidating_ThenShouldBeInvalid()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DeletePublisherCommand(publisherId);
        var bookFaker = new BookFaker();
        List<Book> existingBooks = [bookFaker.Generate(1)[0]];

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooks);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldHaveSingleItem();
        result.Errors[0].ErrorMessage.ShouldBe("Publisher has books and cannot be deleted");
        result.Errors[0].PropertyName.ShouldBe("Id");
    }

    [Test]
    public async Task GivenEmptyGuid_WhenValidating_ThenShouldCallRepositoryWithEmptyGuid()
    {
        // Arrange
        var command = new DeletePublisherCommand(Guid.Empty);

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _validator.ValidateAsync(command);

        // Assert
        _mockRepository.Verify(
            x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenRepositoryThrowsException_WhenValidating_ThenShouldPropagateException()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DeletePublisherCommand(publisherId);
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _validator.ValidateAsync(command)
        );

        exception.Message.ShouldBe("Database error");
    }

    [Test]
    public async Task GivenCancellationToken_WhenValidating_ThenShouldPassTokenToRepository()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var command = new DeletePublisherCommand(publisherId);
        var cancellationToken = new CancellationToken(true);

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), cancellationToken))
            .ReturnsAsync([]);

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _validator.ValidateAsync(command, cancellationToken)
        );
    }

    // Tests for PublisherValidator directly
    [Test]
    public async Task GivenPublisherIdWithNoBooks_WhenValidatingDirectly_ThenShouldBeValid()
    {
        // Arrange
        var publisherId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _publisherValidator.ValidateAsync(publisherId);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task GivenPublisherIdWithExistingBooks_WhenValidatingDirectly_ThenShouldBeInvalid()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        var bookFaker = new BookFaker();
        List<Book> existingBooks = bookFaker.Generate(2);

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBooks);

        // Act
        var result = await _publisherValidator.ValidateAsync(publisherId);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldHaveSingleItem();
        result.Errors[0].ErrorMessage.ShouldBe("Publisher has books and cannot be deleted");
    }

    [Test]
    public async Task GivenEmptyGuidDirect_WhenValidating_ThenShouldCallRepositoryWithCorrectSpec()
    {
        // Arrange
        var publisherId = Guid.Empty;

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _publisherValidator.ValidateAsync(publisherId);

        // Assert
        _mockRepository.Verify(
            x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task GivenRepositoryReturnsNull_WhenValidatingDirectly_ThenShouldHandleGracefully()
    {
        // Arrange
        var publisherId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<Book>)null!);

        // Act & Assert
        await Should.ThrowAsync<NullReferenceException>(async () =>
            await _publisherValidator.ValidateAsync(publisherId)
        );
    }

    [Test]
    public async Task GivenMultipleValidationCalls_WhenValidatingDirectly_ThenShouldCallRepositoryForEach()
    {
        // Arrange
        var publisherId1 = Guid.NewGuid();
        var publisherId2 = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _publisherValidator.ValidateAsync(publisherId1);
        await _publisherValidator.ValidateAsync(publisherId2);

        // Assert
        _mockRepository.Verify(
            x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [Test]
    public async Task GivenRepositoryTimeout_WhenValidatingDirectly_ThenShouldRespectCancellation()
    {
        // Arrange
        var publisherId = Guid.NewGuid();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        _mockRepository
            .Setup(x => x.ListAsync(It.IsAny<BookFilterSpec>(), It.IsAny<CancellationToken>()))
            .Returns(
                async (BookFilterSpec _, CancellationToken ct) =>
                {
                    await Task.Delay(1000, ct);
                    return [];
                }
            );

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _publisherValidator.ValidateAsync(publisherId, cts.Token)
        );
    }
}
