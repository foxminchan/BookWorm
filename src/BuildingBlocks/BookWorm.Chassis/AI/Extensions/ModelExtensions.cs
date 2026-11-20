using Azure.AI.Projects;
using BookWorm.Chassis.AI.Decisions;
using BookWorm.Chassis.AI.Middlewares;
using BookWorm.Constants.Aspire;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookWorm.Chassis.AI.Extensions;

public static class ModelExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        ///     Registers AI services (chat client and embedding generator) based on available
        ///     connection strings in the application configuration.
        /// </summary>
        /// <remarks>
        ///     Conditionally adds an Azure AI Project OpenAI client with chat and embedding support
        ///     if the <c>Components.Foundry.Project</c> connection string is present.
        /// </remarks>
        /// <returns>The <see cref="IHostApplicationBuilder" /> instance for chaining.</returns>
        /// <example>
        ///     <code>
        ///         builder.AddAIServices();
        ///     </code>
        /// </example>
        public IHostApplicationBuilder AddAIProject()
        {
            builder.AddAzureAIProjectClient(Components.Foundry.Project);

            return builder;
        }

        /// <summary>
        ///     Configures AI models for the application, including chat and embedding services.
        /// </summary>
        /// <remarks>
        ///     Adds an Azure AI Project OpenAI client with chat and embedding support.
        ///     Configures the chat client to use function invocation and model logging.
        /// </remarks>
        /// <returns>The <see cref="IHostApplicationBuilder" /> instance for chaining.</returns>
        /// <example>
        ///     <code>
        ///         builder.WithLlmModels();
        ///     </code>
        /// </example>
        public IHostApplicationBuilder WithLlmModels()
        {
            var clientBuilder = builder.AddAzureProjectOpenAIClient(Components.Foundry.Project);

            clientBuilder
                .AddChatClient(Components.Foundry.Chat)
                .UseFunctionInvocation()
                .UseModelLogging();

            clientBuilder.AddEmbeddingGenerator(Components.Foundry.Embedding);

            return builder;
        }

        /// <summary>
        ///     Configures the <see cref="IDecisionClient" /> backed by Microsoft Foundry's
        ///     <c>Microsoft-Decision-1</c> (<c>/providers/microsoft/v1/systemone</c>) endpoint.
        /// </summary>
        /// <returns>The <see cref="IHostApplicationBuilder" /> instance for chaining.</returns>
        /// <example>
        ///     <code>
        ///         builder.WithDecisionModels();
        ///     </code>
        /// </example>
        public IHostApplicationBuilder WithDecisionModels()
        {
            builder.Services.AddSingleton<IDecisionClient>(sp => new FoundrySystemOneDecisionClient(
                sp.GetRequiredService<AIProjectClient>(),
                Components.Foundry.Decision,
                sp.GetService<ILogger<FoundrySystemOneDecisionClient>>()
            ));

            return builder;
        }

        /// <summary>
        ///     Configures OpenTelemetry tracing and metrics for AI-related components,
        ///     including Microsoft Extensions AI and Microsoft Agents AI sources.
        /// </summary>
        /// <remarks>
        ///     Enables the OpenAI experimental OpenTelemetry switch in development environments.
        ///     Registers tracing sources for agent workflows, runtime, and in-process actors,
        ///     as well as meters for agent governance and AI metrics.
        /// </remarks>
        /// <example>
        ///     <code>
        ///         builder.WithAITelemetry();
        ///     </code>
        /// </example>
        public void WithAITelemetry()
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
                        .AddSource(
                            "Microsoft.Agents.AI.Runtime.Abstractions.InMemoryActorStateStorage"
                        )
                        .AddSource(FoundrySystemOneDecisionClient.ActivitySourceName)
                )
                .WithMetrics(x => x.AddMeter("*Microsoft.Agents.AI").AddMeter("agent_governance"));
        }
    }
}
