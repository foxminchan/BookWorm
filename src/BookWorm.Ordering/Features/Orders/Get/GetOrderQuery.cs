using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Core.SharedKernel;
using BookWorm.Ordering.Domain.OrderAggregate;
using BookWorm.Ordering.Domain.OrderAggregate.Specifications;
using BookWorm.Ordering.Grpc;
using BookWorm.Shared.Identity;

namespace BookWorm.Ordering.Features.Orders.Get;

public sealed record GetOrderQuery(Guid OrderId) : ICommand<Result<OrderDetailDto>>;

public sealed class GetOrderHandler(
    IReadRepository<Order> repository,
    IIdentityService identityService,
    BookService bookService) : ICommandHandler<GetOrderQuery, Result<OrderDetailDto>>
{
    public async Task<Result<OrderDetailDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var customerId = identityService.GetUserIdentity();

        Guard.Against.NullOrEmpty(customerId);

        OrderFilterSpec spec;
        if (identityService.IsAdminRole())
        {
            spec = new(request.OrderId);
        }
        else
        {
            spec = new(Guid.Parse(customerId), request.OrderId);
        }

        var order = await repository.FirstOrDefaultAsync(spec, cancellationToken);

        Guard.Against.NotFound(customerId, order);

        List<OrderItemDto> orderItems = [];
        foreach (var item in order.OrderItems)
        {
            var book = await bookService.GetBook(item.BookId);
            orderItems.Add(new(book.Id, book.Name, item.Quantity, item.Price));
        }

        OrderDetailDto orderDetail = new(order.Id, order.Note, order.Status, order.TotalPrice, orderItems);

        return orderDetail;
    }
}
