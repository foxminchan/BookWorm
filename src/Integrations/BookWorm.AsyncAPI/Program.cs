using BookWorm.AsyncAPI;
using BookWorm.AsyncAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container
builder.Services.AddControllers();

// Add service discovery
builder.Services.AddServiceDiscovery();

// Add AsyncAPI with centralized configuration
builder.Services.AddAsyncApiSchemaGeneration(options =>
{
    options.AssemblyMarkerTypes = [typeof(IAsyncApiMarker)];
    
    using var sp = builder.Services.BuildServiceProvider();
    var document = sp.GetRequiredService<DocumentOptions>();

    foreach (var version in sp.GetApiVersionDescription())
    {
        options.AsyncApi = new()
        {
            Info = new("BookWorm Centralized AsyncAPI", version.ApiVersion.ToString())
            {
                Description = "Centralized AsyncAPI documentation for all BookWorm services",
                License = new(document.LicenseName)
                {
                    Url = document.LicenseUrl.ToString(),
                },
                Contact = new()
                {
                    Name = document.AuthorName,
                    Url = document.AuthorUrl.ToString(),
                    Email = document.AuthorEmail,
                },
            },
        };
    }
});

// Add AsyncAPI aggregation service
builder.Services.AddScoped<IAsyncApiAggregatorService, AsyncApiAggregatorService>();

// Add HttpClient for service discovery
builder.Services.AddHttpClient();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.MapControllers();

// Configure AsyncAPI
app.MapAsyncApiDocuments();
app.MapAsyncApiUi();

app.Run();