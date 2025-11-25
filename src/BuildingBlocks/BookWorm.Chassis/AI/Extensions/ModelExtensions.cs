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
                .WithMetrics(x => x.AddMeter("*Microsoft.Agents.AI"));
        }
    }
}
