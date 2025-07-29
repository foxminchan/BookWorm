using BookWorm.Constants.Aspire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.A2A;
using SharpA2A.Core;

namespace BookWorm.Chassis.RAG;

public static class Extensions
{
    private const string ActivitySourceName = "Microsoft.SemanticKernel*";

    public static void AddSkTelemetry(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        AppContext.SetSwitch(
            "Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive",
            builder.Environment.IsDevelopment()
        );

        services
            .AddOpenTelemetry()
            .WithTracing(x => x.AddSource(ActivitySourceName))
            .WithMetrics(x => x.AddMeter(ActivitySourceName));
    }

    public static void AddChatCompletion(this IHostApplicationBuilder builder)
    {
        builder.AddOllamaApiClient(Components.Ollama.Chat);
        builder.Services.AddOllamaChatCompletion();
    }

    public static void AddEmbeddingGenerator(this IHostApplicationBuilder builder)
    {
        builder.AddOllamaApiClient(Components.Ollama.Embedding).AddEmbeddingGenerator();
    }

    public static async Task<A2AAgent> CreateAgentAsync(this string agentUri)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new(agentUri),
            Timeout = TimeSpan.FromSeconds(60),
        };

        var client = new A2AClient(httpClient);
        var cardResolver = new A2ACardResolver(httpClient);
        var agentCard = await cardResolver.GetAgentCardAsync();

        return new(client, agentCard);
    }
}
