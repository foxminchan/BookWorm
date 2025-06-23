using BookWorm.Catalog.Extensions;
using BookWorm.Catalog.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.UseRateLimiter();

app.MapDefaultEndpoints();

var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new(1, 0)).ReportApiVersions().Build();

app.MapEndpoints(apiVersionSet);

app.MapGrpcService<BookService>();

app.UseDefaultOpenApi();

app.UseDefaultAsyncApi();

app.MapGrpcHealthChecksService();

app.Run();
