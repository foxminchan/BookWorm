var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

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

app.MapGet("/", () => TypedResults.Redirect("tickerq")).ExcludeFromDescription();

app.Run();
