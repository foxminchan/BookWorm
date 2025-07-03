using MediatR.Pipeline;

namespace BookWorm.Basket.Features.Get;

public sealed class GetBasketPostProcessor(IBookService bookService)
    : IRequestPostProcessor<GetBasketQuery, CustomerBasketDto>
{
    public async Task Process(
        GetBasketQuery request,
        CustomerBasketDto response,
        CancellationToken cancellationToken
    )
    {
        if (response.Items.Count == 0)
        {
            return;
        }

        var bookIds = response.Items.Select(item => item.Id!).Distinct().ToList();

        var bookResponses = await bookService.GetBooksByIdsAsync(bookIds, cancellationToken);

        if (bookResponses?.Books.Count is null or 0)
        {
            return;
        }

        var bookLookup = bookResponses.Books.ToDictionary(b => b.Id);

        var updatedItems = response
            .Items.Select(item =>
                bookLookup.TryGetValue(item.Id!, out var bookResponse)
                    ? item with
                    {
                        Name = bookResponse.Name,
                        PriceSale = bookResponse.PriceSale,
                        Price = (decimal)bookResponse.Price!,
                    }
                    : item
            )
            .ToList();

        // Since we can't mutate the response directly, we need to use reflection
        // or consider returning a new response type
        var itemsProperty = response.GetType().GetProperty(nameof(response.Items));
        itemsProperty?.SetValue(response, updatedItems);
    }
}
