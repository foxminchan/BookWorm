using BookWorm.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapDefaultEndpoints();

app.Run();
