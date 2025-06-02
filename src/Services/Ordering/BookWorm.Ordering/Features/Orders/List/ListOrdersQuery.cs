using BookWorm.Ordering.Domain.AggregatesModel.OrderAggregate.Specifications;
using BookWorm.Ordering.Infrastructure.Helpers;
using BookWorm.SharedKernel.Results;

namespace BookWorm.Ordering.Features.Orders.List;

public sealed record ListOrdersQuery(
    [property: Description("Number of items to return in a single page of results")]
    [property: DefaultValue(Pagination.DefaultPageIndex)]
        int PageIndex = Pagination.DefaultPageIndex,
    [property: Description("Number of items to return in a single page of results")]
    [property: DefaultValue(Pagination.DefaultPageSize)]
        int PageSize = Pagination.DefaultPageSize,
    [property: Description("Status to filter results by")]
    [property: DefaultValue(null)]
        Status? Status = null,
    [property: Description("Buyer ID to filter results by")]
    [property: DefaultValue(null)]
        Guid? BuyerId = null
) : IQuery<PagedResult<OrderDto>>;

public sealed class ListOrdersHandler(IOrderRepository repository, ClaimsPrincipal claimsPrincipal)
    : IQueryHandler<ListOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(
        ListOrdersQuery request,
        CancellationToken cancellationToken
    )
    {
        if (!claimsPrincipal.GetRoles().Contains(Authorization.Roles.Admin))
        {
            request = request with
            {
                BuyerId = claimsPrincipal.GetClaimValue(KeycloakClaimTypes.Subject).ToBuyerId(),
            };
        }

        var filterSpec = new OrderFilterSpec(
            request.PageIndex,
            request.PageSize,
            request.Status,
            request.BuyerId
        );

        var orders = await repository.ListAsync(filterSpec, cancellationToken);

        var countSpec = new OrderFilterSpec(request.Status);

        var totalItems = await repository.CountAsync(countSpec, cancellationToken);

        return new(orders.ToOrderDtos(), request.PageIndex, request.PageSize, totalItems);
    }
}
