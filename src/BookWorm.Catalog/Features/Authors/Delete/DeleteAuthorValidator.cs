using BookWorm.Catalog.Domain;
using BookWorm.Catalog.Domain.BookAggregate.Specifications;
using BookWorm.Core.SharedKernel;
using FluentValidation;

namespace BookWorm.Catalog.Features.Authors.Delete;

public sealed class DeleteAuthorValidator : AbstractValidator<DeleteAuthorCommand>
{
    public DeleteAuthorValidator(AuthorValidator authorValidator)
    {
        RuleFor(x => x.Id)
            .SetValidator(authorValidator);
    }
}

public sealed class AuthorValidator : AbstractValidator<Guid>
{
    private readonly IReadRepository<Author> _readRepository;

    public AuthorValidator(IReadRepository<Author> readRepository)
    {
        _readRepository = readRepository;

        RuleFor(x => x)
            .MustAsync(IsNotAuthorBook)
            .WithMessage("Author has books and cannot be deleted");
    }

    private async Task<bool> IsNotAuthorBook(Guid authorId, CancellationToken cancellationToken)
    {
        var author = await _readRepository.FirstOrDefaultAsync(new BookAuthorFilterSpec(authorId), cancellationToken);

        return author is null;
    }
}
