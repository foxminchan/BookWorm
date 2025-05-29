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
        var sub = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject);

        var userId = Guard.Against.NotAuthenticated(sub);

        var basket = await repository.GetBasketAsync(userId);

        Guard.Against.NotFound(basket, $"Basket with id {userId} not found.");

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
                PriceSale = bookResponse.PriceSale,
                Price = (decimal)bookResponse.Price!,
            };

            updatedItems.Add(updatedItem);
        }

        _ = response with { Items = updatedItems };
    }
}
