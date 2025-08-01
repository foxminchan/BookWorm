var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseOutputCache();

app.MapDefaultEndpoints();

app.UseTickerQ();

app.MapGet("/", () => Results.Redirect("tickerq")).ExcludeFromDescription();

app.Run();
