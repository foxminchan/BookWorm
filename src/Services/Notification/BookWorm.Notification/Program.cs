using BookWorm.Notification.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
else
{
    app.MapGet("/", () => Results.Redirect("/asyncapi/ui")).ExcludeFromDescription();
}

app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseOutputCache();

app.UseDefaultCors();

app.UseRequestTimeouts();

app.MapDefaultEndpoints();

app.UseDefaultAsyncApi();

app.Run();
