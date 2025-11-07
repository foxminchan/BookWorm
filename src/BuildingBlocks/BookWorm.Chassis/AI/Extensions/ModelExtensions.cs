using BookWorm.Constants.Aspire;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BookWorm.Chassis.AI.Extensions;

public static class ModelExtensions
{
    public static IHostApplicationBuilder AddAIServices(this IHostApplicationBuilder builder)
    {
        if (
            !string.IsNullOrWhiteSpace(
                builder.Configuration.GetConnectionString(Components.Ollama.Chat)
            )
        )
        {
            builder
                .AddOllamaApiClient(Components.Ollama.Chat)
                .AddChatClient(otel =>
                    otel.EnableSensitiveData = builder.Environment.IsDevelopment()
                )
                .UseFunctionInvocation();
        }

        if (
            !string.IsNullOrWhiteSpace(
                builder.Configuration.GetConnectionString(Components.Ollama.Embedding)
            )
        )
        {
            builder.AddOllamaApiClient(Components.Ollama.Embedding).AddEmbeddingGenerator();
        }

        return builder;
    }

    public static void WithAITelemetry(this IHostApplicationBuilder builder)
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
}
