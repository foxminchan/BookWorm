using BookWorm.Catalog.Domain.AggregatesModel.AuthorAggregate;
using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;
using BookWorm.Catalog.Features.Authors.Delete;
using BookWorm.Catalog.UnitTests.Fakers;

namespace BookWorm.Catalog.UnitTests.Features.Authors.Delete;

public sealed class DeleteAuthorValidatorTests
{
    private readonly Mock<IAuthorRepository> _repositoryMock;
    private readonly DeleteAuthorValidator _validator;

    public DeleteAuthorValidatorTests()
    {
        _repositoryMock = new();
        var authorValidator = new AuthorValidator(_repositoryMock.Object);
        _validator = new(authorValidator);
    }

    [Test]
    public async Task GivenValidAuthorId_WhenValidating_ThenShouldPass()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        _repositoryMock
            .Setup(repo =>
                repo.FirstOrDefaultAsync(
                    It.IsAny<BookAuthorFilterSpec>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync((Author?)null);

        var command = new DeleteAuthorCommand(authorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task GivenAuthorWithBooks_WhenValidating_ThenShouldFail()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var authorFaker = new AuthorFaker();
        var author = authorFaker.Generate()[0];

        _repositoryMock
            .Setup(repo =>
                repo.FirstOrDefaultAsync(
                    It.IsAny<BookAuthorFilterSpec>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(author);

        var command = new DeleteAuthorCommand(authorId);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x =>
            x.ErrorMessage == "Author has books and cannot be deleted"
        );
    }
}
