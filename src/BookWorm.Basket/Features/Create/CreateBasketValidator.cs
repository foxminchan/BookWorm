namespace BookWorm.Basket.Features.Create;

public sealed class CreateBasketValidator : AbstractValidator<CreateBasketCommand>
{
    public CreateBasketValidator(BookValidator bookValidator)
    {
        RuleFor(x => x.BookId)
            .NotEmpty()
            .SetValidator(bookValidator);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}

public sealed class BookValidator : AbstractValidator<Guid>
{
    private readonly BookService _bookService;

    public BookValidator(BookService bookService)
    {
        _bookService = bookService;

        RuleFor(x => x)
            .MustAsync(Exist)
            .WithMessage("Book does not exist")
            .MustAsync(InStock)
            .WithMessage("Book is not in stock");
    }

    private async Task<bool> Exist(Guid bookId, CancellationToken cancellationToken)
    {
        return await _bookService.IsBookExists(bookId);
    }

    private async Task<bool> InStock(Guid bookId, CancellationToken cancellationToken)
    {
        var bookStatus = await _bookService.GetBookStatus(bookId);
        return bookStatus.Status == nameof(InStock);
    }
}
