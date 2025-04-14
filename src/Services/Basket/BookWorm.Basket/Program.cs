using BookWorm.Basket.Extensions;
using BookWorm.Basket.Grpc.Services.Basket;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new(1, 0)).ReportApiVersions().Build();

app.MapEndpoints(apiVersionSet);

app.MapGrpcService<BasketService>();

app.UseDefaultOpenApi();

app.UseDefaultAsyncApi();

app.Run();
