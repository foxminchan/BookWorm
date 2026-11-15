using BookWorm.Catalog.Ingestion;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.Security.Extensions;
using BookWorm.ServiceDefaults;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.ConfigureFunctionsWebApplication();
builder.Services.AddOpenTelemetry().UseFunctionsWorkerDefaults();
builder.AddDefaultAuthentication();

builder.AddAzureNpgsqlDataSource(Components.Database.Catalog);
builder.AddQdrantClient(Components.VectorDb);
builder.Services.AddQdrantCollection<Guid, TextSnippet>(TextSnippet.CollectionName);

builder.AddAIProject().WithLlmModels().WithAITelemetry();
builder.Services.AddScoped<IVectorBatchProcessor, VectorBatchProcessor>();

await builder.Build().RunAsync();
