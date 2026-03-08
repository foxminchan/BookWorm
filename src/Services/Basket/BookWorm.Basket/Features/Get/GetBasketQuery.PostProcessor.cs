using Mediator;

namespace BookWorm.Basket.Features.Get;

internal sealed class GetBasketPostProcessor(IBookService bookService)
    : MessagePostProcessor<GetBasketQuery, CustomerBasketDto>
{
    protected override async ValueTask Handle(
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

        for (var i = 0; i < response.Items.Count; i++)
        {
            var item = response.Items[i];

            if (!bookLookup.TryGetValue(item.Id!, out var bookResponse))
            {
                continue;
            }

            response.Items[i] = item with
            {
                Name = bookResponse.Name,
                PriceSale = bookResponse.PriceSale,
                Price = (decimal)bookResponse.Price!,
            };
        }
    }
}
