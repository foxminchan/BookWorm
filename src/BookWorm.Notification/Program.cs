using BookWorm.Notification.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults()
    .AddApplicationServices();

var app = builder.Build();

app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.Run();
