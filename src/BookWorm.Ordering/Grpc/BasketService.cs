using GrpcBasketItem = BookWorm.Basket.Grpc.Book;
using GrpcBasket = BookWorm.Basket.Grpc.BasketResponse;
using GrpcBasketClient = BookWorm.Basket.Grpc.Basket.BasketClient;

namespace BookWorm.Ordering.Grpc;

public sealed class BasketService(GrpcBasketClient basketClient) : IBasketService
{
    public async Task<Basket> GetBasketAsync(Guid basketId, CancellationToken cancellationToken = default)
    {
        var response = await basketClient.GetBasketAsync(new() { BasketId = basketId.ToString() },
            cancellationToken: cancellationToken);

        return MapBasket(response);
    }

    private static Basket MapBasket(GrpcBasket basket)
    {
        return new(Guid.Parse(basket.BasketId), basket.Books.Select(MapBasketItem), (decimal)basket.TotalPrice);
    }

    private static BasketItem MapBasketItem(GrpcBasketItem item)
    {
        return new(Guid.Parse(item.Id), item.Name, item.Quantity, (decimal)item.Price, (decimal)item.PriceSale);
    }
}

public sealed record Basket(Guid Id, IEnumerable<BasketItem> Items, decimal Total);

public sealed record BasketItem(Guid BookId, string BookName, int Quantity, decimal Price, decimal PriceSale);
