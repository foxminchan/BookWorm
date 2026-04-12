using BookWorm.Constants.Aspire;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        ///     Conditionally adds an OpenAI chat client with function invocation support if
        ///     <c>Components.OpenAI.Chat</c> connection string is present, and an embedding
        ///     generator if <c>Components.OpenAI.Embedding</c> connection string is present.
        /// </remarks>
        /// <returns>The <see cref="IHostApplicationBuilder" /> instance for chaining.</returns>
        /// <example>
        ///     <code>
        ///         builder.AddAIServices();
        ///     </code>
        /// </example>
        public IHostApplicationBuilder AddAIServices()
        {
            if (
                !string.IsNullOrWhiteSpace(
                    builder.Configuration.GetConnectionString(Components.OpenAI.Chat)
                )
            )
            {
                builder
                    .AddOpenAIClientFromConfiguration(Components.OpenAI.Chat)
                    .AddChatClient()
                    .UseFunctionInvocation();
            }

            if (
                !string.IsNullOrWhiteSpace(
                    builder.Configuration.GetConnectionString(Components.OpenAI.Embedding)
                )
            )
            {
                builder
                    .AddOpenAIClientFromConfiguration(Components.OpenAI.Embedding)
                    .AddEmbeddingGenerator();
            }

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
                )
                .WithMetrics(x => x.AddMeter("*Microsoft.Agents.AI").AddMeter("agent_governance"));
        }
    }
}
