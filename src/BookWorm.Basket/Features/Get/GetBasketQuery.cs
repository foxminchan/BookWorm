using System.Security.Claims;
using Ardalis.GuardClauses;
using Ardalis.Result;
using BookWorm.Basket.Grpc;
using BookWorm.Basket.Infrastructure.Redis;
using BookWorm.Core.SharedKernel;

namespace BookWorm.Basket.Features.Get;

public sealed record GetBasketQuery : IQuery<Result<BasketDto>>;

public sealed class GetBasketHandler(
    IRedisService redisService,
    IHttpContextAccessor httpContext,
    BookService bookService) : IQueryHandler<GetBasketQuery, Result<BasketDto>>
{
    public async Task<Result<BasketDto>> Handle(GetBasketQuery query, CancellationToken cancellationToken)
    {
        var customerId = httpContext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        Guard.Against.NullOrEmpty(customerId);

        var basket = await redisService.HashGetAsync<Domain.Basket?>(nameof(Basket), customerId);

        Guard.Against.NotFound(customerId, basket);

        var basketDto = new BasketDto(basket.AccountId, []);

        foreach (var item in basket.BasketItems)
        {
            var book = await bookService.GetBook(item.Id);

            basketDto.Items.Add(new(book.Id, book.Name, item.Quantity, book.Price, book.PriceSale));
        }

        return basketDto;
    }
}
