using BookWorm.Chassis.Security.Extensions;
using BookWorm.Chassis.Utilities.Guards;
using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Extensions;
using Mediator;

namespace BookWorm.Ordering.Features.Orders.Get;

public sealed record GetOrderQuery(
    [property: Description("Only 'ADMIN' role can retrieve other users' data")] Guid Id
) : IQuery<OrderDetailDto>;

internal sealed class GetOrderHandler(
    IOrderRepository repository,
    ClaimsPrincipal claimsPrincipal,
    IBookService bookService
) : IQueryHandler<GetOrderQuery, OrderDetailDto>
{
    public async ValueTask<OrderDetailDto> Handle(
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

        var dto = order.ToOrderDetailDto();

        if (dto.Items.Count > 0)
        {
            var bookIds = dto.Items.Select(item => item.Id.ToString()).Distinct();
            var bookResponses = await bookService.GetBooksByIdsAsync(bookIds, cancellationToken);

            if (bookResponses?.Books.Count is not null and not 0)
            {
                var bookLookup = bookResponses.Books.ToDictionary(b => b.Id);

                dto = dto with
                {
                    Items =
                    [
                        .. dto.Items.Select(item =>
                            bookLookup.TryGetValue(item.Id.ToString(), out var book)
                                ? item with
                                {
                                    Name = book.Name,
                                }
                                : item
                        ),
                    ],
                };
            }
        }

        return dto;
    }
}
