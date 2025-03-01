using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

namespace BookWorm.Catalog.Features.Authors.Delete;

public sealed class DeleteAuthorValidator : AbstractValidator<DeleteAuthorCommand>
{
    public DeleteAuthorValidator(AuthorValidator authorValidator)
    {
        RuleFor(x => x.Id).SetValidator(authorValidator);
    }
}

public sealed class AuthorValidator : AbstractValidator<Guid>
{
    private readonly IAuthorRepository _repository;

    public AuthorValidator(IAuthorRepository repository)
    {
        _repository = repository;

        RuleFor(x => x)
            .MustAsync(IsNotAuthorBook)
            .WithMessage("Author has books and cannot be deleted");
    }

    private async Task<bool> IsNotAuthorBook(Guid authorId, CancellationToken cancellationToken)
    {
        var author = await _repository.FirstOrDefaultAsync(
            new BookAuthorFilterSpec(authorId),
            cancellationToken
        );

        return author is null;
    }
}
