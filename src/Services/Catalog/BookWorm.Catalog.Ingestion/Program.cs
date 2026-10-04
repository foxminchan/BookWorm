using BookWorm.Catalog.Ingestion;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Search;
using BookWorm.Constants.Aspire;
using BookWorm.ServiceDefaults;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureFunctionsWebApplication();
builder.Services.AddOpenTelemetry().UseFunctionsWorkerDefaults();

builder.AddAzureNpgsqlDataSource(Components.Database.Catalog);
builder.AddQdrantClient(Components.VectorDb);
builder.Services.AddQdrantCollection<Guid, TextSnippet>(TextSnippet.CollectionName);

builder.AddAIServices().WithAITelemetry();
builder.Services.AddScoped<IVectorBatchProcessor, VectorBatchProcessor>();

await builder.Build().RunAsync();
