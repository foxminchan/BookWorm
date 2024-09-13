var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

var app = builder.Build();

app.UseExceptionHandler();

app.UseAuthorization();

app.UseOpenApi();

app.MapEndpoints();

app.MapDefaultEndpoints();

app.Run();
