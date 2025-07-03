using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;

namespace BookWorm.Chat.Agents;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static void AddAgents(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddKeyedSingleton(
            nameof(BookAgent),
            (sp, _) =>
            {
                var kernel = sp.GetRequiredService<Kernel>();
                var mcpClient = sp.GetRequiredService<IMcpClient>();
                return BookAgent
                    .CreateAgentWithPluginsAsync(kernel, mcpClient)
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
}
