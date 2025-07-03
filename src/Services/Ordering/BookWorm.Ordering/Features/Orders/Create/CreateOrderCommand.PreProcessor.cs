using MediatR.Pipeline;

namespace BookWorm.Ordering.Features.Orders.Create;

public sealed class CreateOrderPreProcessor([AsParameters] BasketMetadata basket)
    : IRequestPreProcessor<CreateOrderCommand>
{
    public async Task Process(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var basketItems = await basket.BasketService.GetBasket(cancellationToken);

        var response = await basket.BookService.GetBooksByIdsAsync(
            basketItems.Items.Select(item => item.Id),
            cancellationToken
        );

        request.Items =
        [
            .. basketItems.Items.Select(item =>
            {
                var book = response?.Books.FirstOrDefault(b => b.Id == item.Id);

                Guard.Against.NotFound(book, item.Id);

                var bookPrice = book.PriceSale ?? book.Price;

                return new OrderItem(Guid.Parse(book.Id), item.Quantity, (decimal)bookPrice!);
            }),
        ];
    }
}
