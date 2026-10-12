using BookWorm.Scheduler.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseRouting();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapQuartzHttpApi();
app.MapQuartzDashboard();

app.MapDefaultEndpoints();

app.Run();
