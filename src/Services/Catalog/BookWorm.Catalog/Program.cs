using BookWorm.Catalog.Extensions;
using BookWorm.Catalog.Grpc.Services;
using BookWorm.ServiceDefaults.Cors;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseDefaultDiagnostics();

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseForwardedHeaders();

app.UseDefaultCors();

app.UseRateLimiter();

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(ApiVersions.V1)
    .ReportApiVersions()
    .Build();

app.MapEndpoints(apiVersionSet);

app.NewVersionedApi("Catalog gRPC").MapGrpcService<BookService>().HasApiVersion(ApiVersions.V1);

app.MapGrpcHealthChecksService();

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.Run();
