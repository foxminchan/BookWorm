using BookWorm.Constants.Aspire;
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
                builder.Configuration.GetConnectionString(Components.OpenAI.Chat)
            )
        )
        {
            builder
                .AddOpenAIClient(
                    Components.OpenAI.Chat,
                    configureOptions =>
                        configureOptions.EnableSensitiveTelemetryData =
                            builder.Environment.IsDevelopment()
                )
                .AddChatClient();
        }

        if (
            !string.IsNullOrWhiteSpace(
                builder.Configuration.GetConnectionString(Components.OpenAI.Embedding)
            )
        )
        {
            builder.AddOpenAIClient(Components.OpenAI.Embedding).AddEmbeddingGenerator();
        }

        return builder;
    }

    public static void WithAITelemetry(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        AppContext.SetSwitch(
            "OpenAI.Experimental.EnableOpenTelemetry",
            builder.Environment.IsDevelopment()
        );

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
