using System.Diagnostics.CodeAnalysis;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace BookWorm.Basket.Grpc;

public sealed class BasketService(IRedisService redisService, BookService bookService) : Basket.BasketBase
{
    [AllowAnonymous]
    public override async Task<BasketResponse> GetBasket(BasketRequest request, ServerCallContext context)
    {
        var basket = await redisService.HashGetAsync<Domain.Basket?>(nameof(Basket), request.BasketId);

        if (basket is null)
        {
            ThrowNotFound();
        }

        return await MapToBasketResponse(basket);
    }

    [DoesNotReturn]
    private static void ThrowNotFound()
    {
        throw new RpcException(new(StatusCode.NotFound, "Basket not found"));
    }

    private async Task<BasketResponse> MapToBasketResponse(Domain.Basket basket)
    {
        var response = new BasketResponse { BasketId = basket.AccountId.ToString(), TotalPrice = 0.0 };

        foreach (var item in basket.BasketItems)
        {
            var book = await bookService.GetBook(item.Id);

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
