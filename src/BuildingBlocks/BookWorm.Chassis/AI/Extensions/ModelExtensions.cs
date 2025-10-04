using BookWorm.Constants.Aspire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.AI.Extensions;

public static class ModelExtensions
{
    public static void AddAgentsTelemetry(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services
            .AddOpenTelemetry()
            .WithTracing(x =>
                x.AddSource("*Microsoft.Extensions.AI")
                    .AddSource("*Microsoft.Agents.AI")
                    .AddSource("Microsoft.Agents.AI.Workflows*")
                    .AddSource("Microsoft.Agents.AI.Runtime.InProcess")
                    .AddSource("Microsoft.Agents.AI.Runtime.Abstractions.InMemoryActorStateStorage")
            )
            .WithMetrics(x => x.AddMeter("*Microsoft.Agents.AI"));
    }

    public static void AddChatClient(this IHostApplicationBuilder builder)
    {
        builder
            .AddOllamaApiClient(Components.Ollama.Chat)
            .AddChatClient(otel => otel.EnableSensitiveData = builder.Environment.IsDevelopment());
    }

    public static void AddEmbeddingGenerator(this IHostApplicationBuilder builder)
    {
        builder.AddOllamaApiClient(Components.Ollama.Embedding).AddEmbeddingGenerator();
    }
}
