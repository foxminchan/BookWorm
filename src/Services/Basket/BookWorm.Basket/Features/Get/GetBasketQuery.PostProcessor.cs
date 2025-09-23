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

        foreach (var item in response.Items)
        {
            if (!bookLookup.TryGetValue(item.Id!, out var bookResponse))
            {
                continue;
            }

            item.Name = bookResponse.Name;
            item.PriceSale = bookResponse.PriceSale;
            item.Price = (decimal)bookResponse.Price!;
        }
    }
}
