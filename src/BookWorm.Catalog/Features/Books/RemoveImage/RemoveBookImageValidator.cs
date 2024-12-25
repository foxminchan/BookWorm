﻿using Ardalis.GuardClauses;
using BookWorm.Catalog.Domain.BookAggregate;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Catalog.Features.Books.RemoveImage;

public sealed class RemoveBookImageValidator : AbstractValidator<RemoveBookImageCommand>
{
    public RemoveBookImageValidator(BookValidator bookValidator)
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .SetValidator(bookValidator);
    }
}

public sealed class BookValidator : AbstractValidator<Guid>
{
    private readonly IReadRepository<Book> _readRepository;

    public BookValidator(IReadRepository<Book> readRepository)
    {
        _readRepository = readRepository;

        RuleFor(x => x)
            .MustAsync(IsHasBookImage)
            .WithMessage("Book does not have an image");
    }

    private async Task<bool> IsHasBookImage(Guid bookId, CancellationToken cancellationToken)
    {
        var book = await _readRepository.GetByIdAsync(bookId, cancellationToken);
        Guard.Against.NotFound(bookId, book);
        return book.ImageUrl is not null;
    }
}
