using BookWorm.Catalog.Ingestion;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.EventBus;
using BookWorm.Chassis.EventBus.Wolverine;
using BookWorm.ServiceDefaults;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenTelemetry().UseFunctionsWorkerDefaults();

builder.AddEventBus<ProcessVectorBatchFunction>(opts =>
    opts.UsePostgresOutbox(builder.Configuration, Components.Database.Catalog)
);

builder.AddAzureNpgsqlDataSource(Components.Database.Catalog);
builder.AddQdrantClient(Components.VectorDb);
builder.Services.AddQdrantCollection<Guid, TextSnippet>(TextSnippet.CollectionName);

builder.AddAIProject().WithLlmModels().WithAITelemetry();
builder.Services.AddScoped<IVectorBatchProcessor, VectorBatchProcessor>();

await builder.Build().RunAsync();
