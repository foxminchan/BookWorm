namespace BookWorm.Basket.Features.Create;

internal sealed class CreateBasketValidator : AbstractValidator<CreateBasketCommand>
{
    public CreateBasketValidator(BookValidator bookValidator)
    {
        RuleFor(x => x.BookId).NotEmpty().SetValidator(bookValidator);

        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

internal sealed class BookValidator : AbstractValidator<Guid>
{
    private readonly IBookService _bookService;

    public BookValidator(IBookService bookService)
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
        return await _bookService.IsBookExistsAsync(bookId, cancellationToken);
    }

    private async Task<bool> InStock(Guid bookId, CancellationToken cancellationToken)
    {
        var bookStatus = await _bookService.GetBookStatusAsync(bookId, cancellationToken);
        return bookStatus.Status == nameof(InStock);
    }
}
