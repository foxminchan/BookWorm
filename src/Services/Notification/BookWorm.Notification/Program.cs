using BookWorm.Notification.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseDefaultAsyncApi();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/asyncapi/ui")).ExcludeFromDescription();
}
else
{
    app.MapGet("/", () => Results.Redirect("/asyncapi/asyncapi.json")).ExcludeFromDescription();
}

app.Run();
