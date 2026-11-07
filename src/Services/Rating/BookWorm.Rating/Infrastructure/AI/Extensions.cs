using A2A;
using BookWorm.Chassis.AI.Agents;
using BookWorm.Chassis.AI.Extensions;
using BookWorm.Chassis.AI.Governance;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Chassis.AI.Presidio;
using BookWorm.Constants.Other;
using BookWorm.Rating.Tools;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using ModelContextProtocol.Client;

namespace BookWorm.Rating.Infrastructure.AI;

internal static class Extensions
{
    private const string DefinitionFile = "Agents/rating-agent.yaml";

    extension(IHostApplicationBuilder builder)
    {
        public void AddAgents()
        {
            var services = builder.Services;

            services.AddA2AGrpc();
            services.AddSingleton<IA2ARequestHandler>(sp =>
                sp.GetRequiredKeyedService<A2AServer>(Agents.RatingAgent)
            );

            builder.AddAIServices().WithAITelemetry();
            builder.AddPresidio();

            builder.AddMcpClient(Constants.Aspire.Services.McpTools);

            builder.AddAgentGovernance("Policies/rating-agent.yaml");

            services.AddSingleton<ReviewTool>();

            builder
                .AddAIAgent(
                    Agents.RatingAgent,
                    (sp, key) =>
                    {
                        var compactionProvider = RatingCompactionPipelineFactory.Create(
                            sp.GetRequiredService<IChatClient>()
                        );

                        var chatClient = sp.GetRequiredService<IChatClient>()
                            .AsBuilder()
                            .UsePIIMiddleware(sp)
                            .UseGuardrailMiddleware()
                            .Use(
                                RatingOutputGuardrailMiddleware.InvokeAsync,
                                RatingOutputGuardrailMiddleware.InvokeStreamingAsync
                            )
                            .UseAIContextProviders(compactionProvider)
                            .Build(sp);

                        var reviewPlugin = sp.GetRequiredService<ReviewTool>();
                        var mcpClient = sp.GetRequiredService<McpClient>();

                        var (options, toolNames) = AgentDefinitionLoader.Load(DefinitionFile, key);
                        var mcpTools = mcpClient.GetAgentTools(toolNames);

                        options.AllowConcurrentInvocation = true;
                        options.ChatOptions?.Tools = [.. reviewPlugin.AsAITools(), .. mcpTools];

                        var agent = new ChatClientAgent(chatClient, options);

                        return agent.WithBookWormGovernance(sp, key);
                    }
                )
                .AddA2AServer()
                .WithInMemorySessionStore();
        }
    }
}
