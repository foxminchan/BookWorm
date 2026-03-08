var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseDefaultOpenApi();

app.MapMcp("/mcp");

app.Run();
