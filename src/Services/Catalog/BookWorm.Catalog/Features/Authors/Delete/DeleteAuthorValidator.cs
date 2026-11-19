using BookWorm.Catalog.Domain.AggregatesModel.BookAggregate.Specifications;

namespace BookWorm.Catalog.Features.Authors.Delete;

internal sealed class DeleteAuthorValidator : AbstractValidator<DeleteAuthorCommand>
{
    public DeleteAuthorValidator(AuthorValidator authorValidator)
    {
        RuleFor(x => x.Id).SetValidator(authorValidator);
    }
}

internal sealed class AuthorValidator : AbstractValidator<AuthorId>
{
    public AuthorValidator(IAuthorRepository repository)
    {
        RuleFor(x => x)
            .MustAsync(
                async (authorId, cancellationToken) =>
                    await repository.FirstOrDefaultAsync(
                        new BookAuthorFilterSpec((Guid)authorId),
                        cancellationToken
                    )
                        is null
            )
            .WithMessage("Author has books and cannot be deleted");
    }
}
