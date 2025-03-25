﻿using BasketGrpcServiceClient = BookWorm.Basket.Grpc.Services.BasketGrpcService.BasketGrpcServiceClient;
using BookGrpcServiceClient = BookWorm.Catalog.Grpc.Services.BookGrpcService.BookGrpcServiceClient;

namespace BookWorm.Ordering.Grpc;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddGrpcServices(this IServiceCollection services)
    {
        services.AddGrpc();

        services
            .AddGrpcClient<BookGrpcServiceClient>(o =>
                o.Address = new($"http+https://{Application.Catalog}")
            )
            .AddStandardResilienceHandler();
        services.AddSingleton<IBookService, BookService>();

        services
            .AddGrpcClient<BasketGrpcServiceClient>(o =>
                o.Address = new($"http+https://{Application.Basket}")
            )
            .AddAuthToken()
            .AddStandardResilienceHandler();
        services.AddSingleton<IBasketService, BasketService>();

        services.AddScoped<BasketMetadata>();
    }
}
