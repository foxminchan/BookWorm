using MediatR.Pipeline;

namespace BookWorm.Basket.Features.Get;

public sealed record GetBasketQuery : IQuery<CustomerBasketDto>;

public sealed class GetBasketHandler(IBasketRepository repository, ClaimsPrincipal claimsPrincipal)
    : IQueryHandler<GetBasketQuery, CustomerBasketDto>
{
    public async Task<CustomerBasketDto> Handle(
        GetBasketQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var basket = await repository.GetBasketAsync(userId);

        if (basket is null)
        {
            throw new NotFoundException($"Basket with id {userId} not found.");
        }

        return basket.ToCustomerBasketDto();
    }
}

public sealed class PostGetBasketHandler(IBookService bookService)
    : IRequestPostProcessor<GetBasketQuery, CustomerBasketDto>
{
    public async Task Process(
        GetBasketQuery request,
        CustomerBasketDto response,
        CancellationToken cancellationToken
    )
    {
        var books = response.Items;

        var bookTasks = books
            .Select(book => bookService.GetBookByIdAsync(book.Id!, cancellationToken))
            .ToList();
        var bookResponses = await Task.WhenAll(bookTasks);

        List<BasketItemDto> updatedItems = [];
        for (var i = 0; i < books.Count; i++)
        {
            var bookResponse =
                bookResponses[i]
                ?? throw new NotFoundException($"Book with id {books[i].Id} not found.");

            var updatedItem = books[i] with
            {
                Name = bookResponse.Name,
                Price = (decimal)bookResponse.Price,
                PriceSale = bookResponse.PriceSale is null ? null : (decimal)bookResponse.PriceSale,
            };

            updatedItems.Add(updatedItem);
        }

        _ = response with { Items = updatedItems };
    }
}
