using BookWorm.ServiceDefaults.Cors;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.UseDefaultCors();

app.MapMcp("/mcp").RequireAuthorization();

app.Run();
