using BookWorm.Chat.Agents;
using BookWorm.Chat.Extensions;
using BookWorm.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.UseRateLimiter();

app.MapDefaultEndpoints();

var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new(1, 0)).ReportApiVersions().Build();

app.MapEndpoints(apiVersionSet, "chats");

app.UseDefaultOpenApi();

app.MapHostSummarizeAgent();

app.MapHostSentimentAgent();

app.Run();
