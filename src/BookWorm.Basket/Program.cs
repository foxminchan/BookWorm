using BookWorm.Basket.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
    .AddApplicationServices();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthorization();

app.UseOpenApi();

app.MapEndpoints();

app.MapDefaultEndpoints();

app.MapGrpcService<BasketService>();

app.Run();
