using Mediator;

namespace BookWorm.Ordering.Features.Orders.Get;

public sealed class GetOrderPostProcessor(IBookService bookService)
    : MessagePostProcessor<GetOrderQuery, OrderDetailDto>
{
    protected override async ValueTask Handle(
        GetOrderQuery request,
        OrderDetailDto response,
        CancellationToken cancellationToken
    )
    {
        if (response.Items.Count == 0)
        {
            return;
        }

        var bookIds = response.Items.Select(item => item.Id.ToString()).Distinct().ToList();

        var bookResponses = await bookService.GetBooksByIdsAsync(bookIds, cancellationToken);

        if (bookResponses?.Books.Count is null or 0)
        {
            return;
        }

        var bookLookup = bookResponses.Books.ToDictionary(b => b.Id);

        foreach (var item in response.Items)
        {
            if (bookLookup.TryGetValue(item.Id.ToString(), out var bookResponse))
            {
                item.Name = bookResponse.Name;
            }
        }
    }
}
