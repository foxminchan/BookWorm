using BookWorm.Catalog.Extensions;
using GrpcBookServer = BookWorm.Catalog.Grpc.BookService;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthorization();

app.UseOpenApi();

app.MapEndpoints();

app.MapDefaultEndpoints();

app.MapGrpcService<GrpcBookServer>();

app.Run();
