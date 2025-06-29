using System.Diagnostics.CodeAnalysis;
using BookWorm.Constants.Aspire;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI;

public static class Extensions
{
    private const string ActivitySourceName = "Experimental.Microsoft.Extensions.AI*";

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static void AddAITelemetry(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services
            .AddOpenTelemetry()
            .WithMetrics(m => m.AddMeter(ActivitySourceName))
            .WithTracing(t => t.AddSource(ActivitySourceName));
    }

    public static void AddChatClient(this IHostApplicationBuilder builder)
    {
        var provider = builder.Services.BuildServiceProvider();

        var loggerFactory =
            provider.GetRequiredService<ILoggerFactory>()
            ?? throw new InvalidOperationException("ILoggerFactory is not registered.");

        builder
            .AddOllamaApiClient(Components.Ollama.Chat)
            .AddChatClient()
            .UseDistributedCache()
            .UseFunctionInvocation()
            .UseLogging(loggerFactory)
            .UseOpenTelemetry(configure: c =>
                c.EnableSensitiveData = builder.Environment.IsDevelopment()
            );
    }

    public static void AddEmbeddingGenerator(this IHostApplicationBuilder builder)
    {
        var provider = builder.Services.BuildServiceProvider();

        var loggerFactory =
            provider.GetRequiredService<ILoggerFactory>()
            ?? throw new InvalidOperationException("ILoggerFactory is not registered.");

        builder
            .AddOllamaApiClient(Components.Ollama.Embedding)
            .AddEmbeddingGenerator()
            .UseLogging(loggerFactory)
            .UseOpenTelemetry(configure: c =>
                c.EnableSensitiveData = builder.Environment.IsDevelopment()
            );
    }
}
