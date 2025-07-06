using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.A2A;
using SharpA2A.AspNetCore;

namespace BookWorm.Chat.Agents;

[ExcludeFromCodeCoverage]
internal static class Extensions
{
    /// <summary>
    /// Registers agent services as keyed singletons in the dependency injection container for use within the application.
    /// </summary>
    /// <remarks>
    /// Adds <c>BookAgent</c>, <c>LanguageAgent</c>, <c>SummarizeAgent</c>, and <c>SentimentAgent</c> as keyed singleton services, enabling their retrieval by name. The <c>BookAgent</c> is initialized with plugin support and additional dependencies, while the other agents are created with a <c>Kernel</c> instance.
    /// </remarks>
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddKeyedSingleton(
            nameof(BookAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                var mcpClient = sp.GetRequiredService<IMcpClient>();
                var resolver = sp.GetRequiredService<ServiceEndpointResolver>();
                return BookAgent
                    .CreateAgentWithPluginsAsync(kernel, mcpClient, resolver)
                    .GetAwaiter()
                    .GetResult();
            }
        );

        services.AddKeyedSingleton(
            nameof(LanguageAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return LanguageAgent.CreateAgent(kernel);
            }
        );

        services.AddKeyedSingleton(
            nameof(SummarizeAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return SummarizeAgent.CreateAgent(kernel);
            }
        );

        services.AddKeyedSingleton(
            nameof(SentimentAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                return SentimentAgent.CreateAgent(kernel);
            }
        );
    }

    /// <summary>
    /// Maps the SummarizeAgent as both A2A and HTTP A2A endpoints at "/agents/summarize" on the web application.
    /// </summary>
    public static void MapHostSummarizeAgent(this WebApplication app)
    {
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(
            nameof(SummarizeAgent)
        );
        var hostAgent = new A2AHostAgent(agent, SummarizeAgent.GetAgentCard());
        app.MapA2A(hostAgent.TaskManager!, "/agents/summarize").WithTags(nameof(SummarizeAgent));
        app.MapHttpA2A(hostAgent.TaskManager!, "/agents/summarize")
            .WithTags(nameof(SummarizeAgent));
    }

    /// <summary>
    /// Maps the SentimentAgent as a hosted A2A and HTTP A2A endpoint at "/agents/sentiment" on the web application.
    /// </summary>
    public static void MapHostSentimentAgent(this WebApplication app)
    {
        var agent = app.Services.GetRequiredKeyedService<ChatCompletionAgent>(
            nameof(SentimentAgent)
        );
        var hostAgent = new A2AHostAgent(agent, SentimentAgent.GetAgentCard());
        app.MapA2A(hostAgent.TaskManager!, "/agents/sentiment").WithTags(nameof(SentimentAgent));
        app.MapHttpA2A(hostAgent.TaskManager!, "/agents/sentiment")
            .WithTags(nameof(SentimentAgent));
    }
}
