using Microsoft.AspNetCore.Mvc;

namespace BookWorm.Ordering.Infrastructure.Services;

public sealed class BasketMetadata(
    [FromServices] IBasketService basketService,
    [FromServices] IBookService bookService
)
{
    public IBasketService BasketService { get; } = basketService;
    public IBookService BookService { get; } = bookService;
}
