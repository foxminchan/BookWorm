using BookWorm.Constants.Aspire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.A2A;
using ModelContextProtocol.Client;

namespace BookWorm.Chassis.AI.Extensions;

public static class KernelExtensions
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

    public static async Task<IReadOnlyList<KernelFunction>> MapToFunctionsAsync(this Kernel kernel)
    {
        var mcpClient = kernel.Services.GetRequiredService<IMcpClient>();
        var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
        var functions = tools.Select(aiFunction => aiFunction.AsKernelFunction());
        return [.. functions];
    }

    public static KernelPlugin MapToAgentPlugin(this Kernel kernel, string agentName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(agentName);

        var agent = kernel.Services.GetRequiredKeyedService<A2AAgent>(agentName);

        return KernelPluginFactory.CreateFromFunctions(
            $"{agentName}Plugin",
            [AgentKernelFunctionFactory.CreateFromAgent(agent)]
        );
    }
}
