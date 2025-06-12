using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Infrastructure.Helpers;
using MediatR.Pipeline;

namespace BookWorm.Ordering.Features.Orders.Get;

public sealed record GetOrderQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] Guid Id
) : IQuery<OrderDetailDto>;

public sealed class GetOrderHandler(IOrderRepository repository, ClaimsPrincipal claimsPrincipal)
    : IQueryHandler<GetOrderQuery, OrderDetailDto>
{
    public async Task<OrderDetailDto> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken
    )
    {
        Order? order;
        if (claimsPrincipal.GetRoles().Contains(Authorization.Roles.Admin))
        {
            order = await repository.GetByIdAsync(request.Id, cancellationToken);
        }
        else
        {
            var buyerId = claimsPrincipal.GetClaimValue(ClaimTypes.NameIdentifier).ToBuyerId();

            order = await repository.FirstOrDefaultAsync(
                new OrderFilterSpec(request.Id, buyerId),
                cancellationToken
            );
        }

        Guard.Against.NotFound(order, request.Id);

        return order.ToOrderDetailDto();
    }
}

public sealed class PostGetOrderHandler(IBookService bookService)
    : IRequestPostProcessor<GetOrderQuery, OrderDetailDto>
{
    public async Task Process(
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

        var updatedItems = response
            .Items.Select(item =>
                bookLookup.TryGetValue(item.Id.ToString(), out var bookResponse)
                    ? item with
                    {
                        Name = bookResponse.Name,
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
