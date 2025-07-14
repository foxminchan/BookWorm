using BookWorm.BackOffice.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDefaultCors();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add problem details for exception handling
builder.Services.AddProblemDetails();

// Add HTTP clients for microservices
builder.Services.AddHttpClient("catalog-api", client =>
{
    client.BaseAddress = new Uri("https://catalog");
});

builder.Services.AddHttpClient("ordering-api", client =>
{
    client.BaseAddress = new Uri("https://ordering");
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
