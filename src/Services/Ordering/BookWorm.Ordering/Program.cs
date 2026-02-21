using BookWorm.Chassis.Security.Keycloak;
using BookWorm.Ordering.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseDefaultCors();

app.UseMiddleware<KeycloakTokenIntrospectionMiddleware>();

app.UseRateLimiter();

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(ApiVersions.V1)
    .ReportApiVersions()
    .Build();

app.MapEndpoints(apiVersionSet);

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.Run();
