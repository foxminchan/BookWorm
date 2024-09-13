using BookWorm.Basket.Features;
using BookWorm.Basket.Features.Get;
using Google.Protobuf.WellKnownTypes;
using GrpcBasketBase = BookWorm.Basket.Grpc.Basket.BasketBase;

namespace BookWorm.Basket.Grpc;

public sealed class BasketService(ISender sender, IBookService bookService, ILogger<BasketService> logger)
    : GrpcBasketBase
{
    [AllowAnonymous]
    public override async Task<BasketResponse> GetBasket(Empty request, ServerCallContext context)
    {
        var userId = context.GetUserIdentity();

        if (string.IsNullOrEmpty(userId))
        {
            return new();
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("[{Service}] - Begin grpc call {Method} with {BasketId}",
                nameof(BasketService), nameof(GetBasket), userId);
        }

        var basket = await sender.Send(new GetBasketQuery());

        return basket.Value is not null ? await MapToBasketResponse(basket.Value) : new();
    }

    private async Task<BasketResponse> MapToBasketResponse(BasketDto basket)
    {
        var response = new BasketResponse { BasketId = basket.Id.ToString(), TotalPrice = 0.0 };

        foreach (var item in basket.Items)
        {
            var book = await bookService.GetBookAsync(item.BookId);

            response.Books.Add(new Book
            {
                Id = book.Id.ToString(),
                Name = book.Name,
                Price = (double)book.Price,
                PriceSale = (double)book.PriceSale,
                Quantity = item.Quantity
            });
        }

        response.TotalPrice =
            response.Books.Sum(x => x.PriceSale > 0 ? x.PriceSale * x.Quantity : x.Price * x.Quantity);

        return response;
    }
}
