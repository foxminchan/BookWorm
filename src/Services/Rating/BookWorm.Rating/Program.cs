using BookWorm.Rating.Agents;
using BookWorm.Rating.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.UseMiddleware<KeycloakTokenIntrospectionMiddleware>();

app.UseRateLimiter();

app.MapDefaultEndpoints();

var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new(1, 0)).ReportApiVersions().Build();

app.MapEndpoints(apiVersionSet, "feedbacks");

app.UseDefaultOpenApi();

app.UseDefaultAsyncApi();

app.MapHostRatingAgent();

app.Run();
